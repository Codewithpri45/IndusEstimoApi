using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories;

public interface IDepartmentMasterRepository
{
    Task<List<DepartmentListDto>> GetDepartmentListAsync();
    Task<string> SaveDepartmentAsync(SaveDepartmentRequest request);
    Task<string> UpdateDepartmentAsync(UpdateDepartmentRequest request);
    Task<string> DeleteDepartmentAsync(int departmentId);
}
