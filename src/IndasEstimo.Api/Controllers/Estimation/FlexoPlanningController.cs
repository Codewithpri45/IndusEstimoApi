using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.Interfaces.Services.Estimation;
using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Api.Controllers.Estimation;

/// <summary>
/// Controller for Flexo Planning and Estimation Module
/// Handles material selection, planning, and quotation operations
/// </summary>
[ApiController]
[Route("api/estimation")]
[Authorize]
public class FlexoPlanningController : ControllerBase
{
    private readonly IMaterialSelectionService _materialService;
    private readonly IMasterDataService _masterDataService;
    private readonly IMachineProcessService _machineService;
    private readonly IToolMaterialService _toolService;
    private readonly IPlanningCalculationService _calculationService;
    private readonly IFlexoCalculationService _flexoCalculationService;
    private readonly IQuotationService _quotationService;
    private readonly ILogger<FlexoPlanningController> _logger;

    public FlexoPlanningController(
        IMaterialSelectionService materialService,
        IMasterDataService masterDataService,
        IMachineProcessService machineService,
        IToolMaterialService toolService,
        IPlanningCalculationService calculationService,
        IFlexoCalculationService flexoCalculationService,
        IQuotationService quotationService,
        ILogger<FlexoPlanningController> logger)
    {
        _materialService = materialService;
        _masterDataService = masterDataService;
        _machineService = machineService;
        _toolService = toolService;
        _calculationService = calculationService;
        _flexoCalculationService = flexoCalculationService;
        _quotationService = quotationService;
        _logger = logger;
    }

    #region Material Selection APIs

