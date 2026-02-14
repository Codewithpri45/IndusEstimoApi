using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class LedgerMasterService : ILedgerMasterService
{
    private readonly ILedgerMasterRepository _repository;
    private readonly ILogger<LedgerMasterService> _logger;

    public LedgerMasterService(
        ILedgerMasterRepository repository,
        ILogger<LedgerMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // ==================== Core CRUD Operations ====================

    public async Task<Result<List<LedgerMasterListDto>>> GetMasterListAsync()
    {
        try
        {
            var data = await _repository.GetMasterListAsync();
            return Result<List<LedgerMasterListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting master list");
            return Result<List<LedgerMasterListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetMasterGridAsync(string masterID)
    {
        try
        {
            var data = await _repository.GetMasterGridAsync(masterID);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting master grid for masterID {MasterID}", masterID);
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<LedgerGridColumnHideDto>>> GetGridColumnHideAsync(string masterID)
    {
        try
        {
            var data = await _repository.GetGridColumnHideAsync(masterID);
            return Result<List<LedgerGridColumnHideDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grid column hide for masterID {MasterID}", masterID);
            return Result<List<LedgerGridColumnHideDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<LedgerGridColumnDto>>> GetGridColumnAsync(string masterID)
    {
        try
        {
            var data = await _repository.GetGridColumnAsync(masterID);
            return Result<List<LedgerGridColumnDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grid column for masterID {MasterID}", masterID);
            return Result<List<LedgerGridColumnDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<LedgerMasterFieldDto>>> GetMasterFieldsAsync(string masterID)
    {
        try
        {
            var data = await _repository.GetMasterFieldsAsync(masterID);
            return Result<List<LedgerMasterFieldDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting master fields for masterID {MasterID}", masterID);
            return Result<List<LedgerMasterFieldDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetLoadedDataAsync(string masterID, string ledgerID)
    {
        try
        {
            var data = await _repository.GetLoadedDataAsync(masterID, ledgerID);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loaded data for masterID {MasterID}, ledgerID {LedgerID}", masterID, ledgerID);
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetDrillDownAsync(string masterID, string tabID, string ledgerID)
    {
        try
        {
            var data = await _repository.GetDrillDownAsync(masterID, tabID, ledgerID);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drill-down data for masterID {MasterID}, tabID {TabID}, ledgerID {LedgerID}", masterID, tabID, ledgerID);
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveLedgerAsync(SaveLedgerRequest request)
    {
        try
        {
            var result = await _repository.SaveLedgerAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving ledger");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateLedgerAsync(UpdateLedgerRequest request)
    {
        try
        {
            var result = await _repository.UpdateLedgerAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ledger");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteLedgerAsync(string ledgerID, string ledgerGroupID)
    {
        try
        {
            var result = await _repository.DeleteLedgerAsync(ledgerID, ledgerGroupID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ledger {LedgerID}", ledgerID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> CheckPermissionAsync(string ledgerID)
    {
        try
        {
            var result = await _repository.CheckPermissionAsync(ledgerID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for ledger {LedgerID}", ledgerID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> SelectBoxLoadAsync(JArray jsonData)
    {
        try
        {
            if (jsonData == null || jsonData.Count == 0)
            {
                return Result<object>.Failure("No field data provided");
            }

            var data = await _repository.SelectBoxLoadAsync(jsonData);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading select box data");
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Ledger Operations ====================

    public async Task<Result<string>> ConvertLedgerToConsigneeAsync(int ledgerID)
    {
        try
        {
            var result = await _repository.ConvertLedgerToConsigneeAsync(ledgerID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting ledger to consignee {LedgerID}", ledgerID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetLedgersByGroupAsync(string groupID)
    {
        try
        {
            var data = await _repository.GetLedgersByGroupAsync(groupID);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ledgers by group {GroupID}", groupID);
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Concern Person Management ====================

    public async Task<Result<List<ConcernPersonDto>>> GetConcernPersonsAsync()
    {
        try
        {
            var data = await _repository.GetConcernPersonsAsync();
            return Result<List<ConcernPersonDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting concern persons");
            return Result<List<ConcernPersonDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveConcernPersonAsync(SaveConcernPersonRequest request)
    {
        try
        {
            var result = await _repository.SaveConcernPersonAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving concern person");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteAllConcernPersonsAsync(string ledgerID)
    {
        try
        {
            var result = await _repository.DeleteAllConcernPersonsAsync(ledgerID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all concern persons for ledger {LedgerID}", ledgerID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteConcernPersonAsync(string concernPersonID, string ledgerID)
    {
        try
        {
            var result = await _repository.DeleteConcernPersonAsync(concernPersonID, ledgerID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting concern person {ConcernPersonID}", concernPersonID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Employee Machine Allocation ====================

    public async Task<Result<List<OperatorDto>>> GetOperatorsAsync()
    {
        try
        {
            var data = await _repository.GetOperatorsAsync();
            return Result<List<OperatorDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting operators");
            return Result<List<OperatorDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<EmployeeDto>>> GetEmployeesAsync(string groupID)
    {
        try
        {
            var data = await _repository.GetEmployeesAsync(groupID);
            return Result<List<EmployeeDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees for group {GroupID}", groupID);
            return Result<List<EmployeeDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveMachineAllocationAsync(SaveMachineAllocationRequest request)
    {
        try
        {
            var result = await _repository.SaveMachineAllocationAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving machine allocation");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetMachineAllocationAsync(string employeeID)
    {
        try
        {
            var result = await _repository.GetMachineAllocationAsync(employeeID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machine allocation for employee {EmployeeID}", employeeID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteMachineAllocationAsync(string ledgerID)
    {
        try
        {
            var result = await _repository.DeleteMachineAllocationAsync(ledgerID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine allocation for ledger {LedgerID}", ledgerID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Supplier Group Allocation ====================

    public async Task<Result<List<LedgerItemGroupDto>>> GetItemGroupsAsync()
    {
        try
        {
            var data = await _repository.GetItemGroupsAsync();
            return Result<List<LedgerItemGroupDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item groups");
            return Result<List<LedgerItemGroupDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<SpareGroupDto>>> GetSpareGroupsAsync()
    {
        try
        {
            var data = await _repository.GetSpareGroupsAsync();
            return Result<List<SpareGroupDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting spare groups");
            return Result<List<SpareGroupDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetGroupAllocationAsync(string supplierID)
    {
        try
        {
            var result = await _repository.GetGroupAllocationAsync(supplierID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group allocation for supplier {SupplierID}", supplierID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetSpareAllocationAsync(string supplierID)
    {
        try
        {
            var result = await _repository.GetSpareAllocationAsync(supplierID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting spare allocation for supplier {SupplierID}", supplierID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveGroupAllocationAsync(SaveGroupAllocationRequest request)
    {
        try
        {
            var result = await _repository.SaveGroupAllocationAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving group allocation");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteGroupAllocationAsync(string ledgerID)
    {
        try
        {
            var result = await _repository.DeleteGroupAllocationAsync(ledgerID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group allocation for ledger {LedgerID}", ledgerID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Business Vertical ====================

    public async Task<Result<object>> GetBusinessVerticalSettingsAsync()
    {
        try
        {
            var data = await _repository.GetBusinessVerticalSettingsAsync();
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business vertical settings");
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetBusinessVerticalDetailsAsync(string ledgerID, string verticalID)
    {
        try
        {
            var data = await _repository.GetBusinessVerticalDetailsAsync(ledgerID, verticalID);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business vertical details for ledgerID {LedgerID}, verticalID {VerticalID}", ledgerID, verticalID);
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveBusinessVerticalAsync(SaveBusinessVerticalRequest request)
    {
        try
        {
            var result = await _repository.SaveBusinessVerticalAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving business vertical");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateBusinessVerticalAsync(UpdateBusinessVerticalRequest request)
    {
        try
        {
            var result = await _repository.UpdateBusinessVerticalAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business vertical");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteBusinessVerticalAsync(string detailID)
    {
        try
        {
            var result = await _repository.DeleteBusinessVerticalAsync(detailID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business vertical {DetailID}", detailID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Embargo Management ====================

    public async Task<Result<string>> PlaceEmbargoAsync(PlaceEmbargoRequest request)
    {
        try
        {
            var result = await _repository.PlaceEmbargoAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing embargo");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetEmbargoDetailsAsync(string ledgerID)
    {
        try
        {
            var data = await _repository.GetEmbargoDetailsAsync(ledgerID);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting embargo details for ledger {LedgerID}", ledgerID);
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetActiveEmbargosAsync()
    {
        try
        {
            var data = await _repository.GetActiveEmbargosAsync();
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active embargos");
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveEmbargoDetailsAsync(SaveEmbargoDetailsRequest request)
    {
        try
        {
            var result = await _repository.SaveEmbargoDetailsAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving embargo details");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Utilities ====================

    public async Task<Result<int>> GetSessionTimeoutAsync()
    {
        try
        {
            var result = await _repository.GetSessionTimeoutAsync();
            return Result<int>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session timeout");
            return Result<int>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetLedgerGroupNameIDAsync(string masterID)
    {
        try
        {
            var result = await _repository.GetLedgerGroupNameIDAsync(masterID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ledger group name ID for master {MasterID}", masterID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<SupplierDto>>> GetSuppliersAsync(string groupNameID)
    {
        try
        {
            var data = await _repository.GetSuppliersAsync(groupNameID);
            return Result<List<SupplierDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suppliers for group name ID {GroupNameID}", groupNameID);
            return Result<List<SupplierDto>>.Failure($"Error: {ex.Message}");
        }
    }
}
