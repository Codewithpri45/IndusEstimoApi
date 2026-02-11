namespace IndasEstimo.Application.DTOs.Estimation;

/// <summary>
/// DTO for generating next quotation number
/// </summary>
public class QuoteNumberDto
{
    public long BookingNo { get; set; }
}

/// <summary>
/// Request DTO for saving quotation
/// </summary>
public class SaveQuotationRequest
{
    // Basic Information
    public long BookingID { get; set; }
    public long MAXBookingNo { get; set; }
    public string JobName { get; set; } = string.Empty;
    public long LedgerID { get; set; }
    public long CategoryID { get; set; }
    public int OrderQuantity { get; set; }
    public string TypeOfCost { get; set; } = string.Empty;
    public decimal FinalCost { get; set; }
    public decimal QuotedCost { get; set; }
    public string? Remark { get; set; }
    public string? ProductCode { get; set; }
    public int ExpectedCompletionDays { get; set; }
    public string CurrencySymbol { get; set; } = "INR";
    public decimal ConversionValue { get; set; } = 1;
    
    // Additional fields as needed - simplified for now
    public string? BookingData { get; set; } // JSON containing all booking details
}

/// <summary>
/// Response DTO after saving quotation
/// </summary>
public class SaveQuotationResponse
{
    public long BookingID { get; set; }
    public long BookingNo { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}

/// <summary>
/// DTO for loaded quotation details
/// </summary>
public class QuotationDetailsDto
{
    public long BookingID { get; set; }
    public long BookingNo { get; set; }
    public string JobName { get; set; } = string.Empty;
    public long LedgerID { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public long CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int OrderQuantity { get; set; }
    public int AnnualQuantity { get; set; }
    public string TypeOfCost { get; set; } = string.Empty;
    public decimal FinalCost { get; set; }
    public decimal QuotedCost { get; set; }
    public string? Remark { get; set; }
    public string? ProductCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string CreatedByUser { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsSendForPriceApproval { get; set; }
    
    // Full JSON data
    public string? FullDataJson { get; set; }
}
