namespace IndasEstimo.Domain.Entities.Inventory;

/// <summary>
/// Main Item Issue Direct header (ItemTransactionMain)
/// </summary>
public class ItemIssueDirectMain
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public string VoucherPrefix { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }

    // Job Details
    public long? JobBookingID { get; set; }
    public long? JobBookingJobCardContentsID { get; set; }
    public long? LedgerID { get; set; }
    public long? DepartmentID { get; set; }
    public long? MachineID { get; set; }
    public long? ProcessID { get; set; }

    // Warehouse/Bin
    public long FloorGodownID { get; set; }
    public long BinID { get; set; }

    // Slip Details
    public string SlipNo { get; set; } = string.Empty;
    public DateTime SlipDate { get; set; }

    // Quantities
    public decimal TotalIssueQuantity { get; set; }
    public decimal IssuedQuantity { get; set; }

    // Other
    public string? Narration { get; set; }
    public string? JobCardNo { get; set; }
    public string? BookingNo { get; set; }
    public decimal? RequiredSheets { get; set; }
    public decimal? AllocatedSheets { get; set; }
    public decimal? RequiredQuantity { get; set; }
    public string? JobName { get; set; }
    public string? ContentName { get; set; }

    // Audit fields
    public long CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public long UserID { get; set; }
    public long CreatedBy { get; set; }
    public long ModifiedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// Item Issue Direct line items (ItemTransactionDetail)
/// </summary>
public class ItemIssueDirectDetail
{
    public long TransactionDetailID { get; set; }
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public long ItemGroupID { get; set; }
    public long? ItemSubGroupID { get; set; }

    // Job/Picklist References
    public long? JobBookingID { get; set; }
    public long? JobBookingJobCardContentsID { get; set; }
    public long? PicklistReleaseTransactionID { get; set; }
    public long? PicklistTransactionID { get; set; }

    // Department/Machine/Process
    public long? DepartmentID { get; set; }
    public long? MachineID { get; set; }
    public long? ProcessID { get; set; }

    // Warehouse/Bin/Batch
    public long FloorGodownID { get; set; }
    public long BinID { get; set; }
    public long? BatchID { get; set; }
    public string? BatchNo { get; set; }
    public string? SupplierBatchNo { get; set; }
    public string? GRNNo { get; set; }
    public DateTime? GRNDate { get; set; }

    // Quantities
    public decimal IssueQuantity { get; set; }
    public decimal? ReleaseQuantity { get; set; }
    public decimal? PendingQuantity { get; set; }
    public decimal? PhysicalStock { get; set; }
    public decimal? AllocatedStock { get; set; }
    public decimal? FreeStock { get; set; }
    public decimal? BatchStock { get; set; }
    public int? AgeingDays { get; set; }

    // Display fields (denormalized for reporting)
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? ItemGroupName { get; set; }
    public string? ItemSubGroupName { get; set; }
    public string? StockUnit { get; set; }
    public string? PicklistNo { get; set; }
    public string? ReleaseNo { get; set; }
    public string? JobCardNo { get; set; }
    public string? JobName { get; set; }
    public string? ContentName { get; set; }
    public string? ProcessName { get; set; }
    public string? MachineName { get; set; }
    public string? DepartmentName { get; set; }
    public string? Warehouse { get; set; }
    public string? Bin { get; set; }

    // Conversions
    public decimal? WtPerPacking { get; set; }
    public decimal? UnitPerPacking { get; set; }
    public decimal? ConversionFactor { get; set; }
    public int? UnitDecimalPlace { get; set; }

    // Audit fields
    public long CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public long UserID { get; set; }
    public long CreatedBy { get; set; }
    public long ModifiedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// Item Issue Consume Main (for job card consumption tracking)
/// </summary>
public class ItemIssueDirectConsumeMain
{
    public long ConsumeMainID { get; set; }
    public long TransactionID { get; set; }
    public long? JobBookingJobCardContentsID { get; set; }
    public decimal ConsumeQuantity { get; set; }

    // Audit fields
    public long CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public long UserID { get; set; }
    public long CreatedBy { get; set; }
    public long ModifiedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// Item Issue Consume Detail (for job card consumption detail tracking)
/// </summary>
public class ItemIssueDirectConsumeDetail
{
    public long ConsumeDetailID { get; set; }
    public long TransactionID { get; set; }
    public long? JobBookingJobCardContentsID { get; set; }
    public long ItemID { get; set; }
    public decimal ConsumeQuantity { get; set; }
    public string? StockUnit { get; set; }

    // Audit fields
    public long CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public long UserID { get; set; }
    public long CreatedBy { get; set; }
    public long ModifiedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
