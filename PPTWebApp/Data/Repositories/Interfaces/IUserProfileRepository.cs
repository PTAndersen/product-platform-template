using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetUserProfileByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<UserProfile>> GetUserProfilesInRangeAsync(int startIndex, int range, CancellationToken cancellationToken);
        Task<int> GetTotalUserProfileCountAsync(CancellationToken cancellationToken);
        Task AddUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken);
        Task UpdateUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken);
        Task DeleteUserProfileAsync(Guid userId, CancellationToken cancellationToken);
    }
}
