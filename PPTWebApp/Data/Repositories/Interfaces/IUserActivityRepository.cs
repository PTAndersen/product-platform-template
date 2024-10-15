using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IUserActivityRepository
    {
        Task<bool> UpdateUserActivityAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserActivity?> GetLastActivityAsync(int userId, CancellationToken cancellationToken);
        Task<IEnumerable<UserActivity>> GetActiveUsersWithinAsync(TimeSpan timeSpan, CancellationToken cancellationToken);
    }
}
