using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IProductGroupMasterService
{
    /// <summary>Get all product HSN groups for main grid. Old VB: Showlist()</summary>
    Task<Result<List<ProductGroupListDto>>> GetProductGroupListAsync();

    /// <summary>Get HSN dropdown list for UnderGroup parent selection. Old VB: UnderGroup()</summary>
    Task<Result<List<ProductHSNDropdownDto>>> GetHSNDropdownAsync();

    /// <summary>Get item groups for dropdown. Old VB: SelItemGroupName()</summary>
    Task<Result<List<ItemGroupDropdownDto>>> GetItemGroupsAsync();

    /// <summary>Check company tax type (VAT applicable). Old VB: CheckTaxType()</summary>
    Task<Result<List<TaxTypeDto>>> GetTaxTypeAsync();

    /// <summary>Check if HSN is in use before delete. Old VB: CheckPermission(ProductHSNID)</summary>
    Task<Result<string>> CheckPermissionAsync(int productHSNId);

    /// <summary>Save new product HSN group. Old VB: SavePGHMData()</summary>
    Task<Result<string>> SaveProductGroupAsync(SaveProductGroupRequest request);

    /// <summary>Update existing product HSN group. Old VB: UpdatePGHM()</summary>
    Task<Result<string>> UpdateProductGroupAsync(UpdateProductGroupRequest request);

    /// <summary>Soft-delete a product HSN group. Old VB: DeletePGHM()</summary>
    Task<Result<string>> DeleteProductGroupAsync(int productHSNId);
}
