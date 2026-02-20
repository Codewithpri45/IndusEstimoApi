using Dapper;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Estimation;

/// <summary>
/// Repository implementation for Material Selection operations
/// Uses Dapper for database queries with parameterization for SQL injection protection
/// </summary>
public class MaterialSelectionRepository : IMaterialSelectionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MaterialSelectionRepository> _logger;

    public MaterialSelectionRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<MaterialSelectionRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    private string GetItemGroupNameId(string contentType)
    {
        return contentType.ToUpper() switch
        {
            "FLEXO" => "-14",
            "ROTOGRAVURE" => "-15",
            "LARGEFORMAT" => "-16",
            _ => "-1,-2"
        };
    }

    public async Task<List<QualityDto>> GetQualityAsync(string contentType)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var itemGroupNameId = GetItemGroupNameId(contentType);

        string query = $@"
            SELECT DISTINCT Quality 
            FROM ItemMaster 
            WHERE ISNULL(ISItemActive, 1) <> 0 
              AND ItemGroupID IN (
                  SELECT ItemGroupID 
                  FROM ItemGroupMaster 
                  WHERE ItemGroupNameID IN ({itemGroupNameId})
                    AND CompanyID = @CompanyID
              )
              AND ISNULL(IsDeletedTransaction, 0) = 0 
              AND ISNULL(Quality, '') <> '' 
              AND CompanyID = @CompanyID
            ORDER BY Quality";

        var results = await connection.QueryAsync<QualityDto>(query, new { CompanyID = companyId });
        return results.ToList();
    }

    public async Task<List<GsmDto>> GetGsmAsync(string contentType, string? quality, decimal thickness)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var itemGroupNameId = GetItemGroupNameId(contentType);
        
        string query;
        
        if (string.IsNullOrEmpty(quality))
        {
            query = $@"
                SELECT DISTINCT GSM 
                FROM ItemMaster 
                WHERE ISNULL(ISItemActive, 1) <> 0 
                  AND ItemGroupID IN (
                      SELECT ItemGroupID 
                      FROM ItemGroupMaster 
                      WHERE ItemGroupNameID IN ({itemGroupNameId})
                        AND CompanyID = @CompanyID
                  )
                  AND ISNULL(IsDeletedTransaction, 0) = 0 
                  AND (ISNULL(GSM, 0) > 0 OR ISNULL(Thickness, 0) > 0)
                  AND CompanyID = @CompanyID
                ORDER BY GSM";

            var resultList = await connection.QueryAsync<GsmDto>(query, new { CompanyID = companyId });
            return resultList.ToList();
        }
        else
        {
            if (thickness > 0)
            {
                query = $@"
                    SELECT DISTINCT GSM 
                    FROM ItemMaster 
                    WHERE ISNULL(ISItemActive, 1) <> 0 
                      AND ItemGroupID IN (
                          SELECT ItemGroupID 
                          FROM ItemGroupMaster 
                          WHERE ItemGroupNameID IN ({itemGroupNameId})
                            AND CompanyID = @CompanyID
                      )
                      AND ItemID IN (
                          SELECT ItemID 
                          FROM ItemMaster 
                          WHERE IsDeletedTransaction = 0 
                            AND Quality = @Quality 
                            AND Thickness = @Thickness
                            AND CompanyID = @CompanyID
                      )
                      AND ISNULL(IsDeletedTransaction, 0) <> 1 
                      AND CompanyID = @CompanyID
                    ORDER BY GSM";
                
                var results = await connection.QueryAsync<GsmDto>(query, new { Quality = quality, Thickness = thickness, CompanyID = companyId });
                return results.ToList();
            }
            else
            {
                query = $@"
                    SELECT DISTINCT GSM 
                    FROM ItemMaster 
                    WHERE ISNULL(ISItemActive, 1) <> 0 
                      AND ItemGroupID IN (
                          SELECT ItemGroupID 
                          FROM ItemGroupMaster 
                          WHERE ItemGroupNameID IN ({itemGroupNameId})
                            AND CompanyID = @CompanyID
                      )
                      AND ItemID IN (
                          SELECT ItemID 
                          FROM ItemMaster 
                          WHERE IsDeletedTransaction = 0 
                            AND Quality = @Quality
                            AND CompanyID = @CompanyID
                      )
                      AND ISNULL(IsDeletedTransaction, 0) <> 1 
                      AND CompanyID = @CompanyID
                    ORDER BY GSM";
                
                var results = await connection.QueryAsync<GsmDto>(query, new { Quality = quality, CompanyID = companyId });
                return results.ToList();
            }
        }
    }

    public async Task<List<ThicknessDto>> GetThicknessAsync(string contentType, string? quality)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var itemGroupNameId = GetItemGroupNameId(contentType);
        
        string query;
        
        if (string.IsNullOrEmpty(quality))
        {
            query = $@"
                SELECT DISTINCT Thickness 
                FROM ItemMaster 
                WHERE ISNULL(ISItemActive, 1) <> 0 
                  AND ItemGroupID IN (
                      SELECT ItemGroupID 
                      FROM ItemGroupMaster 
                      WHERE ItemGroupNameID IN ({itemGroupNameId})
                        AND CompanyID = @CompanyID
                  )
                  AND ISNULL(IsDeletedTransaction, 0) = 0 
                  AND ISNULL(Thickness, 0) > 0 
                  AND CompanyID = @CompanyID
                ORDER BY Thickness";

            var resultList = await connection.QueryAsync<ThicknessDto>(query, new { CompanyID = companyId });
            return resultList.ToList();
        }
        else
        {
            query = $@"
                SELECT DISTINCT Thickness 
                FROM ItemMaster 
                WHERE ISNULL(ISItemActive, 1) <> 0 
                  AND ItemGroupID IN (
                      SELECT ItemGroupID 
                      FROM ItemGroupMaster 
                      WHERE ItemGroupNameID IN ({itemGroupNameId})
                        AND CompanyID = @CompanyID
                  )
                  AND ItemID IN (
                      SELECT ItemID 
                      FROM ItemMaster 
                      WHERE IsDeletedTransaction = 0 
                        AND Quality = @Quality
                        AND CompanyID = @CompanyID
                  )
                  AND ISNULL(IsDeletedTransaction, 0) <> 1 
                  AND CompanyID = @CompanyID
                ORDER BY Thickness";
            
            var results = await connection.QueryAsync<ThicknessDto>(query, new { Quality = quality, CompanyID = companyId });
            return results.ToList();
        }
    }

    public async Task<List<MillDto>> GetMillAsync(string contentType, string? quality, decimal gsm, decimal thickness)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var itemGroupNameId = GetItemGroupNameId(contentType);
        
        string query;
        
        if (string.IsNullOrEmpty(quality) && gsm == 0 && thickness == 0)
        {
            query = $@"
                SELECT DISTINCT Manufecturer AS Mill 
                FROM ItemMaster 
                WHERE ISNULL(ISItemActive, 1) <> 0 
                  AND ItemGroupID IN (
                      SELECT ItemGroupID 
                      FROM ItemGroupMaster 
                      WHERE ItemGroupNameID IN ({itemGroupNameId})
                        AND CompanyID = @CompanyID
                  )
                  AND ISNULL(IsDeletedTransaction, 0) <> 1 
                  AND ISNULL(Manufecturer, '') <> '' 
                  AND CompanyID = @CompanyID
                ORDER BY Mill";

            var resultList = await connection.QueryAsync<MillDto>(query, new { CompanyID = companyId });
            return resultList.ToList();
        }
        else
        {
            if (!string.IsNullOrEmpty(quality) && gsm > 0)
            {
                query = $@"
                    SELECT DISTINCT Manufecturer AS Mill 
                    FROM ItemMaster 
                    WHERE ISNULL(ISItemActive, 1) <> 0 
                      AND ItemGroupID IN (
                          SELECT ItemGroupID 
                          FROM ItemGroupMaster 
                          WHERE ItemGroupNameID IN ({itemGroupNameId})
                            AND CompanyID = @CompanyID
                      )
                      AND ItemID IN (
                          SELECT ItemID 
                          FROM ItemMaster 
                          WHERE Quality = @Quality 
                            AND IsDeletedTransaction = 0
                            AND CompanyID = @CompanyID
                      )
                      AND ItemID IN (
                          SELECT ItemID 
                          FROM ItemMaster 
                          WHERE GSM = @GSM 
                            AND IsDeletedTransaction = 0
                            AND CompanyID = @CompanyID
                      )
                      AND ISNULL(IsDeletedTransaction, 0) <> 1 
                      AND CompanyID = @CompanyID
                    ORDER BY Mill";
                
                var results = await connection.QueryAsync<MillDto>(query, new { Quality = quality, GSM = gsm, CompanyID = companyId });
                return results.ToList();
            }
            else if (!string.IsNullOrEmpty(quality) && thickness > 0)
            {
                query = $@"
                    SELECT DISTINCT Manufecturer AS Mill 
                    FROM ItemMaster 
                    WHERE ISNULL(ISItemActive, 1) <> 0 
                      AND ItemGroupID IN (
                          SELECT ItemGroupID 
                          FROM ItemGroupMaster 
                          WHERE ItemGroupNameID IN ({itemGroupNameId})
                            AND CompanyID = @CompanyID
                      )
                      AND ItemID IN (
                          SELECT ItemID 
                          FROM ItemMaster 
                          WHERE Quality = @Quality 
                            AND IsDeletedTransaction = 0
                            AND CompanyID = @CompanyID
                      )
                      AND ItemID IN (
                          SELECT ItemID 
                          FROM ItemMaster 
                          WHERE Thickness = @Thickness 
                            AND IsDeletedTransaction = 0
                            AND CompanyID = @CompanyID
                      )
                      AND ISNULL(IsDeletedTransaction, 0) <> 1 
                      AND CompanyID = @CompanyID
                    ORDER BY Mill";
                
                var results = await connection.QueryAsync<MillDto>(query, new { Quality = quality, Thickness = thickness, CompanyID = companyId });
                return results.ToList();
            }
            else
            {
                query = $@"
                    SELECT DISTINCT Manufecturer AS Mill 
                    FROM ItemMaster 
                    WHERE ISNULL(ISItemActive, 1) <> 0 
                      AND ItemGroupID IN (
                          SELECT ItemGroupID 
                          FROM ItemGroupMaster 
                          WHERE ItemGroupNameID IN ({itemGroupNameId})
                            AND CompanyID = @CompanyID
                      )
                      AND ItemID IN (
                          SELECT ItemID 
                          FROM ItemMaster 
                          WHERE Quality = @Quality 
                            AND IsDeletedTransaction = 0
                            AND CompanyID = @CompanyID
                      )
                      AND ItemID IN (
                          SELECT ItemID 
                          FROM ItemMaster 
                          WHERE GSM = @GSM 
                            AND IsDeletedTransaction = 0
                            AND CompanyID = @CompanyID
                      )
                      AND ISNULL(IsDeletedTransaction, 0) <> 1 
                      AND CompanyID = @CompanyID
                    ORDER BY Mill";
                
                var results = await connection.QueryAsync<MillDto>(query, new { Quality = quality, GSM = gsm, CompanyID = companyId });
                return results.ToList();
            }
        }
    }

    public async Task<List<FinishDto>> GetFinishAsync(string? quality, decimal gsm, string? mill)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        
        string query;
        
        if (string.IsNullOrEmpty(quality) && gsm == 0 && string.IsNullOrEmpty(mill))
        {
            query = @"
                SELECT DISTINCT Finish 
                FROM ItemMaster 
                WHERE ISNULL(ISItemActive, 1) <> 0 
                  AND ItemGroupID IN (
                      SELECT ItemGroupID 
                      FROM ItemGroupMaster 
                      WHERE CompanyID = @CompanyID 
                        AND ItemGroupNameID IN (-1, -2, -14, -15, -16)
                  )
                  AND ISNULL(IsDeletedTransaction, 0) <> 1 
                  AND ISNULL(Finish, '') <> '' 
                  AND CompanyID = @CompanyID 
                ORDER BY Finish";
            
            var results = await connection.QueryAsync<FinishDto>(query, new { CompanyID = companyId });
            return results.ToList();
        }
        else
        {
            query = @"
                SELECT DISTINCT Finish 
                FROM ItemMaster 
                WHERE ISNULL(ISItemActive, 1) <> 0 
                  AND ItemGroupID IN (
                      SELECT ItemGroupID 
                      FROM ItemGroupMaster 
                      WHERE CompanyID = @CompanyID 
                        AND ItemGroupNameID IN (-1, -2, -14, -15, -16)
                  )
                  AND ItemID IN (
                      SELECT ItemID 
                      FROM ItemMaster 
                      WHERE GSM = @GSM 
                        AND CompanyID = @CompanyID 
                        AND IsDeletedTransaction = 0
                  )
                  AND ItemID IN (
                      SELECT ItemID 
                      FROM ItemMaster 
                      WHERE Quality = @Quality 
                        AND CompanyID = @CompanyID 
                        AND IsDeletedTransaction = 0
                  )
                  AND ItemID IN (
                      SELECT ItemID 
                      FROM ItemMaster 
                      WHERE Manufecturer = @Mill 
                        AND CompanyID = @CompanyID 
                        AND IsDeletedTransaction = 0
                  )
                  AND ISNULL(IsDeletedTransaction, 0) <> 1 
                ORDER BY Finish";
            
            var results = await connection.QueryAsync<FinishDto>(query, new 
            { 
                CompanyID = companyId, 
                GSM = gsm, 
                Quality = quality, 
                Mill = mill 
            });
            return results.ToList();
        }
    }

    public async Task<List<CoatingDto>> GetCoatingAsync()
    {
        using var connection = GetConnection();
        
        string query = @"
            SELECT ParameterValue AS Headname, '0' AS Amount 
            FROM ERPParameterSetting 
            WHERE ParameterType = 'One Time Charges' 
              AND ISNULL(IsDeletedTransaction, 0) = 0 
            ORDER BY ParameterID";

        var results = await connection.QueryAsync<CoatingDto>(query);
        return results.ToList();
    }

    public async Task<List<BFDto>> GetBFAsync(string? quality, decimal gsm, string? mill)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        
        string query;
        
        if (string.IsNullOrEmpty(quality) && gsm == 0 && string.IsNullOrEmpty(mill))
        {
            query = @"
                SELECT DISTINCT BF 
                FROM ItemMaster 
                WHERE ISNULL(ISItemActive, 1) <> 0 
                  AND ItemGroupID IN (
                      SELECT ItemGroupID 
                      FROM ItemGroupMaster 
                      WHERE CompanyID = @CompanyID 
                        AND ItemGroupNameID IN (-1, -2, -14, -15, -16)
                  )
                  AND ISNULL(IsDeletedTransaction, 0) <> 1 
                  AND ISNULL(BF, 0) > 0 
                  AND CompanyID = @CompanyID 
                ORDER BY BF";
            
            var results = await connection.QueryAsync<BFDto>(query, new { CompanyID = companyId });
            return results.ToList();
        }
        else
        {
            query = @"
                SELECT DISTINCT BF 
                FROM ItemMaster 
                WHERE ISNULL(ISItemActive, 1) <> 0 
                  AND ItemGroupID IN (
                      SELECT ItemGroupID 
                      FROM ItemGroupMaster 
                      WHERE CompanyID = @CompanyID 
                        AND ItemGroupNameID IN (-1, -2, -14, -15, -16)
                  )
                  AND ItemID IN (
                      SELECT ItemID 
                      FROM ItemMaster 
                      WHERE GSM = @GSM 
                        AND CompanyID = @CompanyID 
                        AND IsDeletedTransaction = 0
                  )
                  AND ItemID IN (
                      SELECT ItemID 
                      FROM ItemMaster 
                      WHERE Quality = @Quality 
                        AND CompanyID = @CompanyID 
                        AND IsDeletedTransaction = 0
                  )
                  AND ItemID IN (
                      SELECT ItemID 
                      FROM ItemMaster 
                      WHERE Manufecturer = @Mill 
                        AND CompanyID = @CompanyID 
                        AND IsDeletedTransaction = 0
                  )
                  AND ISNULL(IsDeletedTransaction, 0) <> 1 
                  AND ISNULL(BF, 0) > 0 
                ORDER BY BF";
            
            var results = await connection.QueryAsync<BFDto>(query, new 
            { 
                CompanyID = companyId, 
                GSM = gsm, 
                Quality = quality, 
                Mill = mill 
            });
            return results.ToList();
        }
    }

    public async Task<List<FluteDto>> GetFluteAsync()
    {
        using var connection = GetConnection();
        
        // Assuming flute types are stored in ERPParameterSetting or a similar table
        string query = @"
            SELECT ParameterValue AS FluteType 
            FROM ERPParameterSetting 
            WHERE ParameterType = 'Flute Type' 
              AND ISNULL(IsDeletedTransaction, 0) = 0 
            ORDER BY ParameterID";

        var results = await connection.QueryAsync<FluteDto>(query);
        return results.ToList();
    }

    public async Task<List<LayerItemDto>> GetLayerItemsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        
        string query = @"
            SELECT 
                IM.ItemID, 
                IM.ItemCode, 
                IGM.ItemGroupID, 
                ISGM.ItemSubGroupID, 
                IGM.ItemGroupName, 
                ISGM.ItemSubGroupName, 
                IM.ItemName, 
                IM.Quality, 
                IM.SizeW, 
                IM.Thickness, 
                IM.Density, 
                IM.GSM, 
                IM.Manufecturer, 
                IM.EstimationUnit, 
                IM.EstimationRate, 
                IM.StockUnit 
            FROM ItemMaster AS IM 
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID 
            LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID = IM.ItemSubGroupID 
                AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0 
            WHERE IM.CompanyID = @CompanyID 
              AND ISNULL(IM.IsDeletedTransaction, 0) = 0 
              AND IGM.ItemGroupNameID IN (-14, -15) 
            ORDER BY IM.ItemGroupID, ISGM.ItemSubGroupName, IM.Quality, 
                     IM.Thickness, IM.Density, IM.GSM, IM.SizeW";

        var results = await connection.QueryAsync<LayerItemDto>(query, new { CompanyID = companyId });
        return results.ToList();
    }

    public async Task<List<AvailableLayerDto>> GetAvailableLayersAsync(decimal width, decimal widthTolerance)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        
        string query = @"
            SELECT 
                IM.ItemID, 
                IM.ItemCode, 
                IGM.ItemGroupID, 
                ISGM.ItemSubGroupID, 
                IGM.ItemGroupName, 
                ISGM.ItemSubGroupName, 
                IM.ItemName, 
                IM.Quality, 
                IM.SizeW, 
                IM.Thickness, 
                IM.Density, 
                IM.GSM, 
                IM.Manufecturer, 
                IM.EstimationUnit, 
                IM.EstimationRate, 
                IM.StockUnit 
            FROM ItemMaster AS IM 
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID 
            LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID = IM.ItemSubGroupID 
            WHERE IM.CompanyID = @CompanyID 
              AND ISNULL(IM.IsDeletedTransaction, 0) = 0 
              AND IGM.ItemGroupNameID IN (-14, -15) 
              AND (IM.SizeW >= (@Width - @WidthTolerance) 
                   AND IM.SizeW <= (@Width + @WidthTolerance)) 
            ORDER BY IM.ItemGroupID, ISGM.ItemSubGroupName, IM.Quality, 
                     IM.Thickness, IM.Density, IM.GSM, IM.SizeW";

        var results = await connection.QueryAsync<AvailableLayerDto>(query, new 
        { 
            CompanyID = companyId, 
            Width = width, 
            WidthTolerance = widthTolerance 
        });
        return results.ToList();
    }

    public async Task<FilteredPaperDto> GetFilteredPaperAsync(string? quality, string? gsm, string? mill)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        
        var result = new FilteredPaperDto();

        // Get GSM data
        string gsmQuery;
        if (string.IsNullOrEmpty(quality))
        {
            gsmQuery = @"
                SELECT DISTINCT GSM 
                FROM ItemMaster 
                WHERE ItemGroupID IN (
                    SELECT ItemGroupID 
                    FROM ItemGroupMaster 
                    WHERE CompanyID = @CompanyID 
                      AND ItemGroupNameID IN (-1, -2)
                )
                AND ISNULL(IsDeletedTransaction, 0) <> 1 
                AND ISNULL(GSM, 0) > 0 
                AND CompanyID = @CompanyID 
                ORDER BY GSM";
            
            result.GSMs = (await connection.QueryAsync<GsmDto>(gsmQuery, new { CompanyID = companyId })).ToList();
        }
        else
        {
            gsmQuery = @"
                SELECT DISTINCT GSM 
                FROM ItemMaster 
                WHERE ItemGroupID IN (
                    SELECT ItemGroupID 
                    FROM ItemGroupMaster 
                    WHERE CompanyID = @CompanyID 
                      AND ItemGroupNameID IN (-1, -2)
                )
                AND ItemID IN (
                    SELECT ItemID 
                    FROM ItemMaster 
                    WHERE Quality = @Quality 
                      AND CompanyID = @CompanyID 
                      AND IsDeletedTransaction = 0
                )
                AND ISNULL(IsDeletedTransaction, 0) <> 1 
                ORDER BY GSM";
            
            result.GSMs = (await connection.QueryAsync<GsmDto>(gsmQuery, new { CompanyID = companyId, Quality = quality })).ToList();
        }

        // Get Mill data
        string millQuery;
        if (string.IsNullOrEmpty(quality) && string.IsNullOrEmpty(gsm))
        {
            millQuery = @"
                SELECT DISTINCT Manufecturer AS Mill 
                FROM ItemMaster 
                WHERE ItemGroupID IN (
                    SELECT ItemGroupID 
                    FROM ItemGroupMaster 
                    WHERE CompanyID = @CompanyID 
                      AND ItemGroupNameID IN (-1, -2)
                )
                AND ISNULL(IsDeletedTransaction, 0) <> 1 
                AND ISNULL(Manufecturer, '') <> '' 
                AND CompanyID = @CompanyID 
                ORDER BY Mill";
            
            result.Mills = (await connection.QueryAsync<MillDto>(millQuery, new { CompanyID = companyId })).ToList();
        }
        else
        {
            millQuery = @"
                SELECT DISTINCT Manufecturer AS Mill 
                FROM ItemMaster 
                WHERE ItemGroupID IN (
                    SELECT ItemGroupID 
                    FROM ItemGroupMaster 
                    WHERE CompanyID = @CompanyID 
                      AND ItemGroupNameID IN (-1, -2)
                )
                AND ItemID IN (
                    SELECT ItemID 
                    FROM ItemMaster 
                    WHERE GSM = @GSM 
                      AND CompanyID = @CompanyID 
                      AND IsDeletedTransaction = 0
                )
                AND ItemID IN (
                    SELECT ItemID 
                    FROM ItemMaster 
                    WHERE Quality = @Quality 
                      AND CompanyID = @CompanyID 
                      AND IsDeletedTransaction = 0
                )
                AND ISNULL(IsDeletedTransaction, 0) <> 1 
                ORDER BY Mill";
            
            result.Mills = (await connection.QueryAsync<MillDto>(millQuery, new 
            { 
                CompanyID = companyId, 
                GSM = gsm, 
                Quality = quality 
            })).ToList();
        }

        // Get Finish data
        string finishQuery;
        if (string.IsNullOrEmpty(quality) && string.IsNullOrEmpty(gsm) && string.IsNullOrEmpty(mill))
        {
            finishQuery = @"
                SELECT DISTINCT Finish 
                FROM ItemMaster 
                WHERE ItemGroupID IN (
                    SELECT ItemGroupID 
                    FROM ItemGroupMaster 
                    WHERE CompanyID = @CompanyID 
                      AND ItemGroupNameID IN (-1, -2)
                )
                AND CompanyID = @CompanyID 
                AND ISNULL(IsDeletedTransaction, 0) <> 1 
                AND ISNULL(Finish, '') <> '' 
                ORDER BY Finish";
            
            result.Finishes = (await connection.QueryAsync<FinishDto>(finishQuery, new { CompanyID = companyId })).ToList();
        }
        else
        {
            finishQuery = @"
                SELECT DISTINCT Finish 
                FROM ItemMaster 
                WHERE ItemGroupID IN (
                    SELECT ItemGroupID 
                    FROM ItemGroupMaster 
                    WHERE CompanyID = @CompanyID 
                      AND ItemGroupNameID IN (-1, -2)
                )
                AND ItemID IN (
                    SELECT ItemID 
                    FROM ItemMaster 
                    WHERE GSM = @GSM 
                      AND CompanyID = @CompanyID 
                      AND IsDeletedTransaction = 0
                )
                AND ItemID IN (
                    SELECT ItemID 
                    FROM ItemMaster 
                    WHERE Quality = @Quality 
                      AND CompanyID = @CompanyID 
                      AND IsDeletedTransaction = 0
                )
                AND ItemID IN (
                    SELECT ItemID 
                    FROM ItemMaster 
                    WHERE Manufecturer = @Mill 
                      AND CompanyID = @CompanyID 
                      AND IsDeletedTransaction = 0
                )
                AND ISNULL(IsDeletedTransaction, 0) <> 1 
                ORDER BY Finish";
            
            result.Finishes = (await connection.QueryAsync<FinishDto>(finishQuery, new 
            { 
                CompanyID = companyId, 
                GSM = gsm, 
                Quality = quality, 
                Mill = mill 
            })).ToList();
        }

        return result;
    }
}