    /// <summary>
    /// Get distinct quality values based on content type
    /// </summary>
    /// <param name="contentType">Content type (FLEXO, ROTOGRAVURE, LARGEFORMAT, OFFSET)</param>
    /// <returns>List of qualities</returns>
    [HttpGet("materials/quality/{contentType}")]
    [ProducesResponseType(typeof(List<QualityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuality(string contentType)
    {
        var result = await _materialService.GetQualityAsync(contentType);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get distinct GSM values based on content type, quality, and thickness
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="thickness">Thickness filter (optional)</param>
    /// <returns>List of GSM values</returns>
    [HttpGet("materials/gsm/{contentType}")]
    [ProducesResponseType(typeof(List<GsmDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetGsm(
        string contentType,
        [FromQuery] string? quality = null,
        [FromQuery] decimal thickness = 0)
    {
        var result = await _materialService.GetGsmAsync(contentType, quality, thickness);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get distinct thickness values based on content type and quality
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="quality">Quality filter (optional)</param>
    /// <returns>List of thickness values</returns>
    [HttpGet("materials/thickness")]
    [ProducesResponseType(typeof(List<ThicknessDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetThickness(
        [FromQuery] string contentType = "OFFSET",
        [FromQuery] string? quality = null)
    {
        var result = await _materialService.GetThicknessAsync(contentType, quality);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get distinct mill (manufacturer) values based on filters
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional)</param>
    /// <param name="thickness">Thickness filter (optional)</param>
    /// <returns>List of mills</returns>
    [HttpGet("materials/mill/{contentType}")]
    [ProducesResponseType(typeof(List<MillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMill(
        string contentType,
        [FromQuery] string? quality = null,
        [FromQuery] decimal gsm = 0,
        [FromQuery] decimal thickness = 0)
    {
        var result = await _materialService.GetMillAsync(contentType, quality, gsm, thickness);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get distinct finish values based on quality, GSM, and mill
    /// </summary>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional)</param>
    /// <param name="mill">Mill filter (optional)</param>
    /// <returns>List of finishes</returns>
    [HttpGet("materials/finish")]
    [ProducesResponseType(typeof(List<FinishDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFinish(
        [FromQuery] string? quality = null,
        [FromQuery] decimal gsm = 0,
        [FromQuery] string? mill = null)
    {
        var result = await _materialService.GetFinishAsync(quality, gsm, mill);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get coating options from parameter settings
    /// </summary>
    /// <returns>List of coating options</returns>
    [HttpGet("materials/coating")]
    [ProducesResponseType(typeof(List<CoatingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCoating()
    {
        var result = await _materialService.GetCoatingAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get distinct BF (Bursting Factor) values
    /// </summary>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional)</param>
    /// <param name="mill">Mill filter (optional)</param>
    /// <returns>List of BF values</returns>
    [HttpGet("materials/bf")]
    [ProducesResponseType(typeof(List<BFDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBF(
        [FromQuery] string? quality = null,
        [FromQuery] decimal gsm = 0,
        [FromQuery] string? mill = null)
    {
        var result = await _materialService.GetBFAsync(quality, gsm, mill);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get flute types for corrugation
    /// </summary>
    /// <returns>List of flute types</returns>
    [HttpGet("materials/flute")]
    [ProducesResponseType(typeof(List<FluteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFlute()
    {
        var result = await _materialService.GetFluteAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get all layer items (Flexo/Rotogravure materials)
    /// </summary>
    /// <returns>List of layer items</returns>
    [HttpGet("materials/layer-items")]
    [ProducesResponseType(typeof(List<LayerItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLayerItems()
    {
        var result = await _materialService.GetLayerItemsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get available layers based on width tolerance
    /// </summary>
    /// <param name="width">Target width</param>
    /// <param name="widthTolerance">Width tolerance (plus/minus)</param>
    /// <returns>List of available layers within tolerance</returns>
    [HttpGet("materials/available-layers")]
    [ProducesResponseType(typeof(List<AvailableLayerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAvailableLayers(
        [FromQuery] decimal width,
        [FromQuery] decimal widthTolerance)
    {
        var result = await _materialService.GetAvailableLayersAsync(width, widthTolerance);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get filtered paper items (combined Mill, GSM, Finish)
    /// </summary>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional)</param>
    /// <param name="mill">Mill filter (optional)</param>
    /// <returns>Combined filtered data</returns>
    [HttpGet("materials/filtered-paper")]
    [ProducesResponseType(typeof(FilteredPaperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFilteredPaper(
        [FromQuery] string? quality = null,
        [FromQuery] string? gsm = null,
        [FromQuery] string? mill = null)
    {
        var result = await _materialService.GetFilteredPaperAsync(quality, gsm, mill);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    #endregion

    #region Master Data APIs

    /// <summary>
    /// Get all categories with segment information
    /// </summary>
    /// <returns>List of categories</returns>
    [HttpGet("masters/categories")]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _masterDataService.GetCategoriesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get all clients (customers)
    /// </summary>
    /// <returns>List of clients</returns>
    [HttpGet("masters/clients")]
    [ProducesResponseType(typeof(List<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetClients()
    {
        var result = await _masterDataService.GetClientsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get all sales persons
    /// </summary>
    /// <returns>List of sales persons</returns>
    [HttpGet("masters/salespersons")]
    [ProducesResponseType(typeof(List<SalesPersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSalesPersons()
    {
        var result = await _masterDataService.GetSalesPersonsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get all active contents
    /// </summary>
    /// <returns>List of all contents</returns>
    [HttpGet("masters/contents")]
    [ProducesResponseType(typeof(List<ContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllContents()
    {
        var result = await _masterDataService.GetAllContentsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get contents by category ID
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of contents for the specified category</returns>
    [HttpGet("masters/contents/{categoryId}")]
    [ProducesResponseType(typeof(List<ContentByCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetContentsByCategory(long categoryId)
    {
        var result = await _masterDataService.GetContentsByCategoryAsync(categoryId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get category defaults (wastage, colors, etc.)
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Category default settings</returns>
    [HttpGet("masters/category-defaults/{categoryId}")]
    [ProducesResponseType(typeof(CategoryDefaultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategoryDefaults(long categoryId)
    {
        var result = await _masterDataService.GetCategoryDefaultsAsync(categoryId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get winding direction options by content type
    /// </summary>
    /// <param name="contentType">Content domain type</param>
    /// <returns>List of winding direction options with images</returns>
    [HttpGet("masters/winding-direction/{contentType}")]
    [ProducesResponseType(typeof(List<WindingDirectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWindingDirection(string contentType)
    {
        var result = await _masterDataService.GetWindingDirectionAsync(contentType);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get book contents
    /// </summary>
    /// <returns>List of book contents</returns>
    [HttpGet("masters/book-contents")]
    [ProducesResponseType(typeof(List<BookContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBookContents()
    {
        var result = await _masterDataService.GetBookContentsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get one-time charges
    /// </summary>
    /// <returns>List of one-time charge types</returns>
    [HttpGet("masters/one-time-charges")]
    [ProducesResponseType(typeof(List<OneTimeChargeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOneTimeCharges()
    {
        var result = await _masterDataService.GetOneTimeChargesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get content sizes for a specific content
    /// </summary>
    /// <param name="contentName">Content name</param>
    /// <returns>Content size configuration</returns>
    [HttpGet("masters/content-size")]
    [ProducesResponseType(typeof(ContentSizeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetContentSize([FromQuery] string contentName)
    {
        var result = await _masterDataService.GetContentSizeAsync(contentName);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    #endregion

    #region Machine & Process APIs

    /// <summary>
    /// Get machine grid by content domain type
    /// </summary>
    /// <param name="contentType">Content domain type (FLEXO, ROTOGRAVURE, etc.)</param>
    /// <returns>List of machines with specifications</returns>
    [HttpGet("machines/grid/{contentType}")]
    [ProducesResponseType(typeof(List<MachineGridDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMachineGrid(string contentType)
    {
        var result = await _machineService.GetMachineGridAsync(contentType);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get all active machines
    /// </summary>
    /// <returns>List of all machines</returns>
    [HttpGet("machines/list")]
    [ProducesResponseType(typeof(List<MachineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllMachines()
    {
        var result = await _machineService.GetAllMachinesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get default operations by domain type
    /// </summary>
    /// <param name="domainType">Domain type (FLEXO, ROTOGRAVURE, etc.)</param>
    /// <returns>List of default operations/processes</returns>
    [HttpGet("operations/default/{domainType}")]
    [ProducesResponseType(typeof(List<OperationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDefaultOperations(string domainType, [FromQuery] int? categoryId = null)
    {
        var result = await _machineService.GetDefaultOperationsAsync(domainType, categoryId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get operation slabs for a process
    /// </summary>
    /// <param name="processId">Process ID</param>
    /// <returns>List of operation slabs</returns>
    [HttpGet("operations/slabs/{processId}")]
    [ProducesResponseType(typeof(List<OperationSlabDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOperationSlabs(long processId)
    {
        var result = await _machineService.GetOperationSlabsAsync(processId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get allocated items for a machine
    /// </summary>
    /// <param name="machineId">Machine ID</param>
    /// <returns>List of items allocated to the machine</returns>
    [HttpGet("machines/{machineId}/items")]
    [ProducesResponseType(typeof(List<MachineItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMachineItems(long machineId)
    {
        var result = await _machineService.GetMachineItemsAsync(machineId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    #endregion

    #region Tool & Material APIs

    /// <summary>
    /// Search for dies/tools based on dimensions
    /// </summary>
    /// <param name="request">Search criteria with dimensions</param>
    /// <returns>List of matching dies/tools</returns>
    [HttpPost("tools/search-dies")]
    [ProducesResponseType(typeof(List<DieToolDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchDies([FromBody] SearchDiesRequest request)
    {
        var result = await _toolService.SearchDiesAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get reels with width tolerance filtering
    /// </summary>
    /// <param name="reqDeckle">Required deckle width</param>
    /// <param name="widthPlus">Width plus tolerance (optional)</param>
    /// <param name="widthMinus">Width minus tolerance (optional)</param>
    /// <returns>List of reels within tolerance</returns>
    [HttpGet("tools/reels")]
    [ProducesResponseType(typeof(List<ReelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReels(
        [FromQuery] decimal reqDeckle,
        [FromQuery] decimal widthPlus = 0,
        [FromQuery] decimal widthMinus = 0)
    {
        var result = await _toolService.GetReelsAsync(reqDeckle, widthPlus, widthMinus);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get materials allocated to processes
    /// </summary>
    /// <param name="processIds">Comma-separated process IDs</param>
    /// <returns>List of materials for the processes</returns>
    [HttpGet("process/materials/{processIds}")]
    [ProducesResponseType(typeof(List<ProcessMaterialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProcessMaterials(string processIds)
    {
        var result = await _toolService.GetProcessMaterialsAsync(processIds);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    #endregion

    #region Planning & Calculation APIs

    /// <summary>
    /// Calculate Flexo Plan (Core Calculation)
    /// Replaces legacy Shirin Job Logic
    /// </summary>
    [HttpPost("calculate/flexo")]
    [ProducesResponseType(typeof(List<FlexoPlanResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateFlexoPlan([FromBody] FlexoPlanCalculationRequest request)
    {
        var result = await _flexoCalculationService.CalculateFlexoPlanAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Calculate operation cost based on process and quantity
    /// </summary>
    /// <param name="request">Calculation request with process details</param>
    /// <returns>Calculated operation cost</returns>
    [HttpPost("calculate/operation")]
    [ProducesResponseType(typeof(CalculateOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CalculateOperation([FromBody] CalculateOperationRequest request)
    {
        var result = await _calculationService.CalculateOperationAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get all charge types
    /// </summary>
    /// <returns>List of charge types</returns>
    [HttpGet("calculate/charge-types")]
    [ProducesResponseType(typeof(List<ChargeTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetChargeTypes()
    {
        var result = await _calculationService.GetChargeTypesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get material cost formula setting
    /// </summary>
    /// <param name="itemSubGroupId">Item sub-group ID</param>
    /// <param name="plantId">Plant ID</param>
    /// <returns>Material formula configuration</returns>
    [HttpGet("calculate/material-formula")]
    [ProducesResponseType(typeof(MaterialFormulaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMaterialFormula(
        [FromQuery] long itemSubGroupId,
        [FromQuery] long plantId)
    {
        var result = await _calculationService.GetMaterialFormulaAsync(itemSubGroupId, plantId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get wastage percentage from slab
    /// </summary>
    /// <param name="actualSheets">Actual sheet count</param>
    /// <param name="wastageType">Wastage type</param>
    /// <returns>Wastage slab with percentage</returns>
    [HttpGet("calculate/wastage-slab")]
    [ProducesResponseType(typeof(WastageSlabDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWastageSlab(
        [FromQuery] decimal actualSheets,
        [FromQuery] string wastageType)
    {
        var result = await _calculationService.GetWastageSlabAsync(actualSheets, wastageType);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get all wastage types
    /// </summary>
    /// <returns>List of wastage types</returns>
    [HttpGet("calculate/wastage-types")]
    [ProducesResponseType(typeof(List<WastageTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllWastageTypes()
    {
        var result = await _calculationService.GetAllWastageTypesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get keyline coordinates for content type
    /// </summary>
    /// <param name="contentType">Content type (FLEXO, ROTOGRAVURE, etc.)</param>
    /// <param name="grain">Grain direction (optional)</param>
    /// <returns>Keyline trimming and striping coordinates</returns>
    [HttpGet("calculate/keyline/{contentType}")]
    [ProducesResponseType(typeof(KeylineCoordinatesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetKeylineCoordinates(
        string contentType,
        [FromQuery] string? grain = null)
    {
        var result = await _calculationService.GetKeylineCoordinatesAsync(contentType, grain);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Calculate corrugation plan details
    /// </summary>
    /// <param name="request">Corrugation parameters</param>
    /// <returns>Calculated corrugation details</returns>
    [HttpPost("calculate/corrugation")]
    [ProducesResponseType(typeof(CorrugationPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CalculateCorrugation([FromBody] CorrugationPlanRequest request)
    {
        var result = await _calculationService.CalculateCorrugationAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    #endregion

    #region Quotation APIs

    /// <summary>
    /// Get next available quotation number
    /// </summary>
    /// <returns>Next quotation number</returns>
    [HttpGet("quotation/next-number")]
    [ProducesResponseType(typeof(QuoteNumberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNextQuoteNumber()
    {
        var result = await _quotationService.GetNextQuoteNumberAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Save or update quotation
    /// </summary>
    /// <param name="request">Quotation data to save</param>
    /// <returns>Save response with booking ID</returns>
    [HttpPost("quotation/save")]
    [ProducesResponseType(typeof(SaveQuotationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveQuotation([FromBody] SaveQuotationRequest request)
    {
        var result = await _quotationService.SaveQuotationAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Load quotation details by booking ID
    /// </summary>
    /// <param name="bookingId">Booking ID</param>
    /// <returns>Complete quotation details</returns>
    [HttpGet("quotation/{bookingId}")]
    [ProducesResponseType(typeof(QuotationDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoadQuotation(long bookingId)
    {
        var result = await _quotationService.LoadQuotationAsync(bookingId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    #endregion
}
