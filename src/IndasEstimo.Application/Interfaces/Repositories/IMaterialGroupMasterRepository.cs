using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories;

public interface IMaterialGroupMasterRepository
{
    /// <summary>GetGroup() — main grid list.</summary>
    Task<List<MaterialGroupListDto>> GetGroupListAsync();

    /// <summary>GetUnderGroup() — dropdown for parent group selector.</summary>
    Task<List<UnderGroupDropdownDto>> GetUnderGroupAsync();

    /// <summary>SaveGroupData() — insert new material group.</summary>
    Task<string> SaveGroupAsync(SaveMaterialGroupRequest request);

    /// <summary>UpdatGroupData() — update existing material group.</summary>
    Task<string> UpdateGroupAsync(UpdateMaterialGroupRequest request);

    /// <summary>DeleteGroupMasterData() — soft-delete a material group.</summary>
    Task<string> DeleteGroupAsync(long itemSubGroupUniqueId);
}
