namespace IndasEstimo.Application.DTOs.Inventory;

public record ProductDto(
    int ProductId,
    string ProductCode,
    string ProductName,
    string? Description,
    string? Category,
    decimal Price,
    decimal? Cost,
    int StockQuantity,
    string? Unit,
    bool IsActive,
    DateTime CreatedAt);

public record CreateProductRequest(
    string ProductCode,
    string ProductName,
    string? Description,
    string? Category,
    decimal Price,
    decimal? Cost,
    int StockQuantity,
    string? Unit);

public record UpdateProductRequest(
    string ProductName,
    string? Description,
    string? Category,
    decimal Price,
    decimal? Cost,
    int StockQuantity,
    string? Unit,
    bool IsActive);
