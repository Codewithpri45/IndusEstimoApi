using IndasEstimo.Api.Middleware;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Repositories.ToolInventory;
using IndasEstimo.Application.Interfaces.Services.ToolInventory;
using IndasEstimo.Infrastructure.Repositories.ToolInventory;
using IndasEstimo.Infrastructure.Services.ToolInventory;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Menu;
using IndasEstimo.Application.Interfaces.Services.Estimation;
using IndasEstimo.Application.Interfaces.Services.Menu;
using IndasEstimo.Infrastructure.Repositories.Estimation;
using IndasEstimo.Infrastructure.Repositories.Menu;
using IndasEstimo.Infrastructure.Services.Estimation;
using IndasEstimo.Infrastructure.Services.Menu;
using IndasEstimo.Application.Interfaces.Repositories.Inventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Application.Interfaces.Services.Inventory;
using IndasEstimo.Infrastructure.Configuration;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.Database.Services;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Infrastructure.Repositories;
using IndasEstimo.Infrastructure.Repositories.Inventory;
using IndasEstimo.Infrastructure.Repositories.Masters;
using IndasEstimo.Infrastructure.Security;
using IndasEstimo.Infrastructure.Services;
using IndasEstimo.Infrastructure.Services.Inventory;
using IndasEstimo.Infrastructure.Services.Masters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
    
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using IndasEstimo.Application.Interfaces.Repositories.Enquiry;
using IndasEstimo.Application.Interfaces.Repositories.Masters;
using IndasEstimo.Application.Interfaces.Services.Masters;
using IndasEstimo.Infrastructure.Repositories.Enquiry;
using IndasEstimo.Infrastructure.Services.Enquiry;


var builder = WebApplication.CreateBuilder(args);

// ===== Configuration Binding =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EncryptionSettings>(builder.Configuration.GetSection("EncryptionSettings"));

// ===== Database Registration =====
// Connection Factory (No EF Core - Raw SQL only)
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// ===== Multi-Tenancy Services =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// ===== Database Services =====
builder.Services.AddScoped<IMasterDbService, MasterDbService>();
builder.Services.AddScoped<ITenantDbService, TenantDbService>();

builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IPurchaseRequisitionRepository, PurchaseRequisitionRepository>();
builder.Services.AddScoped<IRequisitionApprovalRepository, RequisitionApprovalRepository>();
builder.Services.AddScoped<IPurchaseOrderApprovalRepository, PurchaseOrderApprovalRepository>();
builder.Services.AddScoped<IPurchaseGRNRepository, PurchaseGRNRepository>();
builder.Services.AddScoped<IGRNApprovalRepository, GRNApprovalRepository>();
builder.Services.AddScoped<IItemMasterRepository, ItemMasterRepository>();
builder.Services.AddScoped<ILedgerMasterRepository, LedgerMasterRepository>();
builder.Services.AddScoped<IWarehouseMasterRepository, WarehouseMasterRepository>();
// ===== Application Services =====
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IRequisitionApprovalService, RequisitionApprovalService>();
builder.Services.AddScoped<IPurchaseOrderApprovalService, PurchaseOrderApprovalService>();
builder.Services.AddScoped<IPurchaseGRNService, PurchaseGRNService>();
builder.Services.AddScoped<IGRNApprovalService, GRNApprovalService>();
builder.Services.AddScoped<IItemIssueDirectRepository, ItemIssueDirectRepository>();
builder.Services.AddScoped<IItemIssueDirectService, ItemIssueDirectService>();

builder.Services.AddScoped<IItemMasterService, ItemMasterService>();
builder.Services.AddScoped<ILedgerMasterService, LedgerMasterService>();
builder.Services.AddScoped<IWarehouseMasterService, WarehouseMasterService>();


// ===== Tool Inventory Repositories =====
builder.Services.AddScoped<IToolRequisitionRepository, ToolRequisitionRepository>();
builder.Services.AddScoped<IToolPurchaseOrderRepository, ToolPurchaseOrderRepository>();
builder.Services.AddScoped<IToolReceiptNoteRepository, ToolReceiptNoteRepository>();
builder.Services.AddScoped<IToolReturnToStockRepository, ToolReturnToStockRepository>();
builder.Services.AddScoped<IToolIssueRepository, ToolIssueRepository>();



// ===== Application Services =====
builder.Services.AddScoped<IMasterAuthService, MasterAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDbOperationsService, DbOperationsService>();

// Add this line with your other services
builder.Services.AddScoped<IPurchaseRequisitionService, PurchaseRequisitionService>();

// ===== Estimation Module Services =====
builder.Services.AddScoped<IMaterialSelectionRepository, MaterialSelectionRepository>();
builder.Services.AddScoped<IMaterialSelectionService, MaterialSelectionService>();
builder.Services.AddScoped<IMasterDataRepository, MasterDataRepository>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IMachineProcessRepository, MachineProcessRepository>();
builder.Services.AddScoped<IMachineProcessService, MachineProcessService>();

builder.Services.AddScoped<IToolMaterialRepository, ToolMaterialRepository>();
builder.Services.AddScoped<IToolMaterialService, ToolMaterialService>();

builder.Services.AddScoped<IPlanningCalculationRepository, PlanningCalculationRepository>();
builder.Services.AddScoped<IPlanningCalculationService, PlanningCalculationService>();

builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
builder.Services.AddScoped<IQuotationService, QuotationService>();

builder.Services.AddScoped<IFlexoCalculationService, FlexoCalculationService>();



// ===== Menu Module Services =====
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();



// ===== Tool Inventory Services =====

builder.Services.AddScoped<IToolRequisitionService, ToolRequisitionService>();
builder.Services.AddScoped<IToolPurchaseOrderService, ToolPurchaseOrderService>();
builder.Services.AddScoped<IToolReceiptNoteService, ToolReceiptNoteService>();
builder.Services.AddScoped<IToolReturnToStockService, ToolReturnToStockService>();
builder.Services.AddScoped<IToolIssueService, ToolIssueService>();

// ===== Enquiry Module Services =====
builder.Services.AddScoped<IEnquiryRepository, EnquiryRepository>();
builder.Services.AddScoped<IEnquiryService, EnquiryService>();


// ===== Masters Module Services =====
builder.Services.AddScoped<IMachineMasterRepository, MachineMasterRepository>();
builder.Services.AddScoped<IMachineMasterService, MachineMasterService>();
builder.Services.AddScoped<IProcessMasterRepository, ProcessMasterRepository>();
builder.Services.AddScoped<IProcessMasterService, ProcessMasterService>();
builder.Services.AddScoped<IProductHSNMasterRepository, ProductHSNMasterRepository>();
builder.Services.AddScoped<IProductHSNMasterService, ProductHSNMasterService>();
//Repository registration
builder.Services.AddScoped<IDepartmentMasterRepository, DepartmentMasterRepository>();

//Service registration:
builder.Services.AddScoped<IDepartmentMasterService, DepartmentMasterService>();


// ===== Security Services =====
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IAesEncryptionService, AesEncryptionService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ===== Caching =====
builder.Services.AddMemoryCache();



// ===== Authentication =====
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ===== Controllers & API Explorer =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IndasEstimo API",
        Version = "v1",
        Description = "Multi-Tenant ERP API with Two-Level Authentication"
    });

    // JWT Authorization in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer eyJhbGciOiJIUzI1NiIs...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== CORS (Configure for your React frontend) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== Middleware Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IndasEstimo API v1");
    });
}

// Exception handling FIRST
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
// Authentication BEFORE TenantMiddleware
app.UseAuthentication();

// Tenant resolution AFTER authentication
app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.Run();
