using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/platform/tenants/{tenantId:int}/billing")]
[Authorize(Policy = "Platform.Tenants")]
public class BillingController : ControllerBase
{
    private readonly BillingService _billingService;

    public BillingController(BillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet]
    public async Task<ActionResult<BillingHistoryDto>> GetHistory(int tenantId)
    {
        try
        {
            return Ok(await _billingService.GetHistoryAsync(tenantId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("invoices")]
    public async Task<ActionResult<BillingInvoiceDto>> CreateInvoice(int tenantId, CreateBillingInvoiceDto dto)
    {
        try
        {
            var invoice = await _billingService.CreateInvoiceAsync(tenantId, dto);
            return Ok(invoice);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("invoices/{invoiceId:int}/status")]
    public async Task<ActionResult<BillingInvoiceDto>> UpdateInvoiceStatus(int tenantId, int invoiceId, UpdateBillingInvoiceStatusDto dto)
    {
        try
        {
            var invoice = await _billingService.UpdateInvoiceStatusAsync(tenantId, invoiceId, dto);
            return Ok(invoice);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("invoices/{invoiceId:int}/payments")]
    public async Task<ActionResult<BillingInvoiceDto>> RecordPayment(int tenantId, int invoiceId, RecordPaymentDto dto)
    {
        try
        {
            var invoice = await _billingService.RecordPaymentAsync(tenantId, invoiceId, dto);
            return Ok(invoice);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
