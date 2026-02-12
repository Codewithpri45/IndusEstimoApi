
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/producthsnmaster")]
[Authorize]
public class ProductHSNMasterController : ControllerBase
{
    private readonly IProductHSNMasterService _service;
    private readonly ILogger<ProductHSNMasterController> _logger;

    public ProductHSNMasterController(IProductHSNMasterService service, ILogger<ProductHSNMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetProductHSNs()
    {
        var result = await _service.GetProductHSNsAsync();
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProductHSN([FromBody] CreateProductHSNDto request)
    {
        var result = await _service.CreateProductHSNAsync(request);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProductHSN([FromBody] UpdateProductHSNDto request)
    {
        var result = await _service.UpdateProductHSNAsync(request);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductHSN(long id)
    {
        var result = await _service.DeleteProductHSNAsync(id);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductHSN(long id)
    {
        var result = await _service.GetProductHSNByIdAsync(id);
        return HandleResult(result);
    }

    private IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(new { message = result.ErrorMessage });
    }
}
