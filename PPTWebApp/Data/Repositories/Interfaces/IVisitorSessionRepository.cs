using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IVisitorSessionRepository
    {
        void CreateSession(Guid sessionId);
        VisitorSession? GetSessionById(Guid sessionId);
        bool IsSessionValid(Guid sessionId);
    }
}
