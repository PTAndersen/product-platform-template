public interface IOrderRepository
{
    Task<List<decimal>> GetDailySalesAsync(int daysBack);
}
