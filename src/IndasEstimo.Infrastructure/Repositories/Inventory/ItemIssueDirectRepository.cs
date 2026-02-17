using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories.Inventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;

namespace IndasEstimo.Infrastructure.Repositories.Inventory;

public class ItemIssueDirectRepository : IItemIssueDirectRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ItemIssueDirectRepository> _logger;

    public ItemIssueDirectRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ItemIssueDirectRepository> logger)
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

    // ─── GetIssueNo ──────────────────────────────────────────────────────────

    public async Task<string> GetIssueNoAsync(string prefix)
    {
        var companyId = _currentUserService.GetCompanyId();
        var fYear = _currentUserService.GetFYear();

        var sql = @"
            SELECT ISNULL(MAX(ISNULL(MaxVoucherNo, 0)), 0) + 1
            FROM ItemTransactionMain
            WHERE IsDeletedTransaction = 0
              AND VoucherID = -19
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

    public async Task<List<FloorWarehouseDto>> GetWarehouseListAsync()
    {
        var companyId = _currentUserService.GetCompanyId();

        // MIN(WarehouseID) gives a stable fallback ID for warehouses that have no bins.
        // The frontend uses this WarehouseID directly as FloorWarehouseID when no bin is selected.
        // When bins exist, GetBinsListAsync returns the bin-specific WarehouseID to use instead.
        var sql = @"
            SELECT
                MIN(ISNULL(WarehouseID, 0)) AS WarehouseID,
                WarehouseName AS Warehouse
            FROM WarehouseMaster
            WHERE ISNULL(WarehouseName, '') <> ''
              AND IsDeletedTransaction = 0
              AND CompanyID = @CompanyID
            GROUP BY WarehouseName
            ORDER BY WarehouseName";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<FloorWarehouseDto>(sql, new { CompanyID = companyId });
        return result.ToList();
    }

    // ─── GetBinsList ──────────────────────────────────────────────────────────

    public async Task<List<FloorBinDto>> GetBinsListAsync(string warehouseName)
    {
        var companyId = _currentUserService.GetCompanyId();

        // Each WarehouseName+BinName combination has its own WarehouseID row.
        // The caller stores this WarehouseID as FloorWarehouseID in ItemTransactionDetail.
        var sql = @"
            SELECT DISTINCT
                ISNULL(WarehouseID, 0) AS WarehouseID,
                BinName AS Bin
            FROM WarehouseMaster
            WHERE WarehouseName = @WarehouseName
              AND ISNULL(BinName, '') <> ''
              AND IsDeletedTransaction = 0
              AND CompanyID = @CompanyID
            ORDER BY BinName";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<FloorBinDto>(sql, new
        {
            WarehouseName = warehouseName,
            CompanyID = companyId
        });
        return result.ToList();
    }

    // ─── GetJobCardRender ────────────────────────────────────────────────────

    public async Task<List<JobCardRenderDto>> GetJobCardRenderAsync()
    {
        var companyId = _currentUserService.GetCompanyId();

        var sql = "EXEC ItemIssueDirectJobCardRender @CompanyID";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<JobCardRenderDto>(sql, new { CompanyID = companyId });
        return result.ToList();
    }

    // ─── GetJobAllocatedPicklist ──────────────────────────────────────────────

    public async Task<List<JobAllocatedPicklistDto>> GetJobAllocatedPicklistAsync()
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();

        var sql = @"
            SELECT
                ISNULL(ITM.TransactionID, 0) AS PicklistTransactionID,
                ISNULL(IPR.PicklistReleaseTransactionID, 0) AS PicklistReleaseTransactionID,
                ISNULL(IPR.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                ISNULL(IPR.DepartmentID, 0) AS DepartmentID,
                ISNULL(ITD.MachineID, 0) AS MachineID,
                ISNULL(ITD.ProcessID, 0) AS ProcessID,
                ISNULL(IPR.ItemID, 0) AS ItemID,
                ISNULL(IM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID,
                ISNULL(IM.ItemSubGroupID, 0) AS ItemSubGroupID,
                ISNULL(ITM.VoucherNo, 0) AS PicklistNo,
                ISNULL(IPR.MaxReleaseNo, 0) AS ReleaseNo,
                NULLIF(JJ.JobBookingNo, '') AS BookingNo,
                NULLIF(JC.JobCardContentNo, '') AS JobCardNo,
                NULLIF(JJ.JobName, '') AS JobName,
                NULLIF(JC.PlanContName, '') AS ContentName,
                NULLIF(PM.ProcessName, '') AS ProcessName,
                NULLIF(DM.DepartmentName, '') AS Department,
                NULLIF(MM.MachineName, '') AS MachineName,
                NULLIF(IM.ItemCode, '') AS ItemCode,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                NULLIF(IM.ItemName, '') AS ItemName,
                NULLIF(IM.ItemDescription, '') AS ItemDescription,
                NULLIF(IM.StockUnit, '') AS StockUnit,
                ISNULL(IMS.PhysicalStock, 0) AS PhysicalStock,
                ISNULL(IMS.AllocatedStock, 0) AS AllocatedStock,
                ISNULL(IM.WtPerPacking, 0) AS WtPerPacking,
                ISNULL(IM.UnitPerPacking, 1) AS UnitPerPacking,
                ISNULL(IM.ConversionFactor, 1) AS ConversionFactor,
                ISNULL(IPR.ReleaseQuantity, 0) AS ReleaseQuantity,
                ISNULL((
                    SELECT SUM(ISNULL(IssueQuantity, 0))
                    FROM ItemTransactionMain AS A
                    INNER JOIN ItemTransactionDetail AS B
                        ON A.TransactionID = B.TransactionID
                        AND A.CompanyID = B.CompanyID
                        AND A.VoucherID = -19
                    WHERE ISNULL(B.IsDeletedTransaction, 0) = 0
                      AND B.PicklistReleaseTransactionID = IPR.PicklistReleaseTransactionID
                      AND B.ItemID = IPR.ItemID
                      AND B.JobBookingJobCardContentsID = IPR.JobBookingJobCardContentsID
                      AND A.DepartmentID = IPR.DepartmentID
                      AND B.CompanyID = IPR.CompanyID
                ), 0) AS IssueQuantity,
                (
                    ISNULL(IPR.ReleaseQuantity, 0) - ISNULL((
                        SELECT SUM(ISNULL(IssueQuantity, 0))
                        FROM ItemTransactionMain AS A
                        INNER JOIN ItemTransactionDetail AS B
                            ON A.TransactionID = B.TransactionID
                            AND A.CompanyID = B.CompanyID
                            AND A.VoucherID = -19
                            AND B.IsDeletedTransaction = 0
                        WHERE B.PicklistReleaseTransactionID = IPR.PicklistReleaseTransactionID
                          AND B.ItemID = IPR.ItemID
                          AND B.JobBookingJobCardContentsID = IPR.JobBookingJobCardContentsID
                          AND A.DepartmentID = IPR.DepartmentID
                          AND B.CompanyID = IPR.CompanyID
                    ), 0)
                ) AS PendingQuantity,
                ISNULL(IGM.AllowIssueExtraQuantity, 0) AS AllowIssueExtraQuantity
            FROM ItemPicklistReleaseDetail AS IPR
            INNER JOIN ItemTransactionMain AS ITM
                ON ITM.TransactionID = IPR.PicklistTransactionID AND ITM.CompanyID = IPR.CompanyID
            INNER JOIN ItemTransactionDetail AS ITD
                ON ITD.TransactionID = IPR.PicklistTransactionID
                AND ITD.TransactionDetailID = IPR.PicklistTransactionDetailID
                AND ITD.ItemID = IPR.ItemID
                AND ITD.JobBookingJobCardContentsID = IPR.JobBookingJobCardContentsID
                AND ITM.DepartmentID = IPR.DepartmentID
                AND ITD.CompanyID = IPR.CompanyID
            INNER JOIN ItemMaster AS IM ON IM.ItemID = IPR.ItemID
            INNER JOIN ItemMasterStock AS IMS ON IMS.ItemID = IM.ItemID AND IMS.ProductionUnitID = IPR.ProductionUnitID
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN DepartmentMaster AS DM ON DM.DepartmentID = IPR.DepartmentID
            LEFT JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0
            LEFT JOIN JobBookingJobCardContents AS JC
                ON JC.JobBookingJobCardContentsID = IPR.JobBookingJobCardContentsID AND JC.CompanyID = IPR.CompanyID
            LEFT JOIN JobBookingJobCard AS JJ
                ON JJ.JobBookingID = JC.JobBookingID AND JJ.CompanyID = JC.CompanyID
            LEFT JOIN MachineMaster AS MM ON MM.MachineID = ITD.MachineID
            LEFT JOIN ProcessMaster AS PM ON PM.ProcessID = ITD.ProcessID
            WHERE IPR.ProductionUnitID = @ProductionUnitID
              AND ISNULL(IPR.IsDeletedTransaction, 0) = 0
              AND ISNULL(ITD.IsCancelled, 0) = 0
              AND ISNULL(ITD.IsCompleted, 0) = 0
              AND ISNULL(ITD.IsDeletedTransaction, 0) = 0
              AND (
                    ISNULL(IPR.ReleaseQuantity, 0) - ISNULL((
                        SELECT SUM(ISNULL(IssueQuantity, 0))
                        FROM ItemTransactionMain AS A
                        INNER JOIN ItemTransactionDetail AS B
                            ON A.TransactionID = B.TransactionID
                            AND A.CompanyID = B.CompanyID
                            AND A.VoucherID = -19
                            AND B.IsDeletedTransaction = 0
                        WHERE B.PicklistReleaseTransactionID = IPR.PicklistReleaseTransactionID
                          AND B.ItemID = IPR.ItemID
                          AND B.JobBookingJobCardContentsID = IPR.JobBookingJobCardContentsID
                          AND A.DepartmentID = IPR.DepartmentID
                          AND B.CompanyID = IPR.CompanyID
                    ), 0)
              ) > 0
            ORDER BY IPR.PicklistReleaseTransactionID";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<JobAllocatedPicklistDto>(sql, new
        {
            ProductionUnitID = productionUnitId
        });
        return result.ToList();
    }

    // ─── GetAllPicklist ───────────────────────────────────────────────────────

    public async Task<List<AllPicklistDto>> GetAllPicklistAsync()
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();

        var sql = @"
            SELECT
                ISNULL(IPD.TransactionID, 0) AS TransactionID,
                ISNULL(IM.ItemID, 0) AS ItemID,
                ISNULL(IM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID,
                ISNULL(ISGM.ItemSubGroupID, 0) AS ItemSubGroupID,
                ISNULL(ITS.GRNTransactionID, 0) AS GRNTransactionID,
                ISNULL(ITS.WarehouseID, 0) AS WarehouseID,
                ISNULL(IPD.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                NULLIF(IPD.VoucherNo, '') AS Picklist_Order_No,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                NULLIF('', '') AS ItemCode,
                NULLIF(IM.ItemName, '') AS ItemName,
                NULLIF(IM.ItemDescription, '') AS ItemDescription,
                NULLIF(IM.StockUnit, '') AS StockUnit,
                ISNULL(ITS.ClosingQty, 0) AS BatchStock,
                ISNULL(IMS.PhysicalStock, 0) AS TotalPhysicalStock,
                ISNULL(IMS.AllocatedStock, 0) AS TotalAllocatedStock,
                NULLIF(ITS.GRNNo, '') AS GRNNo,
                REPLACE(CONVERT(varchar(13), ITS.GRNDate, 106), ' ', '-') AS GRNDate,
                NULLIF(ITS.BatchNo, '') AS BatchNo,
                NULLIF(ITS.WarehouseName, '') AS Warehouse,
                NULLIF(ITS.BinName, '') AS Bin,
                ISNULL(IM.WtPerPacking, 0) AS WtPerPacking,
                ISNULL(IM.UnitPerPacking, 1) AS UnitPerPacking,
                ISNULL(IM.ConversionFactor, 1) AS ConversionFactor,
                0 AS Issue_Qty,
                IPS.PendingQuantity
            FROM ItemMaster AS IM
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN ItemMasterStock AS IMS ON IMS.ItemID = IM.ItemID AND IMS.ProductionUnitID = @ProductionUnitID
            INNER JOIN (
                SELECT
                    ISNULL(ITD.CompanyID, 0) AS CompanyID,
                    ISNULL(ITD.ItemID, 0) AS ItemID,
                    ISNULL(ITD.ParentTransactionID, 0) AS GRNTransactionID,
                    ISNULL(SUM(ISNULL(ITD.ReceiptQuantity, 0)), 0) - ISNULL(SUM(ISNULL(ITD.IssueQuantity, 0)), 0) AS ClosingQty,
                    NULLIF(ITD.BatchNo, '') AS BatchNo,
                    ISNULL(ITD.WarehouseID, 0) AS WarehouseID,
                    NULLIF(WM.WarehouseName, '') AS WarehouseName,
                    NULLIF(WM.BinName, '') AS BinName,
                    NULLIF(IT.VoucherNo, '') AS GRNNo,
                    IT.VoucherDate AS GRNDate
                FROM ItemTransactionMain AS ITM
                INNER JOIN ItemTransactionDetail AS ITD
                    ON ITM.TransactionID = ITD.TransactionID AND ITM.CompanyID = ITD.CompanyID
                    AND ITM.VoucherID NOT IN (-8, -9, -11)
                INNER JOIN ItemTransactionMain AS IT
                    ON IT.TransactionID = ITD.ParentTransactionID AND IT.CompanyID = ITD.CompanyID
                INNER JOIN WarehouseMaster AS WM
                    ON WM.WarehouseID = ITD.WarehouseID AND WM.CompanyID = ITD.CompanyID
                WHERE ITM.ProductionUnitID = @ProductionUnitID
                  AND ISNULL(ITD.IsDeletedTransaction, 0) = 0
                  AND (ISNULL(ITD.ReceiptQuantity, 0) > 0 OR ISNULL(ITD.IssueQuantity, 0) > 0)
                GROUP BY
                    ISNULL(ITD.ItemID, 0), ISNULL(ITD.ParentTransactionID, 0),
                    NULLIF(ITD.BatchNo, ''), ISNULL(ITD.WarehouseID, 0),
                    NULLIF(WM.WarehouseName, ''), NULLIF(WM.BinName, ''),
                    NULLIF(IT.VoucherNo, ''), IT.VoucherDate, ISNULL(ITD.CompanyID, 0)
                HAVING (ISNULL(SUM(ISNULL(ITD.ReceiptQuantity, 0)), 0) - ISNULL(SUM(ISNULL(ITD.IssueQuantity, 0)), 0)) > 0
            ) AS ITS ON ITS.ItemID = IM.ItemID
            LEFT JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0
            INNER JOIN (
                SELECT DISTINCT ITPM.TransactionID, ITPD.IsReleased, ITPD.JobBookingJobCardContentsID,
                    ITPM.DepartmentID, ITPD.CompanyID, ITPD.ItemID, ITPD.BatchNo, ITPM.VoucherNo
                FROM ItemTransactionMain AS ITPM
                INNER JOIN ItemTransactionDetail AS ITPD
                    ON ITPM.TransactionID = ITPD.TransactionID AND ITPM.CompanyID = ITPD.CompanyID
                WHERE ITPM.VoucherID = -17
                  AND ITPM.ProductionUnitID = @ProductionUnitID
                  AND ITPM.DepartmentID = -50
                  AND ITPD.JobBookingJobCardContentsID = 0
            ) AS IPD ON IPD.ItemID = ITS.ItemID AND IPD.BatchNo = ITS.BatchNo AND IPD.CompanyID = ITS.CompanyID
            INNER JOIN (
                SELECT ITM.TransactionID, ITD.ItemID, ITD.CompanyID,
                    (SUM(ISNULL(ITD.RequiredQuantity, 0)) - ISNULL(IPS.IssueQuantity, 0)) AS PendingQuantity
                FROM ItemTransactionMain AS ITM
                INNER JOIN ItemTransactionDetail AS ITD
                    ON ITM.TransactionID = ITD.TransactionID AND ITM.CompanyID = ITD.CompanyID
                    AND ITM.VoucherID = -17
                LEFT JOIN (
                    SELECT ITD.PicklistTransactionID, ITD.ItemID, ITM.DepartmentID,
                        ITD.JobBookingJobCardContentsID, ITD.CompanyID,
                        ROUND(SUM(ISNULL(ITD.IssueQuantity, 0)), 2) AS IssueQuantity
                    FROM ItemTransactionMain AS ITM
                    INNER JOIN ItemTransactionDetail AS ITD
                        ON ITM.TransactionID = ITD.TransactionID AND ITM.CompanyID = ITD.CompanyID
                    WHERE ITM.VoucherID = -19
                      AND ISNULL(ITM.IsDeletedTransaction, 0) <> 1
                      AND ISNULL(ITD.JobBookingJobCardContentsID, 0) = 0
                      AND ISNULL(ITM.DepartmentID, 0) = -50
                    GROUP BY ITD.PicklistTransactionID, ITD.ItemID, ITM.DepartmentID,
                        ITD.JobBookingJobCardContentsID, ITD.CompanyID
                ) AS IPS ON IPS.PicklistTransactionID = ITD.TransactionID
                    AND IPS.ItemID = ITD.ItemID
                    AND IPS.DepartmentID = ITM.DepartmentID
                    AND IPS.JobBookingJobCardContentsID = ITD.JobBookingJobCardContentsID
                WHERE ISNULL(ITD.IsCancelled, 0) = 0
                  AND ISNULL(ITD.IsCompleted, 0) = 0
                  AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
                GROUP BY ITM.TransactionID, ITD.ItemID, ITD.CompanyID, IPS.IssueQuantity
            ) AS IPS ON IPS.TransactionID = IPD.TransactionID AND IPS.ItemID = IPD.ItemID
            WHERE ISNULL(IPD.IsReleased, 0) = 1
              AND IM.ItemName LIKE '%%'";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<AllPicklistDto>(sql, new
        {
            ProductionUnitID = productionUnitId
        });
        return result.ToList();
    }

    // ─── GetStockBatchWise ────────────────────────────────────────────────────

    public async Task<List<StockBatchWiseDto>> GetStockBatchWiseAsync(long itemId, long jobBookingJobCardContentsId)
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();

        var issueQtySubQuery = jobBookingJobCardContentsId > 0
            ? @"ISNULL((
                    SELECT ISNULL(SUM(ISNULL(IssueQuantity, 0)), 0)
                    FROM ItemTransactionDetail
                    WHERE ISNULL(IsDeletedTransaction, 0) = 0
                      AND JobBookingJobCardContentsID = @JobBookingJobCardContentsID
                    GROUP BY JobBookingJobCardContentsID
                ), 0)"
            : "0";

        var sql = $@"
            SELECT
                ISNULL(IM.ItemID, 0) AS ItemID,
                ISNULL(IM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID,
                ISNULL(ISGM.ItemSubGroupID, 0) AS ItemSubGroupID,
                ISNULL(Temp.ParentTransactionID, 0) AS ParentTransactionID,
                ISNULL(Temp.WarehouseID, 0) AS WarehouseID,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                NULLIF(IM.ItemCode, '') AS ItemCode,
                NULLIF(IM.ItemName, '') AS ItemName,
                NULLIF(IM.ItemDescription, '') AS ItemDescription,
                NULLIF(IM.StockUnit, '') AS StockUnit,
                ISNULL(Temp.ClosingQty, 0) AS BatchStock,
                NULLIF(Temp.GRNNo, '') AS GRNNo,
                REPLACE(CONVERT(varchar(13), Temp.GRNDate, 106), ' ', '-') AS GRNDate,
                NULLIF(Temp.BatchNo, '') AS BatchNo,
                NULLIF(Temp.BatchID, '') AS BatchID,
                ISNULL(Temp.IssueQTY, 0) AS IssueQuantity,
                NULLIF(Temp.WarehouseName, '') AS Warehouse,
                NULLIF(Temp.BinName, '') AS Bin,
                ISNULL(IM.WtPerPacking, 0) AS WtPerPacking,
                ISNULL(IM.UnitPerPacking, 1) AS UnitPerPacking,
                ISNULL(IM.ConversionFactor, 1) AS ConversionFactor
            FROM ItemMaster AS IM
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN (
                SELECT
                    ISNULL(IM.CompanyID, 0) AS CompanyID,
                    ISNULL(IM.ItemID, 0) AS ItemID,
                    ISNULL(ITD.BatchID, 0) AS BatchID,
                    ISNULL(ITD.WarehouseID, 0) AS WarehouseID,
                    ISNULL(ITD.ParentTransactionID, 0) AS ParentTransactionID,
                    ISNULL(SUM(ISNULL(ITD.ReceiptQuantity, 0)), 0) - ISNULL(SUM(ISNULL(ITD.IssueQuantity, 0)), 0) - ISNULL(SUM(ITD.RejectedQuantity), 0) AS ClosingQty,
                    {issueQtySubQuery} AS IssueQTY,
                    NULLIF(ITD.BatchNo, '') AS BatchNo,
                    NULLIF(WM.WarehouseName, '') AS WarehouseName,
                    NULLIF(WM.BinName, '') AS BinName,
                    NULLIF(IT.VoucherNo, '') AS GRNNo,
                    IT.VoucherDate AS GRNDate
                FROM ItemMaster AS IM
                INNER JOIN ItemTransactionDetail AS ITD
                    ON ITD.ItemID = IM.ItemID
                    AND ISNULL(ITD.IsDeletedTransaction, 0) = 0
                    AND (ISNULL(ITD.ReceiptQuantity, 0) > 0 OR ISNULL(ITD.IssueQuantity, 0) > 0)
                INNER JOIN ItemTransactionMain AS ITM
                    ON ITM.TransactionID = ITD.TransactionID AND ITM.CompanyID = ITD.CompanyID
                    AND ITM.VoucherID NOT IN (-8, -9, -11)
                    AND ISNULL(ITM.IsDeletedTransaction, 0) = 0
                    AND ISNULL(ITD.IsDeletedTransaction, 0) = 0
                INNER JOIN ItemTransactionMain AS IT
                    ON IT.TransactionID = ITD.ParentTransactionID AND ISNULL(IT.IsDeletedTransaction, 0) = 0
                INNER JOIN WarehouseMaster AS WM
                    ON WM.WarehouseID = ITD.WarehouseID AND ISNULL(WM.IsDeletedTransaction, 0) = 0
                WHERE ITD.ProductionUnitID = @ProductionUnitID
                  AND ITD.ItemID = @ItemId
                GROUP BY
                    ISNULL(ITD.BatchID, 0), ISNULL(IM.ItemID, 0), ISNULL(ITD.ParentTransactionID, 0),
                    NULLIF(ITD.BatchNo, ''), ISNULL(ITD.WarehouseID, 0),
                    NULLIF(WM.WarehouseName, ''), NULLIF(WM.BinName, ''),
                    NULLIF(IT.VoucherNo, ''), IT.VoucherDate, ISNULL(IM.CompanyID, 0)
                HAVING (
                    ISNULL(SUM(ISNULL(ITD.ReceiptQuantity, 0)), 0) -
                    ISNULL(SUM(ISNULL(ITD.IssueQuantity, 0)), 0) -
                    ISNULL(SUM(ITD.RejectedQuantity), 0)
                ) > 0
            ) AS Temp ON Temp.ItemID = IM.ItemID
            LEFT JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0
            WHERE IM.ItemID = @ItemId
            ORDER BY ParentTransactionID";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<StockBatchWiseDto>(sql, new
        {
            ItemId = itemId,
            JobBookingJobCardContentsID = jobBookingJobCardContentsId,
            ProductionUnitID = productionUnitId
        });
        return result.ToList();
    }

    // ─── GetIssueList ─────────────────────────────────────────────────────────

    public async Task<List<IssueListDto>> GetIssueListAsync(string fromDate, string toDate)
    {
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        var sql = $@"
            SELECT
                IPM.TransactionID,
                ITD.ItemID,
                IM.ItemGroupID,
                IGM.ItemGroupNameID,
                IM.ItemSubGroupID,
                ITD.WarehouseID,
                ITD.FloorWarehouseID,
                IPM.DepartmentID,
                ISNULL(ITD.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                ISNULL(ITD.PicklistReleaseTransactionID, 0) AS PicklistReleaseTransactionID,
                ISNULL(ITD.PicklistTransactionID, 0) AS PicklistTransactionID,
                IPM.MaxVoucherNo,
                NULLIF(IPM.VoucherNo, '') AS VoucherNo,
                REPLACE(CONVERT(Varchar(13), IPM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                NULLIF(IPM.VoucherNo, '') AS PicklistNo,
                NULLIF(DM.DepartmentName, '') AS DepartmentName,
                NULLIF(JEJC.JobCardContentNo, '') AS JobCardNo,
                NULLIF(JEJ.JobName, '') AS JobName,
                NULLIF(JEJC.PlanContName, '') AS ContentName,
                NULLIF(IM.ItemCode, '') AS ItemCode,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                IM.ItemName,
                LM.LedgerName,
                NULLIF(ITD.StockUnit, '') AS StockUnit,
                SUM(ISNULL(ITD.IssueQuantity, 0)) AS IssueQuantity,
                WM.WarehouseName AS Warehouse,
                WM.BinName AS Bin,
                NULLIF(IPM.DeliveryNoteNo, '') AS DeliveryNoteNo,
                NULLIF(UM.UserName, '') AS UserName,
                NULLIF(IPM.Narration, '') AS Narration,
                MM.MachineId,
                MM.MachineName,
                IPM.FYear,
                PUM.ProductionUnitID,
                PUM.ProductionUnitName,
                CM.CompanyName,
                CM.CompanyID
            FROM ItemTransactionMain AS IPM
            INNER JOIN ItemTransactionDetail AS ITD ON ITD.TransactionID = IPM.TransactionID
            INNER JOIN ItemMaster AS IM ON IM.ItemId = ITD.ItemID
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN UserMaster AS UM ON UM.UserID = IPM.CreatedBy
            LEFT JOIN DepartmentMaster AS DM ON DM.DepartmentID = IPM.DepartmentID
            LEFT JOIN JobBookingJobCardContents AS JEJC
                ON JEJC.JobBookingJobCardContentsID = ITD.JobBookingJobCardContentsID
                AND JEJC.CompanyID = ITD.CompanyID
            LEFT JOIN JobBookingJobCard AS JEJ
                ON JEJ.JobBookingID = JEJC.JobBookingID AND JEJ.CompanyID = JEJC.CompanyID
            LEFT JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0
            LEFT OUTER JOIN WarehouseMaster AS WM
                ON WM.WarehouseID = ITD.FloorWarehouseID AND WM.CompanyID = ITD.CompanyID
            LEFT OUTER JOIN LedgerMaster AS LM ON LM.LedgerID = JEJ.LedgerID
            LEFT JOIN MachineMaster AS MM ON MM.MachineId = ITD.MachineId
            INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = IPM.ProductionUnitID
            INNER JOIN CompanyMaster AS CM ON CM.CompanyID = PUM.CompanyID
            WHERE IPM.VoucherID = -19
              AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
              AND IPM.ProductionUnitID IN ({productionUnitIdStr})
              AND CAST(FLOOR(CAST(IPM.VoucherDate AS float)) AS datetime) >= @FromDate
              AND CAST(FLOOR(CAST(IPM.VoucherDate AS float)) AS datetime) <= @ToDate
            GROUP BY
                IPM.MaxVoucherNo, IPM.TransactionID, ITD.ItemID, LM.LedgerName,
                IM.ItemGroupID, IGM.ItemGroupNameID, IM.ItemSubGroupID,
                ITD.WarehouseID, ITD.FloorWarehouseID, IPM.DepartmentID,
                ITD.JobBookingJobCardContentsID, ITD.PicklistReleaseTransactionID, ITD.PicklistTransactionID,
                IPM.VoucherNo, IPM.VoucherDate, DM.DepartmentName, JEJC.JobCardContentNo,
                JEJ.JobName, JEJC.PlanContName, IM.ItemCode, IGM.ItemGroupName,
                ISGM.ItemSubGroupName, IM.ItemName, IM.ItemDescription, ITD.StockUnit,
                IPM.DeliveryNoteNo, UM.UserName, IPM.Narration,
                WM.WarehouseName, WM.BinName, MM.MachineId, MM.MachineName,
                IPM.FYear, PUM.ProductionUnitID, PUM.ProductionUnitName, CM.CompanyName, CM.CompanyID
            ORDER BY IPM.FYear DESC, MaxVoucherNo DESC";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<IssueListDto>(sql, new
        {
            FromDate = fromDate,
            ToDate = toDate
        });
        return result.ToList();
    }

    // ─── GetIssueVoucherDetails ───────────────────────────────────────────────

    public async Task<List<IssueVoucherDetailDto>> GetIssueVoucherDetailsAsync(long transactionId)
    {
        var sql = @"
            SELECT DISTINCT
                ISNULL(ITD.PicklistTransactionID, 0) AS PicklistTransactionID,
                ISNULL(ITD.TransID, 0) AS TransID,
                ISNULL(ITD.PicklistReleaseTransactionID, 0) AS PicklistReleaseTransactionID,
                ISNULL(ITD.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                ISNULL(ITM.DepartmentID, 0) AS DepartmentID,
                ISNULL(ITD.MachineID, 0) AS MachineID,
                ISNULL(ITD.ProcessID, 0) AS ProcessID,
                ISNULL(ITD.ParentTransactionID, 0) AS ParentTransactionID,
                ISNULL(ITD.ItemID, 0) AS ItemID,
                ISNULL(IM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID,
                ISNULL(IM.ItemSubGroupID, 0) AS ItemSubGroupID,
                ISNULL(ITD.WarehouseID, 0) AS WarehouseID,
                NULLIF(JJ.JobBookingNo, '') AS BookingNo,
                NULLIF(JC.JobCardContentNo, '') AS JobCardNo,
                NULLIF(JJ.JobName, '') AS JobName,
                NULLIF(JC.PlanContName, '') AS ContentName,
                NULLIF(PM.ProcessName, '') AS ProcessName,
                NULLIF(MM.MachineName, '') AS MachineName,
                NULLIF(DM.DepartmentName, '') AS DepartmentName,
                NULLIF(IM.ItemCode, '') AS ItemCode,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                NULLIF(IM.ItemName, '') AS ItemName,
                NULLIF(IM.ItemDescription, '') AS ItemDescription,
                NULLIF(ITD.StockUnit, '') AS StockUnit,
                0 AS BatchStock,
                ISNULL(ITD.IssueQuantity, 0) AS IssueQuantity,
                NULLIF(IT.VoucherNo, '') AS PicklistNo,
                REPLACE(CONVERT(Varchar(13), IT.VoucherDate, 106), ' ', '-') AS PicklistDate,
                ISNULL(ITD.BatchID, 0) AS BatchID,
                NULLIF(ITD.BatchNo, '') AS BatchNo,
                NULLIF(IBD.SupplierBatchNo, '') AS SupplierBatchNo,
                NULLIF(IBD.MfgDate, '') AS MfgDate,
                NULLIF(IBD.ExpiryDate, '') AS ExpiryDate,
                NULLIF(WM.WareHouseName, '') AS Warehouse,
                NULLIF(WM.BinName, '') AS Bin,
                ISNULL(IM.WtPerPacking, 0) AS WtPerPacking,
                ISNULL(IM.UnitPerPacking, 1) AS UnitPerPacking,
                ISNULL(IM.ConversionFactor, 1) AS ConversionFactor
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD
                ON ITD.TransactionID = ITM.TransactionID AND ITD.CompanyID = ITM.CompanyID
            INNER JOIN ItemTransactionMain AS IT
                ON IT.TransactionID = ITD.ParentTransactionID AND IT.CompanyID = ITD.CompanyID
            INNER JOIN ItemMaster AS IM ON IM.ItemID = ITD.ItemID
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN UserMaster AS UM ON UM.UserID = ITM.CreatedBy
            INNER JOIN WarehouseMaster AS WM ON WM.WarehouseID = ITD.WarehouseID
            LEFT JOIN DepartmentMaster AS DM ON DM.DepartmentID = ITM.DepartmentID
            INNER JOIN ItemTransactionBatchDetail AS IBD
                ON IBD.BatchID = ITD.BatchID AND IBD.CompanyID = ITD.CompanyID
            LEFT JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0
            LEFT JOIN JobBookingJobCardContents AS JC
                ON JC.JobBookingJobCardContentsID = ITD.JobBookingJobCardContentsID AND JC.CompanyID = ITD.CompanyID
            LEFT JOIN JobBookingJobCard AS JJ
                ON JJ.JobBookingID = JC.JobBookingID AND JJ.CompanyID = ITD.CompanyID
            LEFT JOIN ProcessMaster AS PM ON PM.ProcessID = ITD.ProcessID
            LEFT JOIN MachineMaster AS MM ON MM.MachineID = ITD.MachineID AND MM.CompanyID = ITD.CompanyID
            WHERE ITM.VoucherID = -19
              AND ITM.TransactionID = @TransactionID
              AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
            ORDER BY TransID";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<IssueVoucherDetailDto>(sql, new { TransactionID = transactionId });
        return result.ToList();
    }

    // ─── GetHeaderName ────────────────────────────────────────────────────────

    public async Task<List<IssueHeaderDto>> GetHeaderNameAsync(long transactionId)
    {
        var companyId = _currentUserService.GetCompanyId();
        var fYear = _currentUserService.GetFYear();

        var sql = @"
            SELECT
                ISNULL(ITM.TransactionID, 0) AS TransactionID,
                ISNULL(ITD.ItemID, 0) AS ItemID,
                ISNULL(IM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID,
                NULLIF(ITM.DeliveryNoteNo, '') AS DeliveryNoteNo,
                ISNULL(IM.ItemSubGroupID, 0) AS ItemSubGroupID,
                ISNULL(ITD.WarehouseID, 0) AS WarehouseID,
                ISNULL(ITD.FloorWarehouseID, 0) AS FloorWarehouseID,
                ISNULL(ITM.DepartmentID, 0) AS DepartmentID,
                ISNULL(ITD.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                NULLIF(ITM.MaxVoucherNo, '') AS MaxVoucherNo,
                NULLIF(ITM.VoucherNo, '') AS VoucherNo,
                REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                NULLIF(IPM.VoucherNo, '') AS PicklistNo,
                NULLIF(DM.DepartmentName, '') AS DepartmentName,
                NULLIF(JEJC.JobCardContentNo, '') AS JobCardNo,
                NULLIF(JEJ.JobName, '') AS JobName,
                NULLIF(JEJC.PlanContName, '') AS ContentName,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                NULLIF('', '') AS ItemCode,
                NULLIF(IM.ItemName, '') AS ItemName,
                NULLIF(IM.ItemDescription, '') AS ItemDescription,
                NULLIF(ITD.StockUnit, '') AS StockUnit,
                NULLIF(ITD.IssueQuantity, '') AS IssueQuantity,
                ISNULL(ITD.BatchID, 0) AS BatchID,
                NULLIF(ITD.BatchNo, '') AS BatchNo,
                NULLIF(IBD.SupplierBatchNo, '') AS SupplierBatchNo,
                NULLIF(WM.WareHouseName, '') AS Warehouse,
                NULLIF(WM.BinName, '') AS Bin,
                NULLIF(UM.UserName, '') AS UserName,
                NULLIF(ITM.Narration, '') AS Narration
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD
                ON ITM.TransactionID = ITD.TransactionID AND ITM.CompanyID = ITD.CompanyID
            INNER JOIN ItemTransactionMain AS IPM
                ON IPM.TransactionID = ITD.PicklistTransactionID AND IPM.CompanyID = ITD.CompanyID
            INNER JOIN ItemMaster AS IM ON IM.ItemID = ITD.ItemID AND IM.CompanyID = ITD.CompanyID
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID AND IGM.CompanyID = IM.CompanyID
            INNER JOIN ItemTransactionBatchDetail AS IBD
                ON IBD.BatchID = ITD.BatchID AND IBD.CompanyID = ITD.CompanyID
            INNER JOIN UserMaster AS UM ON UM.UserID = ITM.CreatedBy AND UM.CompanyID = ITM.CompanyID
            LEFT JOIN DepartmentMaster AS DM ON DM.DepartmentID = ITM.DepartmentID AND DM.CompanyID = ITM.CompanyID
            LEFT JOIN JobBookingJobCardContents AS JEJC
                ON JEJC.JobBookingJobCardContentsID = ITD.JobBookingJobCardContentsID AND JEJC.CompanyID = ITD.CompanyID
            LEFT JOIN JobBookingJobCard AS JEJ
                ON JEJ.JobBookingID = JEJC.JobBookingID AND JEJ.CompanyID = JEJC.CompanyID
            LEFT JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0
            LEFT JOIN WarehouseMaster AS WM ON WM.WarehouseID = ITD.WarehouseID AND WM.CompanyID = ITD.CompanyID
            WHERE ITM.VoucherID = -19
              AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
              AND ITM.TransactionID = @TransactionID
              AND ITM.CompanyID = @CompanyID
              AND ITM.FYear IN (@FYear)";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<IssueHeaderDto>(sql, new
        {
            TransactionID = transactionId,
            CompanyID = companyId,
            FYear = fYear
        });
        return result.ToList();
    }

    // ─── SaveIssueData ────────────────────────────────────────────────────────

    public async Task<IssueSaveResultDto> SaveIssueDataAsync(SaveIssueDataRequest request)
    {
        var companyId = _currentUserService.GetCompanyId();
        var userId = _currentUserService.GetUserId();
        var fYear = _currentUserService.GetFYear();
        var productionUnitId = _currentUserService.GetProductionUnitId();

        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Generate Issue voucher number
            var maxVoucherSql = @"
                SELECT ISNULL(MAX(ISNULL(MaxVoucherNo, 0)), 0) + 1
                FROM ItemTransactionMain
                WHERE VoucherPrefix = @Prefix
                  AND CompanyID = @CompanyID
                  AND FYear = @FYear";

            var maxVoucherNo = await connection.ExecuteScalarAsync<long>(maxVoucherSql, new
            {
                request.Prefix,
                CompanyID = companyId,
                FYear = fYear
            }, transaction);

            var voucherNo = $"{request.Prefix}{maxVoucherNo:D6}";

            // 2. Insert ItemTransactionMain
            var insertMainSql = @"
                INSERT INTO ItemTransactionMain (
                    VoucherID, VoucherPrefix, MaxVoucherNo, VoucherNo, VoucherDate,
                    DepartmentID, DeliveryNoteNo, Narration,
                    ProductionUnitID, CompanyID, FYear,
                    CreatedBy, ModifiedBy, CreatedDate, ModifiedDate
                )
                OUTPUT INSERTED.TransactionID
                VALUES (
                    -19, @Prefix, @MaxVoucherNo, @VoucherNo, @VoucherDate,
                    @DepartmentID, @DeliveryNoteNo, @Narration,
                    @ProductionUnitID, @CompanyID, @FYear,
                    @UserId, @UserId, GETDATE(), GETDATE()
                )";

            var transactionId = await connection.ExecuteScalarAsync<long>(insertMainSql, new
            {
                request.Prefix,
                MaxVoucherNo = maxVoucherNo,
                VoucherNo = voucherNo,
                request.MainData.VoucherDate,
                request.MainData.DepartmentID,
                request.MainData.DeliveryNoteNo,
                request.MainData.Narration,
                ProductionUnitID = productionUnitId,
                CompanyID = companyId,
                FYear = fYear,
                UserId = userId
            }, transaction);

            // 3. Insert ItemTransactionDetail rows
            var insertDetailSql = @"
                INSERT INTO ItemTransactionDetail (
                    TransactionID, ItemID, ItemGroupID, ItemSubGroupID,
                    JobBookingJobCardContentsID, PicklistTransactionID, PicklistReleaseTransactionID,
                    ParentTransactionID, WarehouseID, FloorWarehouseID,
                    BatchID, BatchNo, IssueQuantity, RequiredQuantity,
                    StockUnit, MachineID, ProcessID, DepartmentID,
                    ProductionUnitID, CompanyID, FYear,
                    CreatedBy, ModifiedBy, CreatedDate, ModifiedDate
                )
                VALUES (
                    @TransactionID, @ItemID, @ItemGroupID, @ItemSubGroupID,
                    @JobBookingJobCardContentsID, @PicklistTransactionID, @PicklistReleaseTransactionID,
                    @ParentTransactionID, @WarehouseID, @FloorWarehouseID,
                    @BatchID, @BatchNo, @IssueQuantity, @RequiredQuantity,
                    @StockUnit, @MachineID, @ProcessID, @DepartmentID,
                    @ProductionUnitID, @CompanyID, @FYear,
                    @UserId, @UserId, GETDATE(), GETDATE()
                )";

            foreach (var detail in request.DetailData)
            {
                await connection.ExecuteAsync(insertDetailSql, new
                {
                    TransactionID = transactionId,
                    detail.ItemID,
                    detail.ItemGroupID,
                    detail.ItemSubGroupID,
                    detail.JobBookingJobCardContentsID,
                    detail.PicklistTransactionID,
                    detail.PicklistReleaseTransactionID,
                    detail.ParentTransactionID,
                    detail.WarehouseID,
                    detail.FloorWarehouseID,
                    detail.BatchID,
                    detail.BatchNo,
                    detail.IssueQuantity,
                    detail.RequiredQuantity,
                    detail.StockUnit,
                    detail.MachineID,
                    detail.ProcessID,
                    detail.DepartmentID,
                    ProductionUnitID = productionUnitId,
                    CompanyID = companyId,
                    FYear = fYear,
                    UserId = userId
                }, transaction);
            }

            // 4. Generate Consumption voucher number (VoucherID = -53, prefix = "RFS")
            const string consumePrefix = "RFS";
            const int consumeVoucherId = -53;

            var maxConsumeVoucherSql = @"
                SELECT ISNULL(MAX(ISNULL(MaxVoucherNo, 0)), 0) + 1
                FROM ItemConsumptionMain
                WHERE VoucherID = @VoucherID
                  AND VoucherPrefix = @Prefix
                  AND CompanyID = @CompanyID
                  AND FYear = @FYear";

            var maxConsumeVoucherNo = await connection.ExecuteScalarAsync<long>(maxConsumeVoucherSql, new
            {
                VoucherID = consumeVoucherId,
                Prefix = consumePrefix,
                CompanyID = companyId,
                FYear = fYear
            }, transaction);

            var consumeVoucherNo = $"{consumePrefix}{maxConsumeVoucherNo:D6}";

            // 5. Insert ItemConsumptionMain
            var insertConsumeMainSql = @"
                INSERT INTO ItemConsumptionMain (
                    VoucherID, VoucherPrefix, MaxVoucherNo, VoucherNo, VoucherDate,
                    ReturnTransactionID, DepartmentID,
                    ProductionUnitID, CompanyID, FYear,
                    CreatedBy, ModifiedBy, CreatedDate, ModifiedDate
                )
                OUTPUT INSERTED.ConsumptionTransactionID
                VALUES (
                    @VoucherID, @Prefix, @MaxVoucherNo, @VoucherNo, @VoucherDate,
                    @ReturnTransactionID, @DepartmentID,
                    @ProductionUnitID, @CompanyID, @FYear,
                    @UserId, @UserId, GETDATE(), GETDATE()
                )";

            var consumptionTransactionId = await connection.ExecuteScalarAsync<long>(insertConsumeMainSql, new
            {
                VoucherID = consumeVoucherId,
                Prefix = consumePrefix,
                MaxVoucherNo = maxConsumeVoucherNo,
                VoucherNo = consumeVoucherNo,
                request.ConsumeMainData.VoucherDate,
                ReturnTransactionID = transactionId,
                request.ConsumeMainData.DepartmentID,
                ProductionUnitID = productionUnitId,
                CompanyID = companyId,
                FYear = fYear,
                UserId = userId
            }, transaction);

            // 6. Insert ItemConsumptionDetail rows
            var insertConsumeDetailSql = @"
                INSERT INTO ItemConsumptionDetail (
                    ConsumptionTransactionID, IssueTransactionID,
                    ItemID, JobBookingJobCardContentsID,
                    ConsumedQuantity, StockUnit, BatchID, BatchNo,
                    ProductionUnitID, CompanyID, FYear,
                    CreatedBy, ModifiedBy, CreatedDate, ModifiedDate
                )
                VALUES (
                    @ConsumptionTransactionID, @IssueTransactionID,
                    @ItemID, @JobBookingJobCardContentsID,
                    @ConsumedQuantity, @StockUnit, @BatchID, @BatchNo,
                    @ProductionUnitID, @CompanyID, @FYear,
                    @UserId, @UserId, GETDATE(), GETDATE()
                )";

            foreach (var consumeDetail in request.ConsumeDetailData)
            {
                await connection.ExecuteAsync(insertConsumeDetailSql, new
                {
                    ConsumptionTransactionID = consumptionTransactionId,
                    IssueTransactionID = transactionId,
                    consumeDetail.ItemID,
                    consumeDetail.JobBookingJobCardContentsID,
                    consumeDetail.ConsumedQuantity,
                    consumeDetail.StockUnit,
                    consumeDetail.BatchID,
                    consumeDetail.BatchNo,
                    ProductionUnitID = productionUnitId,
                    CompanyID = companyId,
                    FYear = fYear,
                    UserId = userId
                }, transaction);
            }

            transaction.Commit();

            // 7. Post-save: Update stock and auto-close picklist (outside transaction scope)
            await connection.ExecuteAsync(
                "EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, @TransactionID, 0",
                new { CompanyID = companyId, TransactionID = transactionId });

            await connection.ExecuteAsync(
                "EXEC AUTOCLOSE_PICKLIST_21052022 @TransactionID, @CompanyID, @UserID",
                new { TransactionID = transactionId, CompanyID = companyId, UserID = userId });

            return new IssueSaveResultDto
            {
                VoucherNo = voucherNo,
                TransactionID = transactionId
            };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // ─── UpdateIssue ──────────────────────────────────────────────────────────

    public async Task UpdateIssueAsync(UpdateIssueDataRequest request)
    {
        var companyId = _currentUserService.GetCompanyId();
        var userId = _currentUserService.GetUserId();
        var fYear = _currentUserService.GetFYear();
        var productionUnitId = _currentUserService.GetProductionUnitId();

        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Update ItemTransactionMain
            var updateMainSql = @"
                UPDATE ItemTransactionMain SET
                    VoucherDate     = @VoucherDate,
                    DepartmentID    = @DepartmentID,
                    DeliveryNoteNo  = @DeliveryNoteNo,
                    Narration       = @Narration,
                    ModifiedBy      = @UserId,
                    ModifiedDate    = GETDATE()
                WHERE CompanyID = @CompanyID AND TransactionID = @TransactionID";

            await connection.ExecuteAsync(updateMainSql, new
            {
                request.MainData.VoucherDate,
                request.MainData.DepartmentID,
                request.MainData.DeliveryNoteNo,
                request.MainData.Narration,
                UserId = userId,
                CompanyID = companyId,
                request.TransactionID
            }, transaction);

            // 2. Delete old details and re-insert
            await connection.ExecuteAsync(
                "DELETE FROM ItemTransactionDetail WHERE CompanyID = @CompanyID AND TransactionID = @TransactionID",
                new { CompanyID = companyId, TransactionID = request.TransactionID }, transaction);

            var insertDetailSql = @"
                INSERT INTO ItemTransactionDetail (
                    TransactionID, ItemID, ItemGroupID, ItemSubGroupID,
                    JobBookingJobCardContentsID, PicklistTransactionID, PicklistReleaseTransactionID,
                    ParentTransactionID, WarehouseID, FloorWarehouseID,
                    BatchID, BatchNo, IssueQuantity, RequiredQuantity,
                    StockUnit, MachineID, ProcessID, DepartmentID,
                    ProductionUnitID, CompanyID, FYear,
                    CreatedBy, ModifiedBy, CreatedDate, ModifiedDate
                )
                VALUES (
                    @TransactionID, @ItemID, @ItemGroupID, @ItemSubGroupID,
                    @JobBookingJobCardContentsID, @PicklistTransactionID, @PicklistReleaseTransactionID,
                    @ParentTransactionID, @WarehouseID, @FloorWarehouseID,
                    @BatchID, @BatchNo, @IssueQuantity, @RequiredQuantity,
                    @StockUnit, @MachineID, @ProcessID, @DepartmentID,
                    @ProductionUnitID, @CompanyID, @FYear,
                    @UserId, @UserId, GETDATE(), GETDATE()
                )";

            foreach (var detail in request.DetailData)
            {
                await connection.ExecuteAsync(insertDetailSql, new
                {
                    TransactionID = request.TransactionID,
                    detail.ItemID,
                    detail.ItemGroupID,
                    detail.ItemSubGroupID,
                    detail.JobBookingJobCardContentsID,
                    detail.PicklistTransactionID,
                    detail.PicklistReleaseTransactionID,
                    detail.ParentTransactionID,
                    detail.WarehouseID,
                    detail.FloorWarehouseID,
                    detail.BatchID,
                    detail.BatchNo,
                    detail.IssueQuantity,
                    detail.RequiredQuantity,
                    detail.StockUnit,
                    detail.MachineID,
                    detail.ProcessID,
                    detail.DepartmentID,
                    ProductionUnitID = productionUnitId,
                    CompanyID = companyId,
                    FYear = fYear,
                    UserId = userId
                }, transaction);
            }

            // 3. Update ItemConsumptionMain
            var updateConsumeMainSql = @"
                UPDATE ItemConsumptionMain SET
                    VoucherDate      = @VoucherDate,
                    DepartmentID     = @DepartmentID,
                    ModifiedBy       = @UserId,
                    ModifiedDate     = GETDATE(),
                    ProductionUnitID = @ProductionUnitID
                WHERE CompanyID = @CompanyID AND ReturnTransactionID = @TransactionID";

            await connection.ExecuteAsync(updateConsumeMainSql, new
            {
                request.ConsumeMainData.VoucherDate,
                request.ConsumeMainData.DepartmentID,
                UserId = userId,
                ProductionUnitID = productionUnitId,
                CompanyID = companyId,
                TransactionID = request.TransactionID
            }, transaction);

            // 4. Get ConsumptionTransactionID
            var getConsumptionIdSql = @"
                SELECT ConsumptionTransactionID
                FROM ItemConsumptionMain
                WHERE CompanyID = @CompanyID
                  AND ISNULL(IsDeletedTransaction, 0) <> 1
                  AND ReturnTransactionID = @TransactionID";

            var consumptionTransactionId = await connection.ExecuteScalarAsync<long>(getConsumptionIdSql, new
            {
                CompanyID = companyId,
                TransactionID = request.TransactionID
            }, transaction);

            // 5. Delete old consumption details and re-insert
            await connection.ExecuteAsync(
                "DELETE FROM ItemConsumptionDetail WHERE CompanyID = @CompanyID AND IssueTransactionID = @TransactionID",
                new { CompanyID = companyId, TransactionID = request.TransactionID }, transaction);

            var insertConsumeDetailSql = @"
                INSERT INTO ItemConsumptionDetail (
                    ConsumptionTransactionID, IssueTransactionID,
                    ItemID, JobBookingJobCardContentsID,
                    ConsumedQuantity, StockUnit, BatchID, BatchNo,
                    ProductionUnitID, CompanyID, FYear,
                    CreatedBy, ModifiedBy, CreatedDate, ModifiedDate
                )
                VALUES (
                    @ConsumptionTransactionID, @IssueTransactionID,
                    @ItemID, @JobBookingJobCardContentsID,
                    @ConsumedQuantity, @StockUnit, @BatchID, @BatchNo,
                    @ProductionUnitID, @CompanyID, @FYear,
                    @UserId, @UserId, GETDATE(), GETDATE()
                )";

            foreach (var consumeDetail in request.ConsumeDetailData)
            {
                await connection.ExecuteAsync(insertConsumeDetailSql, new
                {
                    ConsumptionTransactionID = consumptionTransactionId,
                    IssueTransactionID = request.TransactionID,
                    consumeDetail.ItemID,
                    consumeDetail.JobBookingJobCardContentsID,
                    consumeDetail.ConsumedQuantity,
                    consumeDetail.StockUnit,
                    consumeDetail.BatchID,
                    consumeDetail.BatchNo,
                    ProductionUnitID = productionUnitId,
                    CompanyID = companyId,
                    FYear = fYear,
                    UserId = userId
                }, transaction);
            }

            transaction.Commit();

            // 6. Post-update: Update stock and auto-close picklist (outside transaction scope)
            await connection.ExecuteAsync(
                "EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, @TransactionID, 0",
                new { CompanyID = companyId, TransactionID = request.TransactionID });

            await connection.ExecuteAsync(
                "EXEC AUTOCLOSE_PICKLIST_21052022 @TransactionID, @CompanyID, @UserID",
                new { TransactionID = request.TransactionID, CompanyID = companyId, UserID = userId });
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // ─── DeleteIssue ──────────────────────────────────────────────────────────

    public async Task DeleteIssueAsync(long transactionId, long jobBookingJobCardContentsId)
    {
        var companyId = _currentUserService.GetCompanyId();
        var userId = _currentUserService.GetUserId();

        using var connection = GetConnection();
        await connection.OpenAsync();

        // Soft-delete ItemTransactionMain
        await connection.ExecuteAsync(@"
            UPDATE ItemTransactionMain SET
                DeletedBy = @UserId, DeletedDate = GETDATE(), IsDeletedTransaction = 1
            WHERE CompanyID = @CompanyID AND TransactionID = @TransactionID",
            new { UserId = userId, CompanyID = companyId, TransactionID = transactionId });

        // Soft-delete ItemTransactionDetail
        await connection.ExecuteAsync(@"
            UPDATE ItemTransactionDetail SET
                DeletedBy = @UserId, DeletedDate = GETDATE(), IsDeletedTransaction = 1
            WHERE CompanyID = @CompanyID AND TransactionID = @TransactionID",
            new { UserId = userId, CompanyID = companyId, TransactionID = transactionId });

        // Soft-delete ItemConsumptionMain
        await connection.ExecuteAsync(@"
            UPDATE ItemConsumptionMain SET
                DeletedBy = @UserId, DeletedDate = GETDATE(), IsDeletedTransaction = 1
            WHERE CompanyID = @CompanyID AND ReturnTransactionID = @TransactionID",
            new { UserId = userId, CompanyID = companyId, TransactionID = transactionId });

        // Soft-delete ItemConsumptionDetail
        await connection.ExecuteAsync(@"
            UPDATE ItemConsumptionDetail SET
                ModifiedBy = @UserId, DeletedBy = @UserId,
                DeletedDate = GETDATE(), ModifiedDate = GETDATE(), IsDeletedTransaction = 1
            WHERE CompanyID = @CompanyID AND IssueTransactionID = @TransactionID",
            new { UserId = userId, CompanyID = companyId, TransactionID = transactionId });

        // Post-delete: Update stock and auto-close picklist
        await connection.ExecuteAsync(
            "EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, @TransactionID, 0",
            new { CompanyID = companyId, TransactionID = transactionId });

        await connection.ExecuteAsync(
            "EXEC AUTOCLOSE_PICKLIST_21052022 @TransactionID, @CompanyID, @UserID",
            new { TransactionID = transactionId, CompanyID = companyId, UserID = userId });
    }
}
