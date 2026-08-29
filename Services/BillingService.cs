using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services;

public class BillingService
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _currentTenant;

    public BillingService(AppDbContext db, ICurrentTenant currentTenant)
    {
        _db = db;
        _currentTenant = currentTenant;
    }

    public async Task<BillingInvoiceDto> CreateInvoiceAsync(int tenantId, CreateBillingInvoiceDto dto)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        var tenant = await _db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.TenantId == tenantId);
        if (tenant is null)
            throw new KeyNotFoundException("Tenant not found.");

        var subscription = await _db.Subscriptions.SingleOrDefaultAsync(s => s.TenantId == tenantId);
        if (subscription is null)
            throw new InvalidOperationException("The tenant does not have an active subscription record.");

        var planId = dto.PlanId ?? subscription.PlanId;
        if (!await _db.Plans.AnyAsync(p => p.PlanId == planId))
            throw new KeyNotFoundException("Plan not found.");

        var invoice = new BillingInvoice
        {
            TenantId = tenantId,
            SubscriptionId = subscription.SubscriptionId,
            PlanId = planId,
            InvoiceNumber = await GenerateInvoiceNumberAsync(tenantId),
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? tenant.CurrencyCode : dto.Currency.Trim().ToUpperInvariant(),
            Amount = dto.Amount,
            AmountPaid = 0m,
            Status = BillingInvoiceStatus.Open,
            IssueDate = dto.IssueDate ?? DateTime.UtcNow,
            DueDate = dto.DueDate ?? DateTime.UtcNow.AddDays(14),
            PeriodStart = dto.PeriodStart ?? subscription.StartDate,
            PeriodEnd = dto.PeriodEnd ?? subscription.RenewalDate ?? DateTime.UtcNow.AddMonths(1),
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await WithTenantScopeAsync(tenantId, async () =>
        {
            _db.BillingInvoices.Add(invoice);
            await _db.SaveChangesAsync();
        });

        return await GetInvoiceDtoAsync(invoice.BillingInvoiceId, tenantId);
    }

    public async Task<BillingInvoiceDto> RecordPaymentAsync(int tenantId, int invoiceId, RecordPaymentDto dto)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.");

        var invoice = await _db.BillingInvoices
            .Include(i => i.Payments)
            .SingleOrDefaultAsync(i => i.BillingInvoiceId == invoiceId && i.TenantId == tenantId);
        if (invoice is null)
            throw new KeyNotFoundException("Invoice not found.");

        var payment = new SubscriptionPayment
        {
            TenantId = tenantId,
            BillingInvoiceId = invoice.BillingInvoiceId,
            Amount = dto.Amount,
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? invoice.Currency : dto.Currency.Trim().ToUpperInvariant(),
            PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? "Manual" : dto.PaymentMethod.Trim(),
            Reference = string.IsNullOrWhiteSpace(dto.Reference) ? $"MANUAL-{DateTime.UtcNow:yyyyMMddHHmmss}" : dto.Reference.Trim(),
            Status = "Paid",
            PaymentDate = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await WithTenantScopeAsync(tenantId, async () =>
        {
            invoice.AmountPaid += dto.Amount;
            if (invoice.AmountPaid >= invoice.Amount)
            {
                invoice.AmountPaid = invoice.Amount;
                invoice.Status = BillingInvoiceStatus.Paid;
                invoice.PaidAt ??= DateTime.UtcNow;
            }
            else
            {
                invoice.Status = BillingInvoiceStatus.Open;
            }

            invoice.UpdatedAt = DateTime.UtcNow;
            _db.SubscriptionPayments.Add(payment);
            await _db.SaveChangesAsync();
        });

        if (dto.ApplyToSubscription)
        {
            await ApplySuccessfulPaymentToSubscriptionAsync(tenantId, invoice.SubscriptionId);
        }

        return await GetInvoiceDtoAsync(invoice.BillingInvoiceId, tenantId);
    }

    public async Task<BillingInvoiceDto> UpdateInvoiceStatusAsync(int tenantId, int invoiceId, UpdateBillingInvoiceStatusDto dto)
    {
        var invoice = await _db.BillingInvoices
            .SingleOrDefaultAsync(i => i.BillingInvoiceId == invoiceId && i.TenantId == tenantId);
        if (invoice is null)
            throw new KeyNotFoundException("Invoice not found.");

        var normalizedStatus = (dto.Status ?? string.Empty).Trim();
        if (!Enum.TryParse<BillingInvoiceStatus>(normalizedStatus, true, out var status))
            throw new ArgumentException("Status must be Draft, Open, Paid, Overdue, or Cancelled.");

        await WithTenantScopeAsync(tenantId, async () =>
        {
            invoice.Status = status;
            invoice.UpdatedAt = DateTime.UtcNow;
            if (status == BillingInvoiceStatus.Paid)
            {
                invoice.PaidAt ??= DateTime.UtcNow;
                invoice.AmountPaid = Math.Min(invoice.AmountPaid, invoice.Amount);
            }
            else if (status != BillingInvoiceStatus.Paid)
            {
                invoice.PaidAt = null;
            }

            await _db.SaveChangesAsync();
        });

        if (status == BillingInvoiceStatus.Paid)
        {
            await ApplySuccessfulPaymentToSubscriptionAsync(tenantId, invoice.SubscriptionId);
        }

        return await GetInvoiceDtoAsync(invoice.BillingInvoiceId, tenantId);
    }

    public async Task<BillingHistoryDto> GetHistoryAsync(int tenantId)
    {
        var invoices = await _db.BillingInvoices
            .AsNoTracking()
            .Include(i => i.Payments)
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.IssueDate)
            .Select(i => new BillingInvoiceDto
            {
                BillingInvoiceId = i.BillingInvoiceId,
                TenantId = i.TenantId,
                TenantName = i.Tenant.Name,
                SubscriptionId = i.SubscriptionId,
                InvoiceNumber = i.InvoiceNumber,
                Currency = i.Currency,
                Amount = i.Amount,
                AmountPaid = i.AmountPaid,
                Status = i.Status.ToString(),
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                PeriodStart = i.PeriodStart,
                PeriodEnd = i.PeriodEnd,
                PaidAt = i.PaidAt,
                Notes = i.Notes,
                Payments = i.Payments.Select(p => new BillingPaymentDto
                {
                    SubscriptionPaymentId = p.SubscriptionPaymentId,
                    BillingInvoiceId = p.BillingInvoiceId,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    PaymentMethod = p.PaymentMethod,
                    Reference = p.Reference,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate,
                    Notes = p.Notes
                }).OrderByDescending(p => p.PaymentDate).ToList()
            })
            .ToListAsync();

        var payments = await _db.SubscriptionPayments
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .Select(p => new BillingPaymentDto
            {
                SubscriptionPaymentId = p.SubscriptionPaymentId,
                BillingInvoiceId = p.BillingInvoiceId,
                Amount = p.Amount,
                Currency = p.Currency,
                PaymentMethod = p.PaymentMethod,
                Reference = p.Reference,
                Status = p.Status,
                PaymentDate = p.PaymentDate,
                Notes = p.Notes
            })
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return new BillingHistoryDto
        {
            Invoices = invoices,
            Payments = payments
        };
    }

    private async Task ApplySuccessfulPaymentToSubscriptionAsync(int tenantId, int subscriptionId)
    {
        var subscription = await _db.Subscriptions.SingleOrDefaultAsync(s => s.SubscriptionId == subscriptionId && s.TenantId == tenantId);
        if (subscription is null)
            return;

        subscription.Status = SubscriptionStatus.Active;
        subscription.UpdatedAt = DateTime.UtcNow;
        if (!subscription.RenewalDate.HasValue || subscription.RenewalDate.Value <= DateTime.UtcNow)
            subscription.RenewalDate = DateTime.UtcNow.AddMonths(1);

        var tenant = await _db.Tenants.SingleOrDefaultAsync(t => t.TenantId == tenantId);
        if (tenant is not null)
        {
            tenant.Status = "Active";
            tenant.LifecycleStatus = "Active";
            tenant.BillingStatus = "Paid";
        }

        await _db.SaveChangesAsync();
    }

    private async Task<string> GenerateInvoiceNumberAsync(int tenantId)
    {
        var sequence = await _db.BillingInvoices.CountAsync(i => i.TenantId == tenantId) + 1;
        return $"INV-{tenantId}-{DateTime.UtcNow:yyyyMMdd}-{sequence:0000}";
    }

    private async Task<BillingInvoiceDto> GetInvoiceDtoAsync(int invoiceId, int tenantId)
    {
        var result = await _db.BillingInvoices
            .AsNoTracking()
            .Include(i => i.Payments)
            .Where(i => i.BillingInvoiceId == invoiceId && i.TenantId == tenantId)
            .Select(i => new BillingInvoiceDto
            {
                BillingInvoiceId = i.BillingInvoiceId,
                TenantId = i.TenantId,
                TenantName = i.Tenant.Name,
                SubscriptionId = i.SubscriptionId,
                InvoiceNumber = i.InvoiceNumber,
                Currency = i.Currency,
                Amount = i.Amount,
                AmountPaid = i.AmountPaid,
                Status = i.Status.ToString(),
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                PeriodStart = i.PeriodStart,
                PeriodEnd = i.PeriodEnd,
                PaidAt = i.PaidAt,
                Notes = i.Notes,
                Payments = i.Payments.Select(p => new BillingPaymentDto
                {
                    SubscriptionPaymentId = p.SubscriptionPaymentId,
                    BillingInvoiceId = p.BillingInvoiceId,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    PaymentMethod = p.PaymentMethod,
                    Reference = p.Reference,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate,
                    Notes = p.Notes
                }).OrderByDescending(p => p.PaymentDate).ToList()
            })
            .SingleAsync();

        return result;
    }

    private async Task WithTenantScopeAsync(int tenantId, Func<Task> work)
    {
        var previousTenantId = _currentTenant.TenantId;
        _currentTenant.SetTenant(tenantId);

        try
        {
            await work();
        }
        finally
        {
            if (previousTenantId is int id)
            {
                _currentTenant.SetTenant(id);
            }
            else
            {
                _currentTenant.Clear();
            }
        }
    }
}
