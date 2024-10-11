using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IHighlightRepository
    {
        IEnumerable<Product?> GetHighlights();
        bool AddHighlight(int productId, int position);
        void RemoveHighlight(int position);
    }
}
