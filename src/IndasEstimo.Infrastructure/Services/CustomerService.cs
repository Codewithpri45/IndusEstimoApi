using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Infrastructure.Extensions;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Crm;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Domain.Entities.Tenant;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;

namespace IndasEstimo.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IDbConnectionFactory connectionFactory, ITenantProvider tenantProvider, ILogger<CustomerService> logger)
    {
        _connectionFactory = connectionFactory;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private SqlConnection GetTenantConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    public async Task<Result<List<Dictionary<string, object>>>> GetAllCustomersAsync()
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var sql = @"
        SELECT * 
        FROM LedgerMaster 
        WHERE LedgerGroupID = 1 
        ORDER BY LedgerCode";

        var command = new SqlCommand(sql, connection);
        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();

        var customers = new List<Dictionary<string, object>>();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            
            // Dynamically read all columns from the query result
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[columnName] = value;
            }
            
            customers.Add(row);
        }

        return Result<List<Dictionary<string, object>>>.Success(customers);
    }

    public async Task<Result<CustomerDto>> GetCustomerByIdAsync(int customerId)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT CustomerId, CustomerCode, CustomerName, Email, Phone, Address, City, State, Country, PostalCode, IsActive, CreatedAt
            FROM Customers
            WHERE CustomerId = @CustomerId",
            connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var dto = new CustomerDto(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetString(9),
                reader.GetBoolean(10),
                reader.GetDateTime(11)
            );
            return Result<CustomerDto>.Success(dto);
        }

        return Result<CustomerDto>.Failure("Customer not found");
    }

    public async Task<Result<CustomerDto>> GetCustomerByCodeAsync(string customerCode)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT CustomerId, CustomerCode, CustomerName, Email, Phone, Address, City, State, Country, PostalCode, IsActive, CreatedAt
            FROM Customers
            WHERE CustomerCode = @CustomerCode",
            connection);
        command.Parameters.AddWithValue("@CustomerCode", customerCode);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var dto = new CustomerDto(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetString(9),
                reader.GetBoolean(10),
                reader.GetDateTime(11)
            );
            return Result<CustomerDto>.Success(dto);
        }

        return Result<CustomerDto>.Failure("Customer not found");
    }

    public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        // Check if customer code already exists
        var checkCommand = new SqlCommand(
            "SELECT COUNT(1) FROM Customers WHERE CustomerCode = @CustomerCode",
            connection);
        checkCommand.Parameters.AddWithValue("@CustomerCode", request.CustomerCode);

        // checkCommand.LogQuery(_logger);
        var exists = (int)await checkCommand.ExecuteScalarAsync() > 0;
        if (exists)
        {
            return Result<CustomerDto>.Failure("Customer code already exists");
        }

        // Insert new customer
        var insertCommand = new SqlCommand(@"
            INSERT INTO Customers (CustomerCode, CustomerName, Email, Phone, Address, City, State, Country, PostalCode, IsActive, CreatedAt)
            OUTPUT INSERTED.CustomerId
            VALUES (@CustomerCode, @CustomerName, @Email, @Phone, @Address, @City, @State, @Country, @PostalCode, @IsActive, @CreatedAt)",
            connection);

        insertCommand.Parameters.AddWithValue("@CustomerCode", request.CustomerCode);
        insertCommand.Parameters.AddWithValue("@CustomerName", request.CustomerName);
        insertCommand.Parameters.AddWithValue("@Email", (object?)request.Email ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@Phone", (object?)request.Phone ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@Address", (object?)request.Address ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@City", (object?)request.City ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@State", (object?)request.State ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@Country", (object?)request.Country ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@PostalCode", (object?)request.PostalCode ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@IsActive", true);
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

        // insertCommand.LogQuery(_logger);
        var customerId = (int)await insertCommand.ExecuteScalarAsync();

        var dto = new CustomerDto(
            customerId,
            request.CustomerCode,
            request.CustomerName,
            request.Email,
            request.Phone,
            request.Address,
            request.City,
            request.State,
            request.Country,
            request.PostalCode,
            true,
            DateTime.UtcNow
        );

        return Result<CustomerDto>.Success(dto);
    }

    public async Task<Result<CustomerDto>> UpdateCustomerAsync(int customerId, UpdateCustomerRequest request)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        // Check if customer exists
        var checkCommand = new SqlCommand(
            "SELECT COUNT(1) FROM Customers WHERE CustomerId = @CustomerId",
            connection);
        checkCommand.Parameters.AddWithValue("@CustomerId", customerId);

        // checkCommand.LogQuery(_logger);
        var exists = (int)await checkCommand.ExecuteScalarAsync() > 0;
        if (!exists)
        {
            return Result<CustomerDto>.Failure("Customer not found");
        }

        // Update customer
        var updateCommand = new SqlCommand(@"
            UPDATE Customers
            SET CustomerName = @CustomerName,
                Email = @Email,
                Phone = @Phone,
                Address = @Address,
                City = @City,
                State = @State,
                Country = @Country,
                PostalCode = @PostalCode,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE CustomerId = @CustomerId",
            connection);

        updateCommand.Parameters.AddWithValue("@CustomerId", customerId);
        updateCommand.Parameters.AddWithValue("@CustomerName", request.CustomerName);
        updateCommand.Parameters.AddWithValue("@Email", (object?)request.Email ?? DBNull.Value);
        updateCommand.Parameters.AddWithValue("@Phone", (object?)request.Phone ?? DBNull.Value);
        updateCommand.Parameters.AddWithValue("@Address", (object?)request.Address ?? DBNull.Value);
        updateCommand.Parameters.AddWithValue("@City", (object?)request.City ?? DBNull.Value);
        updateCommand.Parameters.AddWithValue("@State", (object?)request.State ?? DBNull.Value);
        updateCommand.Parameters.AddWithValue("@Country", (object?)request.Country ?? DBNull.Value);
        updateCommand.Parameters.AddWithValue("@PostalCode", (object?)request.PostalCode ?? DBNull.Value);
        updateCommand.Parameters.AddWithValue("@IsActive", request.IsActive);
        updateCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        // updateCommand.LogQuery(_logger);
        await updateCommand.ExecuteNonQueryAsync();

        // Fetch updated customer
        var selectCommand = new SqlCommand(@"
            SELECT CustomerId, CustomerCode, CustomerName, Email, Phone, Address, City, State, Country, PostalCode, IsActive, CreatedAt
            FROM Customers
            WHERE CustomerId = @CustomerId",
            connection);
        selectCommand.Parameters.AddWithValue("@CustomerId", customerId);

        // selectCommand.LogQuery(_logger);
        using var reader = await selectCommand.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var dto = new CustomerDto(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetString(9),
                reader.GetBoolean(10),
                reader.GetDateTime(11)
            );
            return Result<CustomerDto>.Success(dto);
        }

        return Result<CustomerDto>.Failure("Customer not found");
    }

    public async Task<Result> DeleteCustomerAsync(int customerId)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            UPDATE Customers
            SET IsActive = 0,
                UpdatedAt = @UpdatedAt
            WHERE CustomerId = @CustomerId",
            connection);

        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        // command.LogQuery(_logger);
        var rowsAffected = await command.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
        {
            return Result.Failure("Customer not found");
        }

        return Result.Success();
    }
}
