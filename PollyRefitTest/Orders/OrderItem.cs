using PollyRefitTest.Products;

namespace PollyRefitTest.Orders
{
    public class OrderItem
    {
        public readonly int Amount;

        public readonly Product Product;

        public OrderItem(Product product, int amount)
        {
            Amount = amount;
            Product = product;
        }
    }
}
