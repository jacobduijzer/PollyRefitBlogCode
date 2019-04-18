using System.Linq;
using FluentAssertions;
using PollyRefitTest.Orders;
using Xunit;

namespace PollyRefitTest.UnitTests.Orders
{
    public class OrderItemShould
    {
        [Fact]
        public void Construct() =>
            new OrderItem(TestHelper.CreateFakeProducts(1).FirstOrDefault(), 1)
                .Should().BeOfType<OrderItem>();

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void SetCorrectAmount(int amount) =>
            new OrderItem(TestHelper.CreateFakeProducts(1).FirstOrDefault(), amount)
                .Amount.Should().Be(amount);
    }
}
