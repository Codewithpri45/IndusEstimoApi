using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Repositories.Estimation;

/// <summary>
/// Repository interface for Material Selection operations
/// Handles all database queries for material cascading dropdowns
/// </summary>
public interface IMaterialSelectionRepository
{
    /// <summary>
    /// Get distinct quality values based on content type
    /// </summary>
    /// <param name="contentType">Content type (FLEXO, ROTOGRAVURE, LARGEFORMAT, OFFSET)</param>
    /// <returns>List of qualities</returns>
    Task<List<QualityDto>> GetQualityAsync(string contentType);

    /// <summary>
    /// Get distinct GSM values based on filters
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="thickness">Thickness filter (optional)</param>
    /// <returns>List of GSM values</returns>
    Task<List<GsmDto>> GetGsmAsync(string contentType, string? quality, decimal thickness);

    /// <summary>
    /// Get distinct thickness values based on filters
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="quality">Quality filter (optional)</param>
    /// <returns>List of thickness values</returns>
    Task<List<ThicknessDto>> GetThicknessAsync(string contentType, string? quality);

    /// <summary>
    /// Get distinct mill (manufacturer) values based on filters
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional)</param>
    /// <param name="thickness">Thickness filter (optional)</param>
    /// <returns>List of mills</returns>
    Task<List<MillDto>> GetMillAsync(string contentType, string? quality, decimal gsm, decimal thickness);

    /// <summary>
    /// Get distinct finish values based on filters
    /// </summary>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional)</param>
    /// <param name="mill">Mill filter (optional)</param>
    /// <returns>List of finishes</returns>
    Task<List<FinishDto>> GetFinishAsync(string? quality, decimal gsm, string? mill);

    /// <summary>
    /// Get coating options from parameter settings
    /// </summary>
    /// <returns>List of coating options</returns>
    Task<List<CoatingDto>> GetCoatingAsync();

    /// <summary>
    /// Get distinct BF (Bursting Factor) values based on filters
    /// </summary>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional)</param>
    /// <param name="mill">Mill filter (optional)</param>
    /// <returns>List of BF values</returns>
    Task<List<BFDto>> GetBFAsync(string? quality, decimal gsm, string? mill);

    /// <summary>
    /// Get flute types from parameter settings
    /// </summary>
    /// <returns>List of flute types</returns>
    Task<List<FluteDto>> GetFluteAsync();

    /// <summary>
    /// Get all layer items (Flexo/Rotogravure materials)
    /// </summary>
    /// <returns>List of layer items</returns>
    Task<List<LayerItemDto>> GetLayerItemsAsync();

    /// <summary>
    /// Get available layers based on width tolerance
    /// </summary>
    /// <param name="width">Target width</param>
    /// <param name="widthTolerance">Width tolerance (plus/minus)</param>
    /// <returns>List of available layers within tolerance</returns>
    Task<List<AvailableLayerDto>> GetAvailableLayersAsync(decimal width, decimal widthTolerance);

    /// <summary>
    /// Get filtered paper items (combined Mill, GSM, Finish based on quality)
    /// </summary>
    /// <param name="quality">Quality filter (optional)</param>
    /// <param name="gsm">GSM filter (optional)</param>
    /// <param name="mill">Mill filter (optional)</param>
    /// <returns>Combined filtered data</returns>
    Task<FilteredPaperDto> GetFilteredPaperAsync(string? quality, string? gsm, string? mill);
}
