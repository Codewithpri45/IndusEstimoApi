using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Auth;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class MasterAuthController : ControllerBase
{
    private readonly IMasterAuthService _masterAuthService;
    private readonly ILogger<MasterAuthController> _logger;

    public MasterAuthController(
        IMasterAuthService masterAuthService,
        ILogger<MasterAuthController> logger)
    {
        _masterAuthService = masterAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Level 1: Master/Tenant selection - Returns tenant metadata without authentication
    /// </summary>
    /// <param name="request">Tenant code to lookup</param>
    /// <returns>Tenant information</returns>
    [HttpPost("master-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MasterLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MasterLogin([FromBody] MasterLoginRequest request)
    {
            _logger.LogInformation("Master login attempt for tenant: {TenantCode}", request.TenantCode);

        var result = await _masterAuthService.GetTenantInfoAsync(request.TenantCode);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all active tenants (for dropdown/selection UI)
    /// </summary>
    /// <returns>List of active tenants</returns>
    [HttpGet("tenants")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<MasterLoginResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTenants()
    {
        var result = await _masterAuthService.GetAllActiveTenantsAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }
}
