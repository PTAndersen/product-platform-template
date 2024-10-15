using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PPTWebApp.Data.Services
{
    public class VisitorSessionService
    {
        private readonly IVisitorSessionRepository _visitorSessionRepository;

        public VisitorSessionService(IVisitorSessionRepository visitorSessionRepository)
        {
            _visitorSessionRepository = visitorSessionRepository ?? throw new ArgumentNullException(nameof(visitorSessionRepository));
        }

        public async Task CreateSessionAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            await _visitorSessionRepository.CreateSessionAsync(sessionId, cancellationToken);
        }

        public async Task<VisitorSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            return await _visitorSessionRepository.GetSessionByIdAsync(sessionId, cancellationToken);
        }

        public async Task<List<int>> GetDailyVisitorCountsAsync(int daysBack, CancellationToken cancellationToken)
        {
            return await _visitorSessionRepository.GetDailyVisitorCountsAsync(daysBack, cancellationToken);
        }

        public async Task<bool> IsSessionValidAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            return await _visitorSessionRepository.IsSessionValidAsync(sessionId, cancellationToken);
        }

        public async Task<Guid> HandleSessionAsync(Guid? sessionId, CancellationToken cancellationToken)
        {
            if (sessionId == null || !(await IsSessionValidAsync(sessionId.Value, cancellationToken)))
            {
                var newSessionId = Guid.NewGuid();
                await CreateSessionAsync(newSessionId, cancellationToken);
                return newSessionId;
            }
            return sessionId.Value;
        }
    }
}
