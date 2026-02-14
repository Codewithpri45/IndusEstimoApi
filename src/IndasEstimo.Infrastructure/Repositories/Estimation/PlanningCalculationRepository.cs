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
/// Repository implementation for Planning and Calculation operations
/// </summary>
public class PlanningCalculationRepository : IPlanningCalculationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PlanningCalculationRepository> _logger;

    public PlanningCalculationRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<PlanningCalculationRepository> logger)
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

    public async Task<CalculateOperationResponse> CalculateOperationAsync(CalculateOperationRequest request)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // Get process details
        string processQuery = @"
            SELECT 
                ISNULL(Rate, 0) AS Rate,
                ISNULL(MinimumCharges, 0) AS MinimumCharges,
                ISNULL(TypeofCharges, 'Per Piece') AS TypeOfCharges
            FROM ProcessMaster
            WHERE ProcessID = @ProcessID
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var processInfo = await connection.QueryFirstOrDefaultAsync<CalculateOperationResponse>(
            processQuery, 
            new { ProcessID = request.ProcessID, CompanyID = companyId });

        if (processInfo == null)
        {
            return new CalculateOperationResponse
            {
                Rate = 0,
                Amount = 0,
                MinimumCharges = 0,
                TypeOfCharges = "Per Piece"
            };
        }

        // Check for slab rate if RateFactor is provided
        if (!string.IsNullOrEmpty(request.RateFactor))
        {
            string slabQuery = @"
                SELECT TOP 1 ISNULL(Rate, 0) AS Rate
                FROM ProcessMasterSlabs
                WHERE ProcessID = @ProcessID
                  AND RateFactor = @RateFactor
                  AND FromValue <= @Quantity
                  AND ToValue >= @Quantity
                  AND CompanyID = @CompanyID
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            var slabRate = await connection.QueryFirstOrDefaultAsync<decimal?>(
                slabQuery,
                new
                {
                    ProcessID = request.ProcessID,
                    RateFactor = request.RateFactor,
                    Quantity = request.Quantity,
                    CompanyID = companyId
                });

            if (slabRate.HasValue && slabRate.Value > 0)
            {
                processInfo.Rate = slabRate.Value;
            }
        }

        // Calculate amount based on charge type
        decimal amount = processInfo.TypeOfCharges switch
        {
            "Per Piece" => request.Quantity * processInfo.Rate,
            "Per 1000" => (request.Quantity / 1000) * processInfo.Rate,
            "Per Sqm" => (request.SizeL * request.SizeW * request.Quantity / 1000000) * processInfo.Rate,
            _ => request.Quantity * processInfo.Rate
        };

        // Apply minimum charges if applicable
        if (amount < processInfo.MinimumCharges)
        {
            amount = processInfo.MinimumCharges;
        }

        processInfo.Amount = Math.Round(amount, 2);
        return processInfo;
    }

    public async Task<List<ChargeTypeDto>> GetChargeTypesAsync()
    {
        using var connection = GetConnection();

        string query = @"
            SELECT DISTINCT TypeofCharges AS ChargeType
            FROM ProcessMaster
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
              AND ISNULL(TypeofCharges, '') <> ''
            ORDER BY TypeofCharges";

        var results = await connection.QueryAsync<ChargeTypeDto>(query);
        return results.ToList();
    }

    public async Task<MaterialFormulaDto?> GetMaterialFormulaAsync(long itemSubGroupId, long plantId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                MCFS.SettingID,
                MCFS.ItemSubGroupID,
                ISGM.ItemSubGroupName,
                ISNULL(MCFS.CostingFormula, '') AS CostingFormula,
                MCFS.FormulaParameters,
                ISNULL(MCFS.ApplyGSTOnEstimationRate, 0) AS ApplyGSTOnEstimationRate
            FROM MaterialGroupCostFormulaSetting AS MCFS
            LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID = MCFS.ItemSubGroupID
            WHERE MCFS.ItemSubGroupID = @ItemSubGroupID
              AND MCFS.PlantID = @PlantID
              AND MCFS.CompanyID = @CompanyID
              AND ISNULL(MCFS.IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryFirstOrDefaultAsync<MaterialFormulaDto>(
            query,
            new { ItemSubGroupID = itemSubGroupId, PlantID = plantId, CompanyID = companyId });

        return result;
    }

    public async Task<WastageSlabDto?> GetWastageSlabAsync(decimal actualSheets, string wastageType)
    {
        // WastageTypeSlab table does not exist. Returning null for now as logic seems machine-specific.
        return await Task.FromResult<WastageSlabDto?>(null);
    }

    public async Task<List<WastageTypeDto>> GetAllWastageTypesAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT DISTINCT WastageType
            FROM MachineMaster
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
              AND ISNULL(WastageType, '') <> ''
            ORDER BY WastageType";

        var results = await connection.QueryAsync<WastageTypeDto>(query, new { CompanyID = companyId });
        return results.ToList();
    }

    public async Task<KeylineCoordinatesDto?> GetKeylineCoordinatesAsync(string contentType, string? grain)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT TOP 1
                ContentType,
                Grain,
                ISNULL(TrimmingLeft, 0) AS TrimmingLeft,
                ISNULL(TrimmingRight, 0) AS TrimmingRight,
                ISNULL(TrimmingTop, 0) AS TrimmingTop,
                ISNULL(TrimmingBottom, 0) AS TrimmingBottom,
                ISNULL(StripingLeft, 0) AS StripingLeft,
                ISNULL(StripingRight, 0) AS StripingRight,
                ISNULL(StripingTop, 0) AS StripingTop,
                ISNULL(StripingBottom, 0) AS StripingBottom
            FROM KeylineCoordinatesMaster
            WHERE ContentType = @ContentType
              AND (@Grain IS NULL OR Grain = @Grain)
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryFirstOrDefaultAsync<KeylineCoordinatesDto>(
            query,
            new { ContentType = contentType, Grain = grain, CompanyID = companyId });

        return result;
    }

    public async Task<CorrugationPlanResponse> CalculateCorrugationAsync(CorrugationPlanRequest request)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // Get flute take-up factor
        string fluteQuery = @"
            SELECT ISNULL(TakeUpFactor, 1.5) AS TakeUpFactor
            FROM FluteMaster
            WHERE FluteName = @FluteType
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var takeUpFactor = await connection.QueryFirstOrDefaultAsync<decimal?>(
            fluteQuery,
            new { FluteType = request.FluteType, CompanyID = companyId }) ?? 1.5m;

        // Get material BF
        string materialQuery = @"
            SELECT ISNULL(BF, 0) AS BurstingFactor
            FROM ItemMaster
            WHERE Quality = @Quality
              AND GSM = @GSM
              AND Manufecturer = @Mill
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var bf = await connection.QueryFirstOrDefaultAsync<decimal?>(
            materialQuery,
            new
            {
                Quality = request.Quality,
                GSM = request.GSM,
                Mill = request.Mill,
                CompanyID = companyId
            }) ?? 0;

        // Calculate bursting strength: BS = BF × GSM (simplified formula)
        var calculatedGSM = request.GSM * takeUpFactor;
        var burstingStrength = bf * calculatedGSM / 100; // Approximate formula

        return new CorrugationPlanResponse
        {
            TakeUpFactor = takeUpFactor,
            BurstingFactor = bf,
            BurstingStrength = Math.Round(burstingStrength, 2),
            CalculatedGSM = Math.Round(calculatedGSM, 2)
        };
    }
}
