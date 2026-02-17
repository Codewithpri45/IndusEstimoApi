namespace IndasEstimo.Application.DTOs.Masters;

public class DepartmentListDto
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = "";
    public string Press { get; set; } = "";
    public int? SequenceNo { get; set; }
}

public class SaveDepartmentRequest
{
    public string DepartmentName { get; set; } = "";
    public string Press { get; set; } = "";
    public int? SequenceNo { get; set; }
}

public class UpdateDepartmentRequest
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = "";
    public string Press { get; set; } = "";
    public int? SequenceNo { get; set; }
}
