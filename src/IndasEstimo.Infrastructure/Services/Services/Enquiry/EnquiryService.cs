
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Enquiry;
using IndasEstimo.Application.Interfaces.Repositories.Enquiry;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Application.Interfaces.Services.Estimation;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Enquiry;

public class EnquiryService : IEnquiryService
{
    private readonly IEnquiryRepository _repository;
    private readonly ILogger<EnquiryService> _logger;

    public EnquiryService(
        IEnquiryRepository repository,
        ILogger<EnquiryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<string>> GenerateEnquiryNoAsync()
    {
        try
        {
            var number = await _repository.GetMaxEnquiryNoAsync();
            return Result<string>.Success(number);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating enquiry number");
            return Result<string>.Failure("Failed to generate enquiry number");
        }
    }

    public async Task<Result<long>> CreateEnquiryAsync(CreateEnquiryDto enquiry)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(enquiry.JobName))
                return Result<long>.Failure("Job Name is required");

            if (enquiry.Quantity <= 0)
                return Result<long>.Failure("Quantity must be greater than 0");

            var enquiryId = await _repository.CreateEnquiryAsync(enquiry);
            return Result<long>.Success(enquiryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating enquiry");
            return Result<long>.Failure("Failed to create enquiry");
        }
    }

    public async Task<Result<bool>> UpdateEnquiryAsync(UpdateEnquiryDto enquiry)
    {
        try
        {
            // Validation logic...
            var success = await _repository.UpdateEnquiryAsync(enquiry);
            return Result<bool>.Success(success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating enquiry {EnquiryID}", enquiry.EnquiryID);
            return Result<bool>.Failure("Failed to update enquiry");
        }
    }

    public async Task<Result<bool>> DeleteEnquiryAsync(long enquiryId)
    {
        try
        {
            var success = await _repository.DeleteEnquiryAsync(enquiryId);
            return Result<bool>.Success(success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting enquiry {EnquiryID}", enquiryId);
            return Result<bool>.Failure("Failed to delete enquiry");
        }
    }

    public async Task<Result<EnquiryListDto>> GetEnquiryByIdAsync(long enquiryId)
    {
        try
        {
            var data = await _repository.GetEnquiryByIdAsync(enquiryId);
            if (data == null)
                return Result<EnquiryListDto>.Failure("Enquiry not found");

            return Result<EnquiryListDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enquiry {EnquiryID}", enquiryId);
            return Result<EnquiryListDto>.Failure("Failed to get enquiry");
        }
    }

    public async Task<Result<IEnumerable<EnquiryListDto>>> GetEnquiryListAsync(EnquiryFilterDto filter)
    {
        try
        {
            var list = await _repository.GetEnquiryListAsync(filter);
            return Result<IEnumerable<EnquiryListDto>>.Success(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enquiry list");
            return Result<IEnumerable<EnquiryListDto>>.Failure("Failed to get enquiry list");
        }
    }

    public async Task<Result<IEnumerable<EnquiryProcessDto>>> GetProcessDataAsync(long enquiryId)
    {
        try
        {
            var list = await _repository.GetProcessDataAsync(enquiryId);
            return Result<IEnumerable<EnquiryProcessDto>>.Success(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process data for enquiry {EnquiryID}", enquiryId);
            return Result<IEnumerable<EnquiryProcessDto>>.Failure("Failed to get process data");
        }
    }
}
