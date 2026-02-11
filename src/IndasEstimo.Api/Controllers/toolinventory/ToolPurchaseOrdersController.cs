using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Application.Interfaces.Services.ToolInventory;
namespace IndasEstimo.Api.Controllers.ToolInventory;

[ApiController]
[Route("api/tool-inventory/tool-purchase-orders")]
[Authorize]
public class ToolPurchaseOrdersController : ControllerBase
{
    private readonly IToolPurchaseOrderService _toolPurchaseOrderService;
    private readonly ILogger<ToolPurchaseOrdersController> _logger;
    public ToolPurchaseOrdersController(
        IToolPurchaseOrderService toolPurchaseOrderService,
        ILogger<ToolPurchaseOrdersController> logger)
    {
        _toolPurchaseOrderService = toolPurchaseOrderService;
        _logger = logger;
    }

    // ==================== CRUD Operations ====================

    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(SaveToolPurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveToolPurchaseOrder([FromBody] SaveToolPurchaseOrderRequest request)
    {
        var result = await _toolPurchaseOrderService.SaveToolPurchaseOrderAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(SaveToolPurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateToolPurchaseOrder([FromBody] UpdateToolPurchaseOrderRequest request)
    {
        var result = await _toolPurchaseOrderService.UpdateToolPurchaseOrderAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteToolPurchaseOrder([FromBody] long transactionId)
    {
        var result = await _toolPurchaseOrderService.DeleteToolPurchaseOrderAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ==================== Retrieve Operations ====================

    [HttpGet("retrive-data/{transactionId}")]
    [ProducesResponseType(typeof(List<ToolPurchaseOrderDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToolPurchaseOrderData(long transactionId)
    {
        var result = await _toolPurchaseOrderService.GetToolPurchaseOrderAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("retrive-overhead/{transactionId}")]
    [ProducesResponseType(typeof(List<ToolPOOverheadDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToolPurchaseOrderOverhead(long transactionId)
    {
        var result = await _toolPurchaseOrderService.GetToolPurchaseOrderOverheadAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("retrive-tax/{transactionId}")]
    [ProducesResponseType(typeof(List<ToolPOTaxDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToolPurchaseOrderTax(long transactionId)
    {
        var result = await _toolPurchaseOrderService.GetToolPurchaseOrderTaxAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("fill-grid")]
    [ProducesResponseType(typeof(List<ToolPurchaseOrderListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToolPurchaseOrderList([FromBody] GetToolPurchaseOrderListRequest request)
    {
        var result = await _toolPurchaseOrderService.GetToolPurchaseOrderListAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("pending-requisitions")]
    [ProducesResponseType(typeof(List<ToolPendingRequisitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPendingRequisitions()
    {
        var result = await _toolPurchaseOrderService.GetPendingRequisitionsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ==================== Helper/Lookup Operations ====================

    [HttpGet("voucher-no")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVoucherNo([FromQuery] string prefix = "TPO")
    {
        var result = await _toolPurchaseOrderService.GetNextVoucherNoAsync(prefix);
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("last-transaction-date")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLastTransactionDate()
    {
        var result = await _toolPurchaseOrderService.GetLastTransactionDateAsync();
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("suppliers")]
    [ProducesResponseType(typeof(List<ToolSupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSuppliers()
    {
        var result = await _toolPurchaseOrderService.GetSuppliersAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("contact-persons/{ledgerId}")]
    [ProducesResponseType(typeof(List<ToolContactPersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetContactPersons(long ledgerId)
    {
        var result = await _toolPurchaseOrderService.GetContactPersonsAsync(ledgerId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("item-rate")]
    [ProducesResponseType(typeof(ToolItemRateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetItemRate([FromQuery] long ledgerId, [FromQuery] long toolId)
    {
        var result = await _toolPurchaseOrderService.GetItemRateAsync(ledgerId, toolId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("allotted-suppliers/{toolGroupId}")]
    [ProducesResponseType(typeof(List<ToolAllottedSupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllottedSuppliers(long toolGroupId)
    {
        var result = await _toolPurchaseOrderService.GetAllottedSuppliersAsync(toolGroupId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("overflow-grid")]
    [ProducesResponseType(typeof(List<ToolOverflowGridDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOverflowGrid([FromQuery] long toolId, [FromQuery] long toolGroupId)
    {
        var result = await _toolPurchaseOrderService.GetOverflowGridAsync(toolId, toolGroupId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("overhead-charge-heads")]
    [ProducesResponseType(typeof(List<ToolOverheadChargeHeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOverheadChargeHeads()
    {
        var result = await _toolPurchaseOrderService.GetOverheadChargeHeadsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("tax-charge-ledgers")]
    [ProducesResponseType(typeof(List<ToolTaxChargeLedgerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTaxChargeLedgers()
    {
        var result = await _toolPurchaseOrderService.GetTaxChargeLedgersAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("hsn-codes")]
    [ProducesResponseType(typeof(List<ToolHSNCodeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHSNCodes()
    {
        var result = await _toolPurchaseOrderService.GetHSNCodesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("currencies")]
    [ProducesResponseType(typeof(List<ToolCurrencyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrencies()
    {
        var result = await _toolPurchaseOrderService.GetCurrenciesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("po-approval-by")]
    [ProducesResponseType(typeof(List<ToolPOApprovalByDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPOApprovalBy()
    {
        var result = await _toolPurchaseOrderService.GetPOApprovalByAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("check-permission/{transactionId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckPermission(long transactionId)
    {
        var result = await _toolPurchaseOrderService.CheckPermissionAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }
}
