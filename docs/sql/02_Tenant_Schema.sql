-- =============================================
-- Tenant Database Schema Template
-- Purpose: This script should be run for EACH tenant database
-- Example: Run this for DB_Tenant1, DB_Tenant2, etc.
-- =============================================

-- NOTE: Replace 'DB_TENANT_NAME' with actual tenant database name
-- USE DB_TENANT_NAME;
-- GO

-- =============================================
-- Table: Roles
-- =============================================
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
    PRINT 'Roles table created successfully';
END
GO

-- =============================================
-- Table: Users
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(100) UNIQUE NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,      -- PBKDF2 hash
        Email NVARCHAR(200) NULL,
        FullName NVARCHAR(200) NULL,
        Status INT NOT NULL DEFAULT 1,            -- 1=Active, 2=Inactive, 3=Locked
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        LastLoginAt DATETIME2 NULL,
        INDEX IX_Users_Username (Username),
        INDEX IX_Users_Status (Status)
    );
    PRINT 'Users table created successfully';
END
GO

-- =============================================
-- Table: UserRoles (Many-to-Many)
-- =============================================
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
    PRINT 'UserRoles table created successfully';
END
GO

-- =============================================
-- Table: Customers (CRM Module)
-- =============================================
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
    PRINT 'Customers table created successfully';
END
GO

-- =============================================
-- Table: Products (Inventory Module)
-- =============================================
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
    PRINT 'Products table created successfully';
END
GO

PRINT 'Tenant database schema creation completed successfully!';
GO
