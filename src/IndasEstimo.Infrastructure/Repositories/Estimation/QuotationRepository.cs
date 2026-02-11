using Dapper;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace IndasEstimo.Infrastructure.Repositories.Estimation;

/// <summary>
/// Repository implementation for Quotation operations
/// </summary>
public class QuotationRepository : IQuotationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<QuotationRepository> _logger;

    public QuotationRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<QuotationRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    public async Task<long> GetNextQuoteNumberAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT ISNULL(MAX(MAXBookingNo), 0) + 1 AS NextNumber
            FROM JobBooking
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsEstimate, 0) = 1";

        var nextNumber = await connection.QueryFirstOrDefaultAsync<long>(query, new { CompanyID = companyId });
        return nextNumber > 0 ? nextNumber : 1;
    }

    public async Task<long> SaveQuotationAsync(SaveQuotationRequest request, long userId, long companyId)
    {
        using var connection = GetConnection();

        // For simplicity, using basic insert - in production, this would be much more complex
        // with multiple related tables (JobBooking, JobBookingContents, JobBookingCostings, etc.)
        
        string insertQuery;
        long bookingId;

        if (request.BookingID > 0)
        {
            // Update existing quotation
            insertQuery = @"
                UPDATE JobBooking
                SET 
                    JobName = @JobName,
                    LedgerID = @LedgerID,
                    CategoryID = @CategoryID,
                    OrderQuantity = @OrderQuantity,
                    TypeOfCost = @TypeOfCost,
                    FinalCost = @FinalCost,
                    QuotedCost = @QuotedCost,
                    Remark = @Remark,
                    ProductCode = @ProductCode,
                    ExpectedCompletionDays = @ExpectedCompletionDays,
                    CurrencySymbol = @CurrencySymbol,
                    ConversionValue = @ConversionValue,
                    ModifiedDate = GETDATE(),
                    ModifiedBy = @UserId
                WHERE BookingID = @BookingID
                  AND CompanyID = @CompanyID;
                
                SELECT @BookingID;";

            bookingId = await connection.QueryFirstOrDefaultAsync<long>(insertQuery, new
            {
                request.BookingID,
                request.JobName,
                request.LedgerID,
                request.CategoryID,
                request.OrderQuantity,
                request.TypeOfCost,
                request.FinalCost,
                request.QuotedCost,
                request.Remark,
                request.ProductCode,
                request.ExpectedCompletionDays,
                request.CurrencySymbol,
                request.ConversionValue,
                UserId = userId,
                CompanyID = companyId
            });
        }
        else
        {
            // Insert new quotation
            insertQuery = @"
                INSERT INTO JobBooking (
                    MAXBookingNo, BookingNo, JobName, LedgerID, CategoryID,
                    OrderQuantity, TypeOfCost, FinalCost, QuotedCost, Remark,
                    ProductCode, ExpectedCompletionDays, CurrencySymbol, ConversionValue,
                    IsEstimate, QuoteType, IsCancelled, IsDeletedTransaction,
                    CreatedDate, CreatedBy, CompanyID
                )
                VALUES (
                    @MAXBookingNo, 
                    CAST(@MAXBookingNo AS VARCHAR) + '/' + FORMAT(GETDATE(), 'yyyy'),
                    @JobName, @LedgerID, @CategoryID,
                    @OrderQuantity, @TypeOfCost, @FinalCost, @QuotedCost, @Remark,
                    @ProductCode, @ExpectedCompletionDays, @CurrencySymbol, @ConversionValue,
                    1, 'Job Costing', 0, 0,
                    GETDATE(), @UserId, @CompanyID
                );
                
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            bookingId = await connection.QueryFirstOrDefaultAsync<long>(insertQuery, new
            {
                request.MAXBookingNo,
                request.JobName,
                request.LedgerID,
                request.CategoryID,
                request.OrderQuantity,
                request.TypeOfCost,
                request.FinalCost,
                request.QuotedCost,
                request.Remark,
                request.ProductCode,
                request.ExpectedCompletionDays,
                request.CurrencySymbol,
                request.ConversionValue,
                UserId = userId,
                CompanyID = companyId
            });
        }

        return bookingId;
    }

    public async Task<QuotationDetailsDto?> LoadQuotationAsync(long bookingId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                JB.BookingID,
                JB.MAXBookingNo AS BookingNo,
                JB.JobName,
                JB.LedgerID,
                REPLACE(LM.LedgerName, '""', '') AS ClientName,
                JB.CategoryID,
                CM.CategoryName,
                JB.OrderQuantity,
                ISNULL(JB.AnnualQuantity, 0) AS AnnualQuantity,
                JB.TypeOfCost,
                JB.FinalCost,
                ISNULL(JB.QuotedCost, 0) AS QuotedCost,
                JB.Remark,
                JB.ProductCode,
                JB.CreatedDate,
                JB.ExpectedDeliveryDate,
                UM.UserName AS CreatedByUser,
                ISNULL(JB.IsApproved, 0) AS IsApproved,
                ISNULL(JB.IsSendForPriceApproval, 0) AS IsSendForPriceApproval
            FROM JobBooking AS JB
            INNER JOIN LedgerMaster AS LM ON LM.LedgerID = JB.LedgerID
            INNER JOIN CategoryMaster AS CM ON CM.CategoryID = JB.CategoryID
            INNER JOIN UserMaster AS UM ON UM.UserID = JB.CreatedBy
            WHERE JB.BookingID = @BookingID
              AND JB.CompanyID = @CompanyID
              AND ISNULL(JB.IsEstimate, 0) = 1
              AND ISNULL(JB.IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryFirstOrDefaultAsync<QuotationDetailsDto>(
            query,
            new { BookingID = bookingId, CompanyID = companyId });

        return result;
    }
}
