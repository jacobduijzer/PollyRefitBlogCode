using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PollyRefitTest.Authentication;
using PollyRefitTest.Products;
using Refit;
using Xunit;

namespace PollyRefitTest.UnitTests.Products
{
    public class ProductServiceShould
    {
        private const string ACCESS_TOKEN = "access_token";

        private readonly Mock<IProductsApi> _mockProductsApi;
        private readonly Mock<IAuthenticationService> _mockAuthenticationService;

        public ProductServiceShould()
        {
            _mockProductsApi = new Mock<IProductsApi>(MockBehavior.Strict);
            _mockAuthenticationService = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _mockAuthenticationService
                .Setup(x => x.GetAccessToken())
                .ReturnsAsync(new AuthenticationResult { access_token = ACCESS_TOKEN })
                .Verifiable();
        }

        [Fact]
        public void Construct() =>
            new ProductService(_mockProductsApi.Object, _mockAuthenticationService.Object).Should().BeOfType<ProductService>();

        [Fact]
        public void RetryToRetrieveProductsAndThrow()
        {
            var mockApi = new Mock<IProductsApi>(MockBehavior.Strict);
            mockApi.Setup(x => x.GetProductsAsync(It.IsAny<string>()))
                .ThrowsAsync(TestHelper.CreateRefitException(HttpMethod.Get, HttpStatusCode.RequestTimeout))
                .Verifiable();

            var service = new ProductService(mockApi.Object, _mockAuthenticationService.Object);
            new Func<Task>(async () => await service.GetProducts()).Should().Throw<Exception>();

            mockApi.Verify(x => x.GetProductsAsync(It.IsAny<string>()), Times.Exactly(3));
        }
    }
}
