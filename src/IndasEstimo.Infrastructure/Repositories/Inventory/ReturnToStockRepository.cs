using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories.Inventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using System.Data;
using System.Transactions;

namespace IndasEstimo.Infrastructure.Repositories.Inventory;

public class ReturnToStockRepository : IReturnToStockRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ReturnToStockRepository> _logger;

    public ReturnToStockRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ReturnToStockRepository> logger)
    {
        _tenantProvider = tenantProvider;
        _connectionFactory = connectionFactory;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    // ─── GetReturnNo ──────────────────────────────────────────────────────────

    public async Task<string> GetReturnNoAsync(string prefix)
    {
        var companyId = _currentUserService.GetCompanyId();
        var fYear = _currentUserService.GetFYear();

        var sql = @"
            SELECT ISNULL(MAX(ISNULL(MaxVoucherNo, 0)), 0) + 1
            FROM ItemTransactionMain
            WHERE IsDeletedTransaction = 0
              AND VoucherID = -25
              AND VoucherPrefix = @Prefix
              AND CompanyID = @CompanyID
              AND FYear = @FYear";

        using var connection = GetConnection();
        var maxNo = await connection.ExecuteScalarAsync<long>(sql, new
        {
            Prefix = prefix,
            CompanyID = companyId,
            FYear = fYear
        });

        return $"{prefix}{maxNo:D6}";
    }

    // ─── GetWarehouseList ────────────────────────────────────────────────────

    public async Task<List<WarehouseDto>> GetWarehouseListAsync()
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();

        var sql = @"
            SELECT DISTINCT WarehouseName AS Warehouse
            FROM WarehouseMaster
            WHERE IsDeletedTransaction = 0
              AND ISNULL(WarehouseName, '') <> ''
              AND ProductionUnitID = @ProductionUnitID
              AND ISNULL(IsFloorWarehouse, 0) = 0
            ORDER BY Warehouse";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<WarehouseDto>(sql, new { ProductionUnitID = productionUnitId });
        return result.ToList();
    }

    // ─── GetBinsList ──────────────────────────────────────────────────────────

    public async Task<List<BinDto>> GetBinsListAsync(string warehouseName)
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();

        var sql = @"
            SELECT DISTINCT
                BinName AS Bin,
                WarehouseID
            FROM WarehouseMaster
            WHERE IsDeletedTransaction = 0
              AND WarehouseName = @WarehouseName
              AND ISNULL(BinName, '') <> ''
              AND ProductionUnitID = @ProductionUnitID
            ORDER BY Bin";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<BinDto>(sql, new
        {
            WarehouseName = warehouseName,
            ProductionUnitID = productionUnitId
        });
        return result.ToList();
    }

    // ─── GetDestinationBinsList ───────────────────────────────────────────────

    public async Task<List<BinDto>> GetDestinationBinsListAsync(string warehouseName)
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();

        var sql = @"
            SELECT DISTINCT
                BinName AS Bin,
                WarehouseID
            FROM WarehouseMaster
            WHERE IsDeletedTransaction = 0
              AND WarehouseName = @WarehouseName
              AND ISNULL(BinName, '') <> ''
              AND ProductionUnitID = @ProductionUnitID
            ORDER BY Bin";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<BinDto>(sql, new
        {
            WarehouseName = warehouseName,
            ProductionUnitID = productionUnitId
        });
        return result.ToList();
    }

    // ─── GetMachinesByDepartment ───────────────────────────────────────────────

    public async Task<List<MachineDto>> GetMachinesByDepartmentAsync(int departmentId)
    {
        var companyId = _currentUserService.GetCompanyId();

        var sql = @"
            SELECT
                ISNULL(MachineID, 0) AS MachineID,
                NULLIF(MachineName, '') AS MachineName
            FROM MachineMaster
            WHERE DepartmentID = @DepartmentID
              AND CompanyID = @CompanyID
              AND IsDeletedTransaction = 0
            ORDER BY MachineName";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<MachineDto>(sql, new
        {
            DepartmentID = departmentId,
            CompanyID = companyId
        });
        return result.ToList();
    }

    // ─── GetReturnList ────────────────────────────────────────────────────────

    public async Task<List<ReturnListDto>> GetReturnListAsync(string fromDate, string toDate)
    {
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr();

        var sql = @"
            SELECT DISTINCT
                ISNULL(ITM.TransactionID, 0) AS TransactionID,
                ITM.FYear,
                ISNULL(ITM.MaxVoucherNo, 0) AS MaxVoucherNo,
                ISNULL(ITM.DepartmentID, 0) AS DepartmentID,
                ISNULL(ITD.ParentTransactionID, 0) AS GRNTransactionID,
                ISNULL(ITD.ItemID, 0) AS ItemID,
                ISNULL(ITD.IssueTransactionID, 0) AS IssueTransactionID,
                ISNULL(ITD.WarehouseID, 0) AS WarehouseID,
                ISNULL(IM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID,
                ISNULL(IM.ItemSubGroupID, 0) AS ItemSubGroupID,
                ISNULL(ITD.JobBookingID, 0) AS JobBookingID,
                ISNULL(ITD.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                NULLIF(ITM.VoucherNo, '') AS VoucherNo,
                REPLACE(CONVERT(VARCHAR(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                NULLIF(IT.VoucherNo, '') AS IssueVoucherNo,
                REPLACE(CONVERT(VARCHAR(13), IT.VoucherDate, 106), ' ', '-') AS IssueVoucherDate,
                NULLIF(JC.JobCardContentNo, '') AS JobCardNo,
                NULLIF(JJ.JobName, '') AS JobName,
                NULLIF(JC.PlanContName, '') AS ContentName,
                NULLIF(DM.DepartmentName, '') AS DepartmentName,
                NULLIF(IM.ItemCode, '') AS ItemCode,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                NULLIF(IM.ItemName, '') AS ItemName,
                NULLIF(IM.StockUnit, '') AS StockUnit,
                ISNULL(ITD.ReceiptQuantity, 0) AS ReturnQuantity,
                ISNULL(ITD.BatchID, 0) AS BatchID,
                NULLIF(ITD.BatchNo, '') AS BatchNo,
                NULLIF(IBD.SupplierBatchNo, '') AS SupplierBatchNo,
                NULLIF(IBD.MfgDate, '') AS MfgDate,
                NULLIF(IBD.ExpiryDate, '') AS ExpiryDate,
                NULLIF(WM.WarehouseName, '') AS Warehouse,
                NULLIF(WM.WarehouseName, '') AS Bin,
                UM.UserName AS CreatedBy,
                PUM.ProductionUnitID,
                PUM.ProductionUnitName,
                CM.CompanyName,
                CM.CompanyID
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD
                ON ITM.TransactionID = ITD.TransactionID
                AND ITM.CompanyID = ITD.CompanyID
            INNER JOIN ItemMaster AS IM
                ON IM.ItemID = ITD.ItemID
            INNER JOIN ProductionUnitMaster AS PUM
                ON PUM.ProductionUnitID = ITM.ProductionUnitID
            INNER JOIN CompanyMaster AS CM
                ON CM.CompanyID = PUM.CompanyID
            INNER JOIN ItemGroupMaster AS IGM
                ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN ItemTransactionMain AS IT
                ON IT.TransactionID = ITD.IssueTransactionID
                AND IT.VoucherID = -19
            INNER JOIN WarehouseMaster AS WM
                ON WM.WarehouseID = ITD.WarehouseID
            INNER JOIN UserMaster AS UM
                ON UM.UserID = ITM.UserID
            INNER JOIN ItemTransactionBatchDetail AS IBD
                ON IBD.BatchID = ITD.BatchID
                AND IBD.CompanyID = ITD.CompanyID
            LEFT JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID
                AND ISNULL(ISGM.IsDeletedTransaction, 0) <> 1
            LEFT JOIN DepartmentMaster AS DM
                ON DM.DepartmentID = ITM.DepartmentID
            LEFT JOIN JobBookingJobCardContents AS JC
                ON JC.JobBookingJobCardContentsID = ITD.JobBookingJobCardContentsID
                AND JC.CompanyID = ITD.CompanyID
            LEFT JOIN JobBookingJobCard AS JJ
                ON JJ.JobBookingID = JC.JobBookingID
                AND JJ.CompanyID = JC.CompanyID
            WHERE ITM.VoucherID = -25
              AND ITM.ProductionUnitID IN (" + productionUnitIdStr + @")
              AND ITM.VoucherDate BETWEEN @FromDate AND @ToDate
              AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
            ORDER BY FYear DESC, MaxVoucherNo DESC";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<ReturnListDto>(sql, new
        {
            FromDate = fromDate,
            ToDate = toDate
        });
        return result.ToList();
    }

    // ─── GetReturnDetails ──────────────────────────────────────────────────────

    public async Task<List<ReturnDetailDto>> GetReturnDetailsAsync(long transactionId)
    {
        var sql = @"
            SELECT
                ISNULL(ITM.TransactionID, 0) AS ReturnTransactionID,
                ISNULL(ITM.DepartmentID, 0) AS DepartmentID,
                ISNULL(ID.MachineID, 0) AS MachineID,
                ISNULL(ID.FloorWarehouseID, 0) AS FloorWarehouseID,
                ISNULL(ITD.ProcessID, 0) AS ProcessID,
                ISNULL(ITD.ParentTransactionID, 0) AS ParentTransactionID,
                ISNULL(ITD.ItemID, 0) AS ItemID,
                ISNULL(ITD.IssueTransactionID, 0) AS TransactionID,
                ISNULL(ITD.WarehouseID, 0) AS BinID,
                ISNULL(IM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID,
                ISNULL(IM.ItemSubGroupID, 0) AS ItemSubGroupID,
                ISNULL(ITD.JobBookingID, 0) AS JobBookingID,
                ISNULL(ITD.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                NULLIF(ITM.VoucherNo, '') AS ReturnVoucherNo,
                REPLACE(CONVERT(VARCHAR(13), ITM.VoucherDate, 106), ' ', '-') AS ReturnVoucherDate,
                NULLIF(IT.VoucherNo, '') AS VoucherNo,
                REPLACE(CONVERT(VARCHAR(13), IT.VoucherDate, 106), ' ', '-') AS VoucherDate,
                NULLIF(JC.JobCardContentNo, '') AS JobCardNo,
                NULLIF(JJ.JobName, '') AS JobName,
                NULLIF(JC.PlanContName, '') AS ContentName,
                NULLIF(DM.DepartmentName, '') AS DepartmentName,
                NULLIF(IM.ItemCode, '') AS ItemCode,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                NULLIF(IM.ItemName, '') AS ItemName,
                NULLIF(IM.StockUnit, '') AS StockUnit,
                ISNULL(ITD.ReceiptQuantity, 0) AS FloorStock,
                ISNULL(ITD.BatchID, 0) AS BatchID,
                NULLIF(ITD.BatchNo, '') AS BatchNo,
                NULLIF(IBD.SupplierBatchNo, '') AS SupplierBatchNo,
                NULLIF(IBD.MfgDate, '') AS MfgDate,
                NULLIF(IBD.ExpiryDate, '') AS ExpiryDate,
                NULLIF(WM.WarehouseName, '') AS WarehouseName,
                NULLIF(WM.WarehouseName, '') AS Bin,
                NULLIF(I.VoucherNo, '') AS GRNNo,
                REPLACE(CONVERT(VARCHAR(13), I.VoucherDate, 106), ' ', '-') AS GRNDate,
                0 AS ConsumeQuantity,
                MM.MachineName
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD
                ON ITM.TransactionID = ITD.TransactionID
                AND ITM.CompanyID = ITD.CompanyID
            INNER JOIN ItemMaster AS IM
                ON IM.ItemID = ITD.ItemID
            INNER JOIN ItemGroupMaster AS IGM
                ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN ItemTransactionMain AS IT
                ON IT.TransactionID = ITD.IssueTransactionID
                AND IT.VoucherID = -19
            INNER JOIN ItemTransactionDetail AS ID
                ON ID.TransactionID = ITD.IssueTransactionID
                AND ID.ItemID = ITD.ItemID
                AND ID.BatchID = ITD.BatchID
                AND ID.CompanyID = ITD.CompanyID
                AND ID.JobBookingJobCardContentsID = ITD.JobBookingJobCardContentsID
            INNER JOIN ItemTransactionMain AS I
                ON I.TransactionID = ITD.ParentTransactionID
            INNER JOIN WarehouseMaster AS WM
                ON WM.WarehouseID = ITD.WarehouseID
            INNER JOIN ItemTransactionBatchDetail AS IBD
                ON IBD.BatchID = ITD.BatchID
                AND IBD.CompanyID = ITD.CompanyID
            LEFT JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID
                AND ISNULL(ISGM.IsDeletedTransaction, 0) <> 1
            LEFT JOIN DepartmentMaster AS DM
                ON DM.DepartmentID = ITM.DepartmentID
            LEFT JOIN JobBookingJobCardContents AS JC
                ON JC.JobBookingJobCardContentsID = ITD.JobBookingJobCardContentsID
                AND JC.CompanyID = ITD.CompanyID
            LEFT JOIN JobBookingJobCard AS JJ
                ON JJ.JobBookingID = JC.JobBookingID
                AND JJ.CompanyID = JC.CompanyID
            LEFT JOIN MachineMaster AS MM
                ON MM.MachineID = ID.MachineID
            WHERE ITM.VoucherID = -25
              AND ITD.TransactionID = @TransactionID";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<ReturnDetailDto>(sql, new { TransactionID = transactionId });
        return result.ToList();
    }

    // ─── GetAvailableFloorStock ────────────────────────────────────────────────
    // Returns items issued to floor (VoucherID=-19) that still have FloorStock > 0
    // FloorStock = IssueQuantity - SUM(ConsumeQuantity + ReturnQuantity)

    public async Task<List<FloorStockDto>> GetAvailableFloorStockAsync(string issueType)
    {
        var companyId    = _currentUserService.GetCompanyId();
        var productionUnitId = _currentUserService.GetProductionUnitId();

        // Common consumed-stock subquery (Job issues — includes JobBookingJobCardContentsID in group)
        const string csJobSubquery = @"
            (SELECT
                ISNULL(ICD.IssueTransactionID, 0)          AS IssueTransactionID,
                ISNULL(ICD.CompanyID, 0)                   AS CompanyID,
                ISNULL(ICD.ItemID, 0)                      AS ItemID,
                ISNULL(ICD.ParentTransactionID, 0)         AS ParentTransactionID,
                ISNULL(ICD.DepartmentID, 0)                AS DepartmentID,
                ISNULL(ICD.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                ISNULL(ICD.BatchID, 0)                     AS BatchID,
                NULLIF(ICD.BatchNo, '')                    AS BatchNo,
                ROUND(SUM(ISNULL(ICD.ConsumeQuantity, 0)) + SUM(ISNULL(ICD.ReturnQuantity, 0)), 3) AS ConsumedStock
            FROM ItemConsumptionMain  AS ICM
            INNER JOIN ItemConsumptionDetail AS ICD
                ON ICM.ConsumptionTransactionID = ICD.ConsumptionTransactionID
               AND ICM.CompanyID = ICD.CompanyID
            WHERE ISNULL(ICD.IsDeletedTransaction, 0) = 0
              AND ICD.ProductionUnitID = @ProductionUnitID
            GROUP BY
                ISNULL(ICD.IssueTransactionID, 0),
                ISNULL(ICD.CompanyID, 0),
                ISNULL(ICD.ItemID, 0),
                ISNULL(ICD.ParentTransactionID, 0),
                ISNULL(ICD.DepartmentID, 0),
                ISNULL(ICD.JobBookingJobCardContentsID, 0),
                ISNULL(ICD.BatchID, 0),
                NULLIF(ICD.BatchNo, '')
            HAVING ROUND(SUM(ISNULL(ICD.ConsumeQuantity, 0)) + SUM(ISNULL(ICD.ReturnQuantity, 0)), 3) > 0
            ) AS CS";

        // Consumed-stock subquery for Non-Job issues — no JobBookingJobCardContentsID, uses 0
        const string csNonJobSubquery = @"
            (SELECT
                ISNULL(ICD.IssueTransactionID, 0) AS IssueTransactionID,
                ISNULL(ICD.CompanyID, 0)          AS CompanyID,
                ISNULL(ICD.ItemID, 0)             AS ItemID,
                ISNULL(ICD.ParentTransactionID, 0) AS ParentTransactionID,
                ISNULL(ICD.DepartmentID, 0)        AS DepartmentID,
                0                                  AS JobBookingJobCardContentsID,
                ISNULL(ICD.BatchID, 0)             AS BatchID,
                NULLIF(ICD.BatchNo, '')             AS BatchNo,
                ROUND(SUM(ISNULL(ICD.ConsumeQuantity, 0)) + SUM(ISNULL(ICD.ReturnQuantity, 0)), 3) AS ConsumedStock
            FROM ItemConsumptionMain  AS ICM
            INNER JOIN ItemConsumptionDetail AS ICD
                ON ICM.ConsumptionTransactionID = ICD.ConsumptionTransactionID
               AND ICM.CompanyID = ICD.CompanyID
            WHERE ISNULL(ICD.IsDeletedTransaction, 0) = 0
              AND ICD.ProductionUnitID = @ProductionUnitID
            GROUP BY
                ISNULL(ICD.IssueTransactionID, 0),
                ISNULL(ICD.CompanyID, 0),
                ISNULL(ICD.ItemID, 0),
                ISNULL(ICD.ParentTransactionID, 0),
                ISNULL(ICD.DepartmentID, 0),
                ISNULL(ICD.BatchID, 0),
                NULLIF(ICD.BatchNo, '')
            HAVING ROUND(SUM(ISNULL(ICD.ConsumeQuantity, 0)) + SUM(ISNULL(ICD.ReturnQuantity, 0)), 3) > 0
            ) AS CS";

        // Shared SELECT columns
        const string selectCols = @"
            SELECT DISTINCT
                ISNULL(ITD.JobBookingID, 0)                 AS JobBookingID,
                ISNULL(ITD.ParentTransactionID, 0)          AS ParentTransactionID,
                ISNULL(ITM.TransactionID, 0)                AS TransactionID,
                ISNULL(ITM.DepartmentID, 0)                 AS DepartmentID,
                ISNULL(ITD.FloorWarehouseID, 0)             AS FloorWarehouseID,
                ISNULL(ITD.JobBookingJobCardContentsID, 0)  AS JobBookingJobCardContentsID,
                ISNULL(ITD.MachineID, 0)                    AS MachineID,
                ISNULL(ITD.ItemID, 0)                       AS ItemID,
                ISNULL(IM.ItemGroupID, 0)                   AS ItemGroupID,
                ISNULL(IGM.ItemGroupNameID, 0)              AS ItemGroupNameID,
                ISNULL(IM.ItemSubGroupID, 0)                AS ItemSubGroupID,
                NULLIF(DM.DepartmentName, '')               AS DepartmentName,
                NULLIF(ITM.VoucherNo, '')                   AS VoucherNo,
                REPLACE(CONVERT(VARCHAR(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                NULLIF(IM.ItemCode, '')                     AS ItemCode,
                NULLIF(IGM.ItemGroupName, '')               AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '')           AS ItemSubGroupName,
                NULLIF(IM.ItemName, '')                     AS ItemName,
                NULLIF('', '')                              AS StockType,
                NULLIF('', '')                              AS StockCategory,
                NULLIF(IM.StockUnit, '')                    AS StockUnit,
                ISNULL(ITD.BatchID, 0)                      AS BatchID,
                NULLIF(ITD.BatchNo, '')                     AS BatchNo,
                NULLIF(IBD.SupplierBatchNo, '')             AS SupplierBatchNo,
                NULLIF(IBD.MfgDate, '')                     AS MfgDate,
                NULLIF(IBD.ExpiryDate, '')                  AS ExpiryDate,
                NULLIF(IT.VoucherNo, '')                    AS GRNNo,
                REPLACE(CONVERT(VARCHAR(13), IT.VoucherDate, 106), ' ', '-') AS GRNDate,
                NULLIF(JC.JobCardContentNo, '')             AS JobCardNo,
                NULLIF(JJ.JobName, '')                      AS JobName,
                NULLIF(JC.PlanContName, '')                 AS ContentName,
                ISNULL(ITD.IssueQuantity, 0)                AS IssueQuantity,
                ISNULL(CS.ConsumedStock, 0)                 AS ConsumeQuantity,
                ROUND(ISNULL(ITD.IssueQuantity, 0) - ISNULL(CS.ConsumedStock, 0), 3) AS FloorStock,
                NULLIF(WM.WarehouseName, '')                AS WarehouseName,
                NULLIF(WM.BinName, '')                      AS Bin,
                NULLIF(MM.MachineName, '')                  AS MachineName";

        // Shared FROM + JOINs
        const string fromJoins = @"
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD
                ON ITD.TransactionID = ITM.TransactionID AND ITD.CompanyID = ITM.CompanyID
            INNER JOIN ItemMaster AS IM
                ON IM.ItemID = ITD.ItemID
            INNER JOIN ItemGroupMaster AS IGM
                ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN ItemTransactionBatchDetail AS IBD
                ON IBD.BatchID = ITD.BatchID AND IBD.CompanyID = ITD.CompanyID
            LEFT OUTER JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID
               AND ISNULL(ISGM.IsDeletedTransaction, 0) <> 1
            LEFT OUTER JOIN DepartmentMaster AS DM
                ON DM.DepartmentID = ITM.DepartmentID
            LEFT OUTER JOIN JobBookingJobCardContents AS JC
                ON JC.JobBookingJobCardContentsID = ITD.JobBookingJobCardContentsID
               AND JC.CompanyID = ITD.CompanyID
            LEFT OUTER JOIN JobBookingJobCard AS JJ
                ON JJ.JobBookingID = JC.JobBookingID AND JJ.CompanyID = JC.CompanyID
            LEFT OUTER JOIN ItemTransactionMain AS IT
                ON IT.TransactionID = ITD.ParentTransactionID
            LEFT OUTER JOIN WarehouseMaster AS WM
                ON WM.WarehouseID = ITD.FloorWarehouseID
            LEFT OUTER JOIN MachineMaster AS MM
                ON MM.MachineId = ITD.MachineID";

        string sql;

        if (issueType == "JobIssueVouchers")
        {
            sql = $@"{selectCols}
                {fromJoins}
                LEFT OUTER JOIN {csJobSubquery}
                    ON CS.IssueTransactionID = ITM.TransactionID
                   AND CS.ItemID = ITD.ItemID
                   AND CS.ParentTransactionID = ITD.ParentTransactionID
                   AND CS.BatchID = ITD.BatchID
                   AND CS.CompanyID = ITD.CompanyID
                WHERE ITM.VoucherID = -19
                  AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
                  AND ITM.ProductionUnitID = @ProductionUnitID
                  AND ISNULL(ITD.JobBookingJobCardContentsID, 0) > 0
                  AND ROUND(ISNULL(ITD.IssueQuantity, 0) - ISNULL(CS.ConsumedStock, 0), 3) > 0
                ORDER BY ITM.TransactionID DESC";
        }
        else if (issueType == "NonJobIssueVouchers")
        {
            sql = $@"{selectCols}
                {fromJoins}
                LEFT OUTER JOIN {csNonJobSubquery}
                    ON CS.IssueTransactionID = ITM.TransactionID
                   AND ITD.IssueQuantity > 0
                   AND CS.ItemID = ITD.ItemID
                   AND CS.ParentTransactionID = ITD.ParentTransactionID
                   AND CS.BatchID = ITD.BatchID
                   AND CS.CompanyID = ITD.CompanyID
                WHERE ITM.VoucherID = -19
                  AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
                  AND ITM.ProductionUnitID = @ProductionUnitID
                  AND ISNULL(ITD.JobBookingJobCardContentsID, 0) = 0
                  AND ROUND(ISNULL(ITD.IssueQuantity, 0) - ISNULL(CS.ConsumedStock, 0), 3) > 0
                ORDER BY ITM.TransactionID DESC";
        }
        else // AllIssueVouchers = UNION ALL of both
        {
            var jobPart = $@"{selectCols}
                {fromJoins}
                LEFT OUTER JOIN {csJobSubquery}
                    ON CS.IssueTransactionID = ITM.TransactionID
                   AND CS.ItemID = ITD.ItemID
                   AND CS.ParentTransactionID = ITD.ParentTransactionID
                   AND CS.BatchID = ITD.BatchID
                   AND CS.CompanyID = ITD.CompanyID
                WHERE ITM.VoucherID = -19
                  AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
                  AND ITM.ProductionUnitID = @ProductionUnitID
                  AND ISNULL(ITD.JobBookingJobCardContentsID, 0) > 0
                  AND ROUND(ISNULL(ITD.IssueQuantity, 0) - ISNULL(CS.ConsumedStock, 0), 3) > 0";

            var nonJobPart = $@"{selectCols}
                {fromJoins}
                LEFT OUTER JOIN {csNonJobSubquery}
                    ON CS.IssueTransactionID = ITM.TransactionID
                   AND ITD.IssueQuantity > 0
                   AND CS.ItemID = ITD.ItemID
                   AND CS.ParentTransactionID = ITD.ParentTransactionID
                   AND CS.BatchID = ITD.BatchID
                   AND CS.CompanyID = ITD.CompanyID
                WHERE ITM.VoucherID = -19
                  AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
                  AND ITM.ProductionUnitID = @ProductionUnitID
                  AND ISNULL(ITD.JobBookingJobCardContentsID, 0) = 0
                  AND ROUND(ISNULL(ITD.IssueQuantity, 0) - ISNULL(CS.ConsumedStock, 0), 3) > 0";

            sql = $@"{jobPart}
                UNION ALL
                {nonJobPart}
                ORDER BY TransactionID DESC";
        }

        using var connection = GetConnection();
        var result = await connection.QueryAsync<FloorStockDto>(sql, new
        {
            ProductionUnitID = productionUnitId,
            CompanyID = companyId
        });
        return result.ToList();
    }

    // ─── SaveReturnData ────────────────────────────────────────────────────────

    public async Task<ReturnOperationResponseDto> SaveReturnDataAsync(SaveReturnDataRequest request)
    {
        var companyId = _currentUserService.GetCompanyId();
        var userId = _currentUserService.GetUserId();
        var fYear = _currentUserService.GetFYear();
        var productionUnitId = _currentUserService.GetProductionUnitId();

        // Generate voucher numbers
        var maxVoucherNo = await GetMaxVoucherNoAsync(request.Prefix);
        var voucherNo = $"{request.Prefix}{maxVoucherNo:D6}";

        var maxConsumptionNo = await GetMaxConsumptionVoucherNoAsync(request.Prefix);
        var consumptionVoucherNo = $"{request.Prefix}{maxConsumptionNo:D6}";

        long transactionId = 0;
        long consumptionTransactionId = 0;

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        using var connection = GetConnection();

        try
        {
            // Insert ItemTransactionMain
            var insertMainSql = @"
                INSERT INTO ItemTransactionMain (
                    VoucherID, VoucherPrefix, MaxVoucherNo, VoucherNo, VoucherDate,
                    DepartmentID, Narration, ProductionUnitID, CompanyID, FYear,
                    UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
                )
                VALUES (
                    -25, @Prefix, @MaxVoucherNo, @VoucherNo, @VoucherDate,
                    @DepartmentID, @Narration, @ProductionUnitID, @CompanyID, @FYear,
                    @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
                );
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            transactionId = await connection.ExecuteScalarAsync<long>(insertMainSql, new
            {
                Prefix = request.Prefix,
                MaxVoucherNo = maxVoucherNo,
                VoucherNo = voucherNo,
                VoucherDate = request.MainData.VoucherDate,
                DepartmentID = request.MainData.DepartmentID,
                Narration = request.MainData.Narration,
                ProductionUnitID = productionUnitId,
                CompanyID = companyId,
                FYear = fYear,
                UserID = userId
            });

            // Insert ItemTransactionDetail records
            foreach (var detail in request.DetailData)
            {
                var insertDetailSql = @"
                    INSERT INTO ItemTransactionDetail (
                        TransactionID, ItemID, ItemGroupID, ItemSubGroupID,
                        JobBookingID, JobBookingJobCardContentsID, ParentTransactionID,
                        IssueTransactionID, WarehouseID, FloorWarehouseID, BatchID, BatchNo,
                        ReceiptQuantity, StockUnit, MachineID, ProcessID,
                        ProductionUnitID, CompanyID, FYear,
                        UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
                    )
                    VALUES (
                        @TransactionID, @ItemID, @ItemGroupID, @ItemSubGroupID,
                        @JobBookingID, @JobBookingJobCardContentsID, @ParentTransactionID,
                        @IssueTransactionID, @WarehouseID, @FloorWarehouseID, @BatchID, @BatchNo,
                        @ReceiptQuantity, @StockUnit, @MachineID, @ProcessID,
                        @ProductionUnitID, @CompanyID, @FYear,
                        @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
                    );";

                await connection.ExecuteAsync(insertDetailSql, new
                {
                    TransactionID = transactionId,
                    ItemID = detail.ItemID,
                    ItemGroupID = detail.ItemGroupID,
                    ItemSubGroupID = detail.ItemSubGroupID,
                    JobBookingID = detail.JobBookingID,
                    JobBookingJobCardContentsID = detail.JobBookingJobCardContentsID,
                    ParentTransactionID = detail.ParentTransactionID,
                    IssueTransactionID = detail.IssueTransactionID,
                    WarehouseID = detail.WarehouseID,
                    FloorWarehouseID = detail.FloorWarehouseID,
                    BatchID = detail.BatchID,
                    BatchNo = detail.BatchNo,
                    ReceiptQuantity = detail.ReceiptQuantity,
                    StockUnit = detail.StockUnit,
                    MachineID = detail.MachineID,
                    ProcessID = detail.ProcessID,
                    ProductionUnitID = productionUnitId,
                    CompanyID = companyId,
                    FYear = fYear,
                    UserID = userId
                });
            }

            // Insert ItemConsumptionMain
            var insertConsumptionMainSql = @"
                INSERT INTO ItemConsumptionMain (
                    VoucherID, VoucherPrefix, MaxVoucherNo, VoucherNo, VoucherDate,
                    ReturnTransactionID, DepartmentID, ProductionUnitID, CompanyID, FYear,
                    UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
                )
                VALUES (
                    -53, @Prefix, @MaxVoucherNo, @VoucherNo, @VoucherDate,
                    @ReturnTransactionID, @DepartmentID, @ProductionUnitID, @CompanyID, @FYear,
                    @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
                );
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            consumptionTransactionId = await connection.ExecuteScalarAsync<long>(insertConsumptionMainSql, new
            {
                Prefix = request.Prefix,
                MaxVoucherNo = maxConsumptionNo,
                VoucherNo = consumptionVoucherNo,
                VoucherDate = request.ConsumeMainData.VoucherDate,
                ReturnTransactionID = transactionId,
                DepartmentID = request.ConsumeMainData.DepartmentID,
                ProductionUnitID = productionUnitId,
                CompanyID = companyId,
                FYear = fYear,
                UserID = userId
            });

            // Insert ItemConsumptionDetail records
            foreach (var detail in request.ConsumeDetailData)
            {
                var insertConsumptionDetailSql = @"
                    INSERT INTO ItemConsumptionDetail (
                        ConsumptionTransactionID, IssueTransactionID, ItemID,
                        JobBookingJobCardContentsID, ConsumedQuantity, StockUnit,
                        BatchID, BatchNo, ProductionUnitID, CompanyID, FYear,
                        UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
                    )
                    VALUES (
                        @ConsumptionTransactionID, 0, @ItemID,
                        @JobBookingJobCardContentsID, @ConsumedQuantity, @StockUnit,
                        @BatchID, @BatchNo, @ProductionUnitID, @CompanyID, @FYear,
                        @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
                    );";

                await connection.ExecuteAsync(insertConsumptionDetailSql, new
                {
                    ConsumptionTransactionID = consumptionTransactionId,
                    ItemID = detail.ItemID,
                    JobBookingJobCardContentsID = detail.JobBookingJobCardContentsID,
                    ConsumedQuantity = detail.ConsumedQuantity,
                    StockUnit = detail.StockUnit,
                    BatchID = detail.BatchID,
                    BatchNo = detail.BatchNo,
                    ProductionUnitID = productionUnitId,
                    CompanyID = companyId,
                    FYear = fYear,
                    UserID = userId
                });
            }

            // Execute stock update stored procedure
            await connection.ExecuteAsync(
                "EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, @TransactionID, 0",
                new { CompanyID = companyId, TransactionID = transactionId });

            scope.Complete();

            return new ReturnOperationResponseDto
            {
                Success = true,
                Message = "Return saved successfully",
                ReturnNo = voucherNo,
                TransactionId = transactionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving return to stock data");
            return new ReturnOperationResponseDto
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    // ─── UpdateReturn ──────────────────────────────────────────────────────────

    public async Task UpdateReturnAsync(UpdateReturnDataRequest request)
    {
        var companyId = _currentUserService.GetCompanyId();
        var userId = _currentUserService.GetUserId();
        var fYear = _currentUserService.GetFYear();
        var productionUnitId = _currentUserService.GetProductionUnitId();

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        using var connection = GetConnection();

        try
        {
            // Update ItemTransactionMain
            var updateMainSql = @"
                UPDATE ItemTransactionMain
                SET VoucherDate = @VoucherDate,
                    DepartmentID = @DepartmentID,
                    Narration = @Narration,
                    ModifiedBy = @UserID,
                    ModifiedDate = GETDATE()
                WHERE TransactionID = @TransactionID
                  AND CompanyID = @CompanyID";

            await connection.ExecuteAsync(updateMainSql, new
            {
                TransactionID = request.TransactionID,
                VoucherDate = request.MainData.VoucherDate,
                DepartmentID = request.MainData.DepartmentID,
                Narration = request.MainData.Narration,
                CompanyID = companyId,
                UserID = userId
            });

            // Delete existing details
            await connection.ExecuteAsync(
                "DELETE FROM ItemTransactionDetail WHERE CompanyID = @CompanyID AND TransactionID = @TransactionID",
                new { CompanyID = companyId, TransactionID = request.TransactionID });

            // Insert new details
            foreach (var detail in request.DetailData)
            {
                var insertDetailSql = @"
                    INSERT INTO ItemTransactionDetail (
                        TransactionID, ItemID, ItemGroupID, ItemSubGroupID,
                        JobBookingID, JobBookingJobCardContentsID, ParentTransactionID,
                        IssueTransactionID, WarehouseID, FloorWarehouseID, BatchID, BatchNo,
                        ReceiptQuantity, StockUnit, MachineID, ProcessID,
                        ProductionUnitID, CompanyID, FYear,
                        UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
                    )
                    VALUES (
                        @TransactionID, @ItemID, @ItemGroupID, @ItemSubGroupID,
                        @JobBookingID, @JobBookingJobCardContentsID, @ParentTransactionID,
                        @IssueTransactionID, @WarehouseID, @FloorWarehouseID, @BatchID, @BatchNo,
                        @ReceiptQuantity, @StockUnit, @MachineID, @ProcessID,
                        @ProductionUnitID, @CompanyID, @FYear,
                        @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
                    );";

                await connection.ExecuteAsync(insertDetailSql, new
                {
                    TransactionID = request.TransactionID,
                    ItemID = detail.ItemID,
                    ItemGroupID = detail.ItemGroupID,
                    ItemSubGroupID = detail.ItemSubGroupID,
                    JobBookingID = detail.JobBookingID,
                    JobBookingJobCardContentsID = detail.JobBookingJobCardContentsID,
                    ParentTransactionID = detail.ParentTransactionID,
                    IssueTransactionID = detail.IssueTransactionID,
                    WarehouseID = detail.WarehouseID,
                    FloorWarehouseID = detail.FloorWarehouseID,
                    BatchID = detail.BatchID,
                    BatchNo = detail.BatchNo,
                    ReceiptQuantity = detail.ReceiptQuantity,
                    StockUnit = detail.StockUnit,
                    MachineID = detail.MachineID,
                    ProcessID = detail.ProcessID,
                    ProductionUnitID = productionUnitId,
                    CompanyID = companyId,
                    FYear = fYear,
                    UserID = userId
                });
            }

            // Execute stock update
            await connection.ExecuteAsync(
                "EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, @TransactionID, 0",
                new { CompanyID = companyId, TransactionID = request.TransactionID });

            // Get consumption transaction ID
            var getConsumptionIdSql = @"
                SELECT TOP 1 ConsumptionTransactionID
                FROM ItemConsumptionMain
                WHERE ReturnTransactionID = @ReturnTransactionID
                  AND CompanyID = @CompanyID
                  AND IsDeletedTransaction = 0";

            var consumptionId = await connection.ExecuteScalarAsync<long?>(getConsumptionIdSql, new
            {
                ReturnTransactionID = request.TransactionID,
                CompanyID = companyId
            });

            if (consumptionId.HasValue)
            {
                // Update ItemConsumptionMain
                var updateConsumptionMainSql = @"
                    UPDATE ItemConsumptionMain
                    SET VoucherDate = @VoucherDate,
                        DepartmentID = @DepartmentID,
                        ModifiedBy = @UserID,
                        ModifiedDate = GETDATE()
                    WHERE ConsumptionTransactionID = @ConsumptionTransactionID
                      AND CompanyID = @CompanyID";

                await connection.ExecuteAsync(updateConsumptionMainSql, new
                {
                    ConsumptionTransactionID = consumptionId.Value,
                    VoucherDate = request.ConsumeMainData.VoucherDate,
                    DepartmentID = request.ConsumeMainData.DepartmentID,
                    CompanyID = companyId,
                    UserID = userId
                });

                // Delete existing consumption details
                await connection.ExecuteAsync(
                    "DELETE FROM ItemConsumptionDetail WHERE CompanyID = @CompanyID AND ConsumptionTransactionID = @ConsumptionTransactionID",
                    new { CompanyID = companyId, ConsumptionTransactionID = consumptionId.Value });

                // Insert new consumption details
                foreach (var detail in request.ConsumeDetailData)
                {
                    var insertConsumptionDetailSql = @"
                        INSERT INTO ItemConsumptionDetail (
                            ConsumptionTransactionID, IssueTransactionID, ItemID,
                            JobBookingJobCardContentsID, ConsumedQuantity, StockUnit,
                            BatchID, BatchNo, ProductionUnitID, CompanyID, FYear,
                            UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
                        )
                        VALUES (
                            @ConsumptionTransactionID, 0, @ItemID,
                            @JobBookingJobCardContentsID, @ConsumedQuantity, @StockUnit,
                            @BatchID, @BatchNo, @ProductionUnitID, @CompanyID, @FYear,
                            @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
                        );";

                    await connection.ExecuteAsync(insertConsumptionDetailSql, new
                    {
                        ConsumptionTransactionID = consumptionId.Value,
                        ItemID = detail.ItemID,
                        JobBookingJobCardContentsID = detail.JobBookingJobCardContentsID,
                        ConsumedQuantity = detail.ConsumedQuantity,
                        StockUnit = detail.StockUnit,
                        BatchID = detail.BatchID,
                        BatchNo = detail.BatchNo,
                        ProductionUnitID = productionUnitId,
                        CompanyID = companyId,
                        FYear = fYear,
                        UserID = userId
                    });
                }
            }

            scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating return to stock");
            throw;
        }
    }

    // ─── DeleteReturn ──────────────────────────────────────────────────────────

    public async Task DeleteReturnAsync(long transactionId)
    {
        var companyId = _currentUserService.GetCompanyId();
        var userId = _currentUserService.GetUserId();

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        using var connection = GetConnection();

        try
        {
            // Check if there are any subsequent transactions using this return
            var checkSql = @"
                SELECT ParentTransactionID
                FROM ItemTransactionDetail
                WHERE TransactionID > @TransactionID
                  AND ParentTransactionID IN (
                      SELECT ParentTransactionID
                      FROM ItemTransactionDetail
                      WHERE TransactionID = @TransactionID
                        AND ISNULL(IsDeletedTransaction, 0) = 0
                        AND CompanyID = @CompanyID
                  )
                  AND ItemID IN (
                      SELECT ItemID
                      FROM ItemTransactionDetail
                      WHERE TransactionID = @TransactionID
                        AND ISNULL(IsDeletedTransaction, 0) = 0
                        AND CompanyID = @CompanyID
                  )
                  AND BatchNo IN (
                      SELECT BatchNo
                      FROM ItemTransactionDetail
                      WHERE TransactionID = @TransactionID
                        AND ISNULL(IsDeletedTransaction, 0) = 0
                        AND CompanyID = @CompanyID
                  )
                  AND BatchID IN (
                      SELECT BatchID
                      FROM ItemTransactionDetail
                      WHERE TransactionID = @TransactionID
                        AND ISNULL(IsDeletedTransaction, 0) = 0
                        AND CompanyID = @CompanyID
                  )
                  AND ISNULL(IsDeletedTransaction, 0) = 0
                  AND CompanyID = @CompanyID";

            var existingTransactions = await connection.QueryAsync<long>(checkSql, new
            {
                TransactionID = transactionId,
                CompanyID = companyId
            });

            if (existingTransactions.Any())
            {
                throw new InvalidOperationException("Cannot delete: Subsequent transactions exist for these items");
            }

            // Soft delete ItemTransactionMain
            var deleteMainSql = @"
                UPDATE ItemTransactionMain
                SET DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE CompanyID = @CompanyID
                  AND TransactionID = @TransactionID";

            await connection.ExecuteAsync(deleteMainSql, new
            {
                UserID = userId,
                CompanyID = companyId,
                TransactionID = transactionId
            });

            // Soft delete ItemTransactionDetail
            var deleteDetailSql = @"
                UPDATE ItemTransactionDetail
                SET DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE CompanyID = @CompanyID
                  AND TransactionID = @TransactionID";

            await connection.ExecuteAsync(deleteDetailSql, new
            {
                UserID = userId,
                CompanyID = companyId,
                TransactionID = transactionId
            });

            // Get consumption transaction ID
            var getConsumptionIdSql = @"
                SELECT TOP 1 ConsumptionTransactionID
                FROM ItemConsumptionMain
                WHERE ReturnTransactionID = @ReturnTransactionID
                  AND CompanyID = @CompanyID
                  AND IsDeletedTransaction = 0";

            var consumptionId = await connection.ExecuteScalarAsync<long?>(getConsumptionIdSql, new
            {
                ReturnTransactionID = transactionId,
                CompanyID = companyId
            });

            if (consumptionId.HasValue)
            {
                // Soft delete ItemConsumptionMain
                var deleteConsumptionMainSql = @"
                    UPDATE ItemConsumptionMain
                    SET DeletedBy = @UserID,
                        DeletedDate = GETDATE(),
                        IsDeletedTransaction = 1
                    WHERE CompanyID = @CompanyID
                      AND ConsumptionTransactionID = @ConsumptionTransactionID";

                await connection.ExecuteAsync(deleteConsumptionMainSql, new
                {
                    UserID = userId,
                    CompanyID = companyId,
                    ConsumptionTransactionID = consumptionId.Value
                });

                // Soft delete ItemConsumptionDetail
                var deleteConsumptionDetailSql = @"
                    UPDATE ItemConsumptionDetail
                    SET DeletedBy = @UserID,
                        DeletedDate = GETDATE(),
                        IsDeletedTransaction = 1
                    WHERE CompanyID = @CompanyID
                      AND ConsumptionTransactionID = @ConsumptionTransactionID";

                await connection.ExecuteAsync(deleteConsumptionDetailSql, new
                {
                    UserID = userId,
                    CompanyID = companyId,
                    ConsumptionTransactionID = consumptionId.Value
                });
            }

            // Execute stock update
            await connection.ExecuteAsync(
                "EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, @TransactionID, 0",
                new { CompanyID = companyId, TransactionID = transactionId });

            scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting return to stock");
            throw;
        }
    }

    // ─── Helper Methods ────────────────────────────────────────────────────────

    private async Task<long> GetMaxVoucherNoAsync(string prefix)
    {
        var companyId = _currentUserService.GetCompanyId();
        var fYear = _currentUserService.GetFYear();

        var sql = @"
            SELECT ISNULL(MAX(ISNULL(MaxVoucherNo, 0)), 0) + 1
            FROM ItemTransactionMain
            WHERE VoucherPrefix = @Prefix
              AND FYear = @FYear
              AND CompanyID = @CompanyID
              AND IsDeletedTransaction = 0";

        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<long>(sql, new
        {
            Prefix = prefix,
            FYear = fYear,
            CompanyID = companyId
        });
    }

    private async Task<long> GetMaxConsumptionVoucherNoAsync(string prefix)
    {
        var companyId = _currentUserService.GetCompanyId();
        var fYear = _currentUserService.GetFYear();

        var sql = @"
            SELECT ISNULL(MAX(ISNULL(MaxVoucherNo, 0)), 0) + 1
            FROM ItemConsumptionMain
            WHERE VoucherPrefix = @Prefix
              AND FYear = @FYear
              AND CompanyID = @CompanyID
              AND IsDeletedTransaction = 0";

        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<long>(sql, new
        {
            Prefix = prefix,
            FYear = fYear,
            CompanyID = companyId
        });
    }
}
