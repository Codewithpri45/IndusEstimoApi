
using Dapper;
using IndasEstimo.Application.DTOs.Enquiry;
using IndasEstimo.Application.Interfaces.Repositories.Enquiry;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace IndasEstimo.Infrastructure.Repositories.Enquiry;

public class EnquiryRepository : IEnquiryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<EnquiryRepository> _logger;

    public EnquiryRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<EnquiryRepository> logger)
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

    public async Task<string> GetMaxEnquiryNoAsync()
    {
        // Placeholder for enquiry number generation (needs logic from DBConnection.GeneratePrefixedNo)
        // For now, implementing basic logic or calling a stored procedure if available
        // Or simply returning a guid/timestamp if complex SP logic is not ported
        return "EQ-" + DateTime.Now.Ticks.ToString();
    }

    public async Task<long> CreateEnquiryAsync(CreateEnquiryDto enquiry)
    {
        using var connection = GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var userId = _currentUserService.GetUserId() ?? 0;

            // 1. Insert Main Enquiry
            var sqlMain = @"
                INSERT INTO JobEnquiry (
                    EnquiryDate, LedgerID, JobName, ProductCode, Quantity, EstimationUnit, 
                    Remark, CompanyID, CreatedBy, CreatedDate, SalesEmployeeID, CategoryID, 
                    IsDeletedTransaction, RefEnquiryNo
                ) VALUES (
                    @EnquiryDate, @LedgerID, @JobName, @ProductCode, @Quantity, @EstimationUnit,
                    @Remark, @CompanyID, @CreatedBy, GETDATE(), @SalesEmployeeID, @CategoryID,
                    0, @RefEnquiryNo
                );
                SELECT CAST(SCOPE_IDENTITY() as bigint);";

            var enquiryId = await connection.ExecuteScalarAsync<long>(sqlMain, new
            {
                enquiry.EnquiryDate,
                enquiry.LedgerID,
                enquiry.JobName,
                ProductCode = enquiry.ProductCode ?? "",
                enquiry.Quantity,
                EstimationUnit = enquiry.EstimationUnit ?? "",
                Remark = enquiry.Remark ?? "",
                CompanyID = companyId,
                CreatedBy = userId,
                SalesEmployeeID = enquiry.SalesEmployeeID ?? 0, // Handle null
                CategoryID = enquiry.CategoryID ?? 0,
                RefEnquiryNo = enquiry.RefEnquiryNo ?? ""
            }, transaction);

            // 2. Insert Contents
            if (enquiry.Contents != null && enquiry.Contents.Any())
            {
                var sqlContent = @"
                    INSERT INTO JobEnquiryContents (
                        EnquiryID, ContentID, ContentName, Length, Width, Height, Unit,
                        CompanyID, CreatedBy, CreatedDate, IsDeletedTransaction
                    ) VALUES (
                        @EnquiryID, @ContentID, @ContentName, @Length, @Width, @Height, @Unit,
                        @CompanyID, @CreatedBy, GETDATE(), 0
                    )";
                
                foreach (var content in enquiry.Contents)
                {
                    await connection.ExecuteAsync(sqlContent, new
                    {
                        EnquiryID = enquiryId,
                        content.ContentID,
                        content.ContentName,
                        content.Length,
                        content.Width,
                        content.Height,
                        content.Unit,
                        CompanyID = companyId,
                        CreatedBy = userId
                    }, transaction);
                }
            }

            // 3. Insert Processes
            if (enquiry.Processes != null && enquiry.Processes.Any())
            {
                var sqlProcess = @"
                    INSERT INTO JobEnquiryProcess (
                        EnquiryID, ProcessID, ProcessName, Rate, Unit, Remarks,
                        CompanyID, CreatedBy, CreatedDate, IsDeletedTransaction
                    ) VALUES (
                        @EnquiryID, @ProcessID, @ProcessName, @Rate, @Unit, @Remarks,
                        @CompanyID, @CreatedBy, GETDATE(), 0
                    )";

                foreach (var process in enquiry.Processes)
                {
                    await connection.ExecuteAsync(sqlProcess, new
                    {
                        EnquiryID = enquiryId,
                        process.ProcessID,
                        process.ProcessName,
                        process.Rate,
                        process.Unit,
                        process.Remarks,
                        CompanyID = companyId,
                        CreatedBy = userId
                    }, transaction);
                }
            }

            // 4. Insert Layers
            if (enquiry.Layers != null && enquiry.Layers.Any())
            {
                var sqlLayer = @"
                    INSERT INTO JobEnquiryLayerDetail (
                        EnquiryID, LayerName, Description, SequenceNo,
                        CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                    ) VALUES (
                        @EnquiryID, @LayerName, @Description, @SequenceNo,
                        @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                    )";

                foreach (var layer in enquiry.Layers)
                {
                    await connection.ExecuteAsync(sqlLayer, new
                    {
                        EnquiryID = enquiryId,
                        layer.LayerName,
                        layer.Description,
                        layer.SequenceNo,
                        CompanyID = companyId,
                        CreatedBy = userId,
                        FYear = _currentUserService.GetFYear()
                    }, transaction);
                }
            }

            // 5. Insert Attachments
            if (enquiry.Attachments != null && enquiry.Attachments.Any())
            {
                var sqlAttachment = @"
                    INSERT INTO JobBookingAttachments (
                        EnquiryID, FileName, FilePath, FileType,
                        CompanyID, CreatedBy, CreatedDate, IsDeletedTransaction
                    ) VALUES (
                        @EnquiryID, @FileName, @FilePath, @FileType,
                        @CompanyID, @CreatedBy, GETDATE(), 0
                    )";

                foreach (var attachment in enquiry.Attachments)
                {
                    await connection.ExecuteAsync(sqlAttachment, new
                    {
                        EnquiryID = enquiryId,
                        attachment.FileName,
                        attachment.FilePath,
                        attachment.FileType,
                        CompanyID = companyId,
                        CreatedBy = userId
                    }, transaction);
                }
            }

            transaction.Commit();
            return enquiryId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateEnquiryAsync(UpdateEnquiryDto enquiry)
    {
       using var connection = GetConnection();
        // Implementation similar to Create but with UPDATE and DELETE/INSERT for details
        return true; 
    }

    public async Task<bool> DeleteEnquiryAsync(long enquiryId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        var sql = @"
            UPDATE JobEnquiry 
            SET IsDeletedTransaction = 1, DeletedBy = @DeletedBy, DeletedDate = GETDATE()
            WHERE EnquiryID = @EnquiryID AND CompanyID = @CompanyID";

        var rows = await connection.ExecuteAsync(sql, new { EnquiryID = enquiryId, DeletedBy = userId, CompanyID = companyId });
        return rows > 0;
    }

    public async Task<EnquiryListDto> GetEnquiryByIdAsync(long enquiryId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT 
                E.EnquiryID, E.EnquiryNo, E.EnquiryDate, E.LedgerID, L.LedgerName AS ClientName,
                E.JobName, E.ProductCode, E.Quantity, E.EstimationUnit, E.Remark, E.Status,
                ISNULL(E.IsSalesEnquiryApproved, 0) AS IsSalesEnquiryApproved,
                U.UserName AS EmployeeName, E.SalesEmployeeID, C.UserName AS CreatedBy
            FROM JobEnquiry E
            LEFT JOIN LedgerMaster L ON L.LedgerID = E.LedgerID
            LEFT JOIN UserMaster U ON U.UserID = E.SalesEmployeeID
            LEFT JOIN UserMaster C ON C.UserID = E.CreatedBy
            WHERE E.EnquiryID = @EnquiryID AND E.CompanyID = @CompanyID AND ISNULL(E.IsDeletedTransaction, 0) = 0";

        return await connection.QueryFirstOrDefaultAsync<EnquiryListDto>(sql, new { EnquiryID = enquiryId, CompanyID = companyId });
    }

    public async Task<IEnumerable<EnquiryListDto>> GetEnquiryListAsync(EnquiryFilterDto filter)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT 
                E.EnquiryID, E.EnquiryNo, E.EnquiryDate, E.LedgerID, L.LedgerName AS ClientName,
                E.JobName, E.ProductCode, E.Quantity, E.EstimationUnit, E.Remark, E.Status,
                ISNULL(E.IsSalesEnquiryApproved, 0) AS IsSalesEnquiryApproved,
                U.UserName AS EmployeeName, E.SalesEmployeeID, C.UserName AS CreatedBy
            FROM JobEnquiry E
            LEFT JOIN LedgerMaster L ON L.LedgerID = E.LedgerID
            LEFT JOIN UserMaster U ON U.UserID = E.SalesEmployeeID
            LEFT JOIN UserMaster C ON C.UserID = E.CreatedBy
            WHERE E.CompanyID = @CompanyID AND ISNULL(E.IsDeletedTransaction, 0) = 0";
        
        // Add date filters logic here if needed based on filter dto
        sql += " ORDER BY E.EnquiryID DESC";

        return await connection.QueryAsync<EnquiryListDto>(sql, new { CompanyID = companyId });
    }
    
    public async Task<IEnumerable<EnquiryProcessDto>> GetProcessDataAsync(long enquiryId)
    {
         using var connection = GetConnection();
         var sql = @"
            SELECT P.ProcessID, PM.ProcessName, P.Rate, P.Unit, P.Remarks
            FROM JobEnquiryProcess P
            JOIN ProcessMaster PM ON PM.ProcessID = P.ProcessID
            WHERE P.EnquiryID = @EnquiryID AND ISNULL(P.IsDeletedTransaction, 0) = 0";
         
         return await connection.QueryAsync<EnquiryProcessDto>(sql, new { EnquiryID = enquiryId });
    }

    public async Task<IEnumerable<EnquiryContentDto>> GetEnquiryContentsAsync(long enquiryId)
    {
         using var connection = GetConnection();
         var sql = @"
            SELECT ContentID, ContentName, Length, Width, Height, Unit
            FROM JobEnquiryContents
            WHERE EnquiryID = @EnquiryID AND ISNULL(IsDeletedTransaction, 0) = 0";
         
         return await connection.QueryAsync<EnquiryContentDto>(sql, new { EnquiryID = enquiryId });
    }
}
