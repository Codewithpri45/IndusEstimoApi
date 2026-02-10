-- =============================================
-- Test Data Seed Script
-- Purpose: Insert sample data for testing
-- =============================================

-- =============================================
-- PART 1: Master Database Test Data
-- =============================================
USE IndusMonarch_Master;
GO

-- Insert Demo Tenant
IF NOT EXISTS (SELECT * FROM Tenants WHERE TenantCode = 'DEMO')
BEGIN
    INSERT INTO Tenants (TenantCode, TenantName, CompanyCode, Status)
    VALUES ('DEMO', 'Demo Company Ltd', 'DEMO-001', 1);
    PRINT 'Demo tenant inserted';
END
GO

-- Get the tenant ID
DECLARE @TenantId INT = (SELECT TenantId FROM Tenants WHERE TenantCode = 'DEMO');

-- Insert Connection String for Demo Tenant
-- NOTE: Update these values to match your SQL Server setup
-- The password below will be encrypted by the application when using the API
-- For this test data, we're storing a placeholder encrypted value
IF NOT EXISTS (SELECT * FROM TenantConnectionStrings WHERE TenantId = @TenantId)
BEGIN
    INSERT INTO TenantConnectionStrings (
        TenantId,
        ServerName,
        DatabaseName,
        DbUserName,
        DbPasswordEncrypted,
        EncryptionIV
    )
    VALUES (
        @TenantId,
        'localhost',                          -- Update to your SQL Server
        'IndusMonarch_Tenant_DEMO',          -- Tenant database name
        'sa',                                 -- Update to your SQL user
        'ENCRYPTED_PASSWORD_PLACEHOLDER',     -- Will be replaced by actual encrypted password
        'IV_PLACEHOLDER'                      -- Will be replaced by actual IV
    );
    PRINT 'Demo tenant connection string inserted (requires encryption setup)';
END
GO

-- Insert Demo License
DECLARE @TenantId2 INT = (SELECT TenantId FROM Tenants WHERE TenantCode = 'DEMO');
IF NOT EXISTS (SELECT * FROM TenantLicenses WHERE TenantId = @TenantId2)
BEGIN
    INSERT INTO TenantLicenses (TenantId, LicenseType, MaxUsers, ExpiryDate, IsActive)
    VALUES (@TenantId2, 3, 100, DATEADD(YEAR, 1, GETDATE()), 1); -- Enterprise, 100 users, 1 year
    PRINT 'Demo tenant license inserted';
END
GO

PRINT 'Master database test data seeded successfully!';
PRINT '';
PRINT '=================================================================';
PRINT 'IMPORTANT: Before testing, you need to:';
PRINT '1. Create the tenant database: IndusMonarch_Tenant_DEMO';
PRINT '2. Run the tenant schema script (02_Tenant_Schema.sql) on it';
PRINT '3. Update TenantConnectionStrings with encrypted password using API';
PRINT '   OR manually encrypt the password using AES-256';
PRINT '=================================================================';
GO

-- =============================================
-- PART 2: Tenant Database Test Data (DEMO)
-- =============================================

-- Create Demo Tenant Database if not exists
USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'IndusMonarch_Tenant_DEMO')
BEGIN
    CREATE DATABASE IndusMonarch_Tenant_DEMO;
    PRINT 'Demo tenant database created successfully';
END
ELSE
BEGIN
    PRINT 'Demo tenant database already exists';
END
GO

USE IndusMonarch_Tenant_DEMO;
GO

-- Run the schema creation first (copied from 02_Tenant_Schema.sql)
-- NOTE: In production, you would run 02_Tenant_Schema.sql separately

-- Roles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        RoleId INT PRIMARY KEY IDENTITY(1,1),
        RoleName NVARCHAR(100) UNIQUE NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        INDEX IX_Roles_RoleName (RoleName)
    );
END
GO

-- Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(100) UNIQUE NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        Email NVARCHAR(200) NULL,
        FullName NVARCHAR(200) NULL,
        Status INT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        LastLoginAt DATETIME2 NULL,
        INDEX IX_Users_Username (Username),
        INDEX IX_Users_Status (Status)
    );
END
GO

-- UserRoles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
BEGIN
    CREATE TABLE UserRoles (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        AssignedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId)
            REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId)
            REFERENCES Roles(RoleId) ON DELETE CASCADE,
        CONSTRAINT UQ_UserRoles_UserRole UNIQUE (UserId, RoleId),
        INDEX IX_UserRoles_UserId (UserId),
        INDEX IX_UserRoles_RoleId (RoleId)
    );
END
GO

-- Customers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        CustomerId INT PRIMARY KEY IDENTITY(1,1),
        CustomerCode NVARCHAR(50) UNIQUE NOT NULL,
        CustomerName NVARCHAR(200) NOT NULL,
        ContactPerson NVARCHAR(200) NULL,
        Email NVARCHAR(200) NULL,
        Phone NVARCHAR(50) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        INDEX IX_Customers_CustomerCode (CustomerCode),
        INDEX IX_Customers_IsActive (IsActive)
    );
END
GO

