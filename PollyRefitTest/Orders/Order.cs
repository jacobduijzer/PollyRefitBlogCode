using System.Collections.Generic;
using PollyRefitTest.Customers;

namespace PollyRefitTest.Orders
{
    public class Order
    {
        public readonly Customer Customer;

        public readonly List<OrderItem> OrderItems;

        public Order(Customer customer)
        {
            OrderItems = new List<OrderItem>();
            Customer = customer;
        }

        public void AddOrderItem(OrderItem orderItem) => OrderItems.Add(orderItem);
    }
}
