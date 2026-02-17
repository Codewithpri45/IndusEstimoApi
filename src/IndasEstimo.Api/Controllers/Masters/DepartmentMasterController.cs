using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/masters/department")]
[Authorize]
public class DepartmentMasterController : ControllerBase
{
    private readonly IDepartmentMasterService _departmentService;
    private readonly ILogger<DepartmentMasterController> _logger;

    public DepartmentMasterController(
        IDepartmentMasterService departmentService,
        ILogger<DepartmentMasterController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of all departments
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetDepartmentList()
    {
        _logger.LogInformation("Getting department list");

        var result = await _departmentService.GetDepartmentListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save new department
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SaveDepartment([FromBody] SaveDepartmentRequest request)
    {
        _logger.LogInformation("Saving new department {DepartmentName}", request.DepartmentName);

        var result = await _departmentService.SaveDepartmentAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        if (result.Data == "Exist")
            return Conflict(new { message = "Department with this name and press already exists" });

        if (result.Data == "Duplicate data found..")
            return Conflict(new { message = "This Sequence No. is already allocated to another department" });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update existing department
    /// </summary>
    [HttpPut("update")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateDepartment([FromBody] UpdateDepartmentRequest request)
    {
        _logger.LogInformation("Updating department {DepartmentID}", request.DepartmentID);

        var result = await _departmentService.UpdateDepartmentAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        if (result.Data == "Exist")
            return Conflict(new { message = "Department with this name and press already exists" });

        if (result.Data == "Duplicate data found..")
            return Conflict(new { message = "This Sequence No. is already allocated to another department" });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete department (soft delete)
    /// </summary>
    [HttpDelete("{departmentId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteDepartment(int departmentId)
    {
        _logger.LogInformation("Deleting department {DepartmentId}", departmentId);

        var result = await _departmentService.DeleteDepartmentAsync(departmentId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}
