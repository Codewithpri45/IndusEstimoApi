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

public class ItemTransferBetweenWarehousesRepository : IItemTransferBetweenWarehousesRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ItemTransferBetweenWarehousesRepository> _logger;

    public ItemTransferBetweenWarehousesRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        ICurrentUserService currentUserService,
        ILogger<ItemTransferBetweenWarehousesRepository> logger)
    {
        _tenantProvider = tenantProvider;
        _connectionFactory = connectionFactory;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    // ─── GetTransferList ──────────────────────────────────────────────────────

    public async Task<List<TransferListDto>> GetTransferListAsync(string fromDate, string toDate)
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr();

        var sql = @"
            SELECT DISTINCT
                ITM.TransactionID,
                ITD.ParentTransactionID,
                ITM.VoucherID,
                ITD.ItemID,
                ITD.ItemGroupID,
                IM.ItemSubGroupID,
                IGM.ItemGroupNameID,
                ITD.WarehouseID,
                ITM.DestinationWarehouseID,
                ITM.MaxVoucherNo,
                ITM.VoucherNo,
                REPLACE(CONVERT(VARCHAR(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                (SELECT COUNT(TransactionDetailID)
                 FROM ItemTransactionDetail
                 WHERE TransactionID = ITM.TransactionID
                   AND CompanyID = ITM.CompanyID
                   AND ISNULL(IssueQuantity, 0) > 0
                   AND IsDeletedTransaction = 0) AS TotalTransferItems,
                IGM.ItemGroupName,
                ISGM.ItemSubGroupName,
                IM.ItemCode,
                IM.ItemName,
                ITD.StockUnit,
                ITD.BatchID,
                ITD.BatchNo,
                IBD.SupplierBatchNo,
                NULLIF(IBD.MfgDate, '') AS MfgDate,
                NULLIF(IBD.ExpiryDate, '') AS ExpiryDate,
                0 AS BatchStock,
                ITD.IssueQuantity AS TransferStock,
                IT.VoucherNo AS GRNNo,
                REPLACE(CONVERT(VARCHAR(13), IT.VoucherDate, 106), ' ', '-') AS GRNDate,
                NULLIF(WM.WarehouseName, '') AS Warehouse,
                NULLIF(WM.BinName, '') AS Bin,
                NULLIF(W.WarehouseName, '') AS DestinationWarehouse,
                NULLIF(W.BinName, '') AS DestinationBin,
                NULLIF(UM.UserName, '') AS CreatedBy,
                NULLIF(ITM.Narration, '') AS Narration,
                NULLIF(ITM.DeliveryNoteNo, '') AS DeliveryNoteNo,
                REPLACE(CONVERT(VARCHAR(13), ITM.DeliveryNoteDate, 106), ' ', '-') AS DeliveryNoteDate,
                ISNULL(IM.WtPerPacking, 0) AS WtPerPacking,
                IM.UnitPerPacking,
                IM.ConversionFactor,
                ITM.FYear,
                PUM.ProductionUnitID,
                PUM.ProductionUnitName,
                CM.CompanyName
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD
                ON ITD.TransactionID = ITM.TransactionID AND ITD.CompanyID = ITM.CompanyID
            INNER JOIN ItemMaster AS IM
                ON IM.ItemID = ITD.ItemID
            INNER JOIN ProductionUnitMaster AS PUM
                ON PUM.ProductionUnitID = ITM.ProductionUnitID
            INNER JOIN CompanyMaster AS CM
                ON CM.CompanyID = PUM.CompanyID
            INNER JOIN ItemGroupMaster AS IGM
                ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN WarehouseMaster AS WM
                ON WM.WarehouseID = ITD.WarehouseID
            INNER JOIN WarehouseMaster AS W
                ON W.WarehouseID = ITM.DestinationWarehouseID
            INNER JOIN ItemTransactionMain AS IT
                ON IT.TransactionID = ITD.ParentTransactionID
            INNER JOIN UserMaster AS UM
                ON UM.UserID = ITM.CreatedBy
            INNER JOIN ItemTransactionBatchDetail AS IBD
                ON IBD.BatchID = ITD.BatchID
            LEFT OUTER JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID
               AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0
            WHERE ITM.VoucherID = -22
              AND ITM.ProductionUnitID IN (" + productionUnitIdStr + @")
              AND CAST(FLOOR(CAST(ITM.VoucherDate AS FLOAT)) AS DATETIME) >= @FromDate
              AND CAST(FLOOR(CAST(ITM.VoucherDate AS FLOAT)) AS DATETIME) <= @ToDate
              AND ITM.ProductionUnitID = @ProductionUnitID
              AND ISNULL(ITM.IsDeletedTransaction, 0) <> 1
              AND ISNULL(ITD.IssueQuantity, 0) > 0
            ORDER BY ITM.TransactionID DESC";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<TransferListDto>(sql, new
        {
            FromDate = fromDate,
            ToDate = toDate,
            ProductionUnitID = productionUnitId
        });
        return result.ToList();
    }

    // ─── GetWarehouseStock ────────────────────────────────────────────────────

    public async Task<List<WarehouseStockDto>> GetWarehouseStockAsync(long warehouseId)
    {
        var sql = @"
            SELECT
                ISNULL(Temp.GRNTransactionID, 0) AS ParentTransactionID,
                ISNULL(IM.ItemID, 0) AS ItemID,
                ISNULL(IM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(ISGM.ItemSubGroupID, 0) AS ItemSubGroupID,
                ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID,
                ISNULL(Temp.WarehouseID, 0) AS WarehouseID,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                NULLIF(IM.ItemCode, '') AS ItemCode,
                NULLIF(IM.ItemName, '') AS ItemName,
                NULLIF(IM.StockUnit, '') AS StockUnit,
                ISNULL(Temp.ClosingQty, 0) AS BatchStock,
                ISNULL(Temp.ClosingQty, 0) AS TransferStock,
                NULLIF(Temp.GRNNo, '') AS GRNNo,
                NULLIF(Temp.GRNDate, '') AS GRNDate,
                ISNULL(Temp.BatchID, 0) AS BatchID,
                NULLIF(Temp.BatchNo, '') AS BatchNo,
                NULLIF(Temp.SupplierBatchNo, '') AS SupplierBatchNo,
                NULLIF(Temp.MfgDate, '') AS MfgDate,
                NULLIF(Temp.ExpiryDate, '') AS ExpiryDate,
                NULLIF(Temp.WarehouseName, '') AS Warehouse,
                NULLIF(Temp.BinName, '') AS Bin,
                ISNULL(IM.WtPerPacking, 0) AS WtPerPacking,
                ISNULL(IM.UnitPerPacking, 1) AS UnitPerPacking,
                ISNULL(IM.ConversionFactor, 1) AS ConversionFactor
            FROM ItemMaster AS IM
            INNER JOIN ItemGroupMaster AS IGM
                ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN (
                SELECT
                    ISNULL(IM2.CompanyID, 0) AS CompanyID,
                    ISNULL(IM2.ItemID, 0) AS ItemID,
                    ISNULL(ITD.ParentTransactionID, 0) AS GRNTransactionID,
                    ISNULL(SUM(ISNULL(ITD.ReceiptQuantity, 0)), 0)
                        - ISNULL(SUM(ISNULL(ITD.IssueQuantity, 0)), 0) AS ClosingQty,
                    ISNULL(ITD.BatchID, 0) AS BatchID,
                    NULLIF(ITD.BatchNo, '') AS BatchNo,
                    NULLIF(IBD.SupplierBatchNo, '') AS SupplierBatchNo,
                    NULLIF(IBD.MfgDate, '') AS MfgDate,
                    NULLIF(IBD.ExpiryDate, '') AS ExpiryDate,
                    NULLIF('', '') AS Pallet_No,
                    ISNULL(ITD.WarehouseID, 0) AS WarehouseID,
                    NULLIF(WM.WarehouseName, '') AS WarehouseName,
                    NULLIF(WM.BinName, '') AS BinName,
                    NULLIF(IT.VoucherNo, '') AS GRNNo,
                    REPLACE(CONVERT(VARCHAR(13), IT.VoucherDate, 106), ' ', '-') AS GRNDate
                FROM ItemMaster AS IM2
                INNER JOIN ItemTransactionDetail AS ITD
                    ON ITD.ItemID = IM2.ItemID AND ISNULL(ITD.IsDeletedTransaction, 0) = 0
                INNER JOIN ItemTransactionMain AS ITM
                    ON ITM.TransactionID = ITD.TransactionID
                   AND ITM.CompanyID = ITD.CompanyID
                   AND ITM.VoucherID NOT IN (-8, -9, -11)
                INNER JOIN ItemTransactionMain AS IT
                    ON IT.TransactionID = ITD.ParentTransactionID
                INNER JOIN WarehouseMaster AS WM
                    ON WM.WarehouseID = ITD.WarehouseID
                INNER JOIN ItemTransactionBatchDetail AS IBD
                    ON IBD.BatchID = ITD.BatchID
                WHERE ITD.WarehouseID = @WarehouseID
                GROUP BY
                    ISNULL(IM2.ItemID, 0),
                    ISNULL(ITD.ParentTransactionID, 0),
                    ISNULL(ITD.BatchID, 0),
                    NULLIF(ITD.BatchNo, ''),
                    NULLIF(IBD.SupplierBatchNo, ''),
                    NULLIF(IBD.MfgDate, ''),
                    NULLIF(IBD.ExpiryDate, ''),
                    ISNULL(ITD.WarehouseID, 0),
                    NULLIF(WM.WarehouseName, ''),
                    NULLIF(WM.BinName, ''),
                    NULLIF(IT.VoucherNo, ''),
                    REPLACE(CONVERT(VARCHAR(13), IT.VoucherDate, 106), ' ', '-'),
                    ISNULL(IM2.CompanyID, 0)
                HAVING (ISNULL(SUM(ISNULL(ITD.ReceiptQuantity, 0)), 0)
                    - ISNULL(SUM(ISNULL(ITD.IssueQuantity, 0)), 0) > 0)
            ) AS Temp ON Temp.ItemID = IM.ItemID
            LEFT OUTER JOIN ItemSubGroupMaster AS ISGM
                ON ISGM.ItemSubGroupID = IM.ItemSubGroupID
               AND ISNULL(ISGM.IsDeletedTransaction, 0) <> 1
            ORDER BY BatchStock DESC, ItemGroupID, ItemName";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<WarehouseStockDto>(sql, new { WarehouseID = warehouseId });
        return result.ToList();
    }

    // ─── GetDestinationBins ───────────────────────────────────────────────────

    public async Task<List<BinDto>> GetDestinationBinsAsync(string warehouseName, long sourceBinId)
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();

        var sql = @"
            SELECT DISTINCT BinName AS Bin, WarehouseID
            FROM WarehouseMaster
            WHERE WarehouseName = @WarehouseName
              AND WarehouseID <> @SourceBinID
              AND ISNULL(BinName, '') <> ''
              AND ProductionUnitID = @ProductionUnitID
            ORDER BY Bin";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<BinDto>(sql, new
        {
            WarehouseName = warehouseName,
            SourceBinID = sourceBinId,
            ProductionUnitID = productionUnitId
        });
        return result.ToList();
    }

    // ─── GetTransferVoucherNo ─────────────────────────────────────────────────

    public async Task<string> GetTransferVoucherNoAsync(string prefix)
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();
        var fYear = _currentUserService.GetFYear();

        var sql = @"
            SELECT ISNULL(MAX(ISNULL(MaxVoucherNo, 0)), 0) + 1
            FROM ItemTransactionMain
            WHERE IsDeletedTransaction = 0
              AND VoucherID = -22
              AND VoucherPrefix = @Prefix
              AND ProductionUnitID = @ProductionUnitID
              AND FYear = @FYear";

        using var connection = GetConnection();
        var maxNo = await connection.ExecuteScalarAsync<long>(sql, new
        {
            Prefix = prefix,
            ProductionUnitID = productionUnitId,
            FYear = fYear
        });

        return $"{prefix}{maxNo:D6}";
    }

    // ─── SaveTransfer ─────────────────────────────────────────────────────────

    public async Task<TransferOperationResponseDto> SaveTransferAsync(SaveTransferRequest request)
    {
        var userId = _currentUserService.GetUserId();
        var fYear = _currentUserService.GetFYear();
        var productionUnitId = _currentUserService.GetProductionUnitId();

        // Generate voucher number
        var maxVoucherNo = await GetMaxVoucherNoAsync(request.Prefix);
        var voucherNo = $"{request.Prefix}{maxVoucherNo:D6}";

        long transactionId;

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        using var connection = GetConnection();

        // Insert ItemTransactionMain
        var insertMainSql = @"
            INSERT INTO ItemTransactionMain (
                VoucherID, VoucherPrefix, MaxVoucherNo, VoucherNo, VoucherDate,
                DestinationWarehouseID, Narration, DeliveryNoteNo, DeliveryNoteDate,
                ProductionUnitID, FYear,
                UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
            )
            VALUES (
                -22, @Prefix, @MaxVoucherNo, @VoucherNo, @VoucherDate,
                @DestinationWarehouseID, @Narration, @DeliveryNoteNo, @DeliveryNoteDate,
                @ProductionUnitID, @FYear,
                @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
            );
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        transactionId = await connection.ExecuteScalarAsync<long>(insertMainSql, new
        {
            Prefix = request.Prefix,
            MaxVoucherNo = maxVoucherNo,
            VoucherNo = voucherNo,
            VoucherDate = request.MainData.VoucherDate,
            DestinationWarehouseID = request.MainData.DestinationWarehouseID,
            Narration = request.MainData.Narration,
            DeliveryNoteNo = request.MainData.DeliveryNoteNo,
            DeliveryNoteDate = request.MainData.DeliveryNoteDate,
            ProductionUnitID = productionUnitId,
            FYear = fYear,
            UserID = userId
        });

        // Insert Issue detail records (IssueQuantity set, ReceiptQuantity=0, source WarehouseID)
        foreach (var detail in request.IssueDetails)
        {
            var insertIssueSql = @"
                INSERT INTO ItemTransactionDetail (
                    TransactionID, ParentTransactionID, ItemGroupID, ItemID,
                    IssueQuantity, ReceiptQuantity, BatchNo, BatchID, StockUnit, WarehouseID,
                    ProductionUnitID, FYear,
                    UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
                )
                VALUES (
                    @TransactionID, @ParentTransactionID, @ItemGroupID, @ItemID,
                    @IssueQuantity, 0, @BatchNo, @BatchID, @StockUnit, @WarehouseID,
                    @ProductionUnitID, @FYear,
                    @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
                );";

            await connection.ExecuteAsync(insertIssueSql, new
            {
                TransactionID = transactionId,
                ParentTransactionID = detail.ParentTransactionID,
                ItemGroupID = detail.ItemGroupID,
                ItemID = detail.ItemID,
                IssueQuantity = detail.IssueQuantity,
                BatchNo = detail.BatchNo,
                BatchID = detail.BatchID,
                StockUnit = detail.StockUnit,
                WarehouseID = detail.WarehouseID,
                ProductionUnitID = productionUnitId,
                FYear = fYear,
                UserID = userId
            });
        }

        // Insert Receipt detail records (ReceiptQuantity set, IssueQuantity=0, destination WarehouseID)
        foreach (var detail in request.ReceiptDetails)
        {
            var insertReceiptSql = @"
                INSERT INTO ItemTransactionDetail (
                    TransactionID, ParentTransactionID, ItemGroupID, ItemID,
                    ReceiptQuantity, IssueQuantity, BatchNo, BatchID, StockUnit, WarehouseID,
                    ProductionUnitID, FYear,
                    UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
                )
                VALUES (
                    @TransactionID, @ParentTransactionID, @ItemGroupID, @ItemID,
                    @ReceiptQuantity, 0, @BatchNo, @BatchID, @StockUnit, @WarehouseID,
                    @ProductionUnitID, @FYear,
                    @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
                );";

            await connection.ExecuteAsync(insertReceiptSql, new
            {
                TransactionID = transactionId,
                ParentTransactionID = detail.ParentTransactionID,
                ItemGroupID = detail.ItemGroupID,
                ItemID = detail.ItemID,
                ReceiptQuantity = detail.ReceiptQuantity,
                BatchNo = detail.BatchNo,
                BatchID = detail.BatchID,
                StockUnit = detail.StockUnit,
                WarehouseID = detail.WarehouseID,
                ProductionUnitID = productionUnitId,
                FYear = fYear,
                UserID = userId
            });
        }

        scope.Complete();

        return new TransferOperationResponseDto
        {
            TransactionID = transactionId,
            VoucherNo = voucherNo
        };
    }

    // ─── UpdateTransfer ───────────────────────────────────────────────────────

    public async Task UpdateTransferAsync(UpdateTransferRequest request)
    {
        var userId = _currentUserService.GetUserId();
        var fYear = _currentUserService.GetFYear();
        var productionUnitId = _currentUserService.GetProductionUnitId();

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        using var connection = GetConnection();

        // Update ItemTransactionMain
        var updateMainSql = @"
            UPDATE ItemTransactionMain SET
                VoucherDate = @VoucherDate,
                DestinationWarehouseID = @DestinationWarehouseID,
                Narration = @Narration,
                DeliveryNoteNo = @DeliveryNoteNo,
                DeliveryNoteDate = @DeliveryNoteDate,
                UserID = @UserID,
                ModifiedBy = @UserID,
                ModifiedDate = GETDATE()
            WHERE ProductionUnitID = @ProductionUnitID
              AND TransactionID = @TransactionID";

        await connection.ExecuteAsync(updateMainSql, new
        {
            VoucherDate = request.MainData.VoucherDate,
            DestinationWarehouseID = request.MainData.DestinationWarehouseID,
            Narration = request.MainData.Narration,
            DeliveryNoteNo = request.MainData.DeliveryNoteNo,
            DeliveryNoteDate = request.MainData.DeliveryNoteDate,
            UserID = userId,
            ProductionUnitID = productionUnitId,
            TransactionID = request.TransactionID
        });

        // Delete existing details
        await connection.ExecuteAsync(
            "DELETE FROM ItemTransactionDetail WHERE ProductionUnitID = @ProductionUnitID AND TransactionID = @TransactionID",
            new { ProductionUnitID = productionUnitId, TransactionID = request.TransactionID });

        // Re-insert Issue details
        foreach (var detail in request.IssueDetails)
        {
            var insertIssueSql = @"
                INSERT INTO ItemTransactionDetail (
                    TransactionID, ParentTransactionID, ItemGroupID, ItemID,
                    IssueQuantity, ReceiptQuantity, BatchNo, BatchID, StockUnit, WarehouseID,
                    ProductionUnitID, FYear,
                    UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
                )
                VALUES (
                    @TransactionID, @ParentTransactionID, @ItemGroupID, @ItemID,
                    @IssueQuantity, 0, @BatchNo, @BatchID, @StockUnit, @WarehouseID,
                    @ProductionUnitID, @FYear,
                    @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
                );";

            await connection.ExecuteAsync(insertIssueSql, new
            {
                TransactionID = request.TransactionID,
                ParentTransactionID = detail.ParentTransactionID,
                ItemGroupID = detail.ItemGroupID,
                ItemID = detail.ItemID,
                IssueQuantity = detail.IssueQuantity,
                BatchNo = detail.BatchNo,
                BatchID = detail.BatchID,
                StockUnit = detail.StockUnit,
                WarehouseID = detail.WarehouseID,
                ProductionUnitID = productionUnitId,
                FYear = fYear,
                UserID = userId
            });
        }

        // Re-insert Receipt details
        foreach (var detail in request.ReceiptDetails)
        {
            var insertReceiptSql = @"
                INSERT INTO ItemTransactionDetail (
                    TransactionID, ParentTransactionID, ItemGroupID, ItemID,
                    ReceiptQuantity, IssueQuantity, BatchNo, BatchID, StockUnit, WarehouseID,
                    ProductionUnitID, FYear,
                    UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate, IsDeletedTransaction
                )
                VALUES (
                    @TransactionID, @ParentTransactionID, @ItemGroupID, @ItemID,
                    @ReceiptQuantity, 0, @BatchNo, @BatchID, @StockUnit, @WarehouseID,
                    @ProductionUnitID, @FYear,
                    @UserID, @UserID, @UserID, GETDATE(), GETDATE(), 0
                );";

            await connection.ExecuteAsync(insertReceiptSql, new
            {
                TransactionID = request.TransactionID,
                ParentTransactionID = detail.ParentTransactionID,
                ItemGroupID = detail.ItemGroupID,
                ItemID = detail.ItemID,
                ReceiptQuantity = detail.ReceiptQuantity,
                BatchNo = detail.BatchNo,
                BatchID = detail.BatchID,
                StockUnit = detail.StockUnit,
                WarehouseID = detail.WarehouseID,
                ProductionUnitID = productionUnitId,
                FYear = fYear,
                UserID = userId
            });
        }

        scope.Complete();
    }

    // ─── DeleteTransfer ───────────────────────────────────────────────────────

    public async Task DeleteTransferAsync(long transactionId)
    {
        var userId = _currentUserService.GetUserId();
        var productionUnitId = _currentUserService.GetProductionUnitId();

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        using var connection = GetConnection();

        await connection.ExecuteAsync(
            @"UPDATE ItemTransactionMain SET
                DeletedBy = @UserID, DeletedDate = GETDATE(), IsDeletedTransaction = 1
              WHERE ProductionUnitID = @ProductionUnitID AND TransactionID = @TransactionID",
            new { UserID = userId, ProductionUnitID = productionUnitId, TransactionID = transactionId });

        await connection.ExecuteAsync(
            @"UPDATE ItemTransactionDetail SET
                DeletedBy = @UserID, DeletedDate = GETDATE(), IsDeletedTransaction = 1
              WHERE ProductionUnitID = @ProductionUnitID AND TransactionID = @TransactionID",
            new { UserID = userId, ProductionUnitID = productionUnitId, TransactionID = transactionId });

        scope.Complete();
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    private async Task<long> GetMaxVoucherNoAsync(string prefix)
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();
        var fYear = _currentUserService.GetFYear();

        var sql = @"
            SELECT ISNULL(MAX(ISNULL(MaxVoucherNo, 0)), 0) + 1
            FROM ItemTransactionMain
            WHERE IsDeletedTransaction = 0
              AND VoucherID = -22
              AND VoucherPrefix = @Prefix
              AND ProductionUnitID = @ProductionUnitID
              AND FYear = @FYear";

        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<long>(sql, new
        {
            Prefix = prefix,
            ProductionUnitID = productionUnitId,
            FYear = fYear
        });
    }
}
