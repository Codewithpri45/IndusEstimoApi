using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services.Inventory;
namespace IndasEstimo.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/purchase-orders")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ILogger<PurchaseOrdersController> _logger;
    public PurchaseOrdersController(
        IPurchaseOrderService purchaseOrderService,
        ILogger<PurchaseOrdersController> logger)
    {
        _purchaseOrderService = purchaseOrderService;
        _logger = logger;
    }

    [HttpPost("savePO")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(SavePurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SavePurchaseOrder([FromBody] SavePurchaseOrderRequest request)
    {
        var result = await _purchaseOrderService.SavePurchaseOrderAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("updatePO")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(SavePurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePurchaseOrder([FromBody] UpdatePurchaseOrderRequest request)
    {
        var result = await _purchaseOrderService.UpdatePurchaseOrderAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("deletePO")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeletePurchaseOrder([FromBody] long transactionId)
    {
        var result = await _purchaseOrderService.DeletePurchaseOrderAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ==================== Retrieve Operations ====================

    [HttpGet("retrive-data/{transactionId}")]
    [ProducesResponseType(typeof(List<PurchaseOrderDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPurchaseOrder(long transactionId)
    {
        var result = await _purchaseOrderService.GetPurchaseOrderAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("fill-grid")]
    [ProducesResponseType(typeof(List<PurchaseOrderListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPurchaseOrderList([FromBody] GetPurchaseOrderListRequest request)
    {
        var result = await _purchaseOrderService.GetPurchaseOrderListAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("pending-requisitions")]
    [ProducesResponseType(typeof(List<PendingRequisitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPendingRequisitions()
    {
        var result = await _purchaseOrderService.GetPendingRequisitionsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ==================== Helper/Lookup Operations ====================

    [HttpGet("voucher-no")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVoucherNo([FromQuery] string prefix = "PO")
    {
        var result = await _purchaseOrderService.GetNextVoucherNoAsync(prefix);
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("last-transaction-date")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLastTransactionDate()
    {
        var result = await _purchaseOrderService.GetLastTransactionDateAsync();
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("suppliers")]
    [ProducesResponseType(typeof(List<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSuppliers()
    {
        var result = await _purchaseOrderService.GetSuppliersAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("contact-persons/{ledgerId}")]
    [ProducesResponseType(typeof(List<ContactPersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetContactPersons(long ledgerId)
    {
        var result = await _purchaseOrderService.GetContactPersonsAsync(ledgerId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("delivery-addresses")]
    [ProducesResponseType(typeof(List<DeliveryAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDeliveryAddresses()
    {
        var result = await _purchaseOrderService.GetDeliveryAddressesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("overhead-charge-heads")]
    [ProducesResponseType(typeof(List<OverheadChargeHeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOverheadChargeHeads()
    {
        var result = await _purchaseOrderService.GetOverheadChargeHeadsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("tax-charge-ledgers")]
    [ProducesResponseType(typeof(List<TaxChargeLedgerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTaxChargeLedgers()
    {
        var result = await _purchaseOrderService.GetTaxChargeLedgersAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("currencies")]
    [ProducesResponseType(typeof(List<CurrencyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrencies()
    {
        var result = await _purchaseOrderService.GetCurrenciesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("hsn-codes")]
    [ProducesResponseType(typeof(List<HSNCodeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHSNCodes()
    {
        var result = await _purchaseOrderService.GetHSNCodesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("attachments/{transactionId}")]
    [ProducesResponseType(typeof(List<AttachmentFileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAttachments(long transactionId)
    {
        var result = await _purchaseOrderService.GetAttachmentsAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }
}