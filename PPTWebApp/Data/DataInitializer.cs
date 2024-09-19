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

                // SQL script to create Identity and application-specific tables if they don't exist, for testing in production
                string createTablesSql = @"
                    -- Table for roles
                    CREATE TABLE IF NOT EXISTS AspNetRoles (
                        Id SERIAL PRIMARY KEY,
                        Name VARCHAR(256) NOT NULL,
                        NormalizedName VARCHAR(256) NOT NULL,
                        ConcurrencyStamp VARCHAR(256)
                    );

                    -- Table for users
                    CREATE TABLE IF NOT EXISTS AspNetUsers (
                        Id SERIAL PRIMARY KEY,
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
                        UserId INTEGER NOT NULL,
                        RoleId INTEGER NOT NULL,
                        PRIMARY KEY (UserId, RoleId),
                        CONSTRAINT FK_AspNetUserRoles_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE,
                        CONSTRAINT FK_AspNetUserRoles_AspNetRoles FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
                    );

                    -- Table for user claims
                    CREATE TABLE IF NOT EXISTS AspNetUserClaims (
                        Id SERIAL PRIMARY KEY,
                        UserId INTEGER NOT NULL,
                        ClaimType TEXT,
                        ClaimValue TEXT,
                        CONSTRAINT FK_AspNetUserClaims_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                    );

                    -- Table for user logins
                    CREATE TABLE IF NOT EXISTS AspNetUserLogins (
                        LoginProvider VARCHAR(128) NOT NULL,
                        ProviderKey VARCHAR(128) NOT NULL,
                        ProviderDisplayName TEXT,
                        UserId INTEGER NOT NULL,
                        PRIMARY KEY (LoginProvider, ProviderKey),
                        CONSTRAINT FK_AspNetUserLogins_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                    );

                    -- Table for user tokens (e.g., password reset, 2FA tokens)
                    CREATE TABLE IF NOT EXISTS AspNetUserTokens (
                        UserId INTEGER NOT NULL,
                        LoginProvider VARCHAR(128) NOT NULL,
                        Name VARCHAR(128) NOT NULL,
                        Value TEXT,
                        PRIMARY KEY (UserId, LoginProvider, Name),
                        CONSTRAINT FK_AspNetUserTokens_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
                    );

                    -- Table for role claims
                    CREATE TABLE IF NOT EXISTS AspNetRoleClaims (
                        Id SERIAL PRIMARY KEY,
                        RoleId INTEGER NOT NULL,
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
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            if (await _userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            if (await _userManager.FindByNameAsync("user") == null)
            {
                var regularUser = new ApplicationUser
                {
                    UserName = "user",
                    Email = "user@example.com",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(regularUser, "User@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(regularUser, "User");
                }
            }
        }
    }
}
