using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/masters/ledgermaster")]
[Authorize]
public class LedgerMasterController : ControllerBase
{
    private readonly ILedgerMasterService _service;
    private readonly ILogger<LedgerMasterController> _logger;

    public LedgerMasterController(
        ILedgerMasterService service,
        ILogger<LedgerMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ==================== Group 1: Core CRUD Operations ====================

    /// <summary>
    /// Get list of ledger groups (masters) user has permission to view
    /// Old: GET /api/ledgermaster/ledgermasterlist
    /// </summary>
    [HttpGet("master-list")]
    [ProducesResponseType(typeof(List<LedgerMasterListDto>), StatusCodes.Status200OK)]
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
    /// Old: GET /api/ledgermaster/grid/{masterID}
    /// </summary>
    [HttpGet("grid/{masterID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMasterGrid(string masterID)
    {
        _logger.LogInformation("Getting grid for masterID {MasterID}", masterID);

        var result = await _service.GetMasterGridAsync(masterID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get grid column hide configuration
    /// Old: GET /api/ledgermaster/grid-column-hide/{masterID}
    /// </summary>
    [HttpGet("grid-column-hide/{masterID}")]
    [ProducesResponseType(typeof(List<LedgerGridColumnHideDto>), StatusCodes.Status200OK)]
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
    /// Old: GET /api/ledgermaster/grid-column/{masterID}
    /// </summary>
    [HttpGet("grid-column/{masterID}")]
    [ProducesResponseType(typeof(List<LedgerGridColumnDto>), StatusCodes.Status200OK)]
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
    /// Get field metadata for specific ledger group
    /// Old: GET /api/ledgermaster/getmasterfields/{masterID}
    /// </summary>
    [HttpGet("master-fields/{masterID}")]
    [ProducesResponseType(typeof(List<LedgerMasterFieldDto>), StatusCodes.Status200OK)]
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
    /// Get loaded data for editing ledger
    /// Old: GET /api/ledgermaster/loaded-data/{masterID}/{ledgerId}
    /// </summary>
    [HttpGet("loaded-data/{masterID}/{ledgerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLoadedData(string masterID, string ledgerId)
    {
        _logger.LogInformation("Getting loaded data for masterID {MasterID}, ledgerId {LedgerId}", masterID, ledgerId);

        var result = await _service.GetLoadedDataAsync(masterID, ledgerId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get drill-down data for specific tab
    /// Old: GET /api/ledgermaster/drill-down/{masterID}/{tabID}/{ledgerID}
    /// </summary>
    [HttpGet("drill-down/{masterID}/{tabID}/{ledgerID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDrillDown(string masterID, string tabID, string ledgerID)
    {
        _logger.LogInformation("Getting drill-down data for masterID {MasterID}, tabID {TabID}, ledgerID {LedgerID}", masterID, tabID, ledgerID);

        var result = await _service.GetDrillDownAsync(masterID, tabID, ledgerID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save new ledger with dynamic fields
    /// Old: POST /api/ledgermaster/save
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveLedger([FromBody] SaveLedgerRequest request)
    {
        _logger.LogInformation("Saving ledger for LedgerGroupID {LedgerGroupID}", request.LedgerGroupID);

        var result = await _service.SaveLedgerAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update existing ledger
    /// Old: POST /api/ledgermaster/update
    /// </summary>
    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLedger([FromBody] UpdateLedgerRequest request)
    {
        _logger.LogInformation("Updating ledger {LedgerID}", request.LedgerID);

        var result = await _service.UpdateLedgerAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete ledger (soft delete)
    /// Old: POST /api/ledgermaster/deleteledger/{ledgerID}/{ledgergroupID}
    /// </summary>
    [HttpPost("delete/{ledgerID}/{ledgergroupID}")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteLedger(string ledgerID, string ledgergroupID)
    {
        _logger.LogInformation("Deleting ledger {LedgerID} from group {LedgerGroupID}", ledgerID, ledgergroupID);

        var result = await _service.DeleteLedgerAsync(ledgerID, ledgergroupID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Check if ledger can be deleted
    /// Old: GET /api/ledgermaster/check-permission/{ledgerID}
    /// </summary>
    [HttpGet("check-permission/{ledgerID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckPermission(string ledgerID)
    {
        _logger.LogInformation("Checking permission for ledger {LedgerID}", ledgerID);

        var result = await _service.CheckPermissionAsync(ledgerID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Load dynamic dropdown data for multiple fields
    /// Old: POST /api/ledgermaster/selectboxload
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

    // ==================== Group 2: Ledger-Specific Operations ====================

    /// <summary>
    /// Convert ledger to consignee type
    /// Old: POST /api/ledgermaster/convert-to-consignee/{ledgerID}
    /// </summary>
    [HttpPost("convert-to-consignee/{ledgerID}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConvertLedgerToConsignee(int ledgerID)
    {
        _logger.LogInformation("Converting ledger {LedgerID} to consignee", ledgerID);

        var result = await _service.ConvertLedgerToConsigneeAsync(ledgerID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get ledgers by group ID
    /// Old: GET /api/ledgermaster/ledgers-by-group/{groupID}
    /// </summary>
    [HttpGet("ledgers-by-group/{groupID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLedgersByGroup(string groupID)
    {
        _logger.LogInformation("Getting ledgers for group {GroupID}", groupID);

        var result = await _service.GetLedgersByGroupAsync(groupID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    // ==================== Group 3: Concern Person Management ====================

    /// <summary>
    /// Get all concern persons for current company
    /// Old: GET /api/ledgermaster/concern-persons
    /// </summary>
    [HttpGet("concern-persons")]
    [ProducesResponseType(typeof(List<ConcernPersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetConcernPersons()
    {
        _logger.LogInformation("Getting concern persons");

        var result = await _service.GetConcernPersonsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save concern person details for a ledger
    /// Old: POST /api/ledgermaster/concern-person/save
    /// </summary>
    [HttpPost("concern-person/save")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveConcernPerson([FromBody] SaveConcernPersonRequest request)
    {
        _logger.LogInformation("Saving concern person for ledger {LedgerID}", request.LedgerID);

        var result = await _service.SaveConcernPersonAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete all concern persons for a ledger
    /// Old: POST /api/ledgermaster/concern-person/delete-all/{ledgerID}
    /// </summary>
    [HttpPost("concern-person/delete-all/{ledgerID}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAllConcernPersons(string ledgerID)
    {
        _logger.LogInformation("Deleting all concern persons for ledger {LedgerID}", ledgerID);

        var result = await _service.DeleteAllConcernPersonsAsync(ledgerID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete specific concern person
    /// Old: POST /api/ledgermaster/concern-person/delete/{concernPersonID}/{ledgerID}
    /// </summary>
    [HttpPost("concern-person/delete/{concernPersonID}/{ledgerID}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteConcernPerson(string concernPersonID, string ledgerID)
    {
        _logger.LogInformation("Deleting concern person {ConcernPersonID} for ledger {LedgerID}", concernPersonID, ledgerID);

        var result = await _service.DeleteConcernPersonAsync(concernPersonID, ledgerID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    // ==================== Group 4: Employee Machine Allocation ====================

    /// <summary>
    /// Get all operators (from Operator ledger group)
    /// Old: GET /api/ledgermaster/operators
    /// </summary>
    [HttpGet("operators")]
    [ProducesResponseType(typeof(List<OperatorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOperators()
    {
        _logger.LogInformation("Getting operators");

        var result = await _service.GetOperatorsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get employees by group ID (from Employee ledger group)
    /// Old: GET /api/ledgermaster/employees/{groupID}
    /// </summary>
    [HttpGet("employees/{groupID}")]
    [ProducesResponseType(typeof(List<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmployees(string groupID)
    {
        _logger.LogInformation("Getting employees for group {GroupID}", groupID);

        var result = await _service.GetEmployeesAsync(groupID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save machine allocation for employee
    /// Old: POST /api/ledgermaster/machine-allocation/save
    /// </summary>
    [HttpPost("machine-allocation/save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveMachineAllocation([FromBody] SaveMachineAllocationRequest request)
    {
        _logger.LogInformation("Saving machine allocation for employee {EmployeeID}", request.EmployeeID);

        var result = await _service.SaveMachineAllocationAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get machine allocation details for employee
    /// Old: GET /api/ledgermaster/machine-allocation/{employeeID}
    /// </summary>
    [HttpGet("machine-allocation/{employeeID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMachineAllocation(string employeeID)
    {
        _logger.LogInformation("Getting machine allocation for employee {EmployeeID}", employeeID);

        var result = await _service.GetMachineAllocationAsync(employeeID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete machine allocation for employee
    /// Old: POST /api/ledgermaster/machine-allocation/delete/{ledgerID}
    /// </summary>
    [HttpPost("machine-allocation/delete/{ledgerID}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMachineAllocation(string ledgerID)
    {
        _logger.LogInformation("Deleting machine allocation for ledger {LedgerID}", ledgerID);

        var result = await _service.DeleteMachineAllocationAsync(ledgerID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    // ==================== Group 5: Supplier Group Allocation ====================

    /// <summary>
    /// Get all item groups for allocation
    /// Old: GET /api/ledgermaster/item-groups
    /// </summary>
    [HttpGet("item-groups")]
    [ProducesResponseType(typeof(List<LedgerItemGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetItemGroups()
    {
        _logger.LogInformation("Getting item groups");

        var result = await _service.GetItemGroupsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all spare part groups for allocation
    /// Old: GET /api/ledgermaster/spare-groups
    /// </summary>
    [HttpGet("spare-groups")]
    [ProducesResponseType(typeof(List<SpareGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSpareGroups()
    {
        _logger.LogInformation("Getting spare groups");

        var result = await _service.GetSpareGroupsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get group allocation details for supplier
    /// Old: GET /api/ledgermaster/group-allocation/{supplierID}
    /// </summary>
    [HttpGet("group-allocation/{supplierID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGroupAllocation(string supplierID)
    {
        _logger.LogInformation("Getting group allocation for supplier {SupplierID}", supplierID);

        var result = await _service.GetGroupAllocationAsync(supplierID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get spare part allocation details for supplier
    /// Old: GET /api/ledgermaster/spare-allocation/{supplierID}
    /// </summary>
    [HttpGet("spare-allocation/{supplierID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSpareAllocation(string supplierID)
    {
        _logger.LogInformation("Getting spare allocation for supplier {SupplierID}", supplierID);

        var result = await _service.GetSpareAllocationAsync(supplierID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save group allocation for supplier
    /// Old: POST /api/ledgermaster/group-allocation/save
    /// </summary>
    [HttpPost("group-allocation/save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveGroupAllocation([FromBody] SaveGroupAllocationRequest request)
    {
        _logger.LogInformation("Saving group allocation for supplier {SupplierID}", request.SupplierID);

        var result = await _service.SaveGroupAllocationAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete group allocation for supplier
    /// Old: POST /api/ledgermaster/group-allocation/delete/{ledgerID}
    /// </summary>
    [HttpPost("group-allocation/delete/{ledgerID}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteGroupAllocation(string ledgerID)
    {
        _logger.LogInformation("Deleting group allocation for ledger {LedgerID}", ledgerID);

        var result = await _service.DeleteGroupAllocationAsync(ledgerID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    // ==================== Group 6: Business Vertical Management ====================

    /// <summary>
    /// Get business vertical configuration settings
    /// Old: GET /api/ledgermaster/business-vertical/settings
    /// </summary>
    [HttpGet("business-vertical/settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBusinessVerticalSettings()
    {
        _logger.LogInformation("Getting business vertical settings");

        var result = await _service.GetBusinessVerticalSettingsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get business vertical details for ledger
    /// Old: GET /api/ledgermaster/business-vertical/{ledgerID}/{verticalID}
    /// </summary>
    [HttpGet("business-vertical/{ledgerID}/{verticalID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBusinessVerticalDetails(string ledgerID, string verticalID)
    {
        _logger.LogInformation("Getting business vertical details for ledger {LedgerID}, vertical {VerticalID}", ledgerID, verticalID);

        var result = await _service.GetBusinessVerticalDetailsAsync(ledgerID, verticalID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save new business vertical
    /// Old: POST /api/ledgermaster/business-vertical/save
    /// </summary>
    [HttpPost("business-vertical/save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveBusinessVertical([FromBody] SaveBusinessVerticalRequest request)
    {
        _logger.LogInformation("Saving business vertical");

        var result = await _service.SaveBusinessVerticalAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update existing business vertical
    /// Old: POST /api/ledgermaster/business-vertical/update
    /// </summary>
    [HttpPost("business-vertical/update")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBusinessVertical([FromBody] UpdateBusinessVerticalRequest request)
    {
        _logger.LogInformation("Updating business vertical {DetailID}", request.BusinessVerticalDetailID);

        var result = await _service.UpdateBusinessVerticalAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete business vertical
    /// Old: POST /api/ledgermaster/business-vertical/delete/{detailID}
    /// </summary>
    [HttpPost("business-vertical/delete/{detailID}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBusinessVertical(string detailID)
    {
        _logger.LogInformation("Deleting business vertical {DetailID}", detailID);

        var result = await _service.DeleteBusinessVerticalAsync(detailID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    // ==================== Group 7: Embargo Management ====================

    /// <summary>
    /// Place embargo on ledger(s)
    /// Old: POST /api/ledgermaster/embargo/place
    /// </summary>
    [HttpPost("embargo/place")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceEmbargo([FromBody] PlaceEmbargoRequest request)
    {
        _logger.LogInformation("Placing embargo");

        var result = await _service.PlaceEmbargoAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get embargo details for specific ledger
    /// Old: GET /api/ledgermaster/embargo/details/{ledgerID}
    /// </summary>
    [HttpGet("embargo/details/{ledgerID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmbargoDetails(string ledgerID)
    {
        _logger.LogInformation("Getting embargo details for ledger {LedgerID}", ledgerID);

        var result = await _service.GetEmbargoDetailsAsync(ledgerID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all active embargos for current company
    /// Old: GET /api/ledgermaster/embargo/active
    /// </summary>
    [HttpGet("embargo/active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActiveEmbargos()
    {
        _logger.LogInformation("Getting active embargos");

        var result = await _service.GetActiveEmbargosAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save embargo details (update embargo status/information)
    /// Old: POST /api/ledgermaster/embargo/save
    /// </summary>
    [HttpPost("embargo/save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveEmbargoDetails([FromBody] SaveEmbargoDetailsRequest request)
    {
        _logger.LogInformation("Saving embargo details");

        var result = await _service.SaveEmbargoDetailsAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    // ==================== Group 8: Utility Methods ====================

    /// <summary>
    /// Get session timeout value from settings
    /// Old: GET /api/ledgermaster/session-timeout
    /// </summary>
    [HttpGet("session-timeout")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSessionTimeout()
    {
        _logger.LogInformation("Getting session timeout");

        var result = await _service.GetSessionTimeoutAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get ledger group name ID for specific master
    /// Old: GET /api/ledgermaster/group-name-id/{masterID}
    /// </summary>
    [HttpGet("group-name-id/{masterID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLedgerGroupNameID(string masterID)
    {
        _logger.LogInformation("Getting ledger group name ID for masterID {MasterID}", masterID);

        var result = await _service.GetLedgerGroupNameIDAsync(masterID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get suppliers filtered by group name ID
    /// Old: GET /api/ledgermaster/suppliers/{groupNameID}
    /// </summary>
    [HttpGet("suppliers/{groupNameID}")]
    [ProducesResponseType(typeof(List<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSuppliers(string groupNameID)
    {
        _logger.LogInformation("Getting suppliers for group name ID {GroupNameID}", groupNameID);

        var result = await _service.GetSuppliersAsync(groupNameID);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}
