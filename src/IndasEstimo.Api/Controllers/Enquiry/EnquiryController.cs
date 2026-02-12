
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Enquiry;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Enquiry;

[ApiController]
[Route("api/enquiry")]
[Authorize]
public class EnquiryController : ControllerBase
{
    private readonly IEnquiryService _service;
    private readonly ILogger<EnquiryController> _logger;

    public EnquiryController(IEnquiryService service, ILogger<EnquiryController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Generate a new unique enquiry number
    /// </summary>
    [HttpGet("number")]
    public async Task<IActionResult> GetEnquiryNo()
    {
        var result = await _service.GenerateEnquiryNoAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new enquiry
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateEnquiry([FromBody] CreateEnquiryDto request)
    {
        var result = await _service.CreateEnquiryAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing enquiry
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateEnquiry([FromBody] UpdateEnquiryDto request)
    {
        var result = await _service.UpdateEnquiryAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete an enquiry
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnquiry(long id)
    {
        var result = await _service.DeleteEnquiryAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Get enquiry by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnquiry(long id)
    {
        var result = await _service.GetEnquiryByIdAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Search enquiries with filters
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> SearchEnquiries([FromBody] EnquiryFilterDto filter)
    {
        var result = await _service.GetEnquiryListAsync(filter);
        return HandleResult(result);
    }

    /// <summary>
    /// Get process data for a specific enquiry
    /// </summary>
    [HttpGet("{id}/process")]
    public async Task<IActionResult> GetProcessData(long id)
    {
        var result = await _service.GetProcessDataAsync(id);
        return HandleResult(result);
    }

    private IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(new { message = result.ErrorMessage });
    }
}
