namespace IndasEstimo.Application.DTOs.Crm;

public record CustomerDto(
    int CustomerId,
    string CustomerCode,
    string CustomerName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    bool IsActive,
    DateTime CreatedAt);

public record CreateCustomerRequest(
    string CustomerCode,
    string CustomerName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode);

public record UpdateCustomerRequest(
    string CustomerName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    bool IsActive);
