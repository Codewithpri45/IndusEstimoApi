using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Services.Estimation;

/// <summary>
/// Service interface for Quotation operations
/// </summary>
public interface IQuotationService
{
    Task<Result<QuoteNumberDto>> GetNextQuoteNumberAsync();
    Task<Result<SaveQuotationResponse>> SaveQuotationAsync(SaveQuotationRequest request);
    Task<Result<QuotationDetailsDto>> LoadQuotationAsync(long bookingId);
}
