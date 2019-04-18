using System.Linq;
using FluentAssertions;
using PollyRefitTest.Customers;
using PollyRefitTest.Orders;
using PollyRefitTest.Products;
using Xunit;

namespace PollyRefitTest.UnitTests.Orders
{
    public class OrderShould
    {
        [Fact]
        public void Construct() =>
            new Order(new Customer("Buying Customer", "buyingcustomer@somedomain.ext"))
                .Should().BeOfType<Order>();

        [Fact]
        public void AddCustomer()
        {
            var order = new Order(new Customer("Buying Customer", "buyingcustomer@somedomain.ext"));
            order.Customer.Should().NotBeNull();
            order.Customer.FullName.Should().Be("Buying Customer");
            order.Customer.Email.Should().Be("buyingcustomer@somedomain.ext");
        }

        [Fact]
        public void InitializeOrderItemCollection() =>
            new Order(new Customer("Buying Customer", "buyingcustomer@somedomain.ext"))
                .OrderItems.Should().NotBeNull().And.BeEmpty();

        [Fact]
        public void AddOrderItems()
        {
            var order = new Order(new Customer("Buying Customer", "buyingcustomer@somedomain.ext"));
            order.OrderItems.Should().BeEmpty();

            order.AddOrderItem(new OrderItem(TestHelper.CreateFakeProducts(1).FirstOrDefault(), 2));

            order.OrderItems.Should().NotBeNullOrEmpty().And.HaveCount(1);
        }
    }
}
