using Microsoft.AspNetCore.Identity;
using Npgsql;
using System.Data;

namespace PPTWebApp.Data.Repositories
{
    public class ApplicationRoleRepository : IApplicationRoleRepository
    {
        private readonly string _connectionString;

        public ApplicationRoleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region Role Management

        public async Task<IdentityResult> CreateRoleAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(role.Name))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidRoleName",
                    Description = "Role name cannot be empty or whitespace."
                });
            }

            role.NormalizedName = role.Name.ToUpperInvariant();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var command = new NpgsqlCommand("INSERT INTO aspnetroles (name, normalizedname) VALUES (@Name, @NormalizedName)", connection);
                command.Parameters.AddWithValue("@Name", role.Name);
                command.Parameters.AddWithValue("@NormalizedName", role.NormalizedName);

                await connection.OpenAsync(cancellationToken);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            return IdentityResult.Success;
        }


        public async Task<IdentityResult> UpdateRoleAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role cannot be null." });
            }

            if (string.IsNullOrEmpty(role.Name) || string.IsNullOrEmpty(role.NormalizedName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role name and normalized name cannot be null or empty." });
            }

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var command = new NpgsqlCommand(
                        "UPDATE aspnetroles SET name = @RoleName, normalizedname = @NormalizedRoleName WHERE id = @RoleId", connection);
                    command.Parameters.AddWithValue("@RoleName", role.Name);
                    command.Parameters.AddWithValue("@NormalizedRoleName", role.NormalizedName);
                    command.Parameters.AddWithValue("@RoleId", role.Id);

                    await connection.OpenAsync(cancellationToken);
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating role: {ex.Message}");
                //TODO: Log error
                return IdentityResult.Failed(new IdentityError { Description = $"Error updating role: {ex.Message}" });
            }
        }

        public async Task<IdentityResult> DeleteRoleAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var command = new NpgsqlCommand("DELETE FROM aspnetroles WHERE id = @RoleId", connection);
                    command.Parameters.AddWithValue("@RoleId", role.Id);

                    await connection.OpenAsync(cancellationToken);
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting role: {ex.Message}");
                return IdentityResult.Failed(new IdentityError { Description = $"Error deleting role: {ex.Message}" });
            }
        }

        public async Task<IdentityRole?> FindRoleByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var command = new NpgsqlCommand("SELECT id, name, normalizedname FROM aspnetroles WHERE id = @RoleId", connection);
                    command.Parameters.Add("@RoleId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = Guid.Parse(roleId);

                    await connection.OpenAsync(cancellationToken);
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            var id = reader["id"]?.ToString();
                            var name = reader["name"]?.ToString();
                            var normalizedName = reader["normalizedname"]?.ToString();

                            if (id != null)
                            {
                                return new IdentityRole
                                {
                                    Id = id,
                                    Name = name,
                                    NormalizedName = normalizedName
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding role by ID: {ex.Message}");
                //TODO: Log error
            }

            return null;
        }



        public async Task<IdentityRole?> FindRoleByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var command = new NpgsqlCommand("SELECT id, name, normalizedname FROM aspnetroles WHERE normalizedname = @NormalizedRoleName", connection);
                    command.Parameters.AddWithValue("@NormalizedRoleName", normalizedRoleName);

                    await connection.OpenAsync(cancellationToken);
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            var id = reader["id"]?.ToString();
                            var name = reader["name"]?.ToString();
                            var normalizedName = reader["normalizedname"]?.ToString();

                            if (id != null)
                            {
                                return new IdentityRole
                                {
                                    Id = id,
                                    Name = name,
                                    NormalizedName = normalizedName
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding role by normalized name: {ex.Message}");
                //TODO: Log error
            }

            return null;
        }

        #endregion
    }
}
