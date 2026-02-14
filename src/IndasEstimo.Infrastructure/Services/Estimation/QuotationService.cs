using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Application.Interfaces.Services.Estimation;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Estimation;

/// <summary>
/// Service implementation for Quotation operations
/// </summary>
public class QuotationService : IQuotationService
{
    private readonly IQuotationRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<QuotationService> _logger;

    public QuotationService(
        IQuotationRepository repository,
        ICurrentUserService currentUserService,
        ILogger<QuotationService> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<QuoteNumberDto>> GetNextQuoteNumberAsync()
    {
        try
        {
            var nextNumber = await _repository.GetNextQuoteNumberAsync();
            return Result<QuoteNumberDto>.Success(new QuoteNumberDto { BookingNo = nextNumber });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next quote number");
            return Result<QuoteNumberDto>.Failure("Failed to get next quote number");
        }
    }

    public async Task<Result<SaveQuotationResponse>> SaveQuotationAsync(SaveQuotationRequest request)
    {
        try
        {
            if (request == null)
            {
                return Result<SaveQuotationResponse>.Failure("Request is required");
            }

            if (string.IsNullOrWhiteSpace(request.JobName))
            {
                return Result<SaveQuotationResponse>.Failure("Job name is required");
            }

            if (request.LedgerID <= 0)
            {
                return Result<SaveQuotationResponse>.Failure("Valid client is required");
            }

            if (request.CategoryID <= 0)
            {
                return Result<SaveQuotationResponse>.Failure("Valid category is required");
            }

            if (request.OrderQuantity <= 0)
            {
                return Result<SaveQuotationResponse>.Failure("Valid order quantity is required");
            }

            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;

            if (userId <= 0 || companyId <= 0)
            {
                return Result<SaveQuotationResponse>.Failure("User session invalid");
            }

            var bookingId = await _repository.SaveQuotationAsync(request, userId, companyId);

            var response = new SaveQuotationResponse
            {
                BookingID = bookingId,
                BookingNo = request.MAXBookingNo > 0 ? request.MAXBookingNo : bookingId,
                Message = request.BookingID > 0 ? "Quotation updated successfully" : "Quotation saved successfully",
                IsSuccess = true
            };

            return Result<SaveQuotationResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving quotation for job: {JobName}", request?.JobName);
            return Result<SaveQuotationResponse>.Failure("Failed to save quotation");
        }
    }

    public async Task<Result<QuotationDetailsDto>> LoadQuotationAsync(long bookingId)
    {
        try
        {
            if (bookingId <= 0)
            {
                return Result<QuotationDetailsDto>.Failure("Valid booking ID is required");
            }

            var data = await _repository.LoadQuotationAsync(bookingId);

            if (data == null)
            {
                return Result<QuotationDetailsDto>.Failure("Quotation not found");
            }

            return Result<QuotationDetailsDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading quotation: {BookingID}", bookingId);
            return Result<QuotationDetailsDto>.Failure("Failed to load quotation");
        }
    }
}
