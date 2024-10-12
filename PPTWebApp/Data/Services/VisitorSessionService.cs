using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Services
{
    public class VisitorSessionService
    {
        private readonly IVisitorSessionRepository _visitorSessionRepository;

        public VisitorSessionService(IVisitorSessionRepository visitorSessionRepository)
        {
            _visitorSessionRepository = visitorSessionRepository ?? throw new ArgumentNullException(nameof(visitorSessionRepository));
        }

        public void CreateSession(Guid sessionId)
        {
            _visitorSessionRepository.CreateSession(sessionId);
        }

        public VisitorSession? GetSessionById(Guid sessionId)
        {
            return _visitorSessionRepository.GetSessionById(sessionId);
        }

        public Task<List<int>> GetDailyVisitorCountsAsync(int daysBack)
        {
            return _visitorSessionRepository.GetDailyVisitorCountsAsync(daysBack);
        }

        public bool IsSessionValid(Guid sessionId)
        {
            return _visitorSessionRepository.IsSessionValid(sessionId);
        }

        public Guid HandleSession(Guid? sessionId)
        {
            if (sessionId == null || !IsSessionValid(sessionId.Value))
            {
                var newSessionId = Guid.NewGuid();
                CreateSession(newSessionId);
                return newSessionId;
            }
            return sessionId.Value;
        }

    }
}
