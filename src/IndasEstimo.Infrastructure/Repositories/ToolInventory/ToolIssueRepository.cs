using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Application.Interfaces.Repositories.ToolInventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Domain.Entities.ToolInventory;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;

namespace IndasEstimo.Infrastructure.Repositories.ToolInventory;

public class ToolIssueRepository : IToolIssueRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToolIssueRepository> _logger;

    public ToolIssueRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ToolIssueRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _tenantProvider = tenantProvider;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    // ==================== CRUD Operations ====================

    public async Task<(long TransactionID, string VoucherNo)> SaveToolIssueAsync(ToolIssue main, List<ToolIssueDetail> details)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? string.Empty;
            var prefix = main.VoucherPrefix ?? "TI";

            // VB SaveToolIssue (line 380)
            // 1. Generate voucher number
            var maxVoucherNo = await connection.QueryFirstOrDefaultAsync<long?>(
                $"SELECT ISNULL(MAX(MaxVoucherNo), 0) FROM ToolTransactionMain WHERE VoucherID = {main.VoucherID} AND VoucherPrefix = '{prefix}' AND CompanyID = {companyId} AND FYear = '{fYear}' AND Isnull(IsDeletedTransaction,0) = 0",
                null, transaction) ?? 0;

            maxVoucherNo++;
            main.MaxVoucherNo = maxVoucherNo;
            main.VoucherNo = $"{prefix}{maxVoucherNo:D6}";

            // 2. Insert Main record (ToolTransactionMain)
            var transactionId = await _dbOperations.InsertDataAsync("ToolTransactionMain", main, connection, transaction, "TransactionID");

            // 3. Insert Detail records (ToolTransactionDetail)
            await _dbOperations.InsertDataAsync("ToolTransactionDetail", details, connection, transaction, "TransactionDetailID", parentTransactionId: transactionId);

            await transaction.CommitAsync();
            return (transactionId, main.VoucherNo);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving tool issue");
            throw;
        }
    }

    public async Task<bool> DeleteToolIssueAsync(long transactionId, long parentTransactionId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var userId = _currentUserService.GetUserId() ?? 0;

            // VB DeleteToolIssue (lines 514-530)
            // Soft delete main table
            var deleteSql = @"
                UPDATE {0}
                SET DeletedBy = @DeletedBy, DeletedDate = GETDATE(), ModifiedDate = GETDATE(), IsDeletedTransaction = 1
                WHERE CompanyID = @CompanyID AND TransactionID = @TransactionID";

            var parameters = new { DeletedBy = userId, CompanyID = companyId, TransactionID = transactionId };

            await connection.ExecuteAsync(string.Format(deleteSql, "ToolTransactionMain"), parameters, transaction);
            await connection.ExecuteAsync(string.Format(deleteSql, "ToolTransactionDetail"), parameters, transaction);

            // Reset IsCompleted = 0 on parent transaction (line 530)
            var resetSql = @"
                UPDATE ToolTransactionDetail
                SET ModifiedBy = @ModifiedBy, CompletedBy = @CompletedBy, CompletedDate = GETDATE(),
                    ModifiedDate = GETDATE(), IsCompleted = 0
                WHERE CompanyID = @CompanyID AND TransactionID = @ParentTransactionID";

            await connection.ExecuteAsync(resetSql,
                new { ModifiedBy = userId, CompletedBy = userId, CompanyID = companyId, ParentTransactionID = parentTransactionId },
                transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting tool issue {TransactionID}", transactionId);
            throw;
        }
    }

    // ==================== Retrieve Operations ====================

    public async Task<List<ToolIssueVoucherDetailsDto>> GetIssueVoucherDetailsAsync(long transactionId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // VB GetIssueVoucherDetails (lines 340-341)
        var sql = @"
            SELECT TTM.TransactionID, TTM.VoucherID, TTM.MaxVoucherNo, TTM.VoucherNo,
                   REPLACE(CONVERT(nvarchar(30), TTM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                   TTM.FYear, TTM.JobBookingID, TTM.JobBookingJobCardContentsID,
                   TTM.DeliveryNoteNo,
                   REPLACE(CONVERT(nvarchar(30), TTM.DeliveryNoteDate, 106), ' ', '-') AS DeliveryNoteDate,
                   NULLIF(TTM.Narration, '') AS Narration,
                   TTD.ToolID, TM.ToolGroupID, NULLIF(TM.ToolCode, '') AS ToolCode,
                   NULLIF(TM.ToolName, '') AS ToolName, NULLIF(TM.ToolDescription, '') AS ToolDescription,
                   NULLIF(TGM.ToolGroupName, '') AS ToolGroupName,
                   TTD.IssueQuantity, NULLIF(TTD.BatchNo, '') AS BatchNo,
                   TTD.WarehouseID, NULLIF(WM.WarehouseName, '') AS WarehouseName,
                   TTD.FloorWarehouseID, NULLIF(WM.BinName, '') AS FloorWarehouseName,
                   NULLIF(TM.StockUnit, '') AS StockUnit,
                   TTM.ProductionUnitID, NULLIF(PU.ProductionUnitName, '') AS ProductionUnitName,
                   NULLIF(CM.CompanyName, '') AS CompanyName,
                   NULLIF(U.UserName, '') AS CreatedBy,
                   REPLACE(CONVERT(nvarchar(30), TTM.CreatedDate, 106), ' ', '-') AS CreatedDate
            FROM ToolTransactionMain AS TTM
            INNER JOIN ToolTransactionDetail AS TTD ON TTD.TransactionID = TTM.TransactionID
                AND TTD.CompanyID = TTM.CompanyID AND Isnull(TTD.IsDeletedTransaction,0) = 0
            INNER JOIN ToolMaster AS TM ON TM.ToolID = TTD.ToolID
                AND Isnull(TM.IsDeletedTransaction,0) = 0
            INNER JOIN ToolGroupMaster AS TGM ON TGM.ToolGroupID = TM.ToolGroupID
            INNER JOIN WarehouseMaster AS WM ON WM.WarehouseID = TTD.WarehouseID
            LEFT JOIN ProductionUnitMaster AS PU ON PU.ProductionUnitID = TTM.ProductionUnitID
            LEFT JOIN CompanyMaster AS CM ON CM.CompanyID = TTM.CompanyID
            LEFT JOIN UserMaster AS U ON U.UserID = TTM.CreatedBy
            WHERE TTM.TransactionID = @TransactionID
                AND TTM.CompanyID = @CompanyID
                AND Isnull(TTM.IsDeletedTransaction,0) = 0";

        var results = await connection.QueryAsync<ToolIssueVoucherDetailsDto>(sql,
            new { TransactionID = transactionId, CompanyID = companyId });

        return results.ToList();
    }

    // ==================== Helper/Lookup Operations ====================

    public async Task<string> GenerateIssueNoAsync(string prefix)
    {
        // VB GetIssueNO (line 146)
        var (voucherNo, _) = await _dbOperations.GenerateVoucherNoAsync(
            "ToolTransactionMain",
            -43, // VoucherID for Tool Issue
            prefix);

        return voucherNo;
    }

    public async Task<List<WarehouseDto>> GetWarehouseListAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // VB GetWarehouseList (line 174)
        var sql = @"
            SELECT DISTINCT ISNULL(WarehouseID, 0) AS WarehouseID,
                   NULLIF(WarehouseName, '') AS WarehouseName,
                   NULLIF(WarehouseCode, '') AS WarehouseCode
            FROM WarehouseMaster
            WHERE WarehouseName <> ''
                AND WarehouseName IS NOT NULL
                AND CompanyID = @CompanyID
                AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY WarehouseName";

        var results = await connection.QueryAsync<WarehouseDto>(sql, new { CompanyID = companyId });
        return results.ToList();
    }

    public async Task<List<BinDto>> GetBinsListAsync(string warehouseName)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // VB GetBinsList (line 198)
        var sql = @"
            SELECT DISTINCT NULLIF(BinName, '') AS FloorWarehouseName,
                   ISNULL(WarehouseID, 0) AS WarehouseID,
                   ISNULL(WarehouseID, 0) AS FloorWarehouseID
            FROM WarehouseMaster
            WHERE WarehouseName = @WarehouseName
                AND ISNULL(BinName, '') <> ''
                AND CompanyID = @CompanyID
                AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY FloorWarehouseName";

        var results = await connection.QueryAsync<BinDto>(sql,
            new { WarehouseName = warehouseName, CompanyID = companyId });

        return results.ToList();
    }

    public async Task<List<StockBatchWiseDto>> GetStockBatchWiseAsync(long jobBookingJobCardContentsId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // VB GetStockBatchWise (lines 308-310)
        var sql = @"
            SELECT DISTINCT
                   ITM.VoucherNo AS GRNNo,
                   REPLACE(CONVERT(nvarchar(30), ITM.VoucherDate, 106), ' ', '-') AS GRNDate,
                   ISNULL(0, 0) AS JobBookingID,
                   ISNULL(0, 0) AS JobBookingJobCardContentsID,
                   ISNULL(TM.ToolID, 0) AS ToolID,
                   ISNULL(TM.ToolGroupID, 0) AS ToolGroupID,
                   ISNULL(TS.ParentTransactionID, 0) AS ParentTransactionID,
                   NULLIF(TM.ToolCode, '') AS ToolCode,
                   NULLIF(TGM.ToolGroupName, '') AS ToolGroupName,
                   NULLIF(ISNULL(TM.ToolName, ''), '') AS ToolName,
                   NULLIF(ISNULL(TM.ToolDescription, ''), '') AS ToolDescription,
                   NULLIF(TM.StockUnit, '') AS StockUnit,
                   ISNULL(TM.PurchaseRate, 0) AS Rate,
                   NULLIF(TS.BatchNo, '') AS BatchNo,
                   ISNULL(TS.ItemID, 0) AS ItemID,
                   NULLIF(TS.JobCardFormNo, '') AS JobCardFormNo,
                   ISNULL(TS.WarehouseID, 0) AS WarehouseID,
                   NULLIF(WM.WarehouseName, '') AS WarehouseName,
                   NULLIF(WM.BinName, '') AS FloorWarehouseName,
                   ISNULL(TS.PhysicalStock, 0) AS PhysicalStock,
                   ISNULL(0, 0) AS BookedStock,
                   ISNULL(0, 0) AS AllocatedStock,
                   ISNULL(TS.PhysicalStock, 0) AS AvailableStock,
                   ISNULL(TM.CompanyID, 0) AS CompanyID
            FROM ToolMaster AS TM
            INNER JOIN ToolGroupMaster AS TGM ON TGM.ToolGroupID = TM.ToolGroupID
                AND ISNULL(TM.IsDeletedTransaction, 0) = 0
            INNER JOIN (
                SELECT TTD.JobBookingJobCardContentsID, TTD.ParentTransactionID, TTD.ToolID, TTD.CompanyID,
                       TTD.WarehouseID, TTD.BatchNo, ISNULL(TTD.ItemID, 0) AS ItemID,
                       NULLIF(TTD.JobCardFormNo, '') AS JobCardFormNo,
                       SUM(ISNULL(TTD.ReceiptQuantity, 0)) AS ReceiptQuantity,
                       SUM(ISNULL(TTD.IssueQuantity, 0)) AS IssueQuantity,
                       SUM(ISNULL(TTD.ReceiptQuantity, 0)) - SUM(ISNULL(TTD.IssueQuantity, 0)) AS PhysicalStock
                FROM ToolTransactionMain AS TTM
                INNER JOIN ToolTransactionDetail AS TTD ON TTD.TransactionID = TTM.TransactionID
                    AND TTD.CompanyID = TTM.CompanyID
                    AND ISNULL(TTM.IsDeletedTransaction, 0) = 0
                    AND TTM.VoucherID <> -115
                    AND (ISNULL(TTD.ReceiptQuantity, 0) > 0 OR ISNULL(TTD.IssueQuantity, 0) > 0)
                    AND ISNULL(TTD.IsDeletedTransaction, 0) = 0
                GROUP BY TTD.JobBookingJobCardContentsID, TTD.ParentTransactionID, TTD.ToolID,
                         TTD.CompanyID, TTD.WarehouseID, TTD.BatchNo, TTD.ItemID, TTD.JobCardFormNo
                HAVING (SUM(ISNULL(TTD.ReceiptQuantity, 0)) - SUM(ISNULL(TTD.IssueQuantity, 0)) > 0)
            ) AS TS ON TS.ToolID = TM.ToolID
            LEFT JOIN ToolTransactionMain AS ITM ON ITM.TransactionID = TS.ParentTransactionID
                AND ISNULL(ITM.IsDeletedTransaction, 0) = 0
            LEFT JOIN WarehouseMaster AS WM ON WM.WarehouseID = TS.WarehouseID
            LEFT JOIN JobBookingJobCardProcessToolAllocation AS JBTA ON JBTA.ToolID = TM.ToolID
                AND ISNULL(JBTA.IsDeletedTransaction, 0) = 0
            WHERE (ISNULL(TM.IsDeletedTransaction, 0) = 0)
                AND (ISNULL(TM.IsBlocked, 0) = 0)
                AND (TS.JobBookingJobCardContentsID = @JobBookingJobCardContentsID)";

        var results = await connection.QueryAsync<StockBatchWiseDto>(sql,
            new { JobBookingJobCardContentsID = jobBookingJobCardContentsId });

        return results.ToList();
    }

    public async Task<List<JobCardDto>> GetJobCardNoAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // VB GetJobCardNo (lines 560-561)
        var sql = @"
            SELECT DISTINCT
                   JC.JobBookingID,
                   NULLIF(JC.JobBookingNo, '') AS JobBookingNo,
                   ISNULL(0, 0) AS JobBookingJobCardContentsID,
                   '' AS JobCardContentNo,
                   '' AS ClientName,
                   '' AS JobDescription
            FROM JobBookingJobCard AS JC
            WHERE (JC.CompanyID = @CompanyID)
                AND (ISNULL(JC.IsDeletedTransaction, 0) = 0)
            ORDER BY JobBookingNo";

        var results = await connection.QueryAsync<JobCardDto>(sql, new { CompanyID = companyId });
        return results.ToList();
    }
}
