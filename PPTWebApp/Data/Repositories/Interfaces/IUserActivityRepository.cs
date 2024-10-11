using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IUserActivityRepository
    {
        void UpdateUserActivity(Guid userId);
        UserActivity? GetLastActivity(int userId);
        IEnumerable<UserActivity> GetActiveUsersWithin(TimeSpan timeSpan);
    }
}
