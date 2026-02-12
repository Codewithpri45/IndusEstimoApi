
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/processmaster")]
[Authorize]
public class ProcessMasterController : ControllerBase
{
    private readonly IProcessMasterService _service;
    private readonly ILogger<ProcessMasterController> _logger;

    public ProcessMasterController(IProcessMasterService service, ILogger<ProcessMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetProcesses()
    {
        var result = await _service.GetProcessesAsync();
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProcess([FromBody] CreateProcessDto request)
    {
        var result = await _service.CreateProcessAsync(request);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProcess([FromBody] UpdateProcessDto request)
    {
        var result = await _service.UpdateProcessAsync(request);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProcess(long id)
    {
        var result = await _service.DeleteProcessAsync(id);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProcess(long id)
    {
        var result = await _service.GetProcessByIdAsync(id);
        return HandleResult(result);
    }

    private IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(new { message = result.ErrorMessage });
    }
}
