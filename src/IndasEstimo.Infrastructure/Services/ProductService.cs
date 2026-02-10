using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Infrastructure.Extensions;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Domain.Entities.Tenant;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;

namespace IndasEstimo.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IDbConnectionFactory connectionFactory, ITenantProvider tenantProvider, ILogger<ProductService> logger)
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

    public async Task<Result<List<ProductDto>>> GetAllProductsAsync()
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT ProductId, ProductCode, ProductName, Description, Category, Price, Cost, StockQuantity, Unit, IsActive, CreatedAt
            FROM Products
            WHERE IsActive = 1
            ORDER BY ProductName",
            connection);

        var products = new List<ProductDto>();
        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new ProductDto(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetDecimal(5),
                reader.GetDecimal(6),
                reader.GetInt32(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.GetBoolean(9),
                reader.GetDateTime(10)
            ));
        }

        return Result<List<ProductDto>>.Success(products);
    }

    public async Task<Result<ProductDto>> GetProductByIdAsync(int productId)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT ProductId, ProductCode, ProductName, Description, Category, Price, Cost, StockQuantity, Unit, IsActive, CreatedAt
            FROM Products
            WHERE ProductId = @ProductId",
            connection);
        command.Parameters.AddWithValue("@ProductId", productId);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var dto = new ProductDto(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetDecimal(5),
                reader.GetDecimal(6),
                reader.GetInt32(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.GetBoolean(9),
                reader.GetDateTime(10)
            );
            return Result<ProductDto>.Success(dto);
        }

        return Result<ProductDto>.Failure("Product not found");
    }

    public async Task<Result<ProductDto>> GetProductByCodeAsync(string productCode)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT ProductId, ProductCode, ProductName, Description, Category, Price, Cost, StockQuantity, Unit, IsActive, CreatedAt
            FROM Products
            WHERE ProductCode = @ProductCode",
            connection);
        command.Parameters.AddWithValue("@ProductCode", productCode);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var dto = new ProductDto(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetDecimal(5),
                reader.GetDecimal(6),
                reader.GetInt32(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.GetBoolean(9),
                reader.GetDateTime(10)
            );
            return Result<ProductDto>.Success(dto);
        }

        return Result<ProductDto>.Failure("Product not found");
    }

    public async Task<Result<ProductDto>> CreateProductAsync(CreateProductRequest request)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        // Check if product code already exists
        var checkCommand = new SqlCommand(
            "SELECT COUNT(1) FROM Products WHERE ProductCode = @ProductCode",
            connection);
        checkCommand.Parameters.AddWithValue("@ProductCode", request.ProductCode);

        // checkCommand.LogQuery(_logger);
        var exists = (int)await checkCommand.ExecuteScalarAsync() > 0;
        if (exists)
        {
            return Result<ProductDto>.Failure("Product code already exists");
        }

        // Insert new product
        var insertCommand = new SqlCommand(@"
            INSERT INTO Products (ProductCode, ProductName, Description, Category, Price, Cost, StockQuantity, Unit, IsActive, CreatedAt)
            OUTPUT INSERTED.ProductId
            VALUES (@ProductCode, @ProductName, @Description, @Category, @Price, @Cost, @StockQuantity, @Unit, @IsActive, @CreatedAt)",
            connection);

        insertCommand.Parameters.AddWithValue("@ProductCode", request.ProductCode);
        insertCommand.Parameters.AddWithValue("@ProductName", request.ProductName);
        insertCommand.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@Category", (object?)request.Category ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@Price", request.Price);
        insertCommand.Parameters.AddWithValue("@Cost", request.Cost);
        insertCommand.Parameters.AddWithValue("@StockQuantity", request.StockQuantity);
        insertCommand.Parameters.AddWithValue("@Unit", (object?)request.Unit ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@IsActive", true);
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

        // insertCommand.LogQuery(_logger);
        var productId = (int)await insertCommand.ExecuteScalarAsync();

        var dto = new ProductDto(
            productId,
            request.ProductCode,
            request.ProductName,
            request.Description,
            request.Category,
            request.Price,
            request.Cost,
            request.StockQuantity,
            request.Unit,
            true,
            DateTime.UtcNow
        );

        return Result<ProductDto>.Success(dto);
    }

    public async Task<Result<ProductDto>> UpdateProductAsync(int productId, UpdateProductRequest request)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        // Check if product exists
        var checkCommand = new SqlCommand(
            "SELECT COUNT(1) FROM Products WHERE ProductId = @ProductId",
            connection);
        checkCommand.Parameters.AddWithValue("@ProductId", productId);

        // checkCommand.LogQuery(_logger);
        var exists = (int)await checkCommand.ExecuteScalarAsync() > 0;
        if (!exists)
        {
            return Result<ProductDto>.Failure("Product not found");
        }

        // Update product
        var updateCommand = new SqlCommand(@"
            UPDATE Products
            SET ProductName = @ProductName,
                Description = @Description,
                Category = @Category,
                Price = @Price,
                Cost = @Cost,
                StockQuantity = @StockQuantity,
                Unit = @Unit,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE ProductId = @ProductId",
            connection);

        updateCommand.Parameters.AddWithValue("@ProductId", productId);
        updateCommand.Parameters.AddWithValue("@ProductName", request.ProductName);
        updateCommand.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
        updateCommand.Parameters.AddWithValue("@Category", (object?)request.Category ?? DBNull.Value);
        updateCommand.Parameters.AddWithValue("@Price", request.Price);
        updateCommand.Parameters.AddWithValue("@Cost", request.Cost);
        updateCommand.Parameters.AddWithValue("@StockQuantity", request.StockQuantity);
        updateCommand.Parameters.AddWithValue("@Unit", (object?)request.Unit ?? DBNull.Value);
        updateCommand.Parameters.AddWithValue("@IsActive", request.IsActive);
        updateCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        // updateCommand.LogQuery(_logger);
        await updateCommand.ExecuteNonQueryAsync();

        // Fetch updated product
        var selectCommand = new SqlCommand(@"
            SELECT ProductId, ProductCode, ProductName, Description, Category, Price, Cost, StockQuantity, Unit, IsActive, CreatedAt
            FROM Products
            WHERE ProductId = @ProductId",
            connection);
        selectCommand.Parameters.AddWithValue("@ProductId", productId);

        // selectCommand.LogQuery(_logger);
        using var reader = await selectCommand.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var dto = new ProductDto(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetDecimal(5),
                reader.GetDecimal(6),
                reader.GetInt32(7),
                reader.IsDBNull(8) ? null : reader.GetString(8),
                reader.GetBoolean(9),
                reader.GetDateTime(10)
            );
            return Result<ProductDto>.Success(dto);
        }

        return Result<ProductDto>.Failure("Product not found");
    }

    public async Task<Result> DeleteProductAsync(int productId)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            UPDATE Products
            SET IsActive = 0,
                UpdatedAt = @UpdatedAt
            WHERE ProductId = @ProductId",
            connection);

        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        // command.LogQuery(_logger);
        var rowsAffected = await command.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
        {
            return Result.Failure("Product not found");
        }

        return Result.Success();
    }
}
