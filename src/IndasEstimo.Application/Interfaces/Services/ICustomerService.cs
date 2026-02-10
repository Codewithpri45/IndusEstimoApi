using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Crm;

namespace IndasEstimo.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<Result<List<Dictionary<string, object>>>> GetAllCustomersAsync();
    Task<Result<CustomerDto>> GetCustomerByIdAsync(int customerId);
    Task<Result<CustomerDto>> GetCustomerByCodeAsync(string customerCode);
    Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerRequest request);
    Task<Result<CustomerDto>> UpdateCustomerAsync(int customerId, UpdateCustomerRequest request);
    Task<Result> DeleteCustomerAsync(int customerId);
}
