using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Infrastructure.Extensions;
using IndasEstimo.Domain.Entities.Inventory;

namespace IndasEstimo.Infrastructure.Repositories.Inventory;

public class PurchaseGRNRepository : IPurchaseGRNRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PurchaseGRNRepository> _logger;

    public PurchaseGRNRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<PurchaseGRNRepository> logger)
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

    public async Task<List<PurchaseSupplierDto>> GetPurchaseSuppliersListAsync()
    {
        var companyId = _currentUserService.GetCompanyId();

        var sql = @"
            SELECT DISTINCT LM.LedgerID, LM.LedgerName
            FROM ItemTransactionMain AS ITM
            INNER JOIN LedgerMaster AS LM ON LM.LedgerID = ITM.LedgerID AND LM.CompanyID = ITM.CompanyID
            INNER JOIN LedgerGroupMaster AS LGM ON LGM.LedgerGroupID = LM.LedgerGroupID
                AND LGM.CompanyID = LM.CompanyID AND LGM.LedgerGroupNameID = 23
            WHERE ITM.CompanyID = @CompanyID AND ITM.VoucherID = -11
            ORDER BY LM.LedgerName";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<PurchaseSupplierDto>(sql, new { CompanyID = companyId });
        return result.ToList();
    }

    public async Task<List<PendingPurchaseOrderDto>> GetPendingOrdersListAsync()
    {
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        var sql = $@"
            SELECT
                ITM.TransactionID, ITD.ClientID, LM1.LedgerName AS ClientName, ITM.VoucherID, ITM.LedgerID,
                ITD.TransID, ITD.ItemID, ITD.ItemGroupID, IM.ItemSubGroupID, IGM.ItemGroupNameID, LM.LedgerName,
                ITM.MaxVoucherNo, ITM.VoucherNo AS PurchaseVoucherNo,
                REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS PurchaseVoucherDate,
                IM.ItemCode, IGM.ItemGroupName, ISGM.ItemSubGroupName, IM.ItemName,
                ISNULL(ITD.PurchaseOrderQuantity, 0) AS PurchaseOrderQuantity,
                ISNULL(ITD.PurchaseOrderQuantity, 0) AS PendingQty,
                ISNULL(IM.PurchaseUnit, '') AS PurchaseUnit, ISNULL(IM.StockUnit, '') AS StockUnit,
                ISNULL(ITD.PurchaseTolerance, 0) AS PurchaseTolerance,
                NULLIF(ISNULL(UA.UserName, ''), '') AS CreatedBy,
                NULLIF(ISNULL(UM.UserName, ''), '') AS ApprovedBy,
                NULLIF(ITD.RefJobCardContentNo, '') AS RefJobCardContentNo,
                NULLIF(ITD.FYear, '') AS FYear,
                ISNULL(ITD.ApprovedQuantity, 0) AS ApprovedQuantity,
                ISNULL(ITD.IsVoucherItemApproved, 0) AS IsVoucherItemApproved,
                NULLIF(ITM.PurchaseDivision, '') AS PurchaseDivision,
                NULLIF(ITD.Remark, '') AS Remark,
                ISNULL(IM.SizeW, 1) AS SizeW, ISNULL(IM.WtPerPacking, 0) AS WtPerPacking,
                ISNULL(IM.UnitPerPacking, 1) AS UnitPerPacking, ISNULL(IM.ConversionFactor, 1) AS ConversionFactor,
                NULLIF(C.ConversionFormula, '') AS FormulaStockToPurchaseUnit,
                ISNULL(C.ConvertedUnitDecimalPlace, 0) AS UnitDecimalPlacePurchaseUnit,
                (SELECT ROUND(SUM(ISNULL(ChallanQuantity, 0)), 3) AS Expr1
                 FROM ItemTransactionDetail
                 WHERE (PurchaseTransactionID = ITD.TransactionID) AND (ItemID = ITD.ItemID)
                   AND (ISNULL(IsDeletedTransaction, 0) <> 1)) AS ReceiptQuantity,
                NULLIF(CU.ConversionFormula, '') AS FormulaPurchaseToStockUnit,
                ISNULL(CU.ConvertedUnitDecimalPlace, 0) AS UnitDecimalPlaceStockUnit,
                ISNULL(IM.GSM, 0) AS GSM, ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM,
                ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM, ISNULL(IM.Thickness, 0) AS Thickness,
                ISNULL(IM.Density, 0) AS Density,
                PUM.ProductionUnitID, PUM.ProductionUnitName, CM.CompanyName, CM.CompanyID,
                JB.JobName, ISNULL(ITM.BiltyNo, '') AS BiltyNo,
                REPLACE(CONVERT(Varchar(13), ITM.BiltyDate, 106), ' ', '-') AS BiltyDate
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID = ITD.TransactionID
                AND ITM.CompanyID = ITD.CompanyID AND ISNULL(ITM.IsDeletedTransaction, 0) = 0
            INNER JOIN UserMaster AS UA ON UA.UserID = ITM.CreatedBy
            INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID
            INNER JOIN CompanyMaster AS CM ON CM.CompanyID = PUM.CompanyID
            INNER JOIN ItemMaster AS IM ON IM.ItemID = ITD.ItemID
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN LedgerMaster AS LM ON LM.LedgerID = ITM.LedgerID
            LEFT OUTER JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID = IM.ItemSubGroupID
                AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0
            LEFT OUTER JOIN UserMaster AS UM ON UM.UserID = ITD.VoucherItemApprovedBy
            LEFT OUTER JOIN ConversionMaster AS C ON C.BaseUnitSymbol = IM.StockUnit
                AND C.ConvertedUnitSymbol = IM.PurchaseUnit
            LEFT OUTER JOIN ConversionMaster AS CU ON CU.BaseUnitSymbol = IM.PurchaseUnit
                AND CU.ConvertedUnitSymbol = IM.StockUnit
            LEFT OUTER JOIN LedgerMaster AS LM1 ON LM1.LedgerID = ITD.ClientID
            LEFT JOIN JobBooking AS JB ON JB.BookingID = ITD.JobBookingID
                AND ISNULL(JB.IsDeletedTransaction, 0) = 0
            WHERE (ITM.VoucherID = -11)
              AND (ISNULL(ITD.IsDeletedTransaction, 0) <> 1)
              AND ITM.ProductionUnitID IN({productionUnitIdStr})
              AND (ISNULL(ITD.IsCompleted, 0) <> 1)
              AND (ISNULL(ITD.IsVoucherItemApproved, 0) = 1)
            ORDER BY ITM.TransactionID DESC";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<PendingPurchaseOrderDto>(sql);
        return result.ToList();
    }

    public async Task<List<ReceiptNoteListDto>> GetReceiptNoteListAsync(string fromDate, string toDate)
    {
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        var sql = $@"
            SELECT
                ISNULL(ITM.EWayBillNumber, '') AS EWayBillNumber,
                REPLACE(CONVERT(Varchar(13), ITM.EWayBillDate, 106), ' ', '-') AS EWayBillDate,
                ITM.TransactionID, NULLIF(ITD.RefJobCardContentNo, '') AS RefJobCardContentNo,
                ITD.PurchaseTransactionID, NULLIF(ITD.Remark, '') AS Remark, ITM.LedgerID, ITM.MaxVoucherNo,
                LM.LedgerName, ITM.VoucherNo AS ReceiptVoucherNo,
                REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS ReceiptVoucherDate,
                NULLIF(ITMP.VoucherNo, '') AS PurchaseVoucherNo,
                REPLACE(CONVERT(Varchar(13), ITMP.VoucherDate, 106), ' ', '-') AS PurchaseVoucherDate,
                ROUND(SUM(ISNULL(ITD.ChallanQuantity, 0)), 2) AS ChallanQuantity,
                NULLIF(ITM.DeliveryNoteNo, '') AS DeliveryNoteNo,
                REPLACE(CONVERT(Varchar(13), ITM.DeliveryNoteDate, 106), ' ', '-') AS DeliveryNoteDate,
                NULLIF(ITM.GateEntryNo, '') AS GateEntryNo,
                REPLACE(CONVERT(Varchar(13), ITM.GateEntryDate, 106), ' ', '-') AS GateEntryDate,
                NULLIF(ITM.LRNoVehicleNo, '') AS LRNoVehicleNo,
                NULLIF(ITM.Transporter, '') AS Transporter,
                NULLIF(EM.LedgerName, '') AS ReceiverName,
                NULLIF(ITM.Narration, '') AS Narration,
                ISNULL(ITM.GateEntryTransactionID, 0) AS GateEntryTransactionID,
                NULLIF(ITM.FYear, '') AS FYear,
                NULLIF(UM.UserName, '') AS CreatedBy,
                ISNULL(ITM.ReceivedBy, 0) AS ReceivedBy,
                ITD.IsVoucherItemApproved, ISNULL(CM.IsGRNApprovalRequired, 0) AS IsGRNApprovalRequired,
                ISNULL(PUM.ProductionUnitID, ITM.ProductionUnitID) AS ProductionUnitID,
                ISNULL(PUM.ProductionUnitName, '') AS ProductionUnitName,
                ISNULL(CM.CompanyName, '') AS CompanyName,
                ISNULL(CM.CompanyID, ITM.CompanyID) AS CompanyID,
                JB.JobName, ISNULL(ITM.BiltyNo, '') AS BiltyNo,
                REPLACE(CONVERT(Varchar(13), ITM.BiltyDate, 106), ' ', '-') AS BiltyDate
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID = ITD.TransactionID
                AND ITM.CompanyID = ITD.CompanyID AND ISNULL(ITM.IsDeletedTransaction, 0) = 0
                AND ISNULL(ITD.IsDeletedTransaction, 0) = 0
            LEFT JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID
            LEFT JOIN CompanyMaster AS CM ON CM.CompanyID = ISNULL(PUM.CompanyID, ITM.CompanyID)
            INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID = ITD.PurchaseTransactionID
                AND ISNULL(ITMP.IsDeletedTransaction, 0) = 0
            INNER JOIN LedgerMaster AS LM ON LM.LedgerID = ITM.LedgerID
                AND LM.IsDeletedTransaction = 0
            LEFT JOIN UserMaster AS UM ON UM.UserID = ITM.CreatedBy
            LEFT JOIN LedgerMaster AS EM ON EM.LedgerID = ITM.ReceivedBy
                AND EM.IsDeletedTransaction = 0
            LEFT JOIN JobBooking AS JB ON JB.BookingID = ITD.JobBookingID
                AND ISNULL(JB.IsDeletedTransaction, 0) = 0
            WHERE (ITM.VoucherID = -14)
              AND ITM.VoucherDate BETWEEN  @FromDate AND  @ToDate
              AND (ITM.ProductionUnitID IN({productionUnitIdStr}) OR ISNULL(ITM.ProductionUnitID, 0) = 0)
            GROUP BY ISNULL(ITM.EWayBillNumber, ''), REPLACE(CONVERT(Varchar(13), ITM.EWayBillDate, 106), ' ', '-'),
                ITM.TransactionID, ITD.PurchaseTransactionID, ITD.Remark, ITM.LedgerID, ITM.VoucherNo, ITM.VoucherDate,
                ITMP.VoucherNo, ITMP.VoucherDate, ITM.DeliveryNoteNo, ITM.DeliveryNoteDate, ITM.GateEntryNo,
                ITM.GateEntryDate, ITM.LRNoVehicleNo, ITM.Transporter, ITM.Narration, ISNULL(ITM.GateEntryTransactionID, 0),
                EM.LedgerName, LM.LedgerName, ITM.FYear, ITM.MaxVoucherNo, UM.UserName, ITM.ReceivedBy,
                ITD.RefJobCardContentNo, ITD.IsVoucherItemApproved, ISNULL(CM.IsGRNApprovalRequired, 0),
                ISNULL(PUM.ProductionUnitID, ITM.ProductionUnitID), ISNULL(PUM.ProductionUnitName, ''),
                ISNULL(CM.CompanyName, ''), ISNULL(CM.CompanyID, ITM.CompanyID),
                JB.JobName, ITM.BiltyDate, ITM.BiltyNo
            ORDER BY FYear DESC, ITM.MaxVoucherNo DESC";

        // Pass DD-MM-YYYY strings directly — SQL converts them via CONVERT(datetime, @param, 105)
        using var connection = GetConnection();
        var result = await connection.QueryAsync<ReceiptNoteListDto>(sql,
            new { FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    public async Task<List<ReceiptVoucherBatchDetailDto>> GetReceiptVoucherBatchDetailAsync(long transactionId)
    {
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        var sql = $@"
            SELECT
                ISNULL(REPLACE(ITD.MfgDate, '1900-01-01', ''), '') AS MFGdate,
                ISNULL(REPLACE(ITD.ExpiryDate, '1900-01-01', ''), '') AS ExpiryDate,
                NULLIF(ITD.SupplierBatchNo, '') AS SupplierBatchNo,
                NULLIF(ITMPD.RefJobCardContentNo, '') AS RefJobCardContentNo,
                ISNULL(ITD.PurchaseTransactionID, 0) AS PurchaseTransactionID,
                ISNULL(ITM.LedgerID, 0) AS LedgerID, ISNULL(ITD.TransID, 0) AS TransID,
                ISNULL(ITD.ItemID, 0) AS ItemID, ISNULL(IM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(IM.ItemSubGroupID, 0) AS ItemSubGroupID, ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID,
                NULLIF(ITMP.VoucherNo, '') AS PurchaseVoucherNo,
                REPLACE(CONVERT(Varchar(13), ITMP.VoucherDate, 106), ' ', '-') AS PurchaseVoucherDate,
                NULLIF(IM.ItemCode, '') AS ItemCode, NULLIF(IM.ItemName, '') AS ItemName,
                ISNULL(ITMPD.PurchaseOrderQuantity, 0) AS PurchaseOrderQuantity,
                NULLIF(ITMPD.PurchaseUnit, '') AS PurchaseUnit,
                ISNULL(ITD.ChallanQuantity, 0) AS ChallanQuantity,
                NULLIF(ITD.BatchNo, '') AS BatchNo, NULLIF(IM.StockUnit, '') AS StockUnit,
                ISNULL(ITD.ReceiptWtPerPacking, 0) AS ReceiptWtPerPacking,
                ISNULL(ITMPD.PurchaseTolerance, 0) AS PurchaseTolerance,
                ISNULL(IM.WtPerPacking, 0) AS WtPerPacking, ISNULL(IM.UnitPerPacking, 1) AS UnitPerPacking,
                ISNULL(IM.ConversionFactor, 1) AS ConversionFactor, ISNULL(IM.SizeW, 1) AS SizeW,
                ISNULL(ITD.WarehouseID, 0) AS WarehouseID,
                NULLIF(WM.WarehouseName, '') AS Warehouse, NULLIF(WM.BinName, '') AS Bin,
                ISNULL(
                    (SELECT SUM(ISNULL(ChallanQuantity, 0)) AS Expr1
                     FROM ItemTransactionDetail
                     WHERE (ISNULL(IsDeletedTransaction, 0) = 0)
                       AND (ISNULL(PurchaseTransactionID, 0) > 0)
                       AND (ISNULL(ChallanQuantity, 0) > 0)
                       AND (PurchaseTransactionID = ITMPD.TransactionID)
                       AND (ItemID = ITMPD.ItemID)), 0) AS ReceiptQuantity,
                NULLIF(CNM.ConversionFormula, '') AS FormulaStockToPurchaseUnit,
                ISNULL(CNM.ConvertedUnitDecimalPlace, 0) AS UnitDecimalPlacePurchaseUnit,
                NULLIF(CU.ConversionFormula, '') AS FormulaPurchaseToStockUnit,
                ISNULL(CU.ConvertedUnitDecimalPlace, 0) AS UnitDecimalPlaceStockUnit,
                ISNULL(IM.GSM, 0) AS GSM, ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM,
                ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM, ISNULL(IM.Thickness, 0) AS Thickness,
                ISNULL(IM.Density, 0) AS Density, ISGM.ItemSubGroupName,
                PUM.ProductionUnitID, PUM.ProductionUnitName, CM.CompanyName
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID = ITD.TransactionID
                AND ITM.CompanyID = ITD.CompanyID AND ISNULL(ITM.IsDeletedTransaction, 0) = 0
                AND ISNULL(ITD.IsDeletedTransaction, 0) = 0
            INNER JOIN ItemMaster AS IM ON IM.ItemID = ITD.ItemID
            INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID
            INNER JOIN CompanyMaster AS CM ON CM.CompanyID = PUM.CompanyID
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID = ITD.PurchaseTransactionID
            INNER JOIN ItemTransactionDetail AS ITMPD ON ITMPD.TransactionID = ITMP.TransactionID
                AND ITMPD.ItemID = IM.ItemID AND ITMPD.TransactionID = ITD.PurchaseTransactionID
                AND ISNULL(ITMP.IsDeletedTransaction, 0) = 0
                AND ISNULL(ITMPD.IsDeletedTransaction, 0) = 0
            INNER JOIN WarehouseMaster AS WM ON WM.WarehouseID = ITD.WarehouseID
                AND WM.CompanyID = ITD.CompanyID
            LEFT OUTER JOIN ConversionMaster AS CNM ON CNM.BaseUnitSymbol = IM.StockUnit
                AND CNM.ConvertedUnitSymbol = IM.PurchaseUnit
            LEFT OUTER JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID = IM.ItemSubGroupID
                AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0
            LEFT OUTER JOIN ConversionMaster AS CU ON CU.BaseUnitSymbol = IM.PurchaseUnit
                AND CU.ConvertedUnitSymbol = IM.StockUnit
            WHERE (ITM.VoucherID = -14)
              AND ITM.TransactionID = @TransactionID
              AND ITM.ProductionUnitID IN({productionUnitIdStr})
            ORDER BY TransID";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<ReceiptVoucherBatchDetailDto>(sql,
            new { TransactionID = transactionId });
        return result.ToList();
    }

    public async Task<PreviousReceivedQuantityDto?> GetPreviousReceivedQuantityAsync(
        long purchaseTransactionId, long itemId, long grnTransactionId)
    {
        var sql = @"
            SELECT
                ISNULL(PTM.TransactionID, 0) AS TransactionID, ISNULL(PTD.ItemID, 0) AS ItemID,
                ISNULL(PTD.PurchaseTolerance, 0) AS PurchaseTolerance,
                ISNULL(PTD.PurchaseOrderQuantity, 0) AS PurchaseOrderQuantity, IM.PurchaseUnit,
                ISNULL(
                    (SELECT SUM(ISNULL(ChallanQuantity, 0)) AS Expr1
                     FROM ItemTransactionDetail
                     WHERE (ISNULL(ChallanQuantity, 0) > 0)
                       AND (PurchaseTransactionID = PTM.TransactionID)
                       AND TransactionID <> @GRNTransactionID
                       AND (ItemID = PTD.ItemID) AND (CompanyID = PTM.CompanyID)
                       AND (ISNULL(IsDeletedTransaction, 0) <> 1)), 0) AS PreReceiptQuantity,
                IM.StockUnit, NULLIF(C.ConversionFormula, '') AS FormulaPurchaseToStockUnit,
                ISNULL(C.ConvertedUnitDecimalPlace, 0) AS UnitDecimalPlaceStockUnit,
                PUM.ProductionUnitID, PUM.ProductionUnitName, CM.CompanyName
            FROM ItemTransactionMain AS PTM
            INNER JOIN ItemTransactionDetail AS PTD ON PTD.TransactionID = PTM.TransactionID
                AND PTM.CompanyID = PTD.CompanyID AND ISNULL(PTM.IsDeletedTransaction, 0) = 0
                AND ISNULL(PTD.IsDeletedTransaction, 0) = 0
            INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = PTM.ProductionUnitID
            INNER JOIN CompanyMaster AS CM ON CM.CompanyID = PUM.CompanyID
            INNER JOIN ItemMaster AS IM ON IM.ItemID = PTD.ItemID
            LEFT OUTER JOIN ConversionMaster AS C ON C.BaseUnitSymbol = IM.PurchaseUnit
                AND C.ConvertedUnitSymbol = IM.StockUnit
            WHERE (PTM.VoucherID = -11)
              AND PTM.TransactionID = @PurchaseTransactionID
              AND PTD.ItemID = @ItemID";

        using var connection = GetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<PreviousReceivedQuantityDto>(sql,
            new { PurchaseTransactionID = purchaseTransactionId, ItemID = itemId, GRNTransactionID = grnTransactionId });
        return result;
    }

    public async Task<List<ReceiverDto>> GetReceiverListAsync()
    {
        var sql = @"
            SELECT DISTINCT LM.LedgerID, LM.LedgerName
            FROM LedgerMaster AS LM
            LEFT JOIN DepartmentMaster AS DM ON DM.DepartmentID = LM.DepartmentID
            INNER JOIN LedgerGroupMaster AS LGM ON LGM.LedgerGroupID = LM.LedgerGroupID
                AND LGM.LedgerGroupNameID = 27
            WHERE ISNULL(LM.IsDeletedTransaction, 0) = 0
              AND DM.DepartmentName LIKE '%Inventory%'
            ORDER BY LM.LedgerName";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<ReceiverDto>(sql);
        return result.ToList();
    }

    public async Task<List<WarehouseDto>> GetWarehouseListAsync()
    {
        var companyId = _currentUserService.GetCompanyId();
        var productionUnitId = _currentUserService.GetProductionUnitId();

        var sql = @"
            SELECT DISTINCT WarehouseName AS Warehouse
            FROM WarehouseMaster
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
              AND ISNULL(WarehouseName, '') <> ''
              AND CompanyID = @CompanyID
              AND ISNULL(ProductionUnitID, 0) = @ProductionUnitID
              AND ISNULL(IsFloorWarehouse, 0) = 0
            ORDER BY WarehouseName";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<WarehouseDto>(sql,
            new { CompanyID = companyId, ProductionUnitID = productionUnitId });
        return result.ToList();
    }

    public async Task<List<BinDto>> GetBinsListAsync(string warehouseName)
    {
        var productionUnitId = _currentUserService.GetProductionUnitId();

        string sql;
        if (string.IsNullOrEmpty(warehouseName))
        {
            sql = @"
                SELECT DISTINCT NULLIF(BinName, '') AS Bin, ISNULL(WarehouseID, 0) AS WarehouseID
                FROM WarehouseMaster
                WHERE ISNULL(IsDeletedTransaction, 0) = 0
                  AND ISNULL(BinName, '') <> ''
                  AND ISNULL(ProductionUnitID, 0) = @ProductionUnitID
                ORDER BY Bin";
        }
        else
        {
            sql = @"
                SELECT DISTINCT NULLIF(BinName, '') AS Bin, ISNULL(WarehouseID, 0) AS WarehouseID
                FROM WarehouseMaster
                WHERE ISNULL(IsDeletedTransaction, 0) = 0
                  AND WarehouseName = @WarehouseName
                  AND ISNULL(BinName, '') <> ''
                  AND ISNULL(ProductionUnitID, 0) = @ProductionUnitID
                ORDER BY Bin";
        }

        using var connection = GetConnection();
        var result = await connection.QueryAsync<BinDto>(sql,
            new { WarehouseName = warehouseName, ProductionUnitID = productionUnitId });
        return result.ToList();
    }

    public async Task<List<GatePassDto>> GetGatePassAsync(long ledgerId)
    {
        var companyId = _currentUserService.GetCompanyId();

        var sql = @"
            SELECT
                GPM.TransactionID, GPM.VoucherID, ISNULL(GP.VoucherNo, '') AS DCNo,
                GPM.VoucherNo, REPLACE(CONVERT(varchar, GPM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                GPM.Prefix, GPM.GatePassEntryType AS GateEntryType, GPM.VehicleNo, GPM.Remark,
                GPM.LedgerID,
                CASE WHEN ISNULL(LM.LedgerName, '') = '' THEN GPM.MaterialSentTo
                     ELSE ISNULL(LM.LedgerName, '') END AS MaterialSentTo,
                GPM.MaterialSentThrough AS SendThrough, GPM.MaterialSentThroughName AS SendThroughName,
                GPM.GatePassTransactionID, NULLIF(GPM.DocumentNo, '') AS DocumentNo
            FROM GatePassEntryMain AS GPM
            LEFT JOIN GatePassEntryMain AS GP ON GP.TransactionID = GPM.GatePassTransactionID
            LEFT JOIN LedgerMaster AS LM ON LM.LedgerID = GPM.LedgerID
            WHERE GPM.VoucherID = '-129'
              AND GPM.GatePassEntryType LIKE '%Item%'
              AND ISNULL(GPM.IsDeletedTransaction, 0) <> 1
              AND GPM.CompanyId = @CompanyID
              AND GPM.LedgerID = @LedgerID
              AND GPM.TransactionID NOT IN (
                  SELECT DISTINCT ISNULL(GateEntryTransactionID, 0) AS GateEntryTransactionID
                  FROM ItemTransactionMain
                  WHERE ISNULL(IsDeletedTransaction, 0) <> 1)";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<GatePassDto>(sql,
            new { CompanyID = companyId, LedgerID = ledgerId });
        return result.ToList();
    }

    public async Task<List<GRNItemDto>> GetGrnItemListAsync(long transactionId)
    {
        var companyId = _currentUserService.GetCompanyId();

        var sql = @"
            SELECT
                REPLACE(CONVERT(Varchar(13), A.VoucherDate, 106), ' ', '-') AS VoucherDate,
                A.TransactionID, A.VoucherNo, NULLIF(B.SupplierBatchNo, '') AS SupplierBatchNo,
                NULLIF(B.BatchNo, '') AS BatchNo, ISNULL(B.ItemID, 0) AS ItemID,
                NULLIF(IM.ItemCode, '') AS ItemCode, NULLIF(IM.ItemName, '') AS ItemName,
                ISNULL(IM.UnitPerPacking, 1) AS UnitPerPacking
            FROM ItemTransactionMain AS A
            INNER JOIN ItemTransactionDetail AS B ON A.TransactionID = B.TransactionID
                AND ISNULL(B.IsDeletedTransaction, 0) = 0
            INNER JOIN ItemMaster AS IM ON IM.ItemID = B.ItemID AND IM.CompanyID = B.CompanyID
                AND ISNULL(IM.IsDeletedTransaction, 0) = 0
            WHERE A.VoucherID = -14
              AND A.TransactionID = @TransactionID
              AND A.CompanyID = @CompanyID
              AND ISNULL(A.IsDeletedTransaction, 0) = 0";

        using var connection = GetConnection();
        var result = await connection.QueryAsync<GRNItemDto>(sql,
            new { TransactionID = transactionId, CompanyID = companyId });
        return result.ToList();
    }

    public async Task<UserAuthorityDto?> GetUserAuthorityAsync()
    {
        var userId = _currentUserService.GetUserId();

        var sql = @"
            SELECT CanReceiveExcessMaterial
            FROM UserMaster
            WHERE ISNULL(IsDeletedUser, 0) <> 1
              AND UserID = @UserID";

        using var connection = GetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<UserAuthorityDto>(sql,
            new { UserID = userId });
        return result;
    }

    public async Task<string> ValidateSupplierBatchReceiptDataAsync(int voucherID, List<SupplierBatchItem> items)
    {
        var companyId = _currentUserService.GetCompanyId();

        using var connection = GetConnection();

        foreach (var item in items)
        {
            var sql = @"
                SELECT ITD.SupplierBatchNo AS SupplierBatchNo
                FROM ItemTransactionDetail ITD
                INNER JOIN ItemTransactionMain AS ITM ON ITM.TransactionID = ITD.TransactionID
                    AND ITM.CompanyID = ITD.CompanyID
                WHERE ITM.LedgerID = @LedgerID
                  AND ITD.SupplierBatchNo = @SupplierBatchNo
                  AND ITM.CompanyID = @CompanyID
                  AND ISNULL(ITM.IsDeletedTransaction, 0) = 0
                  AND ISNULL(ITD.IsDeletedTransaction, 0) = 0
                  AND ITM.VoucherID = @VoucherID";

            var existingBatch = await connection.QueryFirstOrDefaultAsync<string>(sql,
                new {
                    LedgerID = item.LedgerID,
                    SupplierBatchNo = item.SupplierBatchNo,
                    CompanyID = companyId,
                    VoucherID = voucherID
                });

            if (!string.IsNullOrEmpty(existingBatch))
            {
                return $"Supplier batch no - '{item.SupplierBatchNo}' already saved.";
            }
        }

        return "Success";
    }

    public async Task<string> CheckPermissionAsync(long transactionId)
    {
        var companyId = _currentUserService.GetCompanyId();
        var productionUnitId = _currentUserService.GetProductionUnitId();

        using var connection = GetConnection();

        // Check if GRN approval is required
        var approvalSql = @"
            SELECT ISNULL(IsGRNApprovalRequired, 0) AS IsGRNApprovalRequired
            FROM CompanyMaster
            WHERE CompanyID = @CompanyID";

        var isGRNApprovalRequired = await connection.QueryFirstOrDefaultAsync<bool>(approvalSql,
            new { CompanyID = companyId });

        // Check if transaction is used in another process
        var existSql1 = @"
            SELECT *
            FROM ItemTransactionDetail
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
              AND ParentTransactionID = @TransactionID
              AND ProductionUnitID = @ProductionUnitID
              AND TransactionID <> ParentTransactionID";

        var exists1 = await connection.QueryFirstOrDefaultAsync(existSql1,
            new { TransactionID = transactionId, ProductionUnitID = productionUnitId });

        if (exists1 != null)
        {
            return "Exist";
        }

        // Check if GRN is approved (has QC approval)
        if (isGRNApprovalRequired)
        {
            var existSql2 = @"
                SELECT *
                FROM ItemTransactionDetail
                WHERE ISNULL(IsDeletedTransaction, 0) = 0
                  AND ISNULL(QCApprovalNo, '') <> ''
                  AND TransactionID = @TransactionID
                  AND ProductionUnitID = @ProductionUnitID
                  AND (ISNULL(ApprovedQuantity, 0) > 0 OR ISNULL(RejectedQuantity, 0) > 0)";

            var exists2 = await connection.QueryFirstOrDefaultAsync(existSql2,
                new { TransactionID = transactionId, ProductionUnitID = productionUnitId });

            if (exists2 != null)
            {
                return "Exist";
            }
        }

        return "";
    }

    public async Task<string> GetLastTransactionDateAsync()
    {
        var companyId = _currentUserService.GetCompanyId();
        var fYear = _currentUserService.GetFYear();

        var sql = @"
            SELECT TOP 1 CONVERT(VARCHAR(10), VoucherDate, 120) AS VoucherDate
            FROM ItemTransactionMain
            WHERE VoucherID = -14
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
              AND FYear = @FYear
            ORDER BY VoucherDate DESC";

        using var connection = GetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<string>(sql,
            new { CompanyID = companyId, FYear = fYear });
        return result ?? "";
    }

    public async Task<string> GetNextVoucherNoAsync(string prefix)
    {
        var (voucherNo, _) = await _dbOperations.GenerateVoucherNoAsync(
            "ItemTransactionMain", -14, prefix);
        return voucherNo;
    }

    public async Task<(bool Success, string VoucherNo, long TransactionID, string Message)> SaveReceiptDataAsync(
        SaveReceiptDataRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        bool isTransactionCommitted = false;
        string voucherNo = "";
        long transactionId = 0;
        long companyId = 0;

        try
        {
            // 1. Generate voucher number
            var (vNo, maxVoucherNo) = await _dbOperations.GenerateVoucherNoAsync(
                "ItemTransactionMain",
                request.VoucherID,
                request.Prefix);
            voucherNo = vNo;

            // 2. Prepare main data entity
            var mainEntity = new Domain.Entities.Inventory.PurchaseGRN
            {
                VoucherID = request.MainData.VoucherID,
                LedgerID = request.MainData.LedgerID,
                VoucherDate = DateTime.TryParse(request.MainData.VoucherDate, out var vDate) ? vDate : null,
                DeliveryNoteNo = request.MainData.DeliveryNoteNo,
                DeliveryNoteDate = DateTime.TryParse(request.MainData.DeliveryNoteDate, out var dnDate) ? dnDate : null,
                GateEntryNo = request.MainData.GateEntryNo,
                GateEntryDate = DateTime.TryParse(request.MainData.GateEntryDate, out var geDate) ? geDate : null,
                LRNoVehicleNo = request.MainData.LRNoVehicleNo,
                Transporter = request.MainData.Transporter,
                ReceivedBy = request.MainData.ReceivedBy,
                Narration = request.MainData.Narration,
                GateEntryTransactionID = request.MainData.GateEntryTransactionID,
                BiltyNo = request.MainData.BiltyNo,
                BiltyDate = DateTime.TryParse(request.MainData.BiltyDate, out var bDate) ? bDate : null,
                EWayBillNumber = request.MainData.EWayBillNumber,
                EWayBillDate = DateTime.TryParse(request.MainData.EWayBillDate, out var ewDate) ? ewDate : null
            };

            // 3. Insert main record (audit fields handled by DbOperations)
            transactionId = await _dbOperations.InsertDataAsync(
                "ItemTransactionMain",
                mainEntity,
                connection,
                transaction,
                "TransactionID",
                $"VoucherPrefix,MaxVoucherNo,VoucherNo",
                $"'{request.Prefix}',{maxVoucherNo},'{voucherNo}'");

            // 4. Prepare detail data entities
            var detailEntities = request.DetailData.Select(d => new Domain.Entities.Inventory.PurchaseGRNDetail
            {
                PurchaseTransactionID = d.PurchaseTransactionID,
                ItemID = d.ItemID,
                ItemGroupID = d.ItemGroupID,
                ChallanQuantity = d.ChallanQuantity,
                BatchNo = d.BatchNo,
                SupplierBatchNo = d.SupplierBatchNo,
                MfgDate = DateTime.TryParse(d.MfgDate, out var mfg) ? mfg : null,
                ExpiryDate = DateTime.TryParse(d.ExpiryDate, out var exp) ? exp : null,
                WarehouseID = d.WarehouseID,
                ReceiptWtPerPacking = d.ReceiptWtPerPacking,
                PurchaseUnit = d.PurchaseUnit,
                RefJobCardContentNo = d.RefJobCardContentNo,
                Remark = d.Remark,
                JobBookingID = d.JobBookingID
            }).ToList();

            // 5. Insert detail records (parent linkage handled by DbOperations)
            await _dbOperations.InsertDataAsync(
                "ItemTransactionDetail",
                detailEntities,
                connection,
                transaction,
                "TransactionDetailID",
                "",
                "",
                "Receipt Note",
                transactionId);

            // 6. Mark completed PO items
            var productionUnitId = _currentUserService.GetProductionUnitId();
            var userId = _currentUserService.GetUserId();
            foreach (var poItem in request.CompletedPOItems)
            {
                var updatePOSql = @"
                    UPDATE ItemTransactionDetail
                    SET IsCompleted = 1, CompletedDate = GETDATE(), CompletedBy = @UserID
                    WHERE TransactionID = @PurchaseTransactionID
                      AND ItemID = @ItemID
                      AND ProductionUnitID = @ProductionUnitID";

                await connection.ExecuteAsync(updatePOSql,
                    new {
                        poItem.PurchaseTransactionID,
                        poItem.ItemID,
                        UserID = userId,
                        ProductionUnitID = productionUnitId
                    }, transaction);
            }

            // 7. Update BatchID
            var updateBatchSql = @"
                UPDATE ItemTransactionDetail
                SET BatchID = TransactionDetailID
                WHERE TransactionID = @TransactionID";

            await connection.ExecuteAsync(updateBatchSql, new { TransactionID = transactionId }, transaction);

            // 8. Insert into batch detail table
            companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear();
            var insertBatchSql = @"
                INSERT INTO ItemTransactionBatchDetail (
                    BatchID, BatchNo, SupplierBatchNo, MfgDate, ExpiryDate,
                    CompanyID, FYear, CreatedBy, CreatedDate
                )
                SELECT BatchID, BatchNo, SupplierBatchNo, MfgDate, ExpiryDate,
                       CompanyID, FYear, CreatedBy, CreatedDate
                FROM ItemTransactionDetail
                WHERE TransactionID = @TransactionID";

            await connection.ExecuteAsync(insertBatchSql, new { TransactionID = transactionId }, transaction);

            // 9. Commit transaction
            await transaction.CommitAsync();
            isTransactionCommitted = true;

            // 10. Update stock (AFTER transaction commit) - separate try-catch
            try
            {
                var updateStockSql = "EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, @TransactionID, 0";
                await connection.ExecuteAsync(updateStockSql,
                    new { CompanyID = companyId, TransactionID = transactionId });
            }
            catch (Exception stockEx)
            {
                _logger.LogError(stockEx, "Error updating stock for GRN {TransactionID}. GRN was saved but stock update failed.", transactionId);
                // GRN is already saved, so we still return success but with a warning
                return (true, voucherNo, transactionId, "GRN saved but stock update failed: " + stockEx.Message);
            }

            return (true, voucherNo, transactionId, "Success");
        }
        catch (Exception ex)
        {
            if (!isTransactionCommitted)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error rolling back transaction");
                }
            }

            _logger.LogError(ex, "Error saving GRN");
            return (false, "", 0, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateReceiptDataAsync(UpdateReceiptDataRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        bool isTransactionCommitted = false;
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;
        var fYear = _currentUserService.GetFYear();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        try
        {
            // Check if can be updated
            var checkSql = @"
                SELECT TOP 1 TransactionID
                FROM ItemTransactionDetail
                WHERE ISNULL(ParentTransactionID, 0) = @TransactionID
                  AND TransactionID <> @TransactionID
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            var exists = await connection.QueryFirstOrDefaultAsync<long?>(checkSql,
                new { TransactionID = request.TransactionID }, transaction);

            if (exists.HasValue)
            {
                return (false, "This GRN is already processed and cannot be updated.");
            }

            // 1. Prepare main data for update (exclude TransactionID - it's IDENTITY column)
            var updateData = new
            {
                LedgerID = request.MainData.LedgerID,
                VoucherDate = DateTime.TryParse(request.MainData.VoucherDate, out var vDate) ? vDate : (DateTime?)null,
                DeliveryNoteNo = request.MainData.DeliveryNoteNo,
                DeliveryNoteDate = DateTime.TryParse(request.MainData.DeliveryNoteDate, out var dnDate) ? dnDate : (DateTime?)null,
                GateEntryNo = request.MainData.GateEntryNo,
                GateEntryDate = DateTime.TryParse(request.MainData.GateEntryDate, out var geDate) ? geDate : (DateTime?)null,
                LRNoVehicleNo = request.MainData.LRNoVehicleNo,
                Transporter = request.MainData.Transporter,
                ReceivedBy = request.MainData.ReceivedBy,
                Narration = request.MainData.Narration,
                GateEntryTransactionID = request.MainData.GateEntryTransactionID,
                BiltyNo = request.MainData.BiltyNo,
                BiltyDate = DateTime.TryParse(request.MainData.BiltyDate, out var bDate) ? bDate : (DateTime?)null,
                EWayBillNumber = request.MainData.EWayBillNumber,
                EWayBillDate = DateTime.TryParse(request.MainData.EWayBillDate, out var ewDate) ? ewDate : (DateTime?)null,
                TransactionID = request.TransactionID  // Include for WHERE clause only
            };

            // 2. Update main record using DbOperations
            await _dbOperations.UpdateDataAsync(
                "ItemTransactionMain",
                updateData,
                connection,
                transaction,
                new[] { "TransactionID" },  // Use TransactionID in WHERE clause
                "");

            // 3. Delete existing batch details first
            var deleteBatchSql = @"
                DELETE FROM ItemTransactionBatchDetail
                WHERE BatchID IN (SELECT TransactionDetailID FROM ItemTransactionDetail WHERE TransactionID = @TransactionID)";

            await connection.ExecuteAsync(deleteBatchSql,
                new { TransactionID = request.TransactionID }, transaction);

            // 4. Delete existing details
            var deleteDetailSql = @"
                DELETE FROM ItemTransactionDetail
                WHERE CompanyID = @CompanyID
                  AND TransactionID = @TransactionID";

            await connection.ExecuteAsync(deleteDetailSql,
                new { CompanyID = companyId, TransactionID = request.TransactionID }, transaction);

            // 5. Prepare detail data entities
            var detailEntities = request.DetailData.Select(d => new Domain.Entities.Inventory.PurchaseGRNDetail
            {
                PurchaseTransactionID = d.PurchaseTransactionID,
                ItemID = d.ItemID,
                ItemGroupID = d.ItemGroupID,
                ChallanQuantity = d.ChallanQuantity,
                BatchNo = d.BatchNo,
                SupplierBatchNo = d.SupplierBatchNo,
                MfgDate = DateTime.TryParse(d.MfgDate, out var mfg) ? mfg : null,
                ExpiryDate = DateTime.TryParse(d.ExpiryDate, out var exp) ? exp : null,
                WarehouseID = d.WarehouseID,
                ReceiptWtPerPacking = d.ReceiptWtPerPacking,
                PurchaseUnit = d.PurchaseUnit,
                RefJobCardContentNo = d.RefJobCardContentNo,
                Remark = d.Remark,
                JobBookingID = d.JobBookingID
            }).ToList();

            // 6. Insert new detail records using DbOperations
            await _dbOperations.InsertDataAsync(
                "ItemTransactionDetail",
                detailEntities,
                connection,
                transaction,
                "TransactionDetailID",
                "",
                "",
                "Receipt Note",
                request.TransactionID);

            // 7. Update BatchID
            var updateBatchSql = @"
                UPDATE ItemTransactionDetail
                SET BatchID = TransactionDetailID
                WHERE TransactionID = @TransactionID";

            await connection.ExecuteAsync(updateBatchSql, new { TransactionID = request.TransactionID }, transaction);

            // 8. Insert into batch detail table
            var insertBatchSql = @"
                INSERT INTO ItemTransactionBatchDetail (
                    BatchID, BatchNo, SupplierBatchNo, MfgDate, ExpiryDate,
                    CompanyID, FYear, CreatedBy, CreatedDate
                )
                SELECT BatchID, BatchNo, SupplierBatchNo, MfgDate, ExpiryDate,
                       CompanyID, FYear, CreatedBy, CreatedDate
                FROM ItemTransactionDetail
                WHERE TransactionID = @TransactionID";

            await connection.ExecuteAsync(insertBatchSql,
                new { TransactionID = request.TransactionID }, transaction);

            // 9. Commit transaction
            await transaction.CommitAsync();
            isTransactionCommitted = true;

            // 10. Update stock (AFTER transaction commit) - separate try-catch
            try
            {
                var updateStockSql = "EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, @TransactionID, 0";
                await connection.ExecuteAsync(updateStockSql,
                    new { CompanyID = companyId, TransactionID = request.TransactionID });
            }
            catch (Exception stockEx)
            {
                _logger.LogError(stockEx, "Error updating stock for GRN {TransactionID}. GRN was updated but stock update failed.", request.TransactionID);
                return (true, "GRN updated but stock update failed: " + stockEx.Message);
            }

            return (true, "Success");
        }
        catch (Exception ex)
        {
            if (!isTransactionCommitted)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error rolling back transaction");
                }
            }

            _logger.LogError(ex, "Error updating GRN");
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteGRNAsync(DeleteGRNRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        bool isTransactionCommitted = false;
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        try
        {
            // Check if used in another process
            var checkSql = @"
                SELECT TransactionID
                FROM ItemPurchaseInvoiceDetail
                WHERE ISNULL(IsDeletedTransaction, 0) = 0
                  AND ParentTransactionID = @TransactionID";

            var exists = await connection.QueryFirstOrDefaultAsync<long?>(checkSql,
                new { TransactionID = request.TransactionID }, transaction);

            if (exists.HasValue)
            {
                return (false, "This transaction is used in another process. Record cannot be deleted.");
            }

            // Soft delete main record
            var deleteMainSql = @"
                UPDATE ItemTransactionMain
                SET DeletedBy = @UserID, DeletedDate = GETDATE(), IsDeletedTransaction = 1
                WHERE TransactionID = @TransactionID";

            await connection.ExecuteAsync(deleteMainSql,
                new { TransactionID = request.TransactionID, UserID = userId }, transaction);

            // Soft delete detail records
            var deleteDetailSql = @"
                UPDATE ItemTransactionDetail
                SET DeletedBy = @UserID, DeletedDate = GETDATE(), IsDeletedTransaction = 1
                WHERE TransactionID = @TransactionID";

            await connection.ExecuteAsync(deleteDetailSql,
                new { TransactionID = request.TransactionID, UserID = userId }, transaction);

            // Mark PO items as not completed
            foreach (var poItem in request.CompletedPOItems)
            {
                var updatePOSql = @"
                    UPDATE ItemTransactionDetail
                    SET IsCompleted = 0, CompletedDate = GETDATE(), CompletedBy = @UserID
                    WHERE CompanyID = @CompanyID
                      AND TransactionID = @PurchaseTransactionID
                      AND ItemID = @ItemID";

                await connection.ExecuteAsync(updatePOSql,
                    new {
                        CompanyID = companyId,
                        poItem.PurchaseTransactionID,
                        poItem.ItemID,
                        UserID = userId
                    }, transaction);
            }

            // Commit transaction
            await transaction.CommitAsync();
            isTransactionCommitted = true;

            // Update stock (AFTER transaction commit) - separate try-catch
            try
            {
                var updateStockSql = "EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, @TransactionID, 0";
                await connection.ExecuteAsync(updateStockSql,
                    new { CompanyID = companyId, TransactionID = request.TransactionID });
            }
            catch (Exception stockEx)
            {
                _logger.LogError(stockEx, "Error updating stock for GRN {TransactionID}. GRN was deleted but stock update failed.", request.TransactionID);
                return (true, "GRN deleted but stock update failed: " + stockEx.Message);
            }

            return (true, "Success");
        }
        catch (Exception ex)
        {
            if (!isTransactionCommitted)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error rolling back transaction");
                }
            }

            _logger.LogError(ex, "Error deleting GRN");
            return (false, $"Error: {ex.Message}");
        }
    }
}
