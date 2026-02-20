using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories;

public interface IUnitMasterRepository
{
    /// <summary>Get all units for main grid. Old VB: GetUnit()</summary>
    Task<List<UnitListDto>> GetUnitListAsync();

    /// <summary>Save a new unit. Old VB: SaveUnitData()</summary>
    Task<string> SaveUnitAsync(SaveUnitRequest request);

    /// <summary>Update an existing unit. Old VB: UpdatUnitData()</summary>
    Task<string> UpdateUnitAsync(UpdateUnitRequest request);

    /// <summary>Soft-delete a unit. Old VB: DeleteUnitMasterData()</summary>
    Task<string> DeleteUnitAsync(long unitId);
}
