using FluentAssertions;
using PollyRefitTest.Customers;
using Xunit;

namespace PollyRefitTest.UnitTests.Customers
{
    public class CustomerShould
    {
        [Fact]
        public void Construct() =>
             new Customer("Some Customer", "some.customer@somedomain.ext")
                .Should().BeOfType<Customer>();

        [Fact]
        public void SetName() =>
            new Customer("Some Customer", "some.customer@somedomain.ext")
                .FullName.Should().Be("Some Customer");

        [Fact]
        public void SetEmail() =>
            new Customer("Some Customer", "some.customer@somedomain.ext")
                .Email.Should().Be("some.customer@somedomain.ext");

        [Fact]
        public void GetGuid() =>
            new Customer("Some Customer", "some.customer@somedomain.ext")
                .Id.Should().NotBeEmpty();
    }
}
