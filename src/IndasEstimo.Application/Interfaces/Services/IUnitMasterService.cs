using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IUnitMasterService
{
    /// <summary>Get all units for main grid. Old VB: GetUnit()</summary>
    Task<Result<List<UnitListDto>>> GetUnitListAsync();

    /// <summary>Save a new unit. Old VB: SaveUnitData()</summary>
    Task<Result<string>> SaveUnitAsync(SaveUnitRequest request);

    /// <summary>Update an existing unit. Old VB: UpdatUnitData()</summary>
    Task<Result<string>> UpdateUnitAsync(UpdateUnitRequest request);

    /// <summary>Soft-delete a unit. Old VB: DeleteUnitMasterData()</summary>
    Task<Result<string>> DeleteUnitAsync(long unitId);
}
