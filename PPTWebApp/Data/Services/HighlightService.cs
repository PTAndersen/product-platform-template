using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Services
{
    public class HighlightService
    {
        private readonly IHighlightRepository _highlightRepository;

        public HighlightService(IHighlightRepository highlightRepository)
        {
            _highlightRepository = highlightRepository ?? throw new ArgumentNullException(nameof(highlightRepository));
        }

        public bool AddHighlight(int productId, int position)
        {
            return _highlightRepository.AddHighlight(productId, position);
        }

        public IEnumerable<Product?> GetHighlights()
        {
            return _highlightRepository.GetHighlights();
        }

        public void RemoveHighlight(int position)
        {
            _highlightRepository.RemoveHighlight(position);
        }
    }
}
