
using IndasEstimo.Application.DTOs.Enquiry;
using IndasEstimo.Application.Common;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IEnquiryService
{
    Task<Result<string>> GenerateEnquiryNoAsync();
    Task<Result<long>> CreateEnquiryAsync(CreateEnquiryDto enquiry);
    Task<Result<bool>> UpdateEnquiryAsync(UpdateEnquiryDto enquiry);
    Task<Result<bool>> DeleteEnquiryAsync(long enquiryId);
    Task<Result<EnquiryListDto>> GetEnquiryByIdAsync(long enquiryId);
    Task<Result<IEnumerable<EnquiryListDto>>> GetEnquiryListAsync(EnquiryFilterDto filter);
    Task<Result<IEnumerable<EnquiryProcessDto>>> GetProcessDataAsync(long enquiryId);
}
