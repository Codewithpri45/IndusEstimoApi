using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/purchase-grn")]
[Authorize]
public class PurchaseGRNController : ControllerBase
{
    private readonly IPurchaseGRNService _service;
    private readonly ILogger<PurchaseGRNController> _logger;

    public PurchaseGRNController(
        IPurchaseGRNService service,
        ILogger<PurchaseGRNController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get purchase suppliers list - suppliers from approved purchase orders
    /// WHERE: VoucherID=-11, LedgerGroupNameID=23
    /// </summary>
    [HttpGet("suppliers")]
    [ProducesResponseType(typeof(List<PurchaseSupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPurchaseSuppliersList()
    {
        _logger.LogInformation("Getting purchase suppliers list");

        var result = await _service.GetPurchaseSuppliersListAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get pending purchase orders list - approved POs that haven't been fully received
    /// WHERE: VoucherID=-11, IsVoucherItemApproved=1, IsCompleted=0, IsDeleted=0
    /// </summary>
    [HttpGet("pending-orders")]
    [ProducesResponseType(typeof(List<PendingPurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPendingOrdersList()
    {
        _logger.LogInformation("Getting pending purchase orders list");

        var result = await _service.GetPendingOrdersListAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get receipt note list (GRN list) by date range
    /// WHERE: VoucherID=-14
    /// </summary>
    [HttpPost("receipt-notes")]
    [ProducesResponseType(typeof(List<ReceiptNoteListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReceiptNoteList([FromBody] GetReceiptNoteListRequest request)
    {
        _logger.LogInformation("Getting receipt note list from {FromDate} to {ToDate}",
            request.FromDate, request.ToDate);

        var result = await _service.GetReceiptNoteListAsync(request.FromDate, request.ToDate);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get receipt voucher batch detail for a specific GRN transaction
    /// Returns item-wise batch details for a GRN
    /// </summary>
    [HttpGet("receipt-batch-detail/{transactionId}")]
    [ProducesResponseType(typeof(List<ReceiptVoucherBatchDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReceiptVoucherBatchDetail(long transactionId)
    {
        _logger.LogInformation("Getting receipt voucher batch detail for transaction {TransactionId}", transactionId);

        var result = await _service.GetReceiptVoucherBatchDetailAsync(transactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get previous received quantity for a PO item
    /// Used to validate how much has already been received against a PO
    /// </summary>
    [HttpPost("previous-received-quantity")]
    [ProducesResponseType(typeof(PreviousReceivedQuantityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPreviousReceivedQuantity([FromBody] GetPreviousReceivedQuantityRequest request)
    {
        _logger.LogInformation(
            "Getting previous received quantity for PO {PurchaseTransactionId}, Item {ItemId}",
            request.PurchaseTransactionID, request.ItemID);

        var result = await _service.GetPreviousReceivedQuantityAsync(
            request.PurchaseTransactionID, request.ItemID, request.GRNTransactionID);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get receivers list - employees from inventory department who can receive materials
    /// WHERE: LedgerGroupNameID=27, DepartmentName LIKE '%Inventory%'
    /// </summary>
    [HttpGet("receivers")]
    [ProducesResponseType(typeof(List<ReceiverDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReceiverList()
    {
        _logger.LogInformation("Getting receiver list");

        var result = await _service.GetReceiverListAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get warehouse list for the current production unit
    /// </summary>
    [HttpGet("warehouses")]
    [ProducesResponseType(typeof(List<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarehouseList()
    {
        _logger.LogInformation("Getting warehouse list");

        var result = await _service.GetWarehouseListAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get bins list for a specific warehouse
    /// </summary>
    [HttpGet("bins")]
    [ProducesResponseType(typeof(List<BinDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBinsList([FromQuery] string warehouseName = "")
    {
        _logger.LogInformation("Getting bins list for warehouse {WarehouseName}", warehouseName);

        var result = await _service.GetBinsListAsync(warehouseName);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get gate pass entries for a specific supplier
    /// Used for linking gate entry with GRN
    /// </summary>
    [HttpGet("gate-pass/{ledgerId}")]
    [ProducesResponseType(typeof(List<GatePassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetGatePass(long ledgerId)
    {
        _logger.LogInformation("Getting gate pass for ledger {LedgerId}", ledgerId);

        var result = await _service.GetGatePassAsync(ledgerId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get GRN item list for a specific transaction
    /// Returns items in a GRN with batch details
    /// </summary>
    [HttpGet("items/{transactionId}")]
    [ProducesResponseType(typeof(List<GRNItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetGrnItemList(long transactionId)
    {
        _logger.LogInformation("Getting GRN item list for transaction {TransactionId}", transactionId);

        var result = await _service.GetGrnItemListAsync(transactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get user authority - check if user can receive excess material
    /// </summary>
    [HttpGet("user-authority")]
    [ProducesResponseType(typeof(UserAuthorityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserAuthority()
    {
        _logger.LogInformation("Getting user authority");

        var result = await _service.GetUserAuthorityAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Validate supplier batch receipt data
    /// Checks if supplier batch numbers are already used
    /// </summary>
    [HttpPost("validate-supplier-batch")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateSupplierBatchReceiptData([FromBody] ValidateSupplierBatchRequest request)
    {
        _logger.LogInformation("Validating supplier batch data");

        var result = await _service.ValidateSupplierBatchReceiptDataAsync(request.VoucherID, request.Items);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        if (result.Data != "Success")
        {
            return BadRequest(new { message = result.Data });
        }

        return Ok(new { success = true, message = result.Data });
    }

    /// <summary>
    /// Check if GRN can be edited/deleted
    /// Returns "Exist" if GRN is already processed or approved, empty string if can be modified
    /// </summary>
    [HttpGet("check-permission/{transactionId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckPermission(long transactionId)
    {
        _logger.LogInformation("Checking permission for transaction {TransactionId}", transactionId);

        var result = await _service.CheckPermissionAsync(transactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { canModify = string.IsNullOrEmpty(result.Data), message = result.Data });
    }

    /// <summary>
    /// Get last transaction date for GRN
    /// Used for date validation
    /// </summary>
    [HttpGet("last-transaction-date")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLastTransactionDate()
    {
        _logger.LogInformation("Getting last transaction date");

        var result = await _service.GetLastTransactionDateAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { lastDate = result.Data });
    }

    /// <summary>
    /// Get next voucher number preview for GRN (read-only, does not consume the number)
    /// </summary>
    [HttpGet("next-voucher-no")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNextVoucherNo([FromQuery] string prefix = "GRN")
    {
        var result = await _service.GetNextVoucherNoAsync(prefix);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { voucherNo = result.Data });
    }

    /// <summary>
    /// Save new GRN (Goods Receipt Note)
    /// Creates GRN, updates stock, marks PO items as completed if fully received
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager,PurchaseUser,InventoryUser")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveReceiptData([FromBody] SaveReceiptDataRequest request)
    {
        _logger.LogInformation("Saving GRN for supplier {LedgerID}", request.MainData.LedgerID);

        var result = await _service.SaveReceiptDataAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new
        {
            success = true,
            message = "GRN saved successfully",
            voucherNo = result.Data.VoucherNo,
            transactionId = result.Data.TransactionID
        });
    }

    /// <summary>
    /// Update existing GRN
    /// Updates GRN details and recalculates stock
    /// </summary>
    [HttpPut("update")]
    [Authorize(Roles = "Admin,Manager,PurchaseUser,InventoryUser")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateReceiptData([FromBody] UpdateReceiptDataRequest request)
    {
        _logger.LogInformation("Updating GRN {TransactionID}", request.TransactionID);

        var result = await _service.UpdateReceiptDataAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "GRN updated successfully" });
    }

    /// <summary>
    /// Delete GRN (soft delete)
    /// Marks GRN as deleted, reverses stock, unmarks PO items as completed
    /// </summary>
    [HttpPost("delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteGRN([FromBody] DeleteGRNRequest request)
    {
        _logger.LogInformation("Deleting GRN {TransactionID}", request.TransactionID);

        var result = await _service.DeleteGRNAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "GRN deleted successfully" });
    }
}
