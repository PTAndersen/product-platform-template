using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Services
{
    public class UserActivityService
    {
        private readonly IUserActivityRepository _userActivityRepository;

        public UserActivityService(IUserActivityRepository userActivityRepository)
        {
            _userActivityRepository = userActivityRepository ?? throw new ArgumentNullException(nameof(userActivityRepository));
        }

        public void UpdateUserActivity(Guid userId)
        {
            _userActivityRepository.UpdateUserActivity(userId);
        }

        public UserActivity? GetLastActivity(int userId)
        {
            return _userActivityRepository.GetLastActivity(userId);
        }

        public IEnumerable<UserActivity> GetActiveUsersWithin(TimeSpan timeSpan)
        {
            return _userActivityRepository.GetActiveUsersWithin(timeSpan);
        }
    }
}
