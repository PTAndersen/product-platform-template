using Microsoft.AspNetCore.Identity;
using Npgsql;
using PPTWebApp.Data;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public class CustomUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserRoleStore<ApplicationUser>, IUserEmailStore<ApplicationUser>
{
    private readonly string _connectionString;

    public CustomUserStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    #region IUserStore Implementation

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    @"INSERT INTO AspNetUsers 
                  (Id, UserName, PasswordHash, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, SecurityStamp, ConcurrencyStamp, LockoutEnd)
                  VALUES 
                  (@Id, @username, @passwordHash, @normalizedUserName, @Email, @normalizedEmail, @EmailConfirmed, @phoneNumber, @phoneNumberConfirmed, @twoFactorEnabled, @lockoutEnabled, @accessFailedCount, @securityStamp, @concurrencyStamp, @lockoutEnd)", connection);


                command.Parameters.AddWithValue("@Id", Guid.Parse(user.Id));
                command.Parameters.AddWithValue("@username", user.UserName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@normalizedUserName", user.NormalizedUserName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@normalizedEmail", user.NormalizedEmail ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@EmailConfirmed", user.EmailConfirmed);
                command.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@phoneNumberConfirmed", user.PhoneNumberConfirmed);
                command.Parameters.AddWithValue("@twoFactorEnabled", user.TwoFactorEnabled);
                command.Parameters.AddWithValue("@lockoutEnabled", user.LockoutEnabled);
                command.Parameters.AddWithValue("@accessFailedCount", user.AccessFailedCount);
                command.Parameters.AddWithValue("@securityStamp", user.SecurityStamp ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@concurrencyStamp", user.ConcurrencyStamp ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@lockoutEnd", user.LockoutEnd ?? (object)DBNull.Value);

                connection.Open();
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}");

            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserCreationFailed",
                Description = $"An error occurred while creating the user: {ex.Message}"
            });
        }
    }


    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand(
                @"UPDATE aspnetusers 
              SET UserName = @username, 
                  PasswordHash = @passwordHash, 
                  NormalizedUserName = @normalizedUserName 
              WHERE Id = @userId", connection);

            command.Parameters.AddWithValue("@username", user.UserName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@passwordHash", user.PasswordHash ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@normalizedUserName", user.NormalizedUserName ?? (object)DBNull.Value);
            command.Parameters.Add("@userId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = Guid.Parse(user.Id);

            connection.Open();
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return IdentityResult.Success;
    }


    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand("DELETE FROM aspnetusers WHERE Id = @userId", connection);

            command.Parameters.Add("@userId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = Guid.Parse(user.Id);

            connection.Open();
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return IdentityResult.Success;
    }


    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? user = null;

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);

            var command = new NpgsqlCommand(
                "SELECT Id, UserName, PasswordHash, NormalizedUserName FROM aspnetusers WHERE Id = @userId", connection);

            command.Parameters.Add("@userId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = Guid.Parse(userId);

            using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    user = new ApplicationUser
                    {
                        Id = reader["Id"]?.ToString(),
                        UserName = reader["UserName"]?.ToString(),
                        PasswordHash = reader["PasswordHash"]?.ToString(),
                        NormalizedUserName = reader["NormalizedUserName"]?.ToString()
                    };
                }
            }
        }

        return user;
    }



    public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ApplicationUser user = null;

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand("SELECT Id, UserName, PasswordHash, NormalizedUserName FROM AspNetUsers WHERE NormalizedUserName = @normalizedUserName", connection);
            command.Parameters.AddWithValue("@normalizedUserName", normalizedUserName);

            connection.Open();
            using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    user = new ApplicationUser
                    {
                        Id = reader["Id"].ToString(),
                        UserName = reader["UserName"].ToString(),
                        PasswordHash = reader["PasswordHash"].ToString(),
                        NormalizedUserName = reader["NormalizedUserName"].ToString()
                    };
                }
            }
        }

        return user;
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    #endregion

    #region IUserPasswordStore Implementation

    public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    #endregion

    #region IUserRoleStore Implementation

    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand(
                @"INSERT INTO AspNetUserRoles (UserId, RoleId) 
              SELECT @userId, Id FROM AspNetRoles WHERE NormalizedName = @roleName", connection);

            // Ensure that user.Id is passed as a Guid (UUID)
            command.Parameters.Add("@userId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = Guid.Parse(user.Id);
            command.Parameters.AddWithValue("@roleName", roleName.ToUpper());

            connection.Open();
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }


    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand(
                @"DELETE FROM AspNetUserRoles 
                  WHERE UserId = @userId AND RoleId = 
                  (SELECT Id FROM AspNetRoles WHERE NormalizedName = @roleName)", connection);

            command.Parameters.AddWithValue("@userId", user.Id);
            command.Parameters.AddWithValue("@roleName", roleName.ToUpper());

            connection.Open();
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var roles = new List<string>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand(
                @"SELECT r.Name 
                  FROM AspNetRoles r
                  INNER JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
                  WHERE ur.UserId::TEXT = @userId", connection);

            command.Parameters.AddWithValue("@userId", user.Id);

            connection.Open();
            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    roles.Add(reader["Name"].ToString());
                }
            }
        }

        return roles;
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand(@"
            SELECT COUNT(*)
            FROM AspNetUserRoles ur
            INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
            WHERE CAST(ur.UserId AS TEXT) = @userId AND r.NormalizedName = @roleName", connection);

            command.Parameters.AddWithValue("@userId", user.Id);
            command.Parameters.AddWithValue("@roleName", roleName.ToUpper());

            connection.Open();
            var count = (long)await command.ExecuteScalarAsync(cancellationToken);
            return count > 0;
        }
    }


    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var users = new List<ApplicationUser>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand(
                @"SELECT u.Id, u.UserName 
                  FROM AspNetUsers u
                  INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
                  INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                  WHERE r.NormalizedName = @roleName", connection);

            command.Parameters.AddWithValue("@roleName", roleName.ToUpper());

            connection.Open();
            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    users.Add(new ApplicationUser
                    {
                        Id = reader["Id"].ToString(),
                        UserName = reader["UserName"].ToString()
                    });
                }
            }
        }

        return users;
    }

    #endregion

    #region Email Store Implementation

    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.Email = email;
        return Task.CompletedTask; // TODO: Make sure to persist this change to the database if needed.
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.EmailConfirmed = confirmed;
        return Task.CompletedTask; // TODO: Ensure that the database is updated here.
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(normalizedEmail))
            throw new ArgumentNullException(nameof(normalizedEmail));

        ApplicationUser? user = null;

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);

            using (var command = new NpgsqlCommand("SELECT * FROM Users WHERE NormalizedEmail = @NormalizedEmail", connection))
            {
                command.Parameters.AddWithValue("NormalizedEmail", normalizedEmail);

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        user = new ApplicationUser
                        {
                            Id = reader["Id"].ToString(),
                            UserName = reader["UserName"].ToString(),
                            Email = reader["Email"].ToString(),
                            NormalizedEmail = reader["NormalizedEmail"].ToString(),
                            EmailConfirmed = (bool)reader["EmailConfirmed"]
                        };
                    }
                }
            }
        }

        return user; // TODO: Ensure error handling/logging if the user is not found.
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask; // TODO: Make sure to persist this change in the database.
    }

    #endregion

    public void Dispose()
    {
        // Clean up resources if necessary
    }

    
}
