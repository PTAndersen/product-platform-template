using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IVisitorSessionRepository
    {
        Task CreateSessionAsync(Guid sessionId, CancellationToken cancellationToken);
        Task<VisitorSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken);
        Task<bool> IsSessionValidAsync(Guid sessionId, CancellationToken cancellationToken);
        Task<List<int>> GetDailyVisitorCountsAsync(int daysBack, CancellationToken cancellationToken);
    }
}
