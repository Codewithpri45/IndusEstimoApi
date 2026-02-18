using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/masters/process")]
[Authorize]
public class ProcessMasterController : ControllerBase
{
    private readonly IProcessMasterService _service;
    private readonly ILogger<ProcessMasterController> _logger;

    public ProcessMasterController(
        IProcessMasterService service,
        ILogger<ProcessMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get list of all processes for main grid
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<ProcessListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProcessList()
    {
        _logger.LogInformation("Getting process list");

        var result = await _service.GetProcessListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get process names for dropdown
    /// </summary>
    [HttpGet("names")]
    [ProducesResponseType(typeof(List<ProcessNameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProcessNames()
    {
        _logger.LogInformation("Getting process names");

        var result = await _service.GetProcessNamesAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get full process detail by ID (for editing)
    /// </summary>
    [HttpGet("{processId:int}")]
    [ProducesResponseType(typeof(ProcessLoadedDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProcessById(int processId)
    {
        _logger.LogInformation("Getting process {ProcessID}", processId);

        var result = await _service.GetProcessByIdAsync(processId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get departments for dropdown
    /// </summary>
    [HttpGet("departments")]
    [ProducesResponseType(typeof(List<ProcessDepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDepartments()
    {
        _logger.LogInformation("Getting departments for process");

        var result = await _service.GetDepartmentsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get type of charges for dropdown
    /// </summary>
    [HttpGet("type-of-charges")]
    [ProducesResponseType(typeof(List<TypeOfChargesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTypeOfCharges()
    {
        _logger.LogInformation("Getting type of charges");

        var result = await _service.GetTypeOfChargesAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get units for start/end unit dropdowns
    /// </summary>
    [HttpGet("units")]
    [ProducesResponseType(typeof(List<UnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUnits()
    {
        _logger.LogInformation("Getting units");

        var result = await _service.GetUnitsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get tool group list for allocation grid
    /// </summary>
    [HttpGet("tool-groups")]
    [ProducesResponseType(typeof(List<ProcessToolGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetToolGroupList()
    {
        _logger.LogInformation("Getting tool group list");

        var result = await _service.GetToolGroupListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all machines for machine allocation grid
    /// </summary>
    [HttpGet("machines")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMachineGrid()
    {
        _logger.LogInformation("Getting machine grid");

        var result = await _service.GetMachineGridAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all items for material allocation grid
    /// </summary>
    [HttpGet("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetItemGrid()
    {
        _logger.LogInformation("Getting item grid");

        var result = await _service.GetItemGridAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all contents for content allocation grid
    /// </summary>
    [HttpGet("contents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetContentGrid()
    {
        _logger.LogInformation("Getting content grid");

        var result = await _service.GetContentGridAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get existing slabs for a process
    /// </summary>
    [HttpGet("slabs/{processId:int}")]
    [ProducesResponseType(typeof(List<ProcessSlabDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExistingSlabs(int processId)
    {
        _logger.LogInformation("Getting slabs for process {ProcessID}", processId);

        var result = await _service.GetExistingSlabsAsync(processId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get allocated machines for a process
    /// </summary>
    [HttpGet("allocated-machines/{processId:int}")]
    [ProducesResponseType(typeof(List<ProcessMachineAllocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllocatedMachines(int processId)
    {
        _logger.LogInformation("Getting allocated machines for process {ProcessID}", processId);

        var result = await _service.GetAllocatedMachinesAsync(processId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get allocated materials for a process
    /// </summary>
    [HttpGet("allocated-materials/{processId:int}")]
    [ProducesResponseType(typeof(List<ProcessMaterialAllocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllocatedMaterials(int processId)
    {
        _logger.LogInformation("Getting allocated materials for process {ProcessID}", processId);

        var result = await _service.GetAllocatedMaterialsAsync(processId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get inspection parameters for a process
    /// </summary>
    [HttpGet("inspection-parameters/{processId:int}")]
    [ProducesResponseType(typeof(List<ProcessInspectionParameterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInspectionParameters(int processId)
    {
        _logger.LogInformation("Getting inspection parameters for process {ProcessID}", processId);

        var result = await _service.GetInspectionParametersAsync(processId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get line clearance parameters for a process
    /// </summary>
    [HttpGet("line-clearance-parameters/{processId:int}")]
    [ProducesResponseType(typeof(List<ProcessLineClearanceParameterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLineClearanceParameters(int processId)
    {
        _logger.LogInformation("Getting line clearance parameters for process {ProcessID}", processId);

        var result = await _service.GetLineClearanceParametersAsync(processId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save new process with all allocations
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SaveProcess([FromBody] SaveProcessRequest request)
    {
        _logger.LogInformation("Saving process {ProcessName}", request.ProcessDetail?.ProcessName);

        var result = await _service.SaveProcessAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        if (result.Data == "Exist")
            return Conflict(new { message = "This Process Name already exists. Please enter another Process Name." });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update existing process with all allocations
    /// </summary>
    [HttpPut("update")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProcess([FromBody] UpdateProcessRequest request)
    {
        _logger.LogInformation("Updating process {ProcessID}", request.ProcessID);

        var result = await _service.UpdateProcessAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete process (soft delete)
    /// </summary>
    [HttpDelete("{processId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProcess(int processId)
    {
        _logger.LogInformation("Deleting process {ProcessID}", processId);

        var result = await _service.DeleteProcessAsync(processId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}
