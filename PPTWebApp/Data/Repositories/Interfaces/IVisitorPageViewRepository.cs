using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IVisitorPageViewRepository
    {
        Task LogPageViewAsync(VisitorPageView pageView, CancellationToken cancellationToken);
        Task<IEnumerable<VisitorPageView>> GetPageViewsBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken);
    }
}
