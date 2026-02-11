using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Services.Estimation;

/// <summary>
/// Service interface for Material Selection operations
/// Handles business logic for material cascading dropdowns
/// </summary>
public interface IMaterialSelectionService
{
    /// <summary>
    /// Get distinct quality values based on content type
    /// </summary>
    /// <param name="contentType">Content type (FLEXO, ROTOGRAVURE, LARGEFORMAT, OFFSET)</param>
    /// <returns>Result containing list of qualities</returns>
    Task<Result<List<QualityDto>>> GetQualityAsync(string contentType);

    /// <summary>
    /// Get distinct GSM values based on filters
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="thickness">Thickness filter (optional, default 0)</param>
    /// <returns>Result containing list of GSM values</returns>
    Task<Result<List<GsmDto>>> GetGsmAsync(string contentType, string? quality = null, decimal thickness = 0);

    /// <summary>
    /// Get distinct thickness values based on filters
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="quality">Quality filter (optional)</param>
    /// <returns>Result containing list of thickness values</returns>
    Task<Result<List<ThicknessDto>>> GetThicknessAsync(string contentType, string? quality = null);

    /// <summary>
    /// Get distinct mill (manufacturer) values based on filters
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional, default 0)</param>
    /// <param name="thickness">Thickness filter (optional, default 0)</param>
    /// <returns>Result containing list of mills</returns>
    Task<Result<List<MillDto>>> GetMillAsync(string contentType, string? quality = null, decimal gsm = 0, decimal thickness = 0);

    /// <summary>
    /// Get distinct finish values based on filters
    /// </summary>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional, default 0)</param>
    /// <param name="mill">Mill filter (optional)</param>
    /// <returns>Result containing list of finishes</returns>
    Task<Result<List<FinishDto>>> GetFinishAsync(string? quality = null, decimal gsm = 0, string? mill = null);

    /// <summary>
    /// Get coating options from parameter settings
    /// </summary>
    /// <returns>Result containing list of coating options</returns>
    Task<Result<List<CoatingDto>>> GetCoatingAsync();

    /// <summary>
    /// Get distinct BF (Bursting Factor) values based on filters
    /// </summary>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional, default 0)</param>
    /// <param name="mill">Mill filter (optional)</param>
    /// <returns>Result containing list of BF values</returns>
    Task<Result<List<BFDto>>> GetBFAsync(string? quality = null, decimal gsm = 0, string? mill = null);

    /// <summary>
    /// Get flute types from parameter settings
    /// </summary>
    /// <returns>Result containing list of flute types</returns>
    Task<Result<List<FluteDto>>> GetFluteAsync();

    /// <summary>
    /// Get all layer items (Flexo/Rotogravure materials)
    /// </summary>
    /// <returns>Result containing list of layer items</returns>
    Task<Result<List<LayerItemDto>>> GetLayerItemsAsync();

    /// <summary>
    /// Get available layers based on width tolerance
    /// </summary>
    /// <param name="width">Target width</param>
    /// <param name="widthTolerance">Width tolerance (plus/minus)</param>
    /// <returns>Result containing list of available layers within tolerance</returns>
    Task<Result<List<AvailableLayerDto>>> GetAvailableLayersAsync(decimal width, decimal widthTolerance);

    /// <summary>
    /// Get filtered paper items (combined Mill, GSM, Finish based on quality)
    /// </summary>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional)</param>
    /// <param name="mill">Mill filter (optional)</param>
    /// <returns>Result containing combined filtered data</returns>
    Task<Result<FilteredPaperDto>> GetFilteredPaperAsync(string? quality = null, string? gsm = null, string? mill = null);
}
