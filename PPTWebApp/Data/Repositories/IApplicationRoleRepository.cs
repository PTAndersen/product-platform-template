using Microsoft.AspNetCore.Identity;

public interface IApplicationRoleRepository
{
    // Role Management Methods
    Task<IdentityResult> CreateRoleAsync(IdentityRole role, CancellationToken cancellationToken);
    Task<IdentityResult> UpdateRoleAsync(IdentityRole role, CancellationToken cancellationToken);
    Task<IdentityResult> DeleteRoleAsync(IdentityRole role, CancellationToken cancellationToken);
    Task<IdentityRole> FindRoleByIdAsync(string roleId, CancellationToken cancellationToken);
    Task<IdentityRole> FindRoleByNameAsync(string normalizedRoleName, CancellationToken cancellationToken);
}
