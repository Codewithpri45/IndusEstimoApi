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

        // Helper: RoundUp equivalent of VB's Math.Ceiling for division
        static decimal RoundUp(decimal value, int decimals)
        {
            if (decimals == 0)
                return Math.Ceiling(value);
            decimal multiplier = (decimal)Math.Pow(10, decimals);
            return Math.Ceiling(value * multiplier) / multiplier;
        }

        // Derive pages from legacy logic
        long pages = request.JobLeaves > 0 ? request.JobLeaves * 2 :
                     request.JobPages > 0 ? request.JobPages : 1;

        int stitch = request.Stitch > 0 ? request.Stitch : 1;
        int folds = request.Folds;
        if (folds == 0)
        {
            // Legacy derives folds from Ups
            folds = request.Ups switch
            {
                4 => 2, 8 => 3, 12 or 16 => 4, 32 => 5, 64 => 6, _ => 0
            };
        }

        long orderQty = request.OrderQuantity > 0 ? request.OrderQuantity : 
                        (request.Gbl_Order_Quantity.HasValue ? (long)request.Gbl_Order_Quantity.Value : 0);
        
        int noOfPass = request.NoOfPass;
        if (processInfo.TypeOfCharges == "Rate/Total KgOfJob/PerBoxWt" && noOfPass <= 1)
            noOfPass = 15;

        decimal pubSheets = request.PubSheets;
        decimal totalPaperKG = request.TotalPaperKG;
        int totalColors = request.NoOfColors;
        decimal sizeL = request.SizeL;
        decimal sizeW = request.SizeW;
        int totalUps = request.Ups;
        int sets = request.Sets > 0 ? request.Sets : 1;
        decimal jobL = request.JobL;
        decimal jobH = request.JobH;
        decimal jobW = request.JobW;
        long totalPlates = request.TotalPlates;
        decimal quantity = request.Quantity;
        long quantityPcs = request.QuantityPcs;
        int pagesPerSection = request.PagesPerSection > 0 ? request.PagesPerSection : 1;
        decimal noOfForms = request.NoOfForms;
        int frontColors = request.FrontColors;

        // Calculate amount based on charge type — ALL 60+ legacy formulas (Api_shiring_service.vb line 13078-13307)
        decimal amount = processInfo.TypeOfCharges switch
        {
            "Rate/Kg" => (totalPaperKG * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Total KgOfJob" => (totalPaperKG * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Total KgOfJob/PerBoxWt" => (Math.Round(totalPaperKG / noOfPass) * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Color" => (totalColors * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Color" => (totalColors * sizeL * sizeW * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Unit" => (sizeL * sizeW * processInfo.Rate * quantity) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Sheet" => (sizeL * sizeW * processInfo.Rate * pubSheets) + processInfo.SetupCharges,
            "Rate/Sq.Inch" => (sizeL * sizeW * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Sq.Cm" => (sizeL * sizeW * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Sq.Cm/Unit" => (sizeL * sizeW * processInfo.Rate * quantity) + processInfo.SetupCharges,
            "Rate/Sq.Cm/Ups" => (sizeL * sizeW * processInfo.Rate * totalUps) + processInfo.SetupCharges,
            "Rate/100 Sq.Cm/Sheet" => ((sizeL * sizeW) * pubSheets * (processInfo.Rate / 100)) + processInfo.SetupCharges,
            "Rate/100 Sq.Cm/Order Quantity" => ((sizeL * sizeW) * orderQty * (processInfo.Rate / 100)) + processInfo.SetupCharges,
            "Rate/100 Sq.Cm/Sheet Both Side" => ((sizeL * sizeW) * pubSheets * (processInfo.Rate / 100) * 2) + processInfo.SetupCharges,
            "Rate/Sq.Cm/Sheet" => (sizeL * sizeW * processInfo.Rate * pubSheets) + processInfo.SetupCharges,
            "Rate/100 Sq.Inch/Sheet" => ((sizeL * sizeW) * pubSheets * (processInfo.Rate / 100)) + processInfo.SetupCharges,
            "Rate/100 Sq.Inch/Sheet-1" => (processInfo.Rate > 0 ? (((sizeL * sizeW) / 100) / processInfo.Rate) * pubSheets : 0) + processInfo.SetupCharges,
            "Rate/100 Sq.Inch/Order Quantity-1" => (processInfo.Rate > 0 ? (((sizeL * sizeW) / 100) / processInfo.Rate) * pubSheets : 0) + processInfo.SetupCharges,
            "Rate/100 Sq.Inch/Sheet Both Side" => ((sizeL * sizeW) * pubSheets * (processInfo.Rate / 100) * 2) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Sheet Both Side" => ((sizeL * sizeW) * 2 * processInfo.Rate * pubSheets) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Sheet Both Side/Pass" => ((sizeL * sizeW) * 2 * processInfo.Rate * pubSheets * noOfPass) + processInfo.SetupCharges,
            "Rate/Unit" => (quantityPcs * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/1000 Units" => (RoundUp(quantity / 1000, 0) * 1000 * (processInfo.Rate / 1000)) + processInfo.SetupCharges,
            "Rate/Sheet" => (pubSheets * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/1000 Sheets" => (RoundUp(pubSheets / 1000, 0) * 1000 * (processInfo.Rate / 1000)) + processInfo.SetupCharges,
            "Rate/1000 Sheets Both Side" => (RoundUp(pubSheets / 1000, 0) * 1000 * (processInfo.Rate / 1000) * 2) + processInfo.SetupCharges,
            "Rate/Ups" => (totalUps * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Ups/Sheet" => (totalUps * pubSheets * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/(L+W+H)/Ups" => (totalUps * (jobL + jobW + jobH) * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Page" => (pages * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Job" => (1 * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Set" => (sets * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Plate" => (totalPlates * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Total Cuts/1000 Sheets" => ((request.UpsL + request.UpsH) * (processInfo.Rate / 1000) * RoundUp(pubSheets / 1000, 0) * 1000) + processInfo.SetupCharges,
            "Rate/Cut/Sheets" => ((request.UpsL + request.UpsH) * processInfo.Rate * RoundUp(pubSheets / 1000, 0) * 1000) + processInfo.SetupCharges,
            "Rate/Set/1000 Sheets" => (sets * (processInfo.Rate / 1000) * RoundUp(pubSheets / 1000, 0) * 1000) + processInfo.SetupCharges,
            "Rate/Page/Unit" => (pages * quantity * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Set/Unit" => (sets * quantity * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Total Cuts/Sheet" => ((request.UpsL + request.UpsH) * processInfo.Rate * RoundUp(pubSheets / 1000, 0) * 1000) + processInfo.SetupCharges,
            "Rate/Order Quantity" => (orderQty * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/1000 Order Quantity" => (orderQty * (processInfo.Rate / 1000)) + processInfo.SetupCharges,
            "Rate/Page/Order Quantity" => (pages * orderQty * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Set/Order Quantity" => (sets * orderQty * processInfo.Rate) + processInfo.SetupCharges,
            "Rate/Inch/Unit" => ((jobL / 25.4m) * processInfo.Rate * quantity) + processInfo.SetupCharges,
            "Rate/Inch/Order Quantity" => ((jobL / 25.4m) * processInfo.Rate * orderQty) + processInfo.SetupCharges,
            "Rate/Loop/Unit" => (stitch * processInfo.Rate * quantity) + processInfo.SetupCharges,
            "Rate/Loop/Order Quantity" => (stitch * processInfo.Rate * orderQty) + processInfo.SetupCharges,
            "Rate/Color/1000 Sheets" => (totalColors * (processInfo.Rate / 1000) * RoundUp(pubSheets / 1000, 0) * 1000) + processInfo.SetupCharges,
            "Rate/Color/1000 Sheets Both Side" => (totalColors * (processInfo.Rate / 1000) * RoundUp(pubSheets / 1000, 0) * 1000 * 2) + processInfo.SetupCharges,
            "Rate/Ups/1000 Sheets" => (totalUps * RoundUp(pubSheets / 1000, 0) * 1000 * (processInfo.Rate / 1000)) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Set" => ((sizeL * sizeW) * processInfo.Rate * sets) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Order Quantity" => (sizeL * sizeW * processInfo.Rate * orderQty) + processInfo.SetupCharges,
            "Rate/Color/Order Quantity" => (totalColors * processInfo.Rate * orderQty) + processInfo.SetupCharges,
            "Rate/Color/1000 Order Quantity" => (totalColors * (processInfo.Rate / 1000) * orderQty) + processInfo.SetupCharges,
            "Rate/Stitch/Unit" => (stitch * processInfo.Rate * quantity) + processInfo.SetupCharges,
            "Rate/Stitch/Order Quantity" => (stitch * processInfo.Rate * orderQty) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Color/Set" => (processInfo.Rate * (sizeL * sizeW) * frontColors * sets) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Color/Set/Order Quantity" => (processInfo.Rate * (sizeL * sizeW) * frontColors * sets * orderQty) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Color/Set/Unit" => (processInfo.Rate * (sizeL * sizeW) * frontColors * sets * quantity) + processInfo.SetupCharges,
            "Rate/Sq.Mtr/Sheet" => (processInfo.Rate * (sizeL * sizeW) * pubSheets) + processInfo.SetupCharges,
            "Rate/Meter" => (processInfo.Rate * pubSheets) + processInfo.SetupCharges,
            "Rate/Sq.Mtr/Unit" => (processInfo.Rate * (sizeL * sizeW) * quantity) + processInfo.SetupCharges,
            "Rate/Sq.Mtr/Order Quantity" => CalculateSqMeterOrderQty(processInfo.Rate, jobH, jobL, orderQty, processInfo.SetupCharges, request.ContentSizeInputUnit),
            "Rate/Fold/Form/Unit" => (folds > 0 
                ? (processInfo.Rate * (folds * RoundUp(sets / 2m, 0)) * quantity)
                : (processInfo.Rate * (Math.Round((decimal)totalUps / 2) * RoundUp(sets / 2m, 0)) * quantity)) + processInfo.SetupCharges,
            "Rate/Fold/Form/Order Quantity" => (folds > 0 
                ? (processInfo.Rate * (folds * RoundUp(sets / 2m, 0)) * orderQty)
                : (processInfo.Rate * (Math.Round((decimal)totalUps / 2) * RoundUp(sets / 2m, 0)) * orderQty)) + processInfo.SetupCharges,
            "Rate/Fold/Unit" => (folds > 0 
                ? (processInfo.Rate * folds * quantity)
                : (processInfo.Rate * Math.Round((decimal)totalUps / 2) * quantity)) + processInfo.SetupCharges,
            "Rate/Fold/Sheet" => (folds > 0 
                ? (processInfo.Rate * folds * pubSheets)
                : (processInfo.Rate * Math.Round((decimal)totalUps / 2) * pubSheets)) + processInfo.SetupCharges,
            "Rate/Fold/Order Quantity" => (folds > 0 
                ? (processInfo.Rate * folds * orderQty)
                : (processInfo.Rate * Math.Round((decimal)totalUps / 2) * orderQty)) + processInfo.SetupCharges,
            "Rate/SqureCM(Height*Spine)/Order Quantity" => CalculateSpineFormula(processInfo.Rate, jobH, request.BookSpine, pages, request.PaperGSM, orderQty, processInfo.SetupCharges),
            "Rate/Sq.Meter" => (processInfo.Rate * pubSheets) + processInfo.SetupCharges,
            "Rate/Sq.Meter/Coverage Percentage" => (processInfo.Rate * pubSheets) + processInfo.SetupCharges,
            "Rate/Sq.Meter/GSM/Coverage Percentage" => (processInfo.Rate * (pubSheets * 3 / 1000)) + processInfo.SetupCharges,
            "Rate/Section/1000 Order Quantity" => (Math.Round((decimal)pages / pagesPerSection, 2) * (processInfo.Rate / 1000) * orderQty) + processInfo.SetupCharges,
            "Rate/Form/Order Quantity" => (processInfo.Rate * noOfForms * orderQty) + processInfo.SetupCharges,
            "Rate/Zipper Running Meter" => (processInfo.Rate * pubSheets) + processInfo.SetupCharges,
            "Rate/Sq.Inch/Ply/Sheet" => (sizeL * sizeW * processInfo.Rate * pubSheets) + processInfo.SetupCharges,
            "Rate/Box" => sizeL > 0 ? (processInfo.Rate * sizeL) + processInfo.SetupCharges : 0,
            "Rate/Box/Quantity" => sizeL > 0 
                ? (processInfo.Rate * Math.Round((decimal)orderQty / sizeL)) + processInfo.SetupCharges 
                : (processInfo.Rate * 1) + processInfo.SetupCharges,
            "Rate/Packing Box Wt(Kg)" => (sizeL > 0 && totalPaperKG > 0) 
                ? (Math.Round(totalPaperKG / sizeL) * processInfo.Rate) + processInfo.SetupCharges : 0,
            // Legacy aliases
            "Per Piece" => (quantity * processInfo.Rate) + processInfo.SetupCharges,
            "Per 1000" => ((quantity / 1000) * processInfo.Rate) + processInfo.SetupCharges,
            "Per Sqm" => ((sizeL * sizeW * quantity / 1000000) * processInfo.Rate) + processInfo.SetupCharges,
            _ => (quantity * processInfo.Rate) + processInfo.SetupCharges
        };

        // Apply minimum charges if applicable
        if (amount < processInfo.MinimumCharges)
        {
            amount = processInfo.MinimumCharges;
        }

        processInfo.Amount = Math.Round(amount, 2);
        return processInfo;
    }

    /// <summary>
    /// Helper for Rate/Sq.Mtr/Order Quantity — handles unit conversion (MM/INCH/CM)
    /// </summary>
    private static decimal CalculateSqMeterOrderQty(decimal rate, decimal jobH, decimal jobL, long orderQty, decimal setupCharges, string? contentSizeInputUnit)
    {
        string unit = string.IsNullOrWhiteSpace(contentSizeInputUnit) ? "MM" : contentSizeInputUnit.Trim().ToUpper();
        decimal h = jobH, l = jobL;
        
        if (unit == "INCH") { h = Math.Round(h * 0.0254m, 2); l = Math.Round(l * 0.0254m, 2); }
        else if (unit == "MM") { h = Math.Round(h / 1000m, 2); l = Math.Round(l / 1000m, 2); }
        else if (unit == "CM") { h = Math.Round(h / 100m, 2); l = Math.Round(l / 100m, 2); }
        
        return (rate * h * l * orderQty) + setupCharges;
    }

    /// <summary>
    /// Helper for Rate/SqureCM(Height*Spine)/Order Quantity
    /// </summary>
    private static decimal CalculateSpineFormula(decimal rate, decimal jobH, decimal bookSpine, long pages, decimal paperGSM, long orderQty, decimal setupCharges)
    {
        decimal spine = bookSpine > 0 ? bookSpine : (pages * paperGSM / 2000m);
        return (rate * ((jobH / 10m) * (spine / 10m)) * orderQty) + setupCharges;
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
