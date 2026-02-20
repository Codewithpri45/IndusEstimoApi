using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/masters/productionunitmaster")]
[Authorize]
public class ProductionUnitMasterController : ControllerBase
{
    private readonly IProductionUnitMasterService _service;
    private readonly ILogger<ProductionUnitMasterController> _logger;

    public ProductionUnitMasterController(
        IProductionUnitMasterService service,
        ILogger<ProductionUnitMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all production units for main grid.
    /// Old VB method: GetProductionUnitMasterShowList()
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<ProductionUnitListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductionUnitList()
    {
        _logger.LogInformation("Getting production unit list");
        var result = await _service.GetProductionUnitListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get auto-generated next production unit code.
    /// Old VB method: GetProductionUnitNo()
    /// </summary>
    [HttpGet("unitno")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductionUnitNo()
    {
        _logger.LogInformation("Getting next production unit number");
        var result = await _service.GetProductionUnitNoAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get country dropdown list.
    /// Old VB method: GetCountry()
    /// </summary>
    [HttpGet("countries")]
    [ProducesResponseType(typeof(List<CountryDropdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCountry()
    {
        var result = await _service.GetCountryAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get state dropdown list.
    /// Old VB method: GetState()
    /// </summary>
    [HttpGet("states")]
    [ProducesResponseType(typeof(List<StateDropdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetState()
    {
        var result = await _service.GetStateAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get city dropdown list.
    /// Old VB method: GetCity()
    /// </summary>
    [HttpGet("cities")]
    [ProducesResponseType(typeof(List<CityDropdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCity()
    {
        var result = await _service.GetCityAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get company dropdown list.
    /// Old VB method: GetCompanyName()
    /// </summary>
    [HttpGet("companies")]
    [ProducesResponseType(typeof(List<CompanyDropdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCompanyName()
    {
        var result = await _service.GetCompanyNameAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get branch dropdown list.
    /// Old VB method: GetBranch()
    /// </summary>
    [HttpGet("branches")]
    [ProducesResponseType(typeof(List<BranchDropdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBranch()
    {
        var result = await _service.GetBranchAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Save a new production unit. Returns 'Success' or 'fail'.
    /// Old VB method: SaveProductionUnitMasterData()
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveProductionUnit([FromBody] SaveProductionUnitRequest request)
    {
        _logger.LogInformation("Saving production unit {Name}", request.ProductionUnitName);
        var result = await _service.SaveProductionUnitAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Update an existing production unit. Returns 'Success' or 'fail'.
    /// Old VB method: UpdateProductionUnitMasterData()
    /// </summary>
    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProductionUnit([FromBody] UpdateProductionUnitRequest request)
    {
        _logger.LogInformation("Updating production unit {ID}", request.ProductionUnitID);
        var result = await _service.UpdateProductionUnitAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Soft-delete a production unit. Returns 'Success', 'Exist' (used in transactions), or 'fail'.
    /// Old VB method: DeleteProductionUnitMasterData()
    /// </summary>
    [HttpPost("delete/{productionUnitId:long}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProductionUnit(long productionUnitId)
    {
        _logger.LogInformation("Deleting production unit {ID}", productionUnitId);
        var result = await _service.DeleteProductionUnitAsync(productionUnitId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        if (result.Data == "Exist")
            return Conflict(new { message = "Production unit detail has been used in further transactions." });

        return Ok(result.Data);
    }
}
