-- =============================================
-- Master Database Schema
-- Database: IndusMonarch_Master
-- Purpose: Store tenant metadata, connection strings, licenses, and refresh tokens
-- =============================================

USE master;
GO

-- Create Master Database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'IndusMonarch_Master')
BEGIN
    CREATE DATABASE IndusMonarch_Master;
    PRINT 'Master database created successfully';
END
ELSE
BEGIN
    PRINT 'Master database already exists';
END
GO

USE IndusMonarch_Master;
GO

-- =============================================
-- Table: Tenants
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tenants')
BEGIN
    CREATE TABLE Tenants (
        TenantId INT PRIMARY KEY IDENTITY(1,1),
        TenantCode NVARCHAR(50) UNIQUE NOT NULL,
        TenantName NVARCHAR(200) NOT NULL,
        CompanyCode NVARCHAR(50) NULL,
        Status INT NOT NULL DEFAULT 1, -- 1=Active, 2=Suspended, 3=Expired
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        INDEX IX_Tenants_TenantCode (TenantCode),
        INDEX IX_Tenants_Status (Status)
    );
    PRINT 'Tenants table created successfully';
END
GO

-- =============================================
-- Table: TenantConnectionStrings
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TenantConnectionStrings')
BEGIN
    CREATE TABLE TenantConnectionStrings (
        Id INT PRIMARY KEY IDENTITY(1,1),
        TenantId INT NOT NULL,
        ServerName NVARCHAR(200) NOT NULL,
        DatabaseName NVARCHAR(100) NOT NULL,
        DbUserName NVARCHAR(100) NOT NULL,
        DbPasswordEncrypted NVARCHAR(500) NOT NULL, -- AES-256 encrypted
        EncryptionIV NVARCHAR(100) NOT NULL,        -- Initialization Vector
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TenantConnectionStrings_Tenants FOREIGN KEY (TenantId)
            REFERENCES Tenants(TenantId) ON DELETE CASCADE,
        INDEX IX_TenantConnectionStrings_TenantId (TenantId)
    );
    PRINT 'TenantConnectionStrings table created successfully';
END
GO

-- =============================================
-- Table: TenantLicenses
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TenantLicenses')
BEGIN
    CREATE TABLE TenantLicenses (
        Id INT PRIMARY KEY IDENTITY(1,1),
        TenantId INT NOT NULL,
        LicenseType INT NOT NULL,              -- 1=Standard, 2=Premium, 3=Enterprise
        MaxUsers INT NULL,
        ExpiryDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TenantLicenses_Tenants FOREIGN KEY (TenantId)
            REFERENCES Tenants(TenantId) ON DELETE CASCADE,
        INDEX IX_TenantLicenses_TenantId (TenantId)
    );
    PRINT 'TenantLicenses table created successfully';
END
GO

-- =============================================
-- Table: RefreshTokens
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,                   -- Reference to user in tenant DB
        TenantId INT NOT NULL,
        Token NVARCHAR(500) UNIQUE NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        RevokedAt DATETIME2 NULL,
        ReplacedByToken NVARCHAR(500) NULL,    -- For token rotation
        IsRevoked BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_RefreshTokens_Tenants FOREIGN KEY (TenantId)
            REFERENCES Tenants(TenantId) ON DELETE CASCADE,
        INDEX IX_RefreshTokens_Token (Token),
        INDEX IX_RefreshTokens_TenantId (TenantId)
    );
    PRINT 'RefreshTokens table created successfully';
END
GO

PRINT 'Master database schema creation completed successfully!';
GO