-- Products Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        ProductId INT PRIMARY KEY IDENTITY(1,1),
        ProductCode NVARCHAR(50) UNIQUE NOT NULL,
        ProductName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Category NVARCHAR(100) NULL,
        Price DECIMAL(18,2) NOT NULL DEFAULT 0,
        Cost DECIMAL(18,2) NULL,
        StockQuantity INT NOT NULL DEFAULT 0,
        Unit NVARCHAR(50) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        INDEX IX_Products_ProductCode (ProductCode),
        INDEX IX_Products_IsActive (IsActive),
        INDEX IX_Products_Category (Category)
    );
END
GO

-- Insert Roles
IF NOT EXISTS (SELECT * FROM Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO Roles (RoleName, Description, IsActive)
    VALUES
        ('Admin', 'System Administrator', 1),
        ('Manager', 'Department Manager', 1),
        ('User', 'Standard User', 1),
        ('Guest', 'Guest User (Read-only)', 1);
    PRINT 'Roles inserted';
END
GO

-- Insert Test Users
-- NOTE: These password hashes are PBKDF2 hashes of simple passwords for testing
-- In production, users should create strong passwords
-- Password for 'admin' user: 'admin123'
-- Password for 'manager' user: 'manager123'
-- Password for 'user' user: 'user123'

-- IMPORTANT: The hashes below are placeholders
-- The actual PBKDF2 hash will be generated by the PasswordHasher service
-- Format: Base64(Salt + Hash) where Salt=32 bytes, Hash=32 bytes
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, PasswordHash, Email, FullName, Status)
    VALUES
        ('admin', 'PBKDF2_HASH_PLACEHOLDER_ADMIN', 'admin@demo.com', 'System Administrator', 1),
        ('manager', 'PBKDF2_HASH_PLACEHOLDER_MANAGER', 'manager@demo.com', 'Department Manager', 1),
        ('user', 'PBKDF2_HASH_PLACEHOLDER_USER', 'user@demo.com', 'Test User', 1);
    PRINT 'Users inserted (requires password hashing via API)';
END
GO

-- Assign Roles to Users
DECLARE @AdminUserId INT = (SELECT UserId FROM Users WHERE Username = 'admin');
DECLARE @ManagerUserId INT = (SELECT UserId FROM Users WHERE Username = 'manager');
DECLARE @UserUserId INT = (SELECT UserId FROM Users WHERE Username = 'user');

DECLARE @AdminRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Admin');
DECLARE @ManagerRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Manager');
DECLARE @UserRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'User');

IF NOT EXISTS (SELECT * FROM UserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId)
    VALUES
        (@AdminUserId, @AdminRoleId),
        (@ManagerUserId, @ManagerRoleId),
        (@UserUserId, @UserRoleId);
    PRINT 'User roles assigned';
END
GO

-- Insert Sample Customers
IF NOT EXISTS (SELECT * FROM Customers WHERE CustomerCode = 'CUST001')
BEGIN
    INSERT INTO Customers (CustomerCode, CustomerName, ContactPerson, Email, Phone, Address, City, Country, IsActive)
    VALUES
        ('CUST001', 'Acme Corporation', 'John Doe', 'john@acme.com', '+1-555-0100', '123 Main St', 'New York', 'USA', 1),
        ('CUST002', 'TechCorp Solutions', 'Jane Smith', 'jane@techcorp.com', '+1-555-0200', '456 Tech Ave', 'San Francisco', 'USA', 1),
        ('CUST003', 'Global Industries', 'Bob Johnson', 'bob@global.com', '+44-20-1234-5678', '789 Business Rd', 'London', 'UK', 1);
    PRINT 'Sample customers inserted';
END
GO

-- Insert Sample Products
IF NOT EXISTS (SELECT * FROM Products WHERE ProductCode = 'PROD001')
BEGIN
    INSERT INTO Products (ProductCode, ProductName, Description, Category, Price, Cost, StockQuantity, Unit, IsActive)
    VALUES
        ('PROD001', 'Laptop Pro 15', 'High-performance laptop with 15-inch display', 'Electronics', 1299.99, 899.99, 50, 'EA', 1),
        ('PROD002', 'Wireless Mouse', 'Ergonomic wireless mouse with USB receiver', 'Accessories', 29.99, 15.00, 200, 'EA', 1),
        ('PROD003', 'Office Chair Deluxe', 'Ergonomic office chair with lumbar support', 'Furniture', 349.99, 200.00, 25, 'EA', 1),
        ('PROD004', 'USB-C Hub', '7-in-1 USB-C hub with HDMI and card reader', 'Accessories', 49.99, 25.00, 150, 'EA', 1);
    PRINT 'Sample products inserted';
END
GO

PRINT 'Tenant database test data seeded successfully!';
PRINT '';
PRINT '=================================================================';
PRINT 'SETUP COMPLETED! Next steps:';
PRINT '1. You need to manually encrypt the password and update Master DB';
PRINT '2. Or use a setup API endpoint to encrypt credentials properly';
PRINT '3. Update appsettings.json with correct SQL Server credentials';
PRINT '4. Run the API and use the authentication endpoints';
PRINT '=================================================================';
GO
