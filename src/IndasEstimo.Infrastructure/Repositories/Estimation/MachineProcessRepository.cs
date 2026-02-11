using Dapper;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Estimation;

/// <summary>
/// Repository implementation for Machine and Process operations
/// </summary>
public class MachineProcessRepository : IMachineProcessRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MachineProcessRepository> _logger;

    public MachineProcessRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<MachineProcessRepository> logger)
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

    public async Task<List<MachineGridDto>> GetMachineGridAsync(string contentDomainType)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                MachineID,
                MachineName,
                MachineType,
                MachineColors,
                MaxSheetL,
                MaxSheetW,
                MinSheetL,
                MinSheetW,
                ISNULL(PerHourRate, 0) AS PerHourRate,
                PaperGroup
            FROM MachineMaster
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
              AND ISNULL(IsActive, 0) = 1
              AND ContentDomainType = @ContentDomainType
            ORDER BY MachineName";

        var results = await connection.QueryAsync<MachineGridDto>(query, 
            new { CompanyID = companyId, ContentDomainType = contentDomainType });
        return results.ToList();
    }

    public async Task<List<MachineDto>> GetAllMachinesAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                MachineID,
                MachineName,
                MachineType,
                ISNULL(MachineColors, 0) AS MachineColors,
                ISNULL(IsActive, 0) AS IsActive
            FROM MachineMaster
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
              AND ISNULL(IsActive, 0) = 1
            ORDER BY MachineName";

        var results = await connection.QueryAsync<MachineDto>(query, new { CompanyID = companyId });
        return results.ToList();
    }

    public async Task<List<OperationDto>> GetDefaultOperationsAsync(string domainType)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                PM.ProcessID,
                PM.ProcessName,
                PM.TypeofCharges,
                ISNULL(PM.Rate, 0) AS Rate,
                ISNULL(PM.MinimumCharges, 0) AS MinimumCharges,
                ISNULL(PM.SetupCharges, 0) AS SetupCharges,
                ISNULL(PM.IsOnlineProcess, 0) AS IsOnlineProcess,
                ISNULL(DM.SequenceNo, 0) AS SequenceNo,
                PM.DepartmentID
            FROM ProcessMaster AS PM
            INNER JOIN DepartmentMaster AS DM ON DM.DepartmentID = PM.DepartmentID
            WHERE PM.CompanyID = @CompanyID
              AND ISNULL(PM.IsDeletedTransaction, 0) = 0
              AND ISNULL(PM.IsActive, 0) = 1
              AND PM.ContentDomainType = @DomainType
            ORDER BY DM.SequenceNo, PM.ProcessName";

        var results = await connection.QueryAsync<OperationDto>(query, 
            new { CompanyID = companyId, DomainType = domainType });
        return results.ToList();
    }

    public async Task<List<OperationSlabDto>> GetOperationSlabsAsync(long processId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                SlabID,
                ProcessID,
                RateFactor,
                ISNULL(FromValue, 0) AS FromValue,
                ISNULL(ToValue, 0) AS ToValue,
                ISNULL(Rate, 0) AS Rate
            FROM ProcessMasterSlabs
            WHERE ProcessID = @ProcessID
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY FromValue";

        var results = await connection.QueryAsync<OperationSlabDto>(query, 
            new { ProcessID = processId, CompanyID = companyId });
        return results.ToList();
    }

    public async Task<List<MachineItemDto>> GetMachineItemsAsync(long machineId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                IM.ItemID,
                IM.ItemCode,
                IM.ItemName,
                IM.ItemGroupID,
                IGM.ItemGroupName,
                ISNULL(IM.EstimationRate, 0) AS EstimationRate,
                IM.EstimationUnit
            FROM MachineMaterialAllocation AS MMA
            INNER JOIN ItemMaster AS IM ON IM.ItemID = MMA.ItemID
            LEFT JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            WHERE MMA.MachineID = @MachineID
              AND MMA.CompanyID = @CompanyID
              AND ISNULL(MMA.IsDeletedTransaction, 0) = 0
              AND ISNULL(IM.IsDeletedTransaction, 0) = 0
              AND ISNULL(IM.ISItemActive, 1) <> 0
            ORDER BY IM.ItemName";

        var results = await connection.QueryAsync<MachineItemDto>(query, 
            new { MachineID = machineId, CompanyID = companyId });
        return results.ToList();
    }
}
