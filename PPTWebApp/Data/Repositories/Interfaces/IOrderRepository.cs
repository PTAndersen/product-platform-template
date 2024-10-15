using System.Threading;

public interface IOrderRepository
{
    Task<List<decimal>> GetDailySalesAsync(int daysBack, CancellationToken cancellationToken);
}
