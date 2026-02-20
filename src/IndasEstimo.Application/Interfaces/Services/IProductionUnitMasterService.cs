using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IProductionUnitMasterService
{
    Task<Result<List<ProductionUnitListDto>>> GetProductionUnitListAsync();
    Task<Result<string>>                      GetProductionUnitNoAsync();
    Task<Result<List<CountryDropdownDto>>>    GetCountryAsync();
    Task<Result<List<StateDropdownDto>>>      GetStateAsync();
    Task<Result<List<CityDropdownDto>>>       GetCityAsync();
    Task<Result<List<CompanyDropdownDto>>>    GetCompanyNameAsync();
    Task<Result<List<BranchDropdownDto>>>     GetBranchAsync();
    Task<Result<string>> SaveProductionUnitAsync(SaveProductionUnitRequest request);
    Task<Result<string>> UpdateProductionUnitAsync(UpdateProductionUnitRequest request);
    Task<Result<string>> DeleteProductionUnitAsync(long productionUnitId);
}
