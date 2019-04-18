using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PollyRefitTest.Orders;
using PollyRefitTest.Queue;
using Xunit;
using Xunit.Abstractions;

namespace PollyRefitTest.UnitTests.Orders
{
    public class OrderServiceShould
    {
        private readonly Mock<IOrderApi> _mockOrderApi;
        private readonly Mock<IQueueService> _mockQueueService;
        private readonly TestLogger _testLogger;

        public OrderServiceShould(ITestOutputHelper outputHelper)
        {
            _testLogger = new TestLogger(outputHelper);
            _mockOrderApi = new Mock<IOrderApi>(MockBehavior.Strict);
            _mockOrderApi
                .Setup(x => x.SaveOrder(It.IsAny<Order>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockQueueService = new Mock<IQueueService>(MockBehavior.Strict);
            _mockQueueService
                .Setup(x => x.SaveOrder(It.IsAny<Order>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        [Fact]
        public void Construct() =>
            new OrderService(_mockOrderApi.Object, _mockQueueService.Object, _testLogger)
                .Should().BeOfType<OrderService>();

        [Fact]
        public async Task SaveOrder()
        {
            var orderService = new OrderService(_mockOrderApi.Object, _mockQueueService.Object, _testLogger);        
            await orderService.SaveOrder(TestHelper.CreateFakeOrder(5)).ConfigureAwait(false);

            _mockOrderApi.Verify(x => x.SaveOrder(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task RetryOnTimeout()
        {
            var mockOrderApi = new Mock<IOrderApi>(MockBehavior.Strict);
            mockOrderApi
                .SetupSequence(x => x.SaveOrder(It.IsAny<Order>()))
                .Throws(TestHelper.CreateRefitException(HttpMethod.Post, HttpStatusCode.RequestTimeout))
                .Throws(TestHelper.CreateRefitException(HttpMethod.Post, HttpStatusCode.RequestTimeout))
                .Returns(Task.CompletedTask);
                
            var orderService = new OrderService(mockOrderApi.Object, _mockQueueService.Object, _testLogger);

            await orderService.SaveOrder(TestHelper.CreateFakeOrder(3)).ConfigureAwait(false);

            mockOrderApi.Verify(x => x.SaveOrder(It.IsAny<Order>()), Times.Exactly(3));
        }

        [Fact]
        public async Task HandleFallbackOnMultipleTimeouts()
        {
            var mockOrderApi = new Mock<IOrderApi>(MockBehavior.Strict);
            mockOrderApi
                .SetupSequence(x => x.SaveOrder(It.IsAny<Order>()))
                .Throws(TestHelper.CreateRefitException(HttpMethod.Post, HttpStatusCode.RequestTimeout))
                .Throws(TestHelper.CreateRefitException(HttpMethod.Post, HttpStatusCode.RequestTimeout))
                .Throws(TestHelper.CreateRefitException(HttpMethod.Post, HttpStatusCode.RequestTimeout));

            var orderService = new OrderService(mockOrderApi.Object, _mockQueueService.Object, _testLogger);

            await orderService.SaveOrder(TestHelper.CreateFakeOrder(3)).ConfigureAwait(false);

            mockOrderApi.Verify(x => x.SaveOrder(It.IsAny<Order>()), Times.Exactly(4));
            _mockQueueService.Verify(x => x.SaveOrder(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task BreakAfter10Tries()
        {
            var mockOrderApi = new Mock<IOrderApi>(MockBehavior.Strict);
            mockOrderApi.Setup(x => x.SaveOrder(It.IsAny<Order>()))
                .Throws(TestHelper.CreateRefitException(HttpMethod.Post, HttpStatusCode.RequestTimeout));

            var orderService = new OrderService(mockOrderApi.Object, _mockQueueService.Object, _testLogger);

            for (int i = 0; i <= 10; i++)
                await orderService.SaveOrder(TestHelper.CreateFakeOrder(3)).ConfigureAwait(false);

            mockOrderApi.Verify(x => x.SaveOrder(It.IsAny<Order>()), Times.AtLeast(10));
            _mockQueueService.Verify(x => x.SaveOrder(It.IsAny<Order>()), Times.AtLeast(10));
        }
    }
}
