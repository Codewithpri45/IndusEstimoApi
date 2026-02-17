using Dapper;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class DepartmentMasterRepository : IDepartmentMasterRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DepartmentMasterRepository> _logger;

    public DepartmentMasterRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        ICurrentUserService currentUserService,
        ILogger<DepartmentMasterRepository> logger)
    {
        _tenantProvider = tenantProvider;
        _connectionFactory = connectionFactory;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    public async Task<List<DepartmentListDto>> GetDepartmentListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(DepartmentID, 0) AS DepartmentID,
                ISNULL(DepartmentName, '') AS DepartmentName,
                ISNULL(Press, '') AS Press,
                SequenceNo
            FROM DepartmentMaster
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY SequenceNo, DepartmentName";

        var result = await connection.QueryAsync<DepartmentListDto>(sql);
        return result.ToList();
    }

    public async Task<string> SaveDepartmentAsync(SaveDepartmentRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            // Check for duplicate DepartmentName + Press
            var existSql = @"
                SELECT COUNT(1)
                FROM DepartmentMaster
                WHERE DepartmentName = @DepartmentName
                  AND Press = @Press
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            var exists = await connection.QueryFirstOrDefaultAsync<int>(
                existSql,
                new { request.DepartmentName, request.Press },
                transaction);

            if (exists > 0)
                return "Exist";

            // Check for duplicate SequenceNo
            if (request.SequenceNo.HasValue && request.SequenceNo > 0)
            {
                var seqSql = @"
                    SELECT COUNT(1)
                    FROM DepartmentMaster
                    WHERE SequenceNo = @SequenceNo
                      AND ISNULL(IsDeletedTransaction, 0) = 0";

                var seqExists = await connection.QueryFirstOrDefaultAsync<int>(
                    seqSql,
                    new { request.SequenceNo },
                    transaction);

                if (seqExists > 0)
                    return "Duplicate data found..";
            }

            var insertSql = @"
                INSERT INTO DepartmentMaster
                    (DepartmentName, Press, SequenceNo, CompanyID, FYear,
                     UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate)
                VALUES
                    (@DepartmentName, @Press, @SequenceNo, @CompanyID, @FYear,
                     @UserID, @CreatedBy, @ModifiedBy, GETDATE(), GETDATE())";

            await connection.ExecuteAsync(insertSql, new
            {
                request.DepartmentName,
                request.Press,
                request.SequenceNo,
                CompanyID = companyId,
                FYear = fYear,
                UserID = userId,
                CreatedBy = userId,
                ModifiedBy = userId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving department");
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> UpdateDepartmentAsync(UpdateDepartmentRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            // Check for duplicate DepartmentName + Press (excluding current record)
            var existSql = @"
                SELECT COUNT(1)
                FROM DepartmentMaster
                WHERE DepartmentName = @DepartmentName
                  AND Press = @Press
                  AND DepartmentID <> @DepartmentID
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            var exists = await connection.QueryFirstOrDefaultAsync<int>(
                existSql,
                new { request.DepartmentName, request.Press, request.DepartmentID },
                transaction);

            if (exists > 0)
                return "Exist";

            // Check for duplicate SequenceNo (excluding current record)
            if (request.SequenceNo.HasValue && request.SequenceNo > 0)
            {
                var seqSql = @"
                    SELECT COUNT(1)
                    FROM DepartmentMaster
                    WHERE SequenceNo = @SequenceNo
                      AND DepartmentID <> @DepartmentID
                      AND ISNULL(IsDeletedTransaction, 0) = 0";

                var seqExists = await connection.QueryFirstOrDefaultAsync<int>(
                    seqSql,
                    new { request.SequenceNo, request.DepartmentID },
                    transaction);

                if (seqExists > 0)
                    return "Duplicate data found..";
            }

            var updateSql = @"
                UPDATE DepartmentMaster
                SET DepartmentName = @DepartmentName,
                    Press          = @Press,
                    SequenceNo     = @SequenceNo,
                    ModifiedBy     = @ModifiedBy,
                    ModifiedDate   = GETDATE()
                WHERE DepartmentID = @DepartmentID
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            await connection.ExecuteAsync(updateSql, new
            {
                request.DepartmentName,
                request.Press,
                request.SequenceNo,
                ModifiedBy = userId,
                request.DepartmentID
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating department {DepartmentID}", request.DepartmentID);
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> DeleteDepartmentAsync(int departmentId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            var deleteSql = @"
                UPDATE DepartmentMaster
                SET ModifiedBy           = @ModifiedBy,
                    DeletedBy            = @DeletedBy,
                    DeletedDate          = GETDATE(),
                    ModifiedDate         = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE DepartmentID = @DepartmentID";

            await connection.ExecuteAsync(deleteSql, new
            {
                ModifiedBy = userId,
                DeletedBy = userId,
                DepartmentID = departmentId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting department {DepartmentID}", departmentId);
            return "fail";
        }
    }
}
