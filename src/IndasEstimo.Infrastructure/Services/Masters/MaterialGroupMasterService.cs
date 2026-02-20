using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class MaterialGroupMasterService : IMaterialGroupMasterService
{
    private readonly IMaterialGroupMasterRepository _repository;
    private readonly ILogger<MaterialGroupMasterService> _logger;

    public MaterialGroupMasterService(
        IMaterialGroupMasterRepository repository,
        ILogger<MaterialGroupMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<MaterialGroupListDto>>> GetGroupListAsync()
    {
        try
        {
            var result = await _repository.GetGroupListAsync();
            return Result<List<MaterialGroupListDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting material group list");
            return Result<List<MaterialGroupListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<UnderGroupDropdownDto>>> GetUnderGroupAsync()
    {
        try
        {
            var result = await _repository.GetUnderGroupAsync();
            return Result<List<UnderGroupDropdownDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting under group dropdown");
            return Result<List<UnderGroupDropdownDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveGroupAsync(SaveMaterialGroupRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ItemSubGroupName))
                return Result<string>.Failure("Group Name is required");

            if (string.IsNullOrWhiteSpace(request.ItemSubGroupDisplayName))
                return Result<string>.Failure("Display Name is required");

            var result = await _repository.SaveGroupAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving material group");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateGroupAsync(UpdateMaterialGroupRequest request)
    {
        try
        {
            if (request.ItemSubGroupUniqueID <= 0)
                return Result<string>.Failure("Group ID is required");

            if (string.IsNullOrWhiteSpace(request.ItemSubGroupName))
                return Result<string>.Failure("Group Name is required");

            var result = await _repository.UpdateGroupAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating material group {ID}", request.ItemSubGroupUniqueID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteGroupAsync(long itemSubGroupUniqueId)
    {
        try
        {
            var result = await _repository.DeleteGroupAsync(itemSubGroupUniqueId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting material group {ID}", itemSubGroupUniqueId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
