using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Services
{
    public class VisitorPageViewService
    {
        private readonly IVisitorPageViewRepository _visitorPageViewRepository;

        public VisitorPageViewService(IVisitorPageViewRepository visitorPageViewRepository)
        {
            _visitorPageViewRepository = visitorPageViewRepository ?? throw new ArgumentNullException(nameof(visitorPageViewRepository));
        }

        public async Task LogPageViewAsync(Guid sessionId, string pageUrl, string? referrer, CancellationToken cancellationToken)
        {
            var pageView = new VisitorPageView
            {
                SessionId = sessionId,
                PageUrl = pageUrl,
                ViewedAt = DateTime.UtcNow,
                Referrer = referrer
            };

            await _visitorPageViewRepository.LogPageViewAsync(pageView, cancellationToken);
        }

        public async Task<IEnumerable<VisitorPageView>> GetPageViewsBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            return await _visitorPageViewRepository.GetPageViewsBySessionIdAsync(sessionId, cancellationToken);
        }
    }
}
