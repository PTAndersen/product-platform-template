using Microsoft.AspNetCore.Identity;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace PPTWebApp.Data
{
    public class DataInitializer
    {
        private readonly string _connectionString;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataInitializer(string connectionString, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _connectionString = connectionString;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeDataAsync()
        {
            CreateTablesAndInsertDummyData();
            await CreateUsersAndRoles();
        }

        private void CreateTablesAndInsertDummyData()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // SQL script to create Identity and application-specific tables if they don't exist, for testing in development
                string createTablesSql = @"
                    -- Table for roles
                    CREATE TABLE IF NOT EXISTS AspNetRoles (
                        Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),  -- Use UUID for RoleId
                        Name VARCHAR(256) NOT NULL,
                        NormalizedName VARCHAR(256) NOT NULL,
                        ConcurrencyStamp VARCHAR(256)
                    );

                    -- Table for users
                    CREATE TABLE IF NOT EXISTS AspNetUsers (
                        Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),  -- Use UUID for UserId
                        UserName VARCHAR(256) NOT NULL,
                        NormalizedUserName VARCHAR(256) NOT NULL,
                        Email VARCHAR(256),
                        NormalizedEmail VARCHAR(256),
                        EmailConfirmed BOOLEAN NOT NULL,
                        PasswordHash TEXT,
                        SecurityStamp TEXT,
                        ConcurrencyStamp TEXT,
                        PhoneNumber TEXT,
                        PhoneNumberConfirmed BOOLEAN NOT NULL,
                        TwoFactorEnabled BOOLEAN NOT NULL,
                        LockoutEnd TIMESTAMP,
                        LockoutEnabled BOOLEAN NOT NULL,
                        AccessFailedCount INTEGER NOT NULL
                    );

                    -- Table linking users to roles
                    CREATE TABLE IF NOT EXISTS AspNetUserRoles (
                        UserId UUID NOT NULL,  -- Use UUID for UserId
                        RoleId UUID NOT NULL,  -- Use UUID for RoleId
                        PRIMARY KEY (UserId, RoleId),
                        CONSTRAINT FK_AspNetUserRoles_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE,
                        CONSTRAINT FK_AspNetUserRoles_AspNetRoles FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
                    );

                    -- Table for user claims
                    CREATE TABLE IF NOT EXISTS AspNetUserClaims (
                        Id SERIAL PRIMARY KEY,
                        UserId UUID NOT NULL,  -- Use UUID for UserId
                        ClaimType TEXT,
                        ClaimValue TEXT,
                        CONSTRAINT FK_AspNetUserClaims_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                    );

                    -- Table for user logins
                    CREATE TABLE IF NOT EXISTS AspNetUserLogins (
                        LoginProvider VARCHAR(128) NOT NULL,
                        ProviderKey VARCHAR(128) NOT NULL,
                        ProviderDisplayName TEXT,
                        UserId UUID NOT NULL,  -- Use UUID for UserId
                        PRIMARY KEY (LoginProvider, ProviderKey),
                        CONSTRAINT FK_AspNetUserLogins_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                    );

                    -- Table for user tokens (e.g., password reset, 2FA tokens)
                    CREATE TABLE IF NOT EXISTS AspNetUserTokens (
                        UserId UUID NOT NULL,  -- Use UUID for UserId
                        LoginProvider VARCHAR(128) NOT NULL,
                        Name VARCHAR(128) NOT NULL,
                        Value TEXT,
                        PRIMARY KEY (UserId, LoginProvider, Name),
                        CONSTRAINT FK_AspNetUserTokens_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                    );

                    -- Table for role claims
                    CREATE TABLE IF NOT EXISTS AspNetRoleClaims (
                        Id SERIAL PRIMARY KEY,
                        RoleId UUID NOT NULL,  -- Use UUID for RoleId
                        ClaimType TEXT,
                        ClaimValue TEXT,
                        CONSTRAINT FK_AspNetRoleClaims_AspNetRoles FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
                    );

                    -- Create your application's custom tables (Posts and Products)
                    CREATE TABLE IF NOT EXISTS Posts (
                        Id SERIAL PRIMARY KEY,
                        Title VARCHAR(255) NOT NULL,
                        Content TEXT NOT NULL,
                        DatePosted TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS Products (
                        Id SERIAL PRIMARY KEY,
                        Name VARCHAR(255) NOT NULL,
                        Description TEXT,
                        Price DECIMAL(10, 2) NOT NULL
                    );

                    -- Insert dummy data into Posts and Products
                    INSERT INTO Posts (Title, Content) 
                    SELECT 'First Post', 'This is the first post.' 
                    WHERE NOT EXISTS (SELECT 1 FROM Posts);

                    INSERT INTO Products (Name, Description, Price) 
                    SELECT 'Sample Product', 'This is a sample product.', 9.99 
                    WHERE NOT EXISTS (SELECT 1 FROM Products);";

                using (var createCommand = new NpgsqlCommand(createTablesSql, connection))
                {
                    createCommand.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        private async Task CreateUsersAndRoles()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var adminRole = new IdentityRole("Admin");
                await _roleManager.CreateAsync(adminRole);
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                var userRole = new IdentityRole("User");
                await _roleManager.CreateAsync(userRole);
            }

            if (await _userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };

                adminUser.NormalizedUserName = adminUser.UserName.ToUpper();
                adminUser.NormalizedEmail = adminUser.Email.ToUpper();

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Admin user created and assigned to Admin role.");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating Admin user: {error.Description}");
                    }
                }
            }

            if (await _userManager.FindByNameAsync("user") == null)
            {
                var regularUser = new ApplicationUser
                {
                    UserName = "user@example.com",
                    Email = "user@example.com",
                    EmailConfirmed = true
                };

                regularUser.NormalizedUserName = regularUser.UserName.ToUpper();
                regularUser.NormalizedEmail = regularUser.Email.ToUpper();

                var result = await _userManager.CreateAsync(regularUser, "User@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(regularUser, "User");
                    Console.WriteLine("Regular user created and assigned to User role.");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating Regular user: {error.Description}");
                    }
                }
            }
        }
    }
}
