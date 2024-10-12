using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Services
{
    public class DiscountService
    {
        private readonly IDiscountRepository _discountRepository;

        public DiscountService(IDiscountRepository discountRepository)
        {
            _discountRepository = discountRepository ?? throw new ArgumentNullException(nameof(discountRepository));
        }

        public int AddDiscount(Discount discount)
        {
            if (discount == null)
            {
                throw new ArgumentNullException(nameof(discount), "Discount cannot be null.");
            }

            return _discountRepository.AddDiscount(discount);
        }

        public Discount? GetDiscountById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid discount ID.", nameof(id));
            }

            return _discountRepository.GetDiscountById(id);
        }

        public bool UpdateDiscount(Discount discount)
        {
            if (discount == null)
            {
                throw new ArgumentNullException(nameof(discount), "Discount cannot be null.");
            }

            return _discountRepository.UpdateDiscount(discount);
        }

        public bool DeleteDiscount(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid discount ID.", nameof(id));
            }

            return _discountRepository.DeleteDiscount(id);
        }

        public IEnumerable<Discount> GetAllDiscountsInRange(string? keyword, int startIndex, int range)
        {
            return _discountRepository.GetAllDiscountsInRange(keyword, startIndex, range);
        }

        public IEnumerable<Discount> GetAllDiscountsInRange(string? keyword, bool isActive, int startIndex, int range)
        {
            return _discountRepository.GetAllDiscountsInRange(keyword, isActive, startIndex, range);
        }

        public int GetDiscountCount(string? keyword)
        {
            return _discountRepository.GetDiscountCount(keyword);
        }

        public int GetDiscountCount(string? keyword, bool isActive)
        {
            return _discountRepository.GetDiscountCount(keyword, isActive);
        }

    }
}
