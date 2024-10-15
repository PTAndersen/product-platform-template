using PPTWebApp.Data.Models;

public class BasketService
{
    private List<BasketItem> basketItems = new();

    public event Action? OnChange;

    public List<BasketItem> GetBasketItems()
    {
        return basketItems;
    }

    public void AddToBasket(Product product, int quantity = 1)
    {
        var basketItem = basketItems.FirstOrDefault(b => b.Product.Id == product.Id);
        if (basketItem != null)
        {
            basketItem.Quantity += quantity;
        }
        else
        {
            basketItems.Add(new BasketItem { Product = product, Quantity = quantity });
        }

        NotifyStateChanged();
    }

    public void RemoveFromBasket(BasketItem item)
    {
        basketItems.Remove(item);
        NotifyStateChanged();
    }

    public decimal GetTotalPrice()
    {
        return basketItems.Sum(item => item.Product.Price * item.Quantity);
    }

    public int GetTotalItems()
    {
        return basketItems.Sum(item => item.Quantity);
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
