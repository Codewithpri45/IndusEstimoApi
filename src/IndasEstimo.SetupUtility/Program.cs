using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using IndasEstimo.Infrastructure.Security;
using IndasEstimo.Infrastructure.Configuration;

namespace IndasEstimo.SetupUtility;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("  IndusMonarch Setup Utility");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var masterConnectionString = configuration.GetConnectionString("MasterDatabase");
        var encryptionSettings = configuration.GetSection("EncryptionSettings").Get<EncryptionSettings>();

        if (string.IsNullOrEmpty(masterConnectionString))
        {
            Console.WriteLine("ERROR: MasterDatabase connection string not found in appsettings.json");
            return;
        }

        if (encryptionSettings == null || string.IsNullOrEmpty(encryptionSettings.AesKey))
        {
            Console.WriteLine("ERROR: EncryptionSettings not configured properly");
            return;
        }

        var passwordHasher = new PasswordHasher(Options.Create(encryptionSettings!));
        var encryptionService = new AesEncryptionService(Options.Create(encryptionSettings!));

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine();
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Hash a password (PBKDF2)");
            Console.WriteLine("2. Encrypt a database password (AES-256)");
            Console.WriteLine("3. Setup demo tenant credentials");
            Console.WriteLine("4. Test tenant connection");
            Console.WriteLine("5. Exit");
            Console.Write("> ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    HashPassword(passwordHasher);
                    break;
                case "2":
                    EncryptPassword(encryptionService);
                    break;
                case "3":
                    await SetupDemoCredentials(masterConnectionString, passwordHasher, encryptionService);
                    break;
                case "4":
                    await TestTenantConnection(masterConnectionString, encryptionService);
                    break;
                case "5":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }

        Console.WriteLine("Goodbye!");
    }

    static void HashPassword(PasswordHasher passwordHasher)
    {
        Console.WriteLine();
        Console.WriteLine("=== Hash Password (PBKDF2) ===");
        Console.Write("Enter password to hash: ");
        var password = ReadPassword();

        var hash = passwordHasher.HashPassword(password);

        Console.WriteLine();
        Console.WriteLine("Password Hash:");
        Console.WriteLine(hash);
        Console.WriteLine();
        Console.WriteLine("Copy this hash and use it in the Users table (PasswordHash column)");
    }

    static void EncryptPassword(AesEncryptionService encryptionService)
    {
        Console.WriteLine();
        Console.WriteLine("=== Encrypt Database Password (AES-256) ===");
        Console.Write("Enter database password to encrypt: ");
        var password = ReadPassword();

        var encryptedPassword = encryptionService.Encrypt(password, out string iv);

        Console.WriteLine();
        Console.WriteLine("Encrypted Password:");
        Console.WriteLine(encryptedPassword);
        Console.WriteLine();
        Console.WriteLine("Initialization Vector (IV):");
        Console.WriteLine(iv);
        Console.WriteLine();
        Console.WriteLine("Copy both values and use them in TenantConnectionStrings table:");
        Console.WriteLine("- DbPasswordEncrypted = " + encryptedPassword);
        Console.WriteLine("- EncryptionIV = " + iv);
    }

    static async Task SetupDemoCredentials(string masterConnectionString, PasswordHasher passwordHasher, AesEncryptionService encryptionService)
    {
        Console.WriteLine();
        Console.WriteLine("=== Setup Demo Tenant Credentials ===");
        Console.WriteLine();
        Console.WriteLine("This will update the DEMO tenant with working credentials.");
        Console.WriteLine();

        // Get database password
        Console.Write("Enter SQL password for 'sa' user (default: YourPassword123!): ");
        var dbPassword = ReadPassword();
        if (string.IsNullOrEmpty(dbPassword))
        {
            dbPassword = "YourPassword123!";
        }

        // Encrypt database password
        var encryptedDbPassword = encryptionService.Encrypt(dbPassword, out string iv);

        // Hash user passwords
        var adminHash = passwordHasher.HashPassword("admin123");
        var managerHash = passwordHasher.HashPassword("manager123");
        var userHash = passwordHasher.HashPassword("user123");

        try
        {
            using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            // Update Master DB - Connection String
            var updateConnCmd = connection.CreateCommand();
            updateConnCmd.CommandText = @"
                UPDATE TenantConnectionStrings
                SET DbPasswordEncrypted = @EncryptedPassword,
                    EncryptionIV = @IV,
                    ServerName = 'localhost',
                    DatabaseName = 'IndusMonarch_Tenant_DEMO',
                    DbUserName = 'sa'
                WHERE TenantId = (SELECT TenantId FROM Tenants WHERE TenantCode = 'DEMO')";

            updateConnCmd.Parameters.AddWithValue("@EncryptedPassword", encryptedDbPassword);
            updateConnCmd.Parameters.AddWithValue("@IV", iv);

            var rowsAffected = await updateConnCmd.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                Console.WriteLine("WARNING: No tenant connection string found for DEMO. Creating one...");

                var insertConnCmd = connection.CreateCommand();
                insertConnCmd.CommandText = @"
                    INSERT INTO TenantConnectionStrings (TenantId, ServerName, DatabaseName, DbUserName, DbPasswordEncrypted, EncryptionIV)
                    VALUES ((SELECT TenantId FROM Tenants WHERE TenantCode = 'DEMO'),
                            'localhost', 'IndusMonarch_Tenant_DEMO', 'sa', @EncryptedPassword, @IV)";

                insertConnCmd.Parameters.AddWithValue("@EncryptedPassword", encryptedDbPassword);
                insertConnCmd.Parameters.AddWithValue("@IV", iv);

                await insertConnCmd.ExecuteNonQueryAsync();
                Console.WriteLine("✓ Tenant connection string created");
            }
            else
            {
                Console.WriteLine("✓ Tenant connection string updated");
            }

            // Update Tenant DB - User Passwords
            // Build tenant connection string
            var tenantConnectionString = $"Server=localhost;Database=IndusMonarch_Tenant_DEMO;User Id=sa;Password={dbPassword};TrustServerCertificate=True;";

            using var tenantConnection = new SqlConnection(tenantConnectionString);
            await tenantConnection.OpenAsync();

            var updateUsersCmd = tenantConnection.CreateCommand();
            updateUsersCmd.CommandText = @"
                UPDATE Users SET PasswordHash = @AdminHash WHERE Username = 'admin';
                UPDATE Users SET PasswordHash = @ManagerHash WHERE Username = 'manager';
                UPDATE Users SET PasswordHash = @UserHash WHERE Username = 'user';
            ";

            updateUsersCmd.Parameters.AddWithValue("@AdminHash", adminHash);
            updateUsersCmd.Parameters.AddWithValue("@ManagerHash", managerHash);
            updateUsersCmd.Parameters.AddWithValue("@UserHash", userHash);

            await updateUsersCmd.ExecuteNonQueryAsync();
            Console.WriteLine("✓ User passwords updated");

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("SUCCESS! Demo tenant credentials set up.");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("You can now test the API with:");
            Console.WriteLine("Tenant: DEMO");
            Console.WriteLine("Users:");
            Console.WriteLine("  - admin / admin123 (Admin role)");
            Console.WriteLine("  - manager / manager123 (Manager role)");
            Console.WriteLine("  - user / user123 (User role)");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: " + ex.Message);
            Console.WriteLine();
            Console.WriteLine("Make sure:");
            Console.WriteLine("1. SQL Server is running");
            Console.WriteLine("2. Master database exists (run 01_Master_Schema.sql)");
            Console.WriteLine("3. Tenant database exists (run 03_Seed_TestData.sql)");
            Console.WriteLine("4. Connection string in appsettings.json is correct");
        }
    }

    static async Task TestTenantConnection(string masterConnectionString, AesEncryptionService encryptionService)
    {
        Console.WriteLine();
        Console.WriteLine("=== Test Tenant Connection ===");
        Console.Write("Enter tenant code to test (default: DEMO): ");
        var tenantCode = Console.ReadLine();
        if (string.IsNullOrEmpty(tenantCode))
        {
            tenantCode = "DEMO";
        }

        try
        {
            using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT tcs.ServerName, tcs.DatabaseName, tcs.DbUserName, tcs.DbPasswordEncrypted, tcs.EncryptionIV
                FROM TenantConnectionStrings tcs
                INNER JOIN Tenants t ON t.TenantId = tcs.TenantId
                WHERE t.TenantCode = @TenantCode";

            cmd.Parameters.AddWithValue("@TenantCode", tenantCode);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var serverName = reader.GetString(0);
                var databaseName = reader.GetString(1);
                var dbUserName = reader.GetString(2);
                var encryptedPassword = reader.GetString(3);
                var iv = reader.GetString(4);

                Console.WriteLine();
                Console.WriteLine("Tenant Connection Details:");
                Console.WriteLine($"Server: {serverName}");
                Console.WriteLine($"Database: {databaseName}");
                Console.WriteLine($"Username: {dbUserName}");

                // Decrypt password
                var decryptedPassword = encryptionService.Decrypt(encryptedPassword, iv);
                Console.WriteLine($"Password: {new string('*', decryptedPassword.Length)} (decrypted successfully)");

                // Build connection string
                var connectionString = $"Server={serverName};Database={databaseName};User Id={dbUserName};Password={decryptedPassword};TrustServerCertificate=True;";

                // Test connection
                Console.WriteLine();
                Console.Write("Testing connection... ");

                using var testConnection = new SqlConnection(connectionString);
                await testConnection.OpenAsync();

                Console.WriteLine("SUCCESS!");
                Console.WriteLine("✓ Tenant database is accessible");
            }
            else
            {
                Console.WriteLine($"ERROR: Tenant '{tenantCode}' not found in Master database");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: " + ex.Message);
        }
    }

    static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }
}
