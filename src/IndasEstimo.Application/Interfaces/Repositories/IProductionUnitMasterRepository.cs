using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories;

public interface IProductionUnitMasterRepository
{
    /// <summary>GetProductionUnitMasterShowList() — main grid.</summary>
    Task<List<ProductionUnitListDto>> GetProductionUnitListAsync();

    /// <summary>GetProductionUnitNo() — auto-generate next unit code.</summary>
    Task<string> GetProductionUnitNoAsync();

    /// <summary>GetCountry() — country dropdown.</summary>
    Task<List<CountryDropdownDto>> GetCountryAsync();

    /// <summary>GetState() — state dropdown.</summary>
    Task<List<StateDropdownDto>> GetStateAsync();

    /// <summary>GetCity() — city dropdown.</summary>
    Task<List<CityDropdownDto>> GetCityAsync();

    /// <summary>GetCompanyName() — company dropdown.</summary>
    Task<List<CompanyDropdownDto>> GetCompanyNameAsync();

    /// <summary>GetBranch() — branch dropdown.</summary>
    Task<List<BranchDropdownDto>> GetBranchAsync();

    /// <summary>SaveProductionUnitMasterData() — insert new production unit.</summary>
    Task<string> SaveProductionUnitAsync(SaveProductionUnitRequest request);

    /// <summary>UpdateProductionUnitMasterData() — update existing production unit.</summary>
    Task<string> UpdateProductionUnitAsync(UpdateProductionUnitRequest request);

    /// <summary>DeleteProductionUnitMasterData() — soft-delete; returns 'Exist' if used in transactions.</summary>
    Task<string> DeleteProductionUnitAsync(long productionUnitId);
}
