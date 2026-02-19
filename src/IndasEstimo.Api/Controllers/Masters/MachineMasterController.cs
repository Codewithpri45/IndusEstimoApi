using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/masters/machinemaster")]
[Authorize]
public class MachineMasterController : ControllerBase
{
    private readonly IMachineMasterService _service;
    private readonly ILogger<MachineMasterController> _logger;

    public MachineMasterController(
        IMachineMasterService service,
        ILogger<MachineMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all machines with full detail for the grid
    /// Old: MachineMaster()
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<MachineListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMachineList()
    {
        _logger.LogInformation("Getting machine list");

        var result = await _service.GetMachineListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get slab (rate tier) data for a specific machine
    /// Old: ExistSlab(Machineid)
    /// </summary>
    [HttpGet("slabs/{machineId}")]
    [ProducesResponseType(typeof(List<MachineSlabDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMachineSlabs(int machineId)
    {
        _logger.LogInformation("Getting slabs for machine {MachineID}", machineId);

        var result = await _service.GetMachineSlabsAsync(machineId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get online coating rates for a specific machine
    /// Old: GetMachineOnlineCoatingRates(MID)
    /// </summary>
    [HttpGet("coating-rates/{machineId}")]
    [ProducesResponseType(typeof(List<MachineOnlineCoatingRateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMachineOnlineCoatingRates(int machineId)
    {
        _logger.LogInformation("Getting coating rates for machine {MachineID}", machineId);

        var result = await _service.GetMachineOnlineCoatingRatesAsync(machineId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get department dropdown for machine form
    /// Old: GetSelectDepartment()
    /// </summary>
    [HttpGet("departments")]
    [ProducesResponseType(typeof(List<MachineDepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDepartments()
    {
        _logger.LogInformation("Getting departments for machine master");

        var result = await _service.GetDepartmentsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get machine type dropdown values
    /// Old: GetMachineType()
    /// </summary>
    [HttpGet("machine-types")]
    [ProducesResponseType(typeof(List<MachineTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMachineTypes()
    {
        _logger.LogInformation("Getting machine types");

        var result = await _service.GetMachineTypesAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get machine name list for dropdowns
    /// Old: MachineName()
    /// </summary>
    [HttpGet("machine-names")]
    [ProducesResponseType(typeof(List<MachineNameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMachineNames()
    {
        _logger.LogInformation("Getting machine names");

        var result = await _service.GetMachineNamesAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get item sub-groups for group allocation grid
    /// Old: GroupGrid()
    /// </summary>
    [HttpGet("group-grid")]
    [ProducesResponseType(typeof(List<MachineGroupAllocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGroupGrid()
    {
        _logger.LogInformation("Getting group grid");

        var result = await _service.GetGroupGridAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get existing group allocation IDs for a machine
    /// Old: ExistGroupID(MachineID)
    /// </summary>
    [HttpGet("group-allocation/{machineId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGroupAllocationIDs(int machineId)
    {
        _logger.LogInformation("Getting group allocation IDs for machine {MachineID}", machineId);

        var result = await _service.GetGroupAllocationIDsAsync(machineId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get distinct coating names for dropdown
    /// Old: GetCoatingName()
    /// </summary>
    [HttpGet("coating-names")]
    [ProducesResponseType(typeof(List<CoatingNameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCoatingNames()
    {
        _logger.LogInformation("Getting coating names");

        var result = await _service.GetCoatingNamesAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get tools for a specific tool group for tool allocation
    /// Old: GetToolList(ToolGroupID)
    /// </summary>
    [HttpGet("tools/{toolGroupId}")]
    [ProducesResponseType(typeof(List<MachineToolDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetToolList(int toolGroupId)
    {
        _logger.LogInformation("Getting tool list for tool group {ToolGroupID}", toolGroupId);

        var result = await _service.GetToolListAsync(toolGroupId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get existing allocated tool ID string for a machine and tool group
    /// Old: AllocatedToolsList(MachineID, ToolGroupID)
    /// </summary>
    [HttpGet("tool-allocation/{machineId}/{toolGroupId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllocatedTools(int machineId, int toolGroupId)
    {
        _logger.LogInformation("Getting allocated tools for machine {MachineID}, toolGroup {ToolGroupID}", machineId, toolGroupId);

        var result = await _service.GetAllocatedToolsAsync(machineId, toolGroupId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Generate next machine code with MM prefix
    /// Old: GetMachineCode()
    /// </summary>
    [HttpGet("machine-code")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMachineCode()
    {
        _logger.LogInformation("Getting machine code");

        var result = await _service.GetMachineCodeAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Check if machine name already exists
    /// Old: TxtChangeEventFun(VTxtMachine)
    /// </summary>
    [HttpGet("check-name")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckMachineName([FromQuery] string machineName)
    {
        _logger.LogInformation("Checking machine name {MachineName}", machineName);

        var result = await _service.CheckMachineNameExistsAsync(machineName);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save new machine with slabs and coating rates
    /// Old: SaveMachineMasterData(CostingMachineData, ObjMachineSlab, MachineName, CoatingRates)
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveMachine([FromBody] SaveMachineRequest request)
    {
        _logger.LogInformation("Saving machine {MachineName}", request.MachineDetail.MachineName);

        var result = await _service.SaveMachineAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update existing machine with slabs and coating rates
    /// Old: UpdatMachineMasterData(CostingMachineData, ObjMachineSlab, Machineid, CoatingRates)
    /// </summary>
    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMachine([FromBody] UpdateMachineRequest request)
    {
        _logger.LogInformation("Updating machine {MachineID}", request.MachineID);

        var result = await _service.UpdateMachineAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Soft-delete a machine and its slabs
    /// Old: DeleteMachineMasterData(Machineid)
    /// </summary>
    [HttpPost("delete/{machineId}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMachine(int machineId)
    {
        _logger.LogInformation("Deleting machine {MachineID}", machineId);

        var result = await _service.DeleteMachineAsync(machineId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save item sub-group allocations for a machine
    /// Old: SaveGroupAllocation(CostingDataGroupAllocation, MachineID, GridRow)
    /// </summary>
    [HttpPost("save-group-allocation")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveGroupAllocation([FromBody] SaveMachineGroupAllocationRequest request)
    {
        _logger.LogInformation("Saving group allocation for machine {MachineID}", request.MachineID);

        var result = await _service.SaveGroupAllocationAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Remove all group allocations for a machine (soft delete)
    /// Old: DeleteGroupAllo(MachineID)
    /// </summary>
    [HttpPost("delete-group-allocation/{machineId}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteGroupAllocation(int machineId)
    {
        _logger.LogInformation("Deleting group allocation for machine {MachineID}", machineId);

        var result = await _service.DeleteGroupAllocationAsync(machineId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save tool allocations for a machine and tool group
    /// Old: SaveToolAllocation(CostingDataToolAllocation, MachineID, ToolGroupID, GridRow)
    /// </summary>
    [HttpPost("save-tool-allocation")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveToolAllocation([FromBody] SaveMachineToolAllocationRequest request)
    {
        _logger.LogInformation("Saving tool allocation for machine {MachineID}, toolGroup {ToolGroupID}", request.MachineID, request.ToolGroupID);

        var result = await _service.SaveToolAllocationAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Remove tool allocations for a machine and tool group (soft delete)
    /// Old: DeleteToolAllo(MachineID, ToolGroupID)
    /// </summary>
    [HttpPost("delete-tool-allocation/{machineId}/{toolGroupId}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteToolAllocation(int machineId, int toolGroupId)
    {
        _logger.LogInformation("Deleting tool allocation for machine {MachineID}, toolGroup {ToolGroupID}", machineId, toolGroupId);

        var result = await _service.DeleteToolAllocationAsync(machineId, toolGroupId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}
