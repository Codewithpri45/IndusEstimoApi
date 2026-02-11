using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Repositories.Estimation;

/// <summary>
/// Repository interface for Quotation operations
/// </summary>
public interface IQuotationRepository
{
    Task<long> GetNextQuoteNumberAsync();
    Task<long> SaveQuotationAsync(SaveQuotationRequest request, long userId, long companyId);
    Task<QuotationDetailsDto?> LoadQuotationAsync(long bookingId);
}
