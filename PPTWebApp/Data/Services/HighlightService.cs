using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PPTWebApp.Data.Services
{
    public class HighlightService
    {
        private readonly IHighlightRepository _highlightRepository;

        public HighlightService(IHighlightRepository highlightRepository)
        {
            _highlightRepository = highlightRepository ?? throw new ArgumentNullException(nameof(highlightRepository));
        }

        public async Task<bool> AddHighlightAsync(int productId, int position, CancellationToken cancellationToken)
        {
            return await _highlightRepository.AddHighlightAsync(productId, position, cancellationToken);
        }

        public async Task<IEnumerable<Product?>> GetHighlightsAsync(CancellationToken cancellationToken)
        {
            return await _highlightRepository.GetHighlightsAsync(cancellationToken);
        }

        public async Task RemoveHighlightAsync(int position, CancellationToken cancellationToken)
        {
            await _highlightRepository.RemoveHighlightAsync(position, cancellationToken);
        }
    }
}
