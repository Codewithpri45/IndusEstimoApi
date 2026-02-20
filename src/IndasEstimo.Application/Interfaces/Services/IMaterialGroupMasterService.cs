using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IMaterialGroupMasterService
{
    Task<Result<List<MaterialGroupListDto>>>  GetGroupListAsync();
    Task<Result<List<UnderGroupDropdownDto>>> GetUnderGroupAsync();
    Task<Result<string>> SaveGroupAsync(SaveMaterialGroupRequest request);
    Task<Result<string>> UpdateGroupAsync(UpdateMaterialGroupRequest request);
    Task<Result<string>> DeleteGroupAsync(long itemSubGroupUniqueId);
}
