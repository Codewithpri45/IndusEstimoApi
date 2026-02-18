namespace IndasEstimo.Application.DTOs.Estimation;

/// <summary>
/// DTO for Category dropdown
/// </summary>
public class CategoryDto
{
    public long CategoryID { get; set; }
    public long? SegmentID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? SegmentName { get; set; }
}

/// <summary>
/// DTO for Client (Ledger) dropdown
/// </summary>
public class ClientDto
{
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public int CreditDays { get; set; }
    public bool IsLead { get; set; }
}

/// <summary>
/// DTO for Sales Person dropdown
/// </summary>
public class SalesPersonDto
{
    public long EmployeeID { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Content Master
/// </summary>
public class ContentDto
{
    public long ContentID { get; set; }
    public string ContentName { get; set; } = string.Empty;
    public string? ContentCaption { get; set; }
    public string? ContentOpenHref { get; set; }
    public string? ContentClosedHref { get; set; }
    public string? ContentSizes { get; set; }
    public string? ContentDomainType { get; set; }
    public decimal DefaultAcrossGap { get; set; }
    public decimal DefaultAroundGap { get; set; }
    public decimal DefaultPlateBearer { get; set; }
    public decimal DefaultSideStrip { get; set; }
    public string? CategoryName { get; set; }
    public string? SegmentName { get; set; }
}

/// <summary>
/// DTO for Content by Category
/// </summary>
public class ContentByCategoryDto
{
    public long ContentID { get; set; }
    public string ContentName { get; set; } = string.Empty;
    public string? ContentDomainType { get; set; }
}

/// <summary>
/// DTO for Category Defaults
/// </summary>
public class CategoryDefaultsDto
{
    public long CategoryID { get; set; }
    public string? CategoryName { get; set; }
    public decimal DefaultAcrossGap { get; set; }
    public decimal DefaultAroundGap { get; set; }
    public decimal DefaultPlateBearer { get; set; }
    public decimal DefaultSideStrip { get; set; }
}

/// <summary>
/// DTO for Winding Direction
/// </summary>
public class WindingDirectionDto
{
    public long WindingDirectionID { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string? Image { get; set; }
}

/// <summary>
/// DTO for Book Contents
/// </summary>
public class BookContentDto
{
    public long ContentID { get; set; }
    public string ContentName { get; set; } = string.Empty;
    public string? ContentCategory { get; set; }
}

/// <summary>
/// DTO for One Time Charges
/// </summary>
public class OneTimeChargeDto
{
    public string Headname { get; set; } = string.Empty;
    public string Amount { get; set; } = "0";
}

/// <summary>
/// DTO for Content Sizes
/// </summary>
public class ContentSizeDto
{
    public long ContentID { get; set; }
    public string ContentName { get; set; } = string.Empty;
    public string? ContentSizes { get; set; }
}
