
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/machinemaster")]
[Authorize]
public class MachineMasterController : ControllerBase
{
    private readonly IMachineMasterService _service;
    private readonly ILogger<MachineMasterController> _logger;

    public MachineMasterController(IMachineMasterService service, ILogger<MachineMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("code")]
    public async Task<IActionResult> GetMachineCode()
    {
        var result = await _service.GenerateMachineCodeAsync();
        return HandleResult(result);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetMachines()
    {
        var result = await _service.GetMachinesAsync();
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMachine([FromBody] CreateMachineDto request)
    {
        var result = await _service.CreateMachineAsync(request);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMachine([FromBody] UpdateMachineDto request)
    {
        var result = await _service.UpdateMachineAsync(request);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMachine(long id)
    {
        var result = await _service.DeleteMachineAsync(id);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMachine(long id)
    {
        var result = await _service.GetMachineByIdAsync(id);
        return HandleResult(result);
    }

    private IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(new { message = result.ErrorMessage });
    }
}
