using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IVisitorPageViewRepository
    {
        void LogPageView(VisitorPageView pageView);
        IEnumerable<VisitorPageView> GetPageViewsBySessionId(Guid sessionId);
    }
}
