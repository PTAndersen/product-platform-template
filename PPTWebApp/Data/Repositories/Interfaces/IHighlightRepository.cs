using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IHighlightRepository
    {
        Task<IEnumerable<Product?>> GetHighlightsAsync(CancellationToken cancellationToken);
        Task<bool> AddHighlightAsync(int productId, int position, CancellationToken cancellationToken);
        Task RemoveHighlightAsync(int position, CancellationToken cancellationToken);
    }
}
