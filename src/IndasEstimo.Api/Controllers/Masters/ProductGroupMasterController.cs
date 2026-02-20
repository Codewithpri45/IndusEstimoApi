using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/masters/productgroupmaster")]
[Authorize]
public class ProductGroupMasterController : ControllerBase
{
    private readonly IProductGroupMasterService _service;
    private readonly ILogger<ProductGroupMasterController> _logger;

    public ProductGroupMasterController(
        IProductGroupMasterService service,
        ILogger<ProductGroupMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all product HSN groups for main grid.
    /// Old VB method: Showlist()
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<ProductGroupListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductGroupList()
    {
        _logger.LogInformation("Getting product group list");

        var result = await _service.GetProductGroupListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get HSN dropdown list for UnderGroup parent selection.
    /// Old VB method: UnderGroup()
    /// </summary>
    [HttpGet("hsn-dropdown")]
    [ProducesResponseType(typeof(List<ProductHSNDropdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHSNDropdown()
    {
        _logger.LogInformation("Getting HSN dropdown");

        var result = await _service.GetHSNDropdownAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get item groups for dropdown.
    /// Old VB method: SelItemGroupName()
    /// </summary>
    [HttpGet("item-groups")]
    [ProducesResponseType(typeof(List<ItemGroupDropdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetItemGroups()
    {
        _logger.LogInformation("Getting item groups dropdown");

        var result = await _service.GetItemGroupsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Check company tax type (VAT applicable).
    /// Old VB method: CheckTaxType()
    /// </summary>
    [HttpGet("tax-type")]
    [ProducesResponseType(typeof(List<TaxTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTaxType()
    {
        _logger.LogInformation("Getting tax type");

        var result = await _service.GetTaxTypeAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Check if HSN is in use before delete. Returns 'Exist' if in use, empty string if safe to delete.
    /// Old VB method: CheckPermission(ProductHSNID)
    /// </summary>
    [HttpGet("check-permission/{productHSNId:int}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckPermission(int productHSNId)
    {
        _logger.LogInformation("Checking permission for product HSN {ProductHSNID}", productHSNId);

        var result = await _service.CheckPermissionAsync(productHSNId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save a new product HSN group.
    /// Returns 'Success', 'Exist' (duplicate DisplayName), or 'fail'.
    /// Old VB method: SavePGHMData()
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SaveProductGroup([FromBody] SaveProductGroupRequest request)
    {
        _logger.LogInformation("Saving product group {DisplayName}", request.DisplayName);

        var result = await _service.SaveProductGroupAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        if (result.Data == "Exist")
            return Conflict(new { message = "This Product Group already exists. Please enter another name." });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update an existing product HSN group.
    /// Returns 'Success' or 'fail'.
    /// Old VB method: UpdatePGHM()
    /// </summary>
    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProductGroup([FromBody] UpdateProductGroupRequest request)
    {
        _logger.LogInformation("Updating product group {ProductHSNID}", request.ProductHSNID);

        var result = await _service.UpdateProductGroupAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Soft-delete a product HSN group (sets IsDeletedTransaction=1).
    /// Returns 'Success' or 'fail'.
    /// Old VB method: DeletePGHM()
    /// </summary>
    [HttpPost("delete/{productHSNId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProductGroup(int productHSNId)
    {
        _logger.LogInformation("Deleting product group {ProductHSNID}", productHSNId);

        var result = await _service.DeleteProductGroupAsync(productHSNId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}
