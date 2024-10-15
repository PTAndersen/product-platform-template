using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PPTWebApp.Data.Services
{
    public class UserActivityService
    {
        private readonly IUserActivityRepository _userActivityRepository;

        public UserActivityService(IUserActivityRepository userActivityRepository)
        {
            _userActivityRepository = userActivityRepository ?? throw new ArgumentNullException(nameof(userActivityRepository));
        }

        public async Task<bool> UpdateUserActivityAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _userActivityRepository.UpdateUserActivityAsync(userId, cancellationToken);
        }

        public async Task<UserActivity?> GetLastActivityAsync(int userId, CancellationToken cancellationToken)
        {
            return await _userActivityRepository.GetLastActivityAsync(userId, cancellationToken);
        }

        public async Task<IEnumerable<UserActivity>> GetActiveUsersWithinAsync(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            return await _userActivityRepository.GetActiveUsersWithinAsync(timeSpan, cancellationToken);
        }
    }
}
