using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services.Estimation;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Estimation;

/// <summary>
/// Service implementation for Tool and Material operations
/// </summary>
public class ToolMaterialService : IToolMaterialService
{
    private readonly IToolMaterialRepository _repository;
    private readonly ILogger<ToolMaterialService> _logger;

    public ToolMaterialService(
        IToolMaterialRepository repository,
        ILogger<ToolMaterialService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<DieToolDto>>> SearchDiesAsync(SearchDiesRequest request)
    {
        try
        {
            if (request == null)
            {
                return Result<List<DieToolDto>>.Failure("Request is required");
            }

            if (request.SizeL <= 0 || request.SizeW <= 0)
            {
                return Result<List<DieToolDto>>.Failure("Valid dimensions are required");
            }

            var data = await _repository.SearchDiesAsync(request);
            return Result<List<DieToolDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching dies with dimensions: {L}x{W}", request?.SizeL, request?.SizeW);
            return Result<List<DieToolDto>>.Failure("Failed to search dies");
        }
    }

    public async Task<Result<List<ReelDto>>> GetReelsAsync(decimal reqDeckle, decimal widthPlus, decimal widthMinus)
    {
        try
        {
            if (reqDeckle <= 0)
            {
                return Result<List<ReelDto>>.Failure("Valid deckle width is required");
            }

            var data = await _repository.GetReelsAsync(reqDeckle, widthPlus, widthMinus);
            return Result<List<ReelDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reels for deckle: {Deckle}, tolerance: +{Plus}/-{Minus}", 
                reqDeckle, widthPlus, widthMinus);
            return Result<List<ReelDto>>.Failure("Failed to get reels");
        }
    }

    public async Task<Result<List<ProcessMaterialDto>>> GetProcessMaterialsAsync(string processIds)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(processIds))
            {
                return Result<List<ProcessMaterialDto>>.Failure("Process IDs are required");
            }

            var data = await _repository.GetProcessMaterialsAsync(processIds);
            return Result<List<ProcessMaterialDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process materials for processes: {ProcessIds}", processIds);
            return Result<List<ProcessMaterialDto>>.Failure("Failed to get process materials");
        }
    }
}
