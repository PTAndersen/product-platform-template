using System.Threading;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public Task<List<decimal>> GetDailySalesAsync(int daysBack, CancellationToken cancellationToken)
    {
        return _orderRepository.GetDailySalesAsync(daysBack, cancellationToken);
    }
}
