using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/masters/itemmaster")]
[Authorize]
public class ItemMasterController : ControllerBase
{
    private readonly IItemMasterService _service;
    private readonly ILogger<ItemMasterController> _logger;

    public ItemMasterController(
        IItemMasterService service,
        ILogger<ItemMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get list of item groups (masters) user has permission to view
    /// Old: GET /api/itemmaster/itemmasterlist
    /// </summary>
    [HttpGet("master-list")]
    [ProducesResponseType(typeof(List<MasterListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMasterList()
    {
        _logger.LogInformation("Getting master list");

        var result = await _service.GetMasterListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get grid data for specific master using dynamic query
    /// Old: GET /api/itemmaster/grid/{masterID}
    /// </summary>
    [HttpGet("grid/{masterID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGrid(string masterID)
    {
        _logger.LogInformation("Getting grid for masterID {MasterID}", masterID);

        var result = await _service.GetMasterGridAsync(masterID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get grid column hide configuration
    /// Old: GET /api/itemmaster/grid-column-hide/{masterID}
    /// </summary>
    [HttpGet("grid-column-hide/{masterID}")]
    [ProducesResponseType(typeof(List<GridColumnHideDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGridColumnHide(string masterID)
    {
        _logger.LogInformation("Getting grid column hide for masterID {MasterID}", masterID);

        var result = await _service.GetGridColumnHideAsync(masterID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get grid column names
    /// Old: GET /api/itemmaster/grid-column/{masterID}
    /// </summary>
    [HttpGet("grid-column/{masterID}")]
    [ProducesResponseType(typeof(List<GridColumnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGridColumn(string masterID)
    {
        _logger.LogInformation("Getting grid column for masterID {MasterID}", masterID);

        var result = await _service.GetGridColumnAsync(masterID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save new item with dynamic fields
    /// Old: POST /api/itemmaster/save
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveItem([FromBody] SaveItemRequest request)
    {
        _logger.LogInformation("Saving item for ItemGroupID {ItemGroupID}", request.ItemGroupID);

        var result = await _service.SaveItemAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        // Return same format as old API - just the message string
        return Ok(result.Data);
    }

    /// <summary>
    /// Update existing item
    /// Old: POST /api/itemmaster/update
    /// </summary>
    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateItem([FromBody] UpdateItemRequest request)
    {
        _logger.LogInformation("Updating item {ItemID}", request.ItemID);

        var result = await _service.UpdateItemAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete item (soft delete)
    /// Old: POST /api/itemmaster/deleteitem/{itemID}/{itemgroupID}
    /// </summary>
    [HttpPost("delete/{itemID}/{itemgroupID}")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteItem(string itemID, string itemgroupID)
    {
        _logger.LogInformation("Deleting item {ItemID} from group {ItemGroupID}", itemID, itemgroupID);

        var result = await _service.DeleteItemAsync(itemID, itemgroupID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Check if item can be deleted
    /// Old: GET /api/itemmaster/check-permission/{transactionID}
    /// </summary>
    [HttpGet("check-permission/{transactionID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckPermission(string transactionID)
    {
        _logger.LogInformation("Checking permission for transaction {TransactionID}", transactionID);

        var result = await _service.CheckPermissionAsync(transactionID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get loaded data for editing item
    /// Old: GET /api/itemmaster/loaded-data/{masterID}/{itemId}
    /// </summary>
    [HttpGet("loaded-data/{masterID}/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLoadedData(string masterID, string itemId)
    {
        _logger.LogInformation("Getting loaded data for masterID {MasterID}, itemId {ItemId}", masterID, itemId);

        var result = await _service.GetLoadedDataAsync(masterID, itemId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get drill-down data for specific tab
    /// Old: GET /api/itemmaster/drill-down/{masterID}/{tabID}
    /// </summary>
    [HttpGet("drill-down/{masterID}/{tabID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDrillDown(string masterID, string tabID)
    {
        _logger.LogInformation("Getting drill-down data for masterID {MasterID}, tabID {TabID}", masterID, tabID);

        var result = await _service.GetDrillDownDataAsync(masterID, tabID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get field metadata for specific item group
    /// Old: GET /api/itemmaster/getmasterfields/{masterID}
    /// </summary>
    [HttpGet("master-fields/{masterID}")]
    [ProducesResponseType(typeof(List<MasterFieldDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMasterFields(string masterID)
    {
        _logger.LogInformation("Getting master fields for masterID {MasterID}", masterID);

        var result = await _service.GetMasterFieldsAsync(masterID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Load dynamic dropdown data for multiple fields
    /// Old: POST /api/itemmaster/selectboxload
    /// </summary>
    [HttpPost("selectbox-load")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SelectBoxLoad([FromBody] object jsonData)
    {
        _logger.LogInformation("Loading selectbox data");

        // Convert System.Text.Json object to Newtonsoft.Json JArray
        // Serialize using System.Text.Json, then deserialize using Newtonsoft.Json
        var jsonString = System.Text.Json.JsonSerializer.Serialize(jsonData);
        var jArray = JsonConvert.DeserializeObject<JArray>(jsonString);

        if (jArray == null)
            return BadRequest(new { message = "Invalid JSON array" });

        var result = await _service.SelectBoxLoadAsync(jArray);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get list of item sub-groups
    /// Old: GET /api/itemmaster/under-group
    /// </summary>
    [HttpGet("under-group")]
    [ProducesResponseType(typeof(List<UnderGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUnderGroup()
    {
        _logger.LogInformation("Getting under group");

        var result = await _service.GetUnderGroupAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get list of item groups with hierarchy
    /// Old: GET /api/itemmaster/group
    /// </summary>
    [HttpGet("group")]
    [ProducesResponseType(typeof(List<GroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGroup()
    {
        _logger.LogInformation("Getting group");

        var result = await _service.GetGroupAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage});

        return Ok(result.Data);
    }

    /// <summary>
    /// Save new item sub-group
    /// Old: POST /api/itemmaster/save-group
    /// </summary>
    [HttpPost("save-group")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveGroup([FromBody] SaveGroupRequest request)
    {
        _logger.LogInformation("Saving group {GroupName}", request.GroupName);

        var result = await _service.SaveGroupAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update existing item sub-group
    /// Old: POST /api/itemmaster/update-group
    /// </summary>
    [HttpPost("update-group")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupRequest request)
    {
        _logger.LogInformation("Updating group {ItemSubGroupUniqueID}", request.ItemSubGroupUniqueID);

        var result = await _service.UpdateGroupAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete item sub-group
    /// Old: POST /api/itemmaster/delete-group
    /// </summary>
    [HttpPost("delete-group")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteGroup([FromBody] DeleteGroupRequest request)
    {
        _logger.LogInformation("Deleting group {ItemSubGroupUniqueID}", request.ItemSubGroupUniqueID);

        var result = await _service.DeleteGroupAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all item groups
    /// Old: GET /api/itemmaster/items
    /// </summary>
    [HttpGet("items")]
    [ProducesResponseType(typeof(List<ItemGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetItems()
    {
        _logger.LogInformation("Getting items");

        var result = await _service.GetItemsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all ledger groups
    /// Old: GET /api/itemmaster/ledgers
    /// </summary>
    [HttpGet("ledgers")]
    [ProducesResponseType(typeof(List<LedgerGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLedgers()
    {
        _logger.LogInformation("Getting ledgers");

        var result = await _service.GetLedgersAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Check if item can be updated
    /// Old: GET /api/itemmaster/check-permission-update/{itemID}
    /// </summary>
    [HttpGet("check-permission-update/{itemID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckPermissionForUpdate(string itemID)
    {
        _logger.LogInformation("Checking permission for update for itemID {ItemID}", itemID);

        var result = await _service.CheckPermissionForUpdateAsync(itemID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update user-specific item data
    /// Old: POST /api/itemmaster/update-user
    /// </summary>
    [HttpPost("update-user")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUserItem([FromBody] UpdateUserItemRequest request)
    {
        _logger.LogInformation("Updating user item {ItemID}", request.ItemID);

        var result = await _service.UpdateUserItemAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}
