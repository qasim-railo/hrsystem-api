using System.Security.Claims;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Authorize]
[Route("api/support")]
public sealed class SupportController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;

    public SupportController(AppDbContext db, ICurrentTenant tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet("help-center")]
    public async Task<ActionResult<SupportHelpCenterDto>> GetHelpCenter()
    {
        if (_tenant.TenantId is not int tenantId)
            return Forbid();

        await EnsureSeedArticlesAsync(tenantId);

        var articles = await _db.SupportArticles.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsPublished)
            .OrderBy(x => x.Category).ThenBy(x => x.Title)
            .Select(x => new SupportArticleDto
            {
                Id = x.Id,
                Category = x.Category,
                Title = x.Title,
                Summary = x.Summary,
                Content = x.Content,
                Tags = x.Tags
            })
            .ToListAsync();

        return Ok(new SupportHelpCenterDto
        {
            ContactEmail = "support@peopleos.local",
            ContactPhone = "+974 4000 0000",
            SupportHours = "Sunday to Thursday, 8:00 AM – 6:00 PM",
            Articles = articles
        });
    }

    [HttpGet("tickets")]
    public async Task<ActionResult<IReadOnlyList<SupportTicketDto>>> GetTickets()
    {
        if (_tenant.TenantId is not int tenantId)
            return Forbid();

        var isPlatform = User.HasClaim("permission", "Platform.Tenants");
        var query = _db.SupportTickets
            .IgnoreQueryFilters()
            .Include(x => x.Messages.OrderBy(m => m.CreatedAt)).ThenInclude(x => x.Attachments)
            .Include(x => x.Attachments)
            .AsNoTracking()
            .Where(x => isPlatform || x.TenantId == tenantId);

        var tickets = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new SupportTicketDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                Category = x.Category,
                Subject = x.Subject,
                Description = x.Description,
                Priority = x.Priority,
                Status = x.Status,
                RequesterUserId = x.RequesterUserId,
                RequesterName = x.RequesterName,
                RequesterEmail = x.RequesterEmail,
                Source = x.Source,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                ClosedAt = x.ClosedAt,
                Messages = x.Messages.OrderBy(m => m.CreatedAt).Select(m => new SupportTicketMessageDto
                {
                    Id = m.Id,
                    SenderUserId = m.SenderUserId,
                    SenderName = m.SenderName,
                    SenderRole = m.SenderRole,
                    Message = m.Message,
                    IsInternal = m.IsInternal,
                    CreatedAt = m.CreatedAt,
                    Attachments = m.Attachments.Select(a => new SupportTicketAttachmentDto
                    {
                        Id = a.Id,
                        FileId = a.FileId,
                        FileName = a.FileName,
                        FileType = a.FileType
                    }).ToList()
                }).ToList(),
                Attachments = x.Attachments.Select(a => new SupportTicketAttachmentDto
                {
                    Id = a.Id,
                    FileId = a.FileId,
                    FileName = a.FileName,
                    FileType = a.FileType
                }).ToList()
            })
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpGet("tickets/{id:int}")]
    public async Task<ActionResult<SupportTicketDto>> GetTicketById(int id)
    {
        if (_tenant.TenantId is not int tenantId)
            return Forbid();

        var isPlatform = User.HasClaim("permission", "Platform.Tenants");
        var ticket = await _db.SupportTickets
            .IgnoreQueryFilters()
            .Include(x => x.Messages.OrderBy(m => m.CreatedAt)).ThenInclude(x => x.Attachments)
            .Include(x => x.Attachments)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && (isPlatform || x.TenantId == tenantId));

        if (ticket is null)
            return NotFound();

        return Ok(MapTicket(ticket));
    }

    [HttpPost("tickets")]
    public async Task<ActionResult<SupportTicketDto>> CreateTicket(CreateSupportTicketRequest request)
    {
        if (_tenant.TenantId is not int tenantId)
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest("A subject is required.");
        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest("A description is required.");

        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Support user";
        int? userId = int.TryParse(User.FindFirstValue("user_id"), out var parsedUserId) ? parsedUserId : null;

        var ticket = new SupportTicket
        {
            TenantId = tenantId,
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
            Subject = request.Subject.Trim(),
            Description = request.Description.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority.Trim(),
            Status = "Open",
            RequesterUserId = userId,
            RequesterName = string.IsNullOrWhiteSpace(request.RequesterName) ? userName : request.RequesterName.Trim(),
            RequesterEmail = string.IsNullOrWhiteSpace(request.RequesterEmail) ? (User.FindFirstValue(ClaimTypes.Email) ?? "support@peopleos.local") : request.RequesterEmail.Trim(),
            Source = string.IsNullOrWhiteSpace(request.Source) ? "InApp" : request.Source.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.SupportTickets.Add(ticket);
        await _db.SaveChangesAsync();

        var message = new SupportTicketMessage
        {
            TenantId = tenantId,
            TicketId = ticket.Id,
            SenderUserId = userId,
            SenderName = ticket.RequesterName,
            SenderRole = "User",
            Message = ticket.Description,
            IsInternal = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.SupportTicketMessages.Add(message);

        foreach (var fileId in request.AttachmentFileIds.Distinct())
        {
            var fileRecord = await _db.FileRecords.SingleOrDefaultAsync(x => x.FileId == fileId && x.TenantId == tenantId);
            if (fileRecord is null)
                continue;

            _db.SupportTicketAttachments.Add(new SupportTicketAttachment
            {
                TenantId = tenantId,
                TicketId = ticket.Id,
                MessageId = message.Id,
                FileId = fileRecord.FileId,
                FileName = fileRecord.OriginalFileName,
                FileType = fileRecord.MimeType,
                UploadedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, await GetTicket(ticket.Id));
    }

    [HttpPost("tickets/{id:int}/messages")]
    public async Task<ActionResult<SupportTicketDto>> AddTicketMessage(int id, CreateSupportTicketMessageRequest request)
    {
        if (_tenant.TenantId is not int tenantId)
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message text is required.");

        var isPlatform = User.HasClaim("permission", "Platform.Tenants");
        var ticket = await _db.SupportTickets.SingleOrDefaultAsync(x => x.Id == id && (isPlatform || x.TenantId == tenantId));
        if (ticket is null)
            return NotFound();

        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Support user";
        int? userId = int.TryParse(User.FindFirstValue("user_id"), out var parsedUserId) ? parsedUserId : null;
        var message = new SupportTicketMessage
        {
            TenantId = tenantId,
            TicketId = ticket.Id,
            SenderUserId = userId,
            SenderName = userName,
            SenderRole = isPlatform ? "SupportAgent" : "User",
            Message = request.Message.Trim(),
            IsInternal = request.IsInternal,
            CreatedAt = DateTime.UtcNow
        };

        _db.SupportTicketMessages.Add(message);
        foreach (var fileId in request.AttachmentFileIds.Distinct())
        {
            var fileRecord = await _db.FileRecords.SingleOrDefaultAsync(x => x.FileId == fileId && x.TenantId == tenantId);
            if (fileRecord is null)
                continue;

            _db.SupportTicketAttachments.Add(new SupportTicketAttachment
            {
                TenantId = tenantId,
                TicketId = ticket.Id,
                MessageId = message.Id,
                FileId = fileRecord.FileId,
                FileName = fileRecord.OriginalFileName,
                FileType = fileRecord.MimeType,
                UploadedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        ticket.Status = isPlatform && !string.IsNullOrWhiteSpace(request.Message) ? "InProgress" : ticket.Status;
        ticket.UpdatedAt = DateTime.UtcNow;
        if (ticket.Status == "Resolved" && isPlatform)
            ticket.ClosedAt ??= DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(await GetTicket(ticket.Id));
    }

    [HttpPut("tickets/{id:int}/status")]
    public async Task<ActionResult<SupportTicketDto>> UpdateStatus(int id, UpdateSupportTicketStatusRequest request)
    {
        if (_tenant.TenantId is not int tenantId)
            return Forbid();

        var isPlatform = User.HasClaim("permission", "Platform.Tenants");
        var ticket = await _db.SupportTickets.SingleOrDefaultAsync(x => x.Id == id && (isPlatform || x.TenantId == tenantId));
        if (ticket is null)
            return NotFound();

        var status = string.IsNullOrWhiteSpace(request.Status) ? "Open" : request.Status.Trim();
        ticket.Status = status;
        ticket.UpdatedAt = DateTime.UtcNow;
        if (status is "Resolved" or "Closed")
            ticket.ClosedAt ??= DateTime.UtcNow;
        else if (status is not "Open" and not "Pending" and not "InProgress")
            ticket.ClosedAt = null;

        await _db.SaveChangesAsync();
        return Ok(await GetTicket(ticket.Id));
    }

    private static SupportTicketDto MapTicket(SupportTicket ticket)
    {
        return new SupportTicketDto
        {
            Id = ticket.Id,
            TenantId = ticket.TenantId,
            Category = ticket.Category,
            Subject = ticket.Subject,
            Description = ticket.Description,
            Priority = ticket.Priority,
            Status = ticket.Status,
            RequesterUserId = ticket.RequesterUserId,
            RequesterName = ticket.RequesterName,
            RequesterEmail = ticket.RequesterEmail,
            Source = ticket.Source,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ClosedAt = ticket.ClosedAt,
            Messages = ticket.Messages.OrderBy(x => x.CreatedAt).Select(m => new SupportTicketMessageDto
            {
                Id = m.Id,
                SenderUserId = m.SenderUserId,
                SenderName = m.SenderName,
                SenderRole = m.SenderRole,
                Message = m.Message,
                IsInternal = m.IsInternal,
                CreatedAt = m.CreatedAt,
                Attachments = m.Attachments.Select(a => new SupportTicketAttachmentDto
                {
                    Id = a.Id,
                    FileId = a.FileId,
                    FileName = a.FileName,
                    FileType = a.FileType
                }).ToList()
            }).ToList(),
            Attachments = ticket.Attachments.Select(a => new SupportTicketAttachmentDto
            {
                Id = a.Id,
                FileId = a.FileId,
                FileName = a.FileName,
                FileType = a.FileType
            }).ToList()
        };
    }

    private async Task<SupportTicketDto> GetTicket(int ticketId)
    {
        var ticket = await _db.SupportTickets
            .Include(x => x.Messages).ThenInclude(m => m.Attachments)
            .Include(x => x.Attachments)
            .AsNoTracking()
            .SingleAsync(x => x.Id == ticketId);

        return MapTicket(ticket);
    }

    private async Task EnsureSeedArticlesAsync(int tenantId)
    {
        if (await _db.SupportArticles.AnyAsync(x => x.TenantId == tenantId))
            return;

        var articles = new[]
        {
            new SupportArticle
            {
                TenantId = tenantId,
                Category = "Account",
                Title = "Reset your password",
                Summary = "Recover access to your PeopleOS account quickly.",
                Content = "Use the Forgot password link on the login screen or contact your tenant administrator if your account is locked.",
                Tags = "password, login, account"
            },
            new SupportArticle
            {
                TenantId = tenantId,
                Category = "Attendance",
                Title = "Check attendance and clock-in issues",
                Summary = "Troubleshoot common attendance configuration and punch problems.",
                Content = "Verify the employee is assigned to the correct shift, the attendance configuration is active, and the employee is not outside the allowed time window.",
                Tags = "attendance, clock-in, shift"
            },
            new SupportArticle
            {
                TenantId = tenantId,
                Category = "Payroll",
                Title = "View payslips and payroll status",
                Summary = "Understand how payroll approval and payslip generation work.",
                Content = "Payroll can be reviewed from the payroll module after approval. If a payslip is missing, confirm the payroll run was approved and the employee is active.",
                Tags = "payroll, payslip, approval"
            },
            new SupportArticle
            {
                TenantId = tenantId,
                Category = "Support",
                Title = "Submit a request",
                Summary = "Create a support ticket with context and attached files.",
                Content = "Use the support form to describe the issue, include the affected module, and attach screenshots or files that help the team respond faster.",
                Tags = "support, ticket, request"
            }
        };

        _db.SupportArticles.AddRange(articles);
        await _db.SaveChangesAsync();
    }
}
