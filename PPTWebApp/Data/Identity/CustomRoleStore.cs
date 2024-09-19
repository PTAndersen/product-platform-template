using Microsoft.AspNetCore.Identity;
using Npgsql;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public class CustomRoleStore : IRoleStore<IdentityRole>
{
    private readonly string _connectionString;

    public CustomRoleStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand("INSERT INTO AspNetRoles (Name, NormalizedName) VALUES (@roleName, @normalizedRoleName)", connection);
            command.Parameters.AddWithValue("@roleName", role.Name);
            command.Parameters.AddWithValue("@normalizedRoleName", role.NormalizedName);

            connection.Open();
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand("UPDATE AspNetRoles SET Name = @roleName, NormalizedName = @normalizedRoleName WHERE Id = @roleId", connection);
            command.Parameters.AddWithValue("@roleName", role.Name);
            command.Parameters.AddWithValue("@normalizedRoleName", role.NormalizedName);
            command.Parameters.AddWithValue("@roleId", role.Id);

            connection.Open();
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand("DELETE FROM AspNetRoles WHERE Id = @roleId", connection);
            command.Parameters.AddWithValue("@roleId", role.Id);

            connection.Open();
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IdentityRole role = null;

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand("SELECT Id, Name, NormalizedName FROM AspNetRoles WHERE Id = @roleId", connection);
            command.Parameters.AddWithValue("@roleId", roleId);

            connection.Open();
            using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    role = new IdentityRole
                    {
                        Id = reader["Id"].ToString(),
                        Name = reader["Name"].ToString(),
                        NormalizedName = reader["NormalizedName"].ToString()
                    };
                }
            }
        }

        return role;
    }

    public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IdentityRole role = null;

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var command = new NpgsqlCommand("SELECT Id, Name, NormalizedName FROM AspNetRoles WHERE NormalizedName = @normalizedRoleName", connection);
            command.Parameters.AddWithValue("@normalizedRoleName", normalizedRoleName);

            connection.Open();
            using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    role = new IdentityRole
                    {
                        Id = reader["Id"].ToString(),
                        Name = reader["Name"].ToString(),
                        NormalizedName = reader["NormalizedName"].ToString()
                    };
                }
            }
        }

        return role;
    }

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id);
    }

    public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Clean up resources if necessary
    }
}
