namespace IndasEstimo.Application.DTOs.ToolInventory;

// ==================== Request DTOs ====================

/// <summary>
/// Request DTO for saving new tool issue
/// </summary>
public record SaveToolIssueRequest(
    string Prefix,
    List<ToolIssueMainDto> JsonObjectsRecordMain,
    List<ToolIssueDetailDto> JsonObjectsRecordDetail
);

/// <summary>
/// Request DTO for deleting tool issue
/// </summary>
public record DeleteToolIssueRequest(
    long TransactionID,
    long ParentTransactionID
);

// ==================== Main & Detail DTOs ====================

/// <summary>
/// Tool Issue header DTO
/// </summary>
public record ToolIssueMainDto
{
    public DateTime VoucherDate { get; init; }
    public int VoucherID { get; init; } // -43
    public long JobBookingID { get; init; }
    public long JobBookingJobCardContentsID { get; init; }
    public string? DeliveryNoteNo { get; init; }
    public DateTime? DeliveryNoteDate { get; init; }
    public string? Narration { get; init; }
}

/// <summary>
/// Tool Issue detail DTO
/// </summary>
public record ToolIssueDetailDto
{
    public int TransID { get; init; }
    public long ToolID { get; init; }
    public long ItemID { get; init; }
    public decimal IssueQuantity { get; init; }
    public string? BatchNo { get; init; }
    public long WarehouseID { get; init; }
    public long FloorWarehouseID { get; init; } // BIN location
    public string? ToolNarration { get; init; }
    public long ParentTransactionID { get; init; }
    public long JobBookingID { get; init; }
    public long JobBookingJobCardContentsID { get; init; }
    public string? JobCardFormNo { get; init; }
}

// ==================== Response DTOs ====================

/// <summary>
/// Response DTO for tool issue operations
/// </summary>
public record ToolIssueResponse
{
    public long TransactionID { get; init; }
    public string VoucherNo { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// DTO for retrieving tool issue voucher details
/// </summary>
public record ToolIssueVoucherDetailsDto
{
    public long TransactionID { get; init; }
    public int VoucherID { get; init; }
    public long MaxVoucherNo { get; init; }
    public string VoucherNo { get; init; } = string.Empty;
    public string VoucherDate { get; init; } = string.Empty;
    public string FYear { get; init; } = string.Empty;
    public long JobBookingID { get; init; }
    public long JobBookingJobCardContentsID { get; init; }
    public string JobBookingNo { get; init; } = string.Empty;
    public string JobCardContentNo { get; init; } = string.Empty;
    public string? DeliveryNoteNo { get; init; }
    public string? DeliveryNoteDate { get; init; }
    public string? Narration { get; init; }
    public long ToolID { get; init; }
    public long ToolGroupID { get; init; }
    public string ToolCode { get; init; } = string.Empty;
    public string ToolName { get; init; } = string.Empty;
    public string ToolDescription { get; init; } = string.Empty;
    public string ToolGroupName { get; init; } = string.Empty;
    public decimal IssueQuantity { get; init; }
    public string? BatchNo { get; init; }
    public long WarehouseID { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public long FloorWarehouseID { get; init; }
    public string FloorWarehouseName { get; init; } = string.Empty;
    public string StockUnit { get; init; } = string.Empty;
    public long ProductionUnitID { get; init; }
    public string ProductionUnitName { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
    public string CreatedDate { get; init; } = string.Empty;
}

/// <summary>
/// DTO for warehouse list
/// </summary>
public record WarehouseDto
{
    public long WarehouseID { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public string? WarehouseCode { get; init; }
}

/// <summary>
/// DTO for bin/floor warehouse list
/// </summary>
public record BinDto
{
    public long FloorWarehouseID { get; init; }
    public string FloorWarehouseName { get; init; } = string.Empty;
    public long WarehouseID { get; init; }
}

/// <summary>
/// DTO for batch-wise stock details
/// </summary>
public record StockBatchWiseDto
{
    public long ToolID { get; init; }
    public long ToolGroupID { get; init; }
    public string ToolCode { get; init; } = string.Empty;
    public string ToolName { get; init; } = string.Empty;
    public string ToolDescription { get; init; } = string.Empty;
    public string ToolGroupName { get; init; } = string.Empty;
    public string StockUnit { get; init; } = string.Empty;
    public string? BatchNo { get; init; }
    public long WarehouseID { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public long FloorWarehouseID { get; init; }
    public string FloorWarehouseName { get; init; } = string.Empty;
    public decimal PhysicalStock { get; init; }
    public decimal BookedStock { get; init; }
    public decimal AllocatedStock { get; init; }
    public decimal AvailableStock { get; init; }
    public long ItemID { get; init; }
    public long ParentTransactionID { get; init; }
    public string GRNDate { get; init; } = string.Empty;
}

/// <summary>
/// DTO for job card list
/// </summary>
public record JobCardDto
{
    public long JobBookingID { get; init; }
    public string JobBookingNo { get; init; } = string.Empty;
    public long JobBookingJobCardContentsID { get; init; }
    public string JobCardContentNo { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public string? JobDescription { get; init; }
}
