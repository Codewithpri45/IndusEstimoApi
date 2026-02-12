
using System.ComponentModel.DataAnnotations;

namespace IndasEstimo.Application.DTOs.Enquiry;

public class EnquiryListDto
{
    public long EnquiryID { get; set; }
    public string EnquiryNo { get; set; } = string.Empty;
    public DateTime? EnquiryDate { get; set; }
    public long? LedgerID { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string EstimationUnit { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsSalesEnquiryApproved { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public long? SalesEmployeeID { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateEnquiryDto
{
    [Required]
    public DateTime EnquiryDate { get; set; }
    public long LedgerID { get; set; }
    public long? SalesEmployeeID { get; set; }
    public long? CategoryID { get; set; }
    [Required]
    [MaxLength(200)]
    public string JobName { get; set; } = string.Empty;
    public string EstimationUnit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public string RefEnquiryNo { get; set; } = string.Empty;
    public List<EnquiryContentDto> Contents { get; set; } = new();
    public List<EnquiryProcessDto> Processes { get; set; } = new();
    public List<EnquiryLayerDto> Layers { get; set; } = new();
    public List<EnquiryAttachmentDto> Attachments { get; set; } = new();
}

public class UpdateEnquiryDto : CreateEnquiryDto
{
    [Required]
    public long EnquiryID { get; set; }
}

public class EnquiryContentDto
{
    public long? EnquiryContentsID { get; set; }
    public long ContentID { get; set; }
    public string ContentName { get; set; } = string.Empty;
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class EnquiryProcessDto
{
    public long? EnquiryProcessID { get; set; }
    public long ProcessID { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
}

public class EnquiryLayerDto
{
    public long? EnquiryLayerID { get; set; }
    public string LayerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SequenceNo { get; set; }
}

public class EnquiryAttachmentDto
{
    public long? AttachmentID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}

public class EnquiryFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string FilterType { get; set; } = "All"; // All, Converted, NotConverted
    public bool ApplyDateFilter { get; set; }
}
