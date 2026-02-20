using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories;

public interface IProductGroupMasterRepository
{
    /// <summary>Get all product HSN groups for main grid. Old VB: Showlist()</summary>
    Task<List<ProductGroupListDto>> GetProductGroupListAsync();

    /// <summary>Get HSN dropdown list for UnderGroup parent selection. Old VB: UnderGroup()</summary>
    Task<List<ProductHSNDropdownDto>> GetHSNDropdownAsync();

    /// <summary>Get item groups for dropdown. Old VB: SelItemGroupName()</summary>
    Task<List<ItemGroupDropdownDto>> GetItemGroupsAsync();

    /// <summary>Check company tax type (VAT applicable). Old VB: CheckTaxType()</summary>
    Task<List<TaxTypeDto>> GetTaxTypeAsync();

    /// <summary>Check if HSN is in use before delete. Old VB: CheckPermission(ProductHSNID)</summary>
    Task<string> CheckPermissionAsync(int productHSNId);

    /// <summary>Save new product HSN group. Old VB: SavePGHMData()</summary>
    Task<string> SaveProductGroupAsync(SaveProductGroupRequest request);

    /// <summary>Update existing product HSN group. Old VB: UpdatePGHM()</summary>
    Task<string> UpdateProductGroupAsync(UpdateProductGroupRequest request);

    /// <summary>Soft-delete a product HSN group. Old VB: DeletePGHM()</summary>
    Task<string> DeleteProductGroupAsync(int productHSNId);
}
