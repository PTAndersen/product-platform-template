using Npgsql;
using System.Data;
using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly string _connectionString;

    private readonly IUserProfileRepository _userProfileRepository;

    public ApplicationUserRepository(string connectionString, IUserProfileRepository userProfileRepository)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private async Task<ApplicationUser> MapToApplicationUser(NpgsqlDataReader reader, CancellationToken cancellationToken)
    {
        var userId = reader.GetGuid(reader.GetOrdinal("id")).ToString();
        //TODO: Handle NULL better?
        var userProfile = await _userProfileRepository.GetUserProfileByIdAsync(Guid.Parse(userId), cancellationToken) ?? new UserProfile
            {
                UserId = Guid.Parse(userId),
                FirstName = string.Empty,
                LastName = string.Empty,
                Telephone = string.Empty,
                CreatedAt = DateTime.UtcNow
            };
        var applicationUser = new ApplicationUser
        {
            Id = userId,
            UserName = reader.IsDBNull(reader.GetOrdinal("username")) ? null : reader.GetString(reader.GetOrdinal("username")),
            NormalizedUserName = reader.IsDBNull(reader.GetOrdinal("normalizedusername")) ? null : reader.GetString(reader.GetOrdinal("normalizedusername")),
            Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
            NormalizedEmail = reader.IsDBNull(reader.GetOrdinal("normalizedemail")) ? null : reader.GetString(reader.GetOrdinal("normalizedemail")),
            EmailConfirmed = !reader.IsDBNull(reader.GetOrdinal("emailconfirmed")) && reader.GetBoolean(reader.GetOrdinal("emailconfirmed")),
            PasswordHash = reader.IsDBNull(reader.GetOrdinal("passwordhash")) ? null : reader.GetString(reader.GetOrdinal("passwordhash")),
            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("phonenumber")) ? null : reader.GetString(reader.GetOrdinal("phonenumber")),
            PhoneNumberConfirmed = !reader.IsDBNull(reader.GetOrdinal("phonenumberconfirmed")) && reader.GetBoolean(reader.GetOrdinal("phonenumberconfirmed")),
            TwoFactorEnabled = !reader.IsDBNull(reader.GetOrdinal("twofactorenabled")) && reader.GetBoolean(reader.GetOrdinal("twofactorenabled")),
            LockoutEnabled = !reader.IsDBNull(reader.GetOrdinal("lockoutenabled")) && reader.GetBoolean(reader.GetOrdinal("lockoutenabled")),
            AccessFailedCount = reader.IsDBNull(reader.GetOrdinal("accessfailedcount")) ? 0 : reader.GetInt32(reader.GetOrdinal("accessfailedcount")),
            SecurityStamp = reader.IsDBNull(reader.GetOrdinal("securitystamp")) ? null : reader.GetString(reader.GetOrdinal("securitystamp")),
            ConcurrencyStamp = reader.IsDBNull(reader.GetOrdinal("concurrencystamp")) ? null : reader.GetString(reader.GetOrdinal("concurrencystamp")),
            LockoutEnd = reader.IsDBNull(reader.GetOrdinal("lockoutend")) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("lockoutend")),
            Profile = userProfile
        };

        return applicationUser;
    }

    #region User Management

    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    @"INSERT INTO aspnetuserroles (userid, roleid) 
                  SELECT @UserId, id FROM aspnetroles WHERE normalizedname = @RoleName", connection);

                command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));
                command.Parameters.AddWithValue("@RoleName", roleName.ToUpper());

                await connection.OpenAsync(cancellationToken);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding user to role: {ex.Message}");
            // TODO: Log error
            throw;
        }
    }

    public async Task AddUserAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                var command = new NpgsqlCommand(
                    @"INSERT INTO aspnetusers 
                (id, username, passwordhash, normalizedusername, email, normalizedemail, emailconfirmed, phonenumber, phonenumberconfirmed, twofactorenabled, lockoutenabled, accessfailedcount, securitystamp, concurrencystamp, lockoutend)
                VALUES 
                (@Id, @UserName, @PasswordHash, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed, @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled, @LockoutEnabled, @AccessFailedCount, @SecurityStamp, @ConcurrencyStamp, @LockoutEnd)",
                    connection);

                command.Parameters.AddWithValue("@Id", Guid.Parse(user.Id));
                command.Parameters.AddWithValue("@UserName", user.UserName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NormalizedUserName", user.NormalizedUserName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NormalizedEmail", user.NormalizedEmail ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@EmailConfirmed", user.EmailConfirmed);
                command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PhoneNumberConfirmed", user.PhoneNumberConfirmed);
                command.Parameters.AddWithValue("@TwoFactorEnabled", user.TwoFactorEnabled);
                command.Parameters.AddWithValue("@LockoutEnabled", user.LockoutEnabled);
                command.Parameters.AddWithValue("@AccessFailedCount", user.AccessFailedCount);
                command.Parameters.AddWithValue("@SecurityStamp", user.SecurityStamp ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ConcurrencyStamp", user.ConcurrencyStamp ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LockoutEnd", user.LockoutEnd ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync(cancellationToken);

                if (user.Profile != null)
                {
                    user.Profile.UserId = Guid.Parse(user.Id);
                    await _userProfileRepository.AddUserProfileAsync(user.Profile, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding user: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var command = new NpgsqlCommand("DELETE FROM aspnetusers WHERE id = @UserId", connection, transaction);
                        command.Parameters.AddWithValue("@UserId", Guid.Parse(userId));

                        await command.ExecuteNonQueryAsync(cancellationToken);

                        await _userProfileRepository.DeleteUserProfileAsync(Guid.Parse(userId), cancellationToken);

                        await transaction.CommitAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        Console.WriteLine($"Error deleting user and profile: {ex.Message}");
                        // TODO: Log error
                        throw;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user: {ex.Message}");
            // TODO: Log error
            throw;
        }
    }


    public async Task<ApplicationUser?> GetUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    "SELECT * FROM aspnetusers WHERE normalizedemail = @NormalizedEmail", connection);
                command.Parameters.AddWithValue("@NormalizedEmail", normalizedEmail);

                await connection.OpenAsync(cancellationToken);
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        return await MapToApplicationUser(reader, cancellationToken);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user by email: {ex.Message}");
            // TODO: Log error
            throw;
        }

        return null;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    "SELECT * FROM aspnetusers WHERE id = @UserId", connection);
                command.Parameters.Add("@UserId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = Guid.Parse(userId);

                await connection.OpenAsync(cancellationToken);
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        return await MapToApplicationUser(reader, cancellationToken);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user by ID: {ex.Message}");
            // TODO: Log error
            throw;
        }

        return null;
    }

    public async Task<ApplicationUser?> GetUserByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    "SELECT * FROM aspnetusers WHERE normalizedusername = @NormalizedUserName", connection);
                command.Parameters.AddWithValue("@NormalizedUserName", normalizedUserName);

                await connection.OpenAsync(cancellationToken);
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        return await MapToApplicationUser(reader, cancellationToken);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user by name: {ex.Message}");
            // TODO: Log error
            throw;
        }

        return null;
    }

    public async Task UpdateUserAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var command = new NpgsqlCommand(
                            @"UPDATE aspnetusers 
                            SET username = @UserName, passwordhash = @PasswordHash, normalizedusername = @NormalizedUserName,
                            email = @Email, normalizedemail = @NormalizedEmail, emailconfirmed = @EmailConfirmed,
                            phonenumber = @PhoneNumber, phonenumberconfirmed = @PhoneNumberConfirmed, twofactorenabled = @TwoFactorEnabled,
                            lockoutenabled = @LockoutEnabled, accessfailedcount = @AccessFailedCount, securitystamp = @SecurityStamp,
                            concurrencystamp = @ConcurrencyStamp, lockoutend = @LockoutEnd
                            WHERE id = @UserId",
                            connection, transaction);

                        command.Parameters.AddWithValue("@UserName", user.UserName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@NormalizedUserName", user.NormalizedUserName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@NormalizedEmail", user.NormalizedEmail ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@EmailConfirmed", user.EmailConfirmed);
                        command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PhoneNumberConfirmed", user.PhoneNumberConfirmed);
                        command.Parameters.AddWithValue("@TwoFactorEnabled", user.TwoFactorEnabled);
                        command.Parameters.AddWithValue("@LockoutEnabled", user.LockoutEnabled);
                        command.Parameters.AddWithValue("@AccessFailedCount", user.AccessFailedCount);
                        command.Parameters.AddWithValue("@SecurityStamp", user.SecurityStamp ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ConcurrencyStamp", user.ConcurrencyStamp ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@LockoutEnd", user.LockoutEnd ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));

                        await command.ExecuteNonQueryAsync(cancellationToken);

                        var userProfile = user.Profile ?? new UserProfile { UserId = Guid.Parse(user.Id) };
                        await _userProfileRepository.UpdateUserProfileAsync(userProfile, cancellationToken);

                        await transaction.CommitAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        Console.WriteLine($"Error updating user and profile: {ex.Message}");
                        // TODO: Log error
                        throw;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user: {ex.Message}");
            // TODO: Log error
            throw;
        }
    }




    #endregion

    #region Email management

    public async Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new NpgsqlCommand(
            "SELECT email FROM aspnetusers WHERE id = @UserId", connection);

        command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result == DBNull.Value ? null : result?.ToString();
    }

    public async Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new NpgsqlCommand(
            "SELECT emailconfirmed FROM aspnetusers WHERE id = @UserId", connection);

        command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null && (bool)result;
    }

    public async Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.Email = email;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new NpgsqlCommand(
            @"UPDATE aspnetusers 
          SET email = @Email, normalizedemail = @NormalizedEmail 
          WHERE id = @UserId", connection);

        command.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@NormalizedEmail", email?.ToUpperInvariant() ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.EmailConfirmed = confirmed;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new NpgsqlCommand(
            @"UPDATE aspnetusers 
          SET emailconfirmed = @EmailConfirmed 
          WHERE id = @UserId", connection);

        command.Parameters.AddWithValue("@EmailConfirmed", confirmed);
        command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Password Management

    public async Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(user.Id)) throw new ArgumentException("User ID cannot be null or empty.", nameof(user));
        if (!Guid.TryParse(user.Id, out var userId))
        {
            throw new ArgumentException("Invalid User ID format.", nameof(user.Id));
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            var command = new NpgsqlCommand(
                @"UPDATE aspnetusers 
                SET passwordhash = @PasswordHash
                WHERE id = @UserId", connection);

            command.Parameters.AddWithValue("@PasswordHash", passwordHash ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);

            user.PasswordHash = passwordHash;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Database error occurred: {ex.Message}");
            //TODO: Log error
            throw new ApplicationException("An error occurred while updating the password hash.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            //TODO: Log error
            throw new ApplicationException("An error occurred while updating the password hash.", ex);
        }

    }


    public async Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    "SELECT passwordhash FROM aspnetusers WHERE id = @UserId", connection);

                command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));

                await connection.OpenAsync(cancellationToken);
                var passwordHash = await command.ExecuteScalarAsync(cancellationToken);

                return passwordHash != null ? passwordHash.ToString() : string.Empty;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving password hash: {ex.Message}");
            // TODO: Log error
            throw;
        }
    }

    public async Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    "SELECT passwordhash FROM aspnetusers WHERE id = @UserId", connection);

                command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));

                await connection.OpenAsync(cancellationToken);
                var passwordHash = await command.ExecuteScalarAsync(cancellationToken);

                return !string.IsNullOrEmpty(passwordHash?.ToString());
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if user has a password: {ex.Message}");
            // TODO: Log error
            throw;
        }
    }

    #endregion

    #region Role Management

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var roles = new List<string>();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    @"SELECT r.name 
                      FROM aspnetroles r
                      INNER JOIN aspnetuserroles ur ON ur.roleid = r.id
                      WHERE ur.userid = @UserId", connection);

                command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));

                await connection.OpenAsync(cancellationToken);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        roles.Add(reader.GetString(reader.GetOrdinal("name")));
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving roles for user: {ex.Message}");
            // TODO: Log error
            throw;
        }

        return roles;
    }


    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var users = new List<ApplicationUser>();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    @"SELECT u.id, u.username, u.email, u.normalizedusername, u.normalizedemail
                      FROM aspnetusers u
                      INNER JOIN aspnetuserroles ur ON ur.userid = u.id
                      INNER JOIN aspnetroles r ON ur.roleid = r.id
                      WHERE r.normalizedname = @RoleName", connection);

                command.Parameters.AddWithValue("@RoleName", roleName.ToUpper());

                await connection.OpenAsync(cancellationToken);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        users.Add(await MapToApplicationUser(reader, cancellationToken));
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving users in role: {ex.Message}");
            // TODO: Log error
            throw;
        }

        return users;
    }


    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    @"SELECT COUNT(*) 
                      FROM aspnetuserroles ur
                      INNER JOIN aspnetroles r ON ur.roleid = r.id
                      WHERE ur.userid = @UserId AND r.normalizedname = @RoleName", connection);

                command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));
                command.Parameters.AddWithValue("@RoleName", roleName.ToUpper());

                await connection.OpenAsync(cancellationToken);
                var result = await command.ExecuteScalarAsync(cancellationToken);

                var count = result != null ? (long)result : 0;
                return count > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if user is in role: {ex.Message}");
            //TODO: Log error
            return false;
        }
    }


    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand(
                    @"DELETE FROM aspnetuserroles 
                      WHERE userid = @UserId AND roleid = 
                      (SELECT id FROM aspnetroles WHERE normalizedname = @RoleName)", connection);

                command.Parameters.AddWithValue("@UserId", Guid.Parse(user.Id));
                command.Parameters.AddWithValue("@RoleName", roleName.ToUpper());

                await connection.OpenAsync(cancellationToken);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing user from role: {ex.Message}");
            // TODO: Log error
            throw;
        }
    }

    #endregion

    public async Task<IList<ApplicationUser>> SearchUsersAsync(string? keyword, string? role, int startIndex, int range, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var users = new List<ApplicationUser>();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string query = @"
                    SELECT u.*
                    FROM AspNetUsers u
                    LEFT JOIN AspNetUserRoles ur ON u.id = ur.userid
                    LEFT JOIN AspNetRoles r ON ur.roleid = r.id
                    WHERE 1 = 1"
                    + (string.IsNullOrEmpty(keyword) ? "" : " AND u.username ILIKE '%' || @keyword || '%'")
                    + (string.IsNullOrEmpty(role) ? "" : " AND r.normalizedname = @role")
                    + " ORDER BY u.username LIMIT @range OFFSET @startIndex";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        command.Parameters.AddWithValue("@keyword", keyword);
                    }
                    if (!string.IsNullOrEmpty(role))
                    {
                        command.Parameters.AddWithValue("@role", role.ToUpper());
                    }
                    command.Parameters.AddWithValue("@range", range);
                    command.Parameters.AddWithValue("@startIndex", startIndex);

                    await connection.OpenAsync(cancellationToken);
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            users.Add(await MapToApplicationUser(reader, cancellationToken));
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching users: {ex.Message}");
            // TODO: Log error
            throw;
        }

        return users;
    }

    public async Task<int> GetTotalUserCountAsync(string? keyword, string? role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int totalCount = 0;

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string query = @"
                    SELECT COUNT(*)
                    FROM AspNetUsers u
                    LEFT JOIN AspNetUserRoles ur ON u.id = ur.userid
                    LEFT JOIN AspNetRoles r ON ur.roleid = r.id
                    WHERE 1 = 1"
                    + (string.IsNullOrEmpty(keyword) ? "" : " AND u.username ILIKE '%' || @keyword || '%'")
                    + (string.IsNullOrEmpty(role) ? "" : " AND r.normalizedname = @role");

                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        command.Parameters.AddWithValue("@keyword", keyword);
                    }
                    if (!string.IsNullOrEmpty(role))
                    {
                        command.Parameters.AddWithValue("@role", role.ToUpper());
                    }

                    await connection.OpenAsync(cancellationToken);
                    totalCount = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting total user count: {ex.Message}");
            // TODO: Log error
            throw;
        }

        return totalCount;
    }

    public async Task<List<int>> GetDailyUserRegistrationsAsync(int daysBack, CancellationToken cancellationToken)
    {
        var dailyRegistrations = new List<int>();

        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                string query = $@"
                    WITH date_series AS (
                        SELECT 
                            CURRENT_DATE - INTERVAL '1 day' * generate_series(0, @DaysBack - 1) AS day
                    )
                    SELECT 
                        day, 
                        COALESCE(COUNT(up.userid), 0) AS registrations
                    FROM 
                        date_series d
                    LEFT JOIN 
                        userprofiles up ON d.day = up.createdat::date
                    LEFT JOIN 
                        aspnetusers u ON up.userid = u.id
                    GROUP BY 
                        day
                    ORDER BY 
                        day ASC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DaysBack", daysBack);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            dailyRegistrations.Add(reader.GetInt32(1));
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving daily user registrations: {ex.Message}");
            // TODO: Log error
            throw;
        }

        return dailyRegistrations;
    }


}
