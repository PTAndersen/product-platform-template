using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using System.Collections.Generic;

namespace PPTWebApp.Data.Services
{
    public class VisitorPageViewService
    {
        private readonly IVisitorPageViewRepository _visitorPageViewRepository;

        public VisitorPageViewService(IVisitorPageViewRepository visitorPageViewRepository)
        {
            _visitorPageViewRepository = visitorPageViewRepository ?? throw new ArgumentNullException(nameof(visitorPageViewRepository));
        }

        public void LogPageView(Guid sessionId, string pageUrl, string? referrer)
        {
            var pageView = new VisitorPageView
            {
                SessionId = sessionId,
                PageUrl = pageUrl,
                ViewedAt = DateTime.UtcNow,
                Referrer = referrer
            };

            _visitorPageViewRepository.LogPageView(pageView);
        }

        public IEnumerable<VisitorPageView> GetPageViewsBySessionId(Guid sessionId)
        {
            return _visitorPageViewRepository.GetPageViewsBySessionId(sessionId);
        }
    }
}
