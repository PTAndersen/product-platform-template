using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories
{
    public interface IDiscountRepository
    {
        Discount? GetDiscountById(int id);
        int AddDiscount(Discount discount);
        bool UpdateDiscount(Discount discount);
        bool DeleteDiscount(int id);
    }
}
