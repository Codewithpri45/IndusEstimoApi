using Dapper;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;

namespace IndasEstimo.Infrastructure.Repositories.Inventory;

public class GRNApprovalRepository : IGRNApprovalRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GRNApprovalRepository> _logger;

    public GRNApprovalRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<GRNApprovalRepository> logger)
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

    public async Task<List<GRNListDto>> GetGRNListAsync(string radioValue, string fromDate, string toDate)
    {
        using var connection = GetConnection();

        var productionUnitIds = await _dbOperations.GetProductionUnitIdsAsync();
        var fYear = _currentUserService.GetFYear();

        string sql;
        string dateFilter = "";

        if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
        {
            dateFilter = " AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= @FromDate)) " +
                        " AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= @ToDate)) ";
        }

        if (radioValue == "Pending Receipt Note")
        {
            // Pending GRNs - not yet approved (IsVoucherItemApproved <> 1 AND QCApprovalNo = '')
            sql = @"
                SELECT
                    Isnull(ITM.TransactionID,0) AS TransactionID,
                    Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,
                    Isnull(ITM.LedgerID,0) AS LedgerID,
                    Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,
                    NullIf(LM.LedgerName,'') AS LedgerName,
                    NullIf(ITM.VoucherNo,'') AS ReceiptVoucherNo,
                    Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS ReceiptVoucherDate,
                    NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,
                    Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,
                    ROUND(SUM(Isnull(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,
                    NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,
                    Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-') AS DeliveryNoteDate,
                    NullIf(ITM.GateEntryNo,'') AS GateEntryNo,
                    Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-') AS GateEntryDate,
                    NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo,
                    NullIf(ITM.Transporter,'') AS Transporter,
                    NullIf(EM.LedgerName,'') AS ReceiverName,
                    NullIf(ITM.Narration,'') AS Narration,
                    NullIf(ITM.FYear,'') AS FYear,
                    NullIf(UM.UserName,'') AS CreatedBy,
                    Isnull(ITM.ReceivedBy,0) AS ReceivedBy,
                    PUM.ProductionUnitID,
                    PUM.ProductionUnitName,
                    CM.CompanyName
                FROM ItemTransactionMain AS ITM
                INNER JOIN ItemTransactionDetail AS ITD
                    ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID
                INNER JOIN ItemTransactionMain AS ITMP
                    ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID
                INNER JOIN LedgerMaster AS LM
                    ON LM.LedgerID=ITM.LedgerID
                INNER JOIN UserMaster AS UM
                    ON UM.UserID=ITM.CreatedBy
                INNER JOIN ProductionUnitMaster As PUM
                    ON PUM.ProductionUnitID = ITM.ProductionUnitID
                INNER JOIN CompanyMaster AS CM
                    ON CM.CompanyID = PUM.CompanyID
                LEFT JOIN LedgerMaster AS EM
                    ON EM.LedgerID=ITM.ReceivedBy
                WHERE ITM.VoucherID = -14
                    AND ITM.ProductionUnitID IN(" + productionUnitIds + @")
                    AND isnull(ITM.IsDeletedTransaction,0)<>1
                    AND Isnull(ITD.IsVoucherItemApproved,0)<>1
                    AND Isnull(ITD.QCApprovalNo,'')='' " + dateFilter + @"
                GROUP BY
                    Isnull(ITM.TransactionID,0),Isnull(ITD.PurchaseTransactionID,0),
                    Isnull(ITM.LedgerID,0),NullIf(ITM.VoucherNo,''),
                    Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-'),
                    NullIf(ITMP.VoucherNo,''),Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-'),
                    NullIf(ITM.DeliveryNoteNo,''),Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-'),
                    NullIf(ITM.GateEntryNo,''),Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-'),
                    NullIf(ITM.LRNoVehicleNo,''),NullIf(ITM.Transporter,''),NullIf(ITM.Narration,''),
                    NullIf(EM.LedgerName,''),NullIf(LM.LedgerName,''),NullIf(ITM.FYear,''),
                    Isnull(ITM.MaxVoucherNo,0),NullIf(UM.UserName,''),Isnull(ITM.ReceivedBy,0),
                    PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName
                ORDER BY FYear, MaxVoucherNo DESC";
        }
        else // Approved Receipt Note
        {
            // Approved GRNs - already approved (IsVoucherItemApproved <> 0 AND QCApprovalNo <> '')
            sql = @"
                SELECT
                    Isnull(ITM.TransactionID,0) AS TransactionID,
                    Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,
                    Isnull(ITM.LedgerID,0) AS LedgerID,
                    Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,
                    NullIf(LM.LedgerName,'') AS LedgerName,
                    NullIf(ITM.VoucherNo,'') AS ReceiptVoucherNo,
                    Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS ReceiptVoucherDate,
                    NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,
                    Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,
                    ROUND(SUM(Isnull(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,
                    NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,
                    Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-') AS DeliveryNoteDate,
                    NullIf(ITM.GateEntryNo,'') AS GateEntryNo,
                    Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-') AS GateEntryDate,
                    NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo,
                    NullIf(ITM.Transporter,'') AS Transporter,
                    NullIf(EM.LedgerName,'') AS ReceiverName,
                    NullIf(ITM.Narration,'') AS Narration,
                    NullIf(ITM.FYear,'') AS FYear,
                    NullIf(UM.UserName,'') AS CreatedBy,
                    Isnull(ITM.ReceivedBy,0) AS ReceivedBy,
                    Nullif(UA.UserName,'') AS ApprovedBy,
                    Replace(Convert(Varchar(13),ITD.VoucherItemApprovedDate,106),' ','-') AS ApprovalDate,
                    PUM.ProductionUnitID,
                    PUM.ProductionUnitName,
                    CM.CompanyName
                FROM ItemTransactionMain AS ITM
                INNER JOIN ItemTransactionDetail AS ITD
                    ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID
                INNER JOIN ItemTransactionMain AS ITMP
                    ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID
                INNER JOIN LedgerMaster AS LM
                    ON LM.LedgerID=ITM.LedgerID
                INNER JOIN UserMaster AS UM
                    ON UM.UserID=ITM.CreatedBy
                INNER JOIN ProductionUnitMaster As PUM
                    ON PUM.ProductionUnitID = ITM.ProductionUnitID
                INNER JOIN CompanyMaster AS CM
                    ON CM.CompanyID = PUM.CompanyID
                LEFT JOIN LedgerMaster AS EM
                    ON EM.LedgerID=ITM.ReceivedBy
                LEFT JOIN UserMaster AS UA
                    ON UA.UserID=ITD.VoucherItemApprovedBy
                WHERE ITM.VoucherID = -14
                    AND ITM.FYear = @FYear
                    AND ITM.ProductionUnitID IN(" + productionUnitIds + @")
                    AND isnull(ITM.IsDeletedTransaction,0)<>1
                    AND Isnull(ITD.IsVoucherItemApproved,0)<>0
                    AND Isnull(ITD.QCApprovalNo,'')<>'' " + dateFilter + @"
                GROUP BY
                    Isnull(ITM.TransactionID,0),Isnull(ITD.PurchaseTransactionID,0),
                    Isnull(ITM.LedgerID,0),NullIf(ITM.VoucherNo,''),
                    Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-'),
                    NullIf(ITMP.VoucherNo,''),Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-'),
                    NullIf(ITM.DeliveryNoteNo,''),Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-'),
                    NullIf(ITM.GateEntryNo,''),Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-'),
                    NullIf(ITM.LRNoVehicleNo,''),NullIf(ITM.Transporter,''),NullIf(ITM.Narration,''),
                    NullIf(EM.LedgerName,''),NullIf(LM.LedgerName,''),NullIf(ITM.FYear,''),
                    Isnull(ITM.MaxVoucherNo,0),NullIf(UM.UserName,''),Isnull(ITM.ReceivedBy,0),
                    Nullif(UA.UserName,''),Replace(Convert(Varchar(13),ITD.VoucherItemApprovedDate,106),' ','-'),
                    PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName
                ORDER BY FYear, MaxVoucherNo DESC";
        }

        var result = await connection.QueryAsync<GRNListDto>(sql, new { FromDate = fromDate, ToDate = toDate, FYear = fYear });
        return result.ToList();
    }

    public async Task<List<GRNBatchDetailDto>> GetGRNBatchDetailAsync(long transactionId, string radioValue)
    {
        using var connection = GetConnection();

        string sql;

        if (radioValue == "Pending Receipt Note")
        {
            // Pending GRN - show ChallanQuantity as ApprovedQuantity, RejectedQuantity = 0
            sql = @"
                SELECT
                    ITM.TransactionID,
                    ITD.BatchID,
                    ITD.BatchNo,
                    ITD.SupplierBatchNo,
                    IQC.VoucherNo AS VOUCHERNO,
                    Isnull(IQC.QCTransactionID,0) AS QCTransactionID,
                    NullIf(ISGM.ItemSubGroupID,'') AS ItemSubGroupID,
                    Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,
                    Isnull(ITM.LedgerID,0) AS LedgerID,
                    Isnull(ITD.TransID,0) AS TransID,
                    Isnull(ITD.ItemID,0) AS ItemID,
                    Isnull(IM.ItemGroupID,0) AS ItemGroupID,
                    Isnull(IGM.ItemGroupNameID,0) AS ItemGroupNameID,
                    NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,
                    Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,
                    NullIf(IM.ItemCode,'') AS ItemCode,
                    NullIf(IGM.ItemGroupName,'') AS ItemGroupName,
                    NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,
                    NullIf(IM.ItemName,'') AS ItemName,
                    NullIf(IM.ItemDescription,'') AS ItemDescription,
                    Isnull(ITMPD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,
                    NullIf(ITMPD.PurchaseUnit,'') AS PurchaseUnit,
                    Isnull(ITD.ChallanQuantity,0) AS ChallanQuantity,
                    Isnull(ITD.ChallanQuantity,0) AS ApprovedQuantity,
                    0 AS RejectedQuantity,
                    NullIf(ITD.BatchNo,'') AS BatchNo,
                    NullIf(ITD.StockUnit,'') AS StockUnit,
                    Isnull(ITD.ReceiptWtPerPacking,0) AS ReceiptWtPerPacking,
                    Isnull(ITMPD.PurchaseTolerance,0) AS PurchaseTolerance,
                    Isnull(IM.WtPerPacking,0) AS WtPerPacking,
                    Isnull(IM.UnitPerPacking,1) AS UnitPerPacking,
                    Isnull(IM.ConversionFactor,1) AS ConversionFactor,
                    0 AS SizeW,
                    Isnull(ITD.WarehouseID,0) AS WarehouseID,
                    Nullif(WM.WarehouseName,'') AS Warehouse,
                    Nullif(WM.BinName,'') AS Bin,
                    Isnull(UOM.DecimalPlace,0) AS UnitDecimalPlace,
                    NullIf(IM.Quality,'') AS ItemQuality,
                    '' AS QCApprovalNO,
                    '' AS QCApprovedNarration
                FROM ItemTransactionMain AS ITM
                INNER JOIN ItemTransactionDetail AS ITD
                    ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID
                INNER JOIN ItemMaster AS IM
                    ON IM.ItemID=ITD.ItemID
                INNER JOIN ItemGroupMaster AS IGM
                    ON IGM.ItemGroupID=IM.ItemGroupID
                INNER JOIN ItemTransactionMain AS ITMP
                    ON ITMP.TransactionID=ITD.PurchaseTransactionID
                INNER JOIN ItemTransactionDetail AS ITMPD
                    ON ITMPD.TransactionID=ITMP.TransactionID
                    AND ITMPD.ItemID=IM.ItemID
                    AND ITMPD.TransactionID=ITD.PurchaseTransactionID
                INNER JOIN WarehouseMaster AS WM
                    ON WM.WarehouseID=ITD.WarehouseID AND WM.CompanyID=ITD.CompanyID
                LEFT JOIN ItemSubGroupMaster AS ISGM
                    ON ISGM.ItemSubGroupID=IM.ItemSubGroupID
                LEFT JOIN UnitMaster AS UOM
                    ON UOM.UnitSymbol=IM.StockUnit
                LEFT JOIN ItemQCInspectionMain AS IQC
                    ON IQC.GRNTransactionID = ITD.TransactionID
                    AND IQC.BatchID = ITD.BatchID
                    AND IQC.ItemID = ITD.ItemID
                    AND Isnull(IQC.IsDeletedTransaction,0)=0
                WHERE ITM.VoucherID=-14
                    AND ITM.TransactionID=@TransactionID
                ORDER BY TransID";
        }
        else // Approved Receipt Note
        {
            // Approved GRN - show actual ApprovedQuantity, RejectedQuantity, QCApprovalNO, QCApprovedNarration
            sql = @"
                SELECT
                    ITM.TransactionID,
                    ITD.BatchID,
                    ITD.BatchNo,
                    ITD.SupplierBatchNo,
                    IQC.VoucherNo AS VOUCHERNO,
                    Isnull(IQC.QCTransactionID,0) AS QCTransactionID,
                    NullIf(ISGM.ItemSubGroupID,'') AS ItemSubGroupID,
                    Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,
                    Isnull(ITM.LedgerID,0) AS LedgerID,
                    Isnull(ITD.TransID,0) AS TransID,
                    Isnull(ITD.ItemID,0) AS ItemID,
                    Isnull(IM.ItemGroupID,0) AS ItemGroupID,
                    Isnull(IGM.ItemGroupNameID,0) AS ItemGroupNameID,
                    NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,
                    Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,
                    NullIf(IM.ItemCode,'') AS ItemCode,
                    NullIf(IGM.ItemGroupName,'') AS ItemGroupName,
                    NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,
                    NullIf(IM.ItemName,'') AS ItemName,
                    NullIf(IM.ItemDescription,'') AS ItemDescription,
                    Isnull(ITMPD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,
                    NullIf(ITMPD.PurchaseUnit,'') AS PurchaseUnit,
                    Isnull(ITD.ChallanQuantity,0) AS ChallanQuantity,
                    Isnull(ITD.ApprovedQuantity,0) AS ApprovedQuantity,
                    Isnull(ITD.RejectedQuantity,0) AS RejectedQuantity,
                    nullif(ITD.QCApprovalNO,'') AS QCApprovalNO,
                    nullif(ITD.QCApprovedNarration,'') AS QCApprovedNarration,
                    NullIf(ITD.BatchNo,'') AS BatchNo,
                    NullIf(ITD.StockUnit,'') AS StockUnit,
                    Isnull(ITD.ReceiptWtPerPacking,0) AS ReceiptWtPerPacking,
                    Isnull(ITMPD.PurchaseTolerance,0) AS PurchaseTolerance,
                    Isnull(IM.WtPerPacking,0) AS WtPerPacking,
                    Isnull(IM.UnitPerPacking,1) AS UnitPerPacking,
                    Isnull(IM.ConversionFactor,1) AS ConversionFactor,
                    0 AS SizeW,
                    Isnull(ITD.WarehouseID,0) AS WarehouseID,
                    Nullif(WM.WarehouseName,'') AS Warehouse,
                    Nullif(WM.BinName,'') AS Bin,
                    Isnull(UOM.DecimalPlace,0)  AS UnitDecimalPlace,
                    NullIf(IM.Quality,'') AS ItemQuality
                FROM ItemTransactionMain AS ITM
                INNER JOIN ItemTransactionDetail AS ITD
                    ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID
                INNER JOIN ItemMaster AS IM
                    ON IM.ItemID=ITD.ItemID
                INNER JOIN ItemGroupMaster AS IGM
                    ON IGM.ItemGroupID=IM.ItemGroupID
                INNER JOIN ItemTransactionMain AS ITMP
                    ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID
                INNER JOIN ItemTransactionDetail AS ITMPD
                    ON ITMPD.TransactionID=ITMP.TransactionID
                    AND ITMPD.ItemID=IM.ItemID
                    AND ITMPD.TransactionID=ITD.PurchaseTransactionID
                    AND ITMPD.CompanyID=ITMP.CompanyID
                INNER JOIN WarehouseMaster AS WM
                    ON WM.WarehouseID=ITD.WarehouseID AND WM.CompanyID=ITD.CompanyID
                LEFT JOIN ItemSubGroupMaster AS ISGM
                    ON ISGM.ItemSubGroupID=IM.ItemSubGroupID
                LEFT JOIN UnitMaster AS UOM
                    ON UOM.UnitSymbol=IM.StockUnit
                LEFT JOIN ItemQCInspectionMain AS IQC
                    ON IQC.GRNTransactionID = ITD.TransactionID
                    AND IQC.ItemID = ITD.ItemID
                    AND IQC.BatchID = ITD.BatchID
                    AND Isnull(IQC.IsDeletedTransaction,0)=0
                WHERE ITM.VoucherID=-14
                    AND ITM.TransactionID=@TransactionID
                ORDER BY TransID";
        }

        var result = await connection.QueryAsync<GRNBatchDetailDto>(sql, new { TransactionID = transactionId });
        return result.ToList();
    }

    public async Task<(bool Success, string Message)> ApproveGRNAsync(ApproveGRNRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            // 1. Check if GRN is already processed (has child transactions)
            var checkSql = @"
                SELECT TOP 1 TransactionID
                FROM ItemTransactionDetail
                WHERE Isnull(IsDeletedTransaction, 0) = 0
                    AND ParentTransactionID = @GRNTransactionID
                    AND TransactionID <> ParentTransactionID";

            var exists = await connection.QueryFirstOrDefaultAsync<long?>(
                checkSql,
                new { GRNTransactionID = request.GRNTransactionID },
                transaction);

            if (exists.HasValue)
            {
                return (false, "This GRN is already processed and cannot be modified.");
            }

            // 2. If unapproving (Approved Receipt Note), check if QC samples exist
            if (request.RadioButtonValue == "Approved Receipt Note")
            {
                var qcCheckSql = @"
                    SELECT TOP 1 GRNTransactionID
                    FROM ItemQCInspectionMain
                    WHERE Isnull(IsDeletedTransaction, 0) = 0
                        AND GRNTransactionID = @GRNTransactionID";

                var qcExists = await connection.QueryFirstOrDefaultAsync<long?>(
                    qcCheckSql,
                    new { GRNTransactionID = request.GRNTransactionID },
                    transaction);

                if (qcExists.HasValue)
                {
                    return (false, "QC samples exist for this GRN. Cannot unapprove.");
                }
            }

            // 3. Update each item using DbOperations
            foreach (var item in request.Items)
            {
                var updateData = new
                {
                    ApprovedQuantity = item.ApprovedQuantity,
                    RejectedQuantity = item.RejectedQuantity,
                    QCApprovalNO = item.QCApprovalNO,
                    QCApprovedNarration = item.QCApprovedNarration,
                    IsVoucherItemApproved = item.IsVoucherItemApproved,
                    TransactionID = item.TransactionID,
                    ItemID = item.ItemID
                };

                await _dbOperations.UpdateDataAsync(
                    "ItemTransactionDetail",
                    updateData,
                    connection,
                    transaction,
                    new[] { "TransactionID", "ItemID" },
                    $"VoucherItemApprovedBy={userId},VoucherItemApprovedDate=Getdate()",
                    $"Isnull(IsDeletedTransaction, 0) = 0 AND ProductionUnitID={productionUnitId}");
            }

            // 4. Commit transaction
            await transaction.CommitAsync();

            // 5. Update stock for each item (AFTER commit)
            foreach (var item in request.Items)
            {
                try
                {
                    var updateStockSql = "EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, 0, @ItemID";
                    await connection.ExecuteAsync(updateStockSql,
                        new { CompanyID = companyId, ItemID = item.ItemID });
                }
                catch (Exception stockEx)
                {
                    _logger.LogError(stockEx, "Error updating stock for ItemID {ItemID}", item.ItemID);
                    // Continue with other items even if one fails
                }
            }

            return (true, "Success");
        }
        catch (Exception ex)
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Error rolling back transaction");
            }

            _logger.LogError(ex, "Error approving GRN");
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<string> CheckPermissionAsync(long transactionId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT TOP 1 TransactionID
            FROM ItemTransactionDetail
            WHERE Isnull(IsDeletedTransaction, 0) = 0
                AND ParentTransactionID = @TransactionID
                AND TransactionID <> ParentTransactionID";

        var exists = await connection.QueryFirstOrDefaultAsync<long?>(sql, new { TransactionID = transactionId });

        return exists.HasValue ? "Exist" : "";
    }
}
