using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PPTWebApp.Data.Services
{
    public class DiscountService
    {
        private readonly IDiscountRepository _discountRepository;

        public DiscountService(IDiscountRepository discountRepository)
        {
            _discountRepository = discountRepository ?? throw new ArgumentNullException(nameof(discountRepository));
        }

        public async Task<int> AddDiscountAsync(Discount discount, CancellationToken cancellationToken)
        {
            if (discount == null)
            {
                throw new ArgumentNullException(nameof(discount), "Discount cannot be null.");
            }

            return await _discountRepository.AddDiscountAsync(discount, cancellationToken);
        }

        public async Task<Discount?> GetDiscountByIdAsync(int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid discount ID.", nameof(id));
            }

            return await _discountRepository.GetDiscountByIdAsync(id, cancellationToken);
        }

        public async Task<bool> UpdateDiscountAsync(Discount discount, CancellationToken cancellationToken)
        {
            if (discount == null)
            {
                throw new ArgumentNullException(nameof(discount), "Discount cannot be null.");
            }

            return await _discountRepository.UpdateDiscountAsync(discount, cancellationToken);
        }

        public async Task<bool> DeleteDiscountAsync(int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid discount ID.", nameof(id));
            }

            return await _discountRepository.DeleteDiscountAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<Discount>> GetAllDiscountsInRangeAsync(string? keyword, int startIndex, int range, CancellationToken cancellationToken)
        {
            return await _discountRepository.GetAllDiscountsInRangeAsync(keyword, startIndex, range, cancellationToken);
        }

        public async Task<IEnumerable<Discount>> GetAllDiscountsInRangeAsync(string? keyword, bool isActive, int startIndex, int range, CancellationToken cancellationToken)
        {
            return await _discountRepository.GetAllDiscountsInRangeAsync(keyword, isActive, startIndex, range, cancellationToken);
        }

        public async Task<int> GetDiscountCountAsync(string? keyword, CancellationToken cancellationToken)
        {
            return await _discountRepository.GetDiscountCountAsync(keyword, cancellationToken);
        }

        public async Task<int> GetDiscountCountAsync(string? keyword, bool isActive, CancellationToken cancellationToken)
        {
            return await _discountRepository.GetDiscountCountAsync(keyword, isActive, cancellationToken);
        }
    }
}
