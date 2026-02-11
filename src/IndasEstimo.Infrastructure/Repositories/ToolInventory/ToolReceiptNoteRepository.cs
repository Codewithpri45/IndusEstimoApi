using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Interfaces.Repositories.ToolInventory;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Domain.Entities.ToolInventory;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Application.Interfaces.Services;
using Dapper;

namespace IndasEstimo.Infrastructure.Repositories.ToolInventory;

public class ToolReceiptNoteRepository : IToolReceiptNoteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToolReceiptNoteRepository> _logger;

    public ToolReceiptNoteRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ToolReceiptNoteRepository> logger)
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

    public async Task<long> SaveToolReceiptNoteAsync(
        ToolReceiptNote main,
        List<ToolReceiptNoteDetail> details)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            // 1. Insert Main record
            var transactionId = await _dbOperations.InsertDataAsync("ToolTransactionMain", main, connection, transaction, "TransactionID");

            // 2. Set ParentTransactionID = TransactionID for all details
            foreach (var detail in details)
            {
                detail.ParentTransactionID = transactionId;
            }

            // 3. Insert Detail records
            await _dbOperations.InsertDataAsync("ToolTransactionDetail", details, connection, transaction, "TransactionDetailID", parentTransactionId: transactionId);

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving tool receipt note");
            throw;
        }
    }

    public async Task<long> UpdateToolReceiptNoteAsync(
        long transactionId,
        ToolReceiptNote main,
        List<ToolReceiptNoteDetail> details)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            // 1. Update Main record
            main.TransactionID = transactionId;
            main.ModifiedBy = _currentUserService.GetUserId() ?? 0;
            main.ProductionUnitID = productionUnitId;

            await _dbOperations.UpdateDataAsync("ToolTransactionMain", main, connection, transaction, new[] { "TransactionID", "ProductionUnitID" });

            // 2. Delete existing detail records
            await connection.ExecuteAsync(
                "DELETE FROM ToolTransactionDetail WHERE TransactionID = @TransactionID AND ProductionUnitID = @ProductionUnitID",
                new { TransactionID = transactionId, ProductionUnitID = productionUnitId }, transaction);

            // 3. Set ParentTransactionID = TransactionID for all details
            foreach (var detail in details)
            {
                detail.ParentTransactionID = transactionId;
            }

            // 4. Re-insert all detail records
            await _dbOperations.InsertDataAsync("ToolTransactionDetail", details, connection, transaction, "TransactionDetailID", parentTransactionId: transactionId);

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating tool receipt note {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<bool> DeleteToolReceiptNoteAsync(long transactionId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;
            var userId = _currentUserService.GetUserId() ?? 0;

            // Soft delete main and detail tables
            var softDeleteSql = @"
                UPDATE {0}
                SET ModifiedBy = @ModifiedBy,
                    DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    ModifiedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE ProductionUnitID = @ProductionUnitID
                  AND TransactionID = @TransactionID";

            var parameters = new { ModifiedBy = userId, DeletedBy = userId, ProductionUnitID = productionUnitId, TransactionID = transactionId };

            await connection.ExecuteAsync(string.Format(softDeleteSql, "ToolTransactionMain"), parameters, transaction);
            await connection.ExecuteAsync(string.Format(softDeleteSql, "ToolTransactionDetail"), parameters, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting tool receipt note {TransactionID}", transactionId);
            throw;
        }
    }

    public async Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextReceiptNoAsync(string prefix)
    {
        return await _dbOperations.GenerateVoucherNoAsync(
            "ToolTransactionMain",
            -116,
            prefix);
    }

    public async Task<string?> GetVoucherNoAsync(long transactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;
        var sql = "SELECT VoucherNo FROM ToolTransactionMain WHERE TransactionID = @TransactionID AND ProductionUnitID = @ProductionUnitID";
        return await connection.ExecuteScalarAsync<string>(sql, new { TransactionID = transactionId, ProductionUnitID = productionUnitId });
    }

    public async Task<bool> IsToolReceiptNoteUsedAsync(long transactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // Check if any child transactions exist
        var childSql = @"
            SELECT COUNT(1) FROM ToolTransactionDetail
            WHERE Isnull(IsDeletedTransaction, 0) = 0
              AND ParentTransactionID = @TransactionID
              AND ProductionUnitID = @ProductionUnitID
              AND TransactionID <> ParentTransactionID";

        var childCount = await connection.ExecuteScalarAsync<int>(childSql, new { TransactionID = transactionId, ProductionUnitID = productionUnitId });

        if (childCount > 0) return true;

        // Check QC approval
        var qcSql = @"
            SELECT COUNT(1) FROM ToolTransactionDetail
            WHERE Isnull(IsDeletedTransaction, 0) = 0
              AND Isnull(QCApprovalNo, '') <> ''
              AND TransactionID = @TransactionID
              AND (Isnull(ApprovedQuantity, 0) > 0 OR Isnull(RejectedQuantity, 0) > 0)";

        var qcCount = await connection.ExecuteScalarAsync<int>(qcSql, new { TransactionID = transactionId });
        return qcCount > 0;
    }

    // ==================== Retrieve Operations ====================

    public async Task<List<ToolReceiptNoteDataDto>> GetToolReceiptNoteDataAsync(long transactionId)
    {
        using var connection = GetConnection();

        // SQL copied directly from VB GetReceiptVoucherBatchDetail (MSSQL branch, lines 142-162)
        var sql = @"
            Select Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,
                Isnull(ITM.LedgerID,0) As LedgerID,Isnull(ITD.TransID,0) As TransID,Isnull(ITD.ToolID,0) As ToolID,
                Isnull(ITD.ToolGroupID,0) As ToolGroupID,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,
                Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,
                NullIf(SPM.ToolCode,'') AS ToolCode,NullIf(SPM.ToolType,'') AS ToolType, NullIf(Isnull(SPM.ToolName,''),'') AS ToolName,
                Isnull(ITMPD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,NullIf(ITMPD.PurchaseUnit,'') AS PurchaseUnit,
                Isnull(ITD.ChallanQuantity, 0) As ChallanQuantity, NullIf(ITD.BatchNo,'') AS BatchNo,NullIf(ITD.StockUnit,'') AS StockUnit,
                Isnull(ITD.ReceiptWtPerPacking,0) AS ReceiptWtPerPacking,Isnull(ITMPD.PurchaseTolerance,0) AS PurchaseTolerance,
                Isnull(ITD.WarehouseID, 0) As WarehouseID, Nullif(WM.WarehouseName,'') AS Warehouse,  Nullif(WM.WarehouseID,'') AS Bin,
                Isnull((Select Sum(Isnull(ChallanQuantity,0))  From ToolTransactionDetail Where Isnull(IsDeletedTransaction,0)=0 And
                Isnull(PurchaseTransactionID,0)>0 AND Isnull(ChallanQuantity,0)>0 AND PurchaseTransactionID=ITMPD.TransactionID AND ToolID=ITMPD.ToolID),0) AS ReceiptQuantity
            From ToolTransactionMain As ITM
            INNER Join ToolTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID
            INNER JOIN ToolMaster AS SPM ON SPM.ToolID=ITD.ToolID
            INNER Join ToolTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID And ITMP.CompanyID=ITD.CompanyID
            INNER Join ToolTransactionDetail AS ITMPD ON ITMPD.TransactionID=ITMP.TransactionID And ITMPD.ToolID=ITD.ToolID
                And ITMPD.TransactionID=ITD.PurchaseTransactionID And ITMPD.CompanyID=ITMP.CompanyID
            INNER JOIN WarehouseMaster AS WM ON WM.WarehouseID=ITD.WarehouseID And WM.CompanyID=ITD.CompanyID
            Where ITM.VoucherID = -116 And ITM.TransactionID = @TransactionID
            Order By TransID";

        var result = await connection.QueryAsync<ToolReceiptNoteDataDto>(sql, new { TransactionID = transactionId });
        return result.ToList();
    }

    public async Task<List<ToolReceiptNoteListDto>> GetToolReceiptNoteListAsync(string fromDate, string toDate)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // SQL copied directly from VB GetReceiptNoteList (MSSQL branch, lines 219-244)
        var sql = @"
            Select Isnull(ITM.EWayBillNumber,'') AS EWayBillNumber,Replace(Convert(Varchar(13),ITM.EWayBillDate,106),' ','-') AS EWayBillDate,Isnull(ITM.TransactionID,0) AS TransactionID,nullif(ITD.RefJobCardContentNo ,'') AS RefJobCardContentNo,
                Isnull(ITD.PurchaseTransactionID,0) As PurchaseTransactionID,Isnull(ITM.LedgerID,0) As LedgerID,NullIf(LM.LedgerName,'') AS LedgerName,NullIf(LM.LedgerName,'') AS ReceiverName,
                Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS ReceiptVoucherNo,
                Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS ReceiptVoucherDate,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,
                Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,
                ROUND(SUM(Isnull(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,
                Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-') AS DeliveryNoteDate,NullIf(ITM.GateEntryNo,'') AS GateEntryNo,
                Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-') AS GateEntryDate,NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo,
                NullIf(ITM.Transporter,'') AS Transporter,NullIf(ITM.Narration,'') AS Narration,
                NullIf(ITM.FYear,'') AS FYear,NullIf(UM.UserName,'') AS CreatedBy,Isnull(ITM.ReceivedBy,0) AS ReceivedBy,ISNULL(PUM.ProductionUnitID,0) as ProductionUnitID,ISNULL(PUM.ProductionUnitName,0) as ProductionUnitName,ISNULL(CM.CompanyName,0) as CompanyName
            From ToolTransactionMain AS ITM
            INNER JOIN ToolTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID
            INNER JOIN ToolTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID
            INNER JOIN UserMaster AS UM ON UM.UserID=ITM.CreatedBy
            INNER JOIN LedgerMaster as LM on LM.LedgerID=ITM.LedgerID
            INNER JOIN ProductionUnitMaster As PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID And ISNULL(PUM.IsDeletedTransaction,0) =0
            INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID
            Where ITM.VoucherID=-116 And  ITM.ProductionUnitID IN(@ProductionUnitID) AND ITM.VoucherDate BETWEEN @FromDate AND @ToDate and isnull(ITM.IsDeletedTransaction,0)<>1
            GROUP BY Isnull(ITM.EWayBillNumber,''),Replace(Convert(Varchar(13),ITM.EWayBillDate,106),' ','-'),Isnull(ITM.TransactionID,0),Isnull(ITD.PurchaseTransactionID,0),Isnull(ITM.LedgerID,0),nullif(LM.LedgerName,''),
                NullIf(ITM.VoucherNo,''),Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-'),
                NullIf(ITMP.VoucherNo,''),Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-'),NullIf(ITM.DeliveryNoteNo,''),
                Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-'),NullIf(ITM.GateEntryNo,''),
                Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-'),NullIf(ITM.LRNoVehicleNo,''),NullIf(ITM.Transporter,''),
                NullIf(ITM.Narration,''),NullIf(ITM.FYear,''),Isnull(ITM.MaxVoucherNo,0),
                NullIf(UM.UserName,''),Isnull(ITM.ReceivedBy,0),nullif(ITD.RefJobCardContentNo,''),ISNULL(PUM.ProductionUnitID,0),ISNULL(PUM.ProductionUnitName,0),ISNULL(CM.CompanyName,0)
            Order By FYear Desc,MaxVoucherNo Desc";

        var result = await connection.QueryAsync<ToolReceiptNoteListDto>(
            sql, new { ProductionUnitID = productionUnitId, FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    public async Task<List<ToolPendingPurchaseOrderDto>> GetPendingPurchaseOrdersAsync()
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // SQL copied directly from VB GetPendingOrdersList (MSSQL branch, lines 189-191)
        var sql = @"
            Select Isnull(ITM.TransactionID,0) AS TransactionID,Isnull(ITM.VoucherID,0) AS VoucherID,Isnull(ITM.LedgerID,0) AS LedgerID,Isnull(ITD.TransID,0) As TransID,Isnull(ITD.ToolID,0) As ToolID,Isnull(ITD.ToolGroupID,0) As ToolGroupID,NullIf(LM.LedgerName,'') AS LedgerName,Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS PurchaseVoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS PurchaseVoucherDate,     NullIf(SPM.ToolCode,'') AS ToolCode,NullIf(SPM.ToolType,'') AS ToolType, NullIf(Isnull(SPM.ToolName,''),'') AS ToolName,Isnull(ITD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity, (Isnull(ITD.PurchaseOrderQuantity,0)-Isnull(TR.ReceiptQuantity,0)) AS PendingQty, Isnull(ITD.PurchaseUnit,'') AS PurchaseUnit,Isnull(ITD.StockUnit,'') AS StockUnit,     Isnull(ITD.PurchaseTolerance,0) AS PurchaseTolerance,NullIf(Isnull(UA.UserName,''),'') AS CreatedBy,NullIf(Isnull(UM.UserName,''),'') AS ApprovedBy,  nullif(ITD.RefJobCardContentNo ,'') AS RefJobCardContentNo,NullIf(ITD.FYear,'') AS FYear,NullIf(ITM.PurchaseDivision,'') AS PurchaseDivision,     NULLIf(ITM.PurchaseReferenceRemark,'') AS PurchaseReferenceRemark,isnull(TR.ReceiptQuantity,0) AS ReceiptQuantity,ISNULL(PUM.ProductionUnitID,0) as ProductionUnitID,ISNULL(PUM.ProductionUnitName,0) as ProductionUnitName,ISNULL(CM.CompanyName,0) as CompanyName
            From ToolTransactionMain AS ITM INNER JOIN ToolTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN ToolMaster AS SPM ON SPM.ToolID=ITD.ToolID INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID INNER JOIN ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID And ISNULL(PUM.IsDeletedTransaction,0) =0 INNER JOIN CompanyMaster As CM On CM.CompanyID = PUM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherToolApprovedBy LEFT JOIN (Select PurchaseTransactionID,ToolID,CompanyID,ROUND(Sum(Isnull(ChallanQuantity,0)),3) AS ReceiptQuantity From ToolTransactionDetail Where Isnull(IsDeletedTransaction,0)<>1 GROUP BY PurchaseTransactionID,ToolID,CompanyID) AS TR ON TR.PurchaseTransactionID =ITD.TransactionID AND TR.ToolID=ITD.ToolID AND TR.CompanyID=ITD.CompanyID
            Where ITM.VoucherID= -117 And ITM.ProductionUnitID IN(@ProductionUnitID) AND Isnull(ITD.IsDeletedTransaction,0)<>1 AND Isnull(ITD.IsCompleted,0)<>1 AND Isnull(ITD.IsVoucherToolApproved,0)=1 Order By FYear,MaxVoucherNo Desc";

        var result = await connection.QueryAsync<ToolPendingPurchaseOrderDto>(
            sql, new { ProductionUnitID = productionUnitId });
        return result.ToList();
    }

    // ==================== Helper/Lookup Operations ====================

    public async Task<string> GetLastTransactionDateAsync()
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT TOP 1 Replace(Convert(Varchar(13), VoucherDate, 106), ' ', '-')
            FROM ToolTransactionMain
            WHERE VoucherID = -116
              AND ProductionUnitID = @ProductionUnitID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY VoucherDate DESC";

        var date = await connection.ExecuteScalarAsync<string>(sql, new { ProductionUnitID = productionUnitId });
        return date ?? string.Empty;
    }

    public async Task<List<ToolReceiverDto>> GetReceiversAsync()
    {
        using var connection = GetConnection();

        // SQL copied directly from VB GetReceiverList (MSSQL branch, lines 268-270)
        // Uses UNPIVOT/PIVOT pattern on LedgerMasterDetails, LedgerGroupNameID=27
        var sql = @"
            Select Distinct Isnull(LM.LedgerID,0) AS LedgerID,NullIf(LM.LedgerName,'') AS LedgerName From (Select A.[LedgerID] AS LedgerID,A.[CompanyID] AS CompanyID,A.[LedgerName],A.[City],A.[State],A.[Country] From (SELECT [LedgerID],[LedgerGroupID],[CompanyID],[LedgerName],[City],[State],[Country] FROM (SELECT [LedgerID],[LedgerGroupID],[CompanyID],[FieldName],nullif([FieldValue],'''') as FieldValue
            FROM [LedgerMasterDetails] Where Isnull(IsDeletedTransaction,0)<>1 AND LedgerGroupID IN(Select Distinct LedgerGroupID From LedgerGroupMaster Where LedgerGroupNameID=27))x unpivot (value for name in ([FieldValue])) up pivot (max(value) for FieldName in ([LedgerName],[City],[State],[Country])) p) AS A) AS LM
            Order By NullIf(LM.LedgerName,'')";

        var result = await connection.QueryAsync<ToolReceiverDto>(sql);
        return result.ToList();
    }

    public async Task<ToolPreviousReceivedQtyDto> GetPreviousReceivedQuantityAsync(long purchaseTransactionId, long toolId, long grnTransactionId)
    {
        using var connection = GetConnection();

        // SQL copied directly from VB GetPreviousReceivedQuantity (MSSQL branch, lines 297-304)
        var sql = @"
            Select Isnull(PTM.TransactionID,0) AS TransactionID,Isnull(PTD.ToolID,0) AS ToolID,
                Isnull(PTD.PurchaseTolerance,0) As PurchaseTolerance,Isnull(PTD.PurchaseOrderQuantity,0) As PurchaseOrderQuantity,
                PTD.PurchaseUnit,Isnull((Select Sum(Isnull(ChallanQuantity,0)) From ToolTransactionDetail
                Where ISNULL(ChallanQuantity,0)>0 And PurchaseTransactionID=PTM.TransactionID And TransactionID<>@GRNTransactionID AND ToolID=PTD.ToolID
                AND CompanyID=PTM.CompanyID AND Isnull(IsDeletedTransaction,0)<>1),0 ) AS PreReceiptQuantity,PTD.PurchaseUnit
            From ToolTransactionMain AS PTM
            INNER JOIN ToolTransactionDetail AS PTD ON PTD.TransactionID=PTM.TransactionID AND PTM.CompanyID=PTD.CompanyID
            Where PTM.VoucherID=-117 AND PTM.TransactionID=@PurchaseTransactionID AND PTD.ToolID=@ToolID";

        var result = await connection.QueryFirstOrDefaultAsync<ToolPreviousReceivedQtyDto>(
            sql, new { PurchaseTransactionID = purchaseTransactionId, ToolID = toolId, GRNTransactionID = grnTransactionId });
        return result ?? new ToolPreviousReceivedQtyDto();
    }

    public async Task<List<ToolWarehouseDto>> GetWarehousesAsync()
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // SQL copied directly from VB GetWarehouseList (line 323)
        var sql = @"
            Select DISTINCT Nullif(WarehouseName,'') AS Warehouse
            From WarehouseMaster
            Where WarehouseName <> '' AND WarehouseName IS NOT NULL
              AND CompanyID = (SELECT CompanyID FROM ProductionUnitMaster WHERE ProductionUnitID = @ProductionUnitID)
              AND ProductionUnitID = @ProductionUnitID
            Order By Nullif(WarehouseName,'')";

        var result = await connection.QueryAsync<ToolWarehouseDto>(sql, new { ProductionUnitID = productionUnitId });
        return result.ToList();
    }

    public async Task<List<ToolBinDto>> GetBinsAsync()
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // SQL copied directly from VB GetBinsList (MSSQL branch, line 344)
        var sql = @"
            SELECT Distinct Nullif(WarehouseName,'') AS WarehouseName,Nullif(BinName,'') AS Bin,Isnull(WarehouseID,0) AS WarehouseID
            FROM WarehouseMaster
            Where Isnull(BinName,'')<>''
              AND CompanyID = (SELECT CompanyID FROM ProductionUnitMaster WHERE ProductionUnitID = @ProductionUnitID)
              AND ProductionUnitID = @ProductionUnitID
            Order By Nullif(BinName,'')";

        var result = await connection.QueryAsync<ToolBinDto>(sql, new { ProductionUnitID = productionUnitId });
        return result.ToList();
    }

    public async Task<bool> CheckPermissionAsync(long transactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // Check if any child transactions exist (VB CheckPermission lines 633)
        var childSql = @"
            Select * From ToolTransactionDetail
            Where Isnull(IsDeletedTransaction, 0) = 0
              And ParentTransactionID = @TransactionID
              And ProductionUnitID = @ProductionUnitID
              And TransactionID <> ParentTransactionID";

        var childResults = await connection.QueryAsync(childSql, new { TransactionID = transactionId, ProductionUnitID = productionUnitId });
        if (childResults.Any()) return true;

        // Check QC approval (VB CheckPermission lines 644)
        var qcSql = @"
            Select * From ToolTransactionDetail
            Where Isnull(IsDeletedTransaction, 0) = 0
              And isnull(QCApprovalNo,'')<>''
              AND TransactionID=@TransactionID
              AND (Isnull(ApprovedQuantity,0)>0 OR Isnull(RejectedQuantity,0)>0)";

        var qcResults = await connection.QueryAsync(qcSql, new { TransactionID = transactionId });
        return qcResults.Any();
    }
}
