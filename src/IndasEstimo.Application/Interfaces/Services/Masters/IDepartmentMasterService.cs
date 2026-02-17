using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IDepartmentMasterService
{
    Task<Result<List<DepartmentListDto>>> GetDepartmentListAsync();
    Task<Result<string>> SaveDepartmentAsync(SaveDepartmentRequest request);
    Task<Result<string>> UpdateDepartmentAsync(UpdateDepartmentRequest request);
    Task<Result<string>> DeleteDepartmentAsync(int departmentId);
}
