using PPTWebApp.Data.Models;

public interface IApplicationUserRepository
{
    // User Management
    Task AddUserAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task UpdateUserAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetUserByNameAsync(string normalizedUserName, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    // Password Management
    Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken);
    Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken);

    // Role Management
    Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken);
    Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken);
    Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken);
    Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken);

    // Email Management
    Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken);
    Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken);
    Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken);

    // User Search with Pagination
    Task<IList<ApplicationUser>> SearchUsersAsync(string? keyword, string? role, int startIndex, int range, CancellationToken cancellationToken);

    Task<int> GetTotalUserCountAsync(string? keyword, string? role, CancellationToken cancellationToken);

    // Other
    Task<List<int>> GetDailyUserRegistrationsAsync(int daysBack);
}
