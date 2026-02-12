
using IndasEstimo.Application.DTOs.Enquiry;

namespace IndasEstimo.Application.Interfaces.Repositories.Enquiry;

public interface IEnquiryRepository
{
    Task<string> GetMaxEnquiryNoAsync();
    Task<long> CreateEnquiryAsync(CreateEnquiryDto enquiry);
    Task<bool> UpdateEnquiryAsync(UpdateEnquiryDto enquiry);
    Task<bool> DeleteEnquiryAsync(long enquiryId);
    Task<EnquiryListDto> GetEnquiryByIdAsync(long enquiryId);
    Task<IEnumerable<EnquiryListDto>> GetEnquiryListAsync(EnquiryFilterDto filter);
    Task<IEnumerable<EnquiryProcessDto>> GetProcessDataAsync(long enquiryId);
    Task<IEnumerable<EnquiryContentDto>> GetEnquiryContentsAsync(long enquiryId);
}
