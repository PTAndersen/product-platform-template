using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IDiscountRepository
    {
        Task <IEnumerable<Discount>> GetAllDiscountsInRangeAsync(string? keyword, int startIndex, int range, CancellationToken cancellationToken);
        Task<IEnumerable<Discount>> GetAllDiscountsInRangeAsync(string? keyword, bool isActive, int startIndex, int range, CancellationToken cancellationToken);
        Task<int> GetDiscountCountAsync(string? keyword, CancellationToken cancellationToken);
        Task<int> GetDiscountCountAsync(string? keyword, bool isActive, CancellationToken cancellationToken);
        Task<Discount?> GetDiscountByIdAsync(int id, CancellationToken cancellationToken);
        Task<int> AddDiscountAsync(Discount discount, CancellationToken cancellationToken);
        Task<bool> UpdateDiscountAsync(Discount discount, CancellationToken cancellationToken);
        Task<bool> DeleteDiscountAsync(int id, CancellationToken cancellationToken);
    }
}
