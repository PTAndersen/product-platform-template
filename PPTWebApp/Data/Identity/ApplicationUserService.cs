using Microsoft.AspNetCore.Identity;
using PPTWebApp.Data;
using System.Threading;
using System.Threading.Tasks;

public class ApplicationUserService : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserRoleStore<ApplicationUser>, IUserEmailStore<ApplicationUser>
{
    private readonly IApplicationUserRepository _applicationUserRepository;

    public ApplicationUserService(IApplicationUserRepository applicationUserRepository)
    {
        _applicationUserRepository = applicationUserRepository;
    }

    #region IUserStore Implementation

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            await _applicationUserRepository.AddUserAsync(user, cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserCreationFailed",
                Description = $"An error occurred while creating the user: {ex.Message}"
            });
        }
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            await _applicationUserRepository.UpdateUserAsync(user, cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserUpdateFailed",
                Description = $"An error occurred while updating the user: {ex.Message}"
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            await _applicationUserRepository.DeleteUserAsync(user.Id, cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserDeletionFailed",
                Description = $"An error occurred while deleting the user: {ex.Message}"
            });
        }
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _applicationUserRepository.GetUserByIdAsync(userId, cancellationToken);
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return await _applicationUserRepository.GetUserByNameAsync(normalizedUserName, cancellationToken);
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

    public async Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return await _applicationUserRepository.GetPasswordHashAsync(user, cancellationToken);
    }

    public async Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
    {
        await _applicationUserRepository.SetPasswordHashAsync(user, passwordHash, cancellationToken);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return _applicationUserRepository.HasPasswordAsync(user, cancellationToken);
    }

    #endregion

    #region IUserRoleStore Implementation

    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        await _applicationUserRepository.AddToRoleAsync(user, roleName, cancellationToken);
    }

    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        await _applicationUserRepository.RemoveFromRoleAsync(user, roleName, cancellationToken);
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return await _applicationUserRepository.GetRolesAsync(user, cancellationToken);
    }

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        return await _applicationUserRepository.GetUsersInRoleAsync(roleName, cancellationToken);
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        return await _applicationUserRepository.IsInRoleAsync(user, roleName, cancellationToken);
    }

    #endregion

    #region IUserEmailStore Implementation

    public async Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        await _applicationUserRepository.SetEmailAsync(user, email, cancellationToken);
    }

    public async Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return await _applicationUserRepository.GetEmailAsync(user, cancellationToken);
    }

    public async Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return await _applicationUserRepository.GetEmailConfirmedAsync(user, cancellationToken);
    }

    public async Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        await _applicationUserRepository.SetEmailConfirmedAsync(user, confirmed, cancellationToken);
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return await _applicationUserRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }



    #endregion

    public async Task<IList<ApplicationUser>> SearchUsersAsync(string? keyword, string? role, int startIndex, int range, CancellationToken cancellationToken)
    {
        try
        {
            return await _applicationUserRepository.SearchUsersAsync(keyword, role, startIndex, range, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SearchUsersAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<int> GetTotalUserCountAsync(string? keyword, string? role, CancellationToken cancellationToken)
    {
        try
        {
            return await _applicationUserRepository.GetTotalUserCountAsync(keyword, role, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTotalUserCountAsync: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        // Clean up resources if necessary
    }
}
