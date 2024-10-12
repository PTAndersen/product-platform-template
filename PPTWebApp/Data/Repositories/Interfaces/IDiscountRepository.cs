using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IDiscountRepository
    {
        IEnumerable<Discount> GetAllDiscountsInRange(string? keyword, int startIndex, int range);
        IEnumerable<Discount> GetAllDiscountsInRange(string? keyword, bool isActive, int startIndex, int range);
        int GetDiscountCount(string? keyword);
        int GetDiscountCount(string? keyword, bool isActive);
        Discount? GetDiscountById(int id);
        int AddDiscount(Discount discount);
        bool UpdateDiscount(Discount discount);
        bool DeleteDiscount(int id);
    }
}
