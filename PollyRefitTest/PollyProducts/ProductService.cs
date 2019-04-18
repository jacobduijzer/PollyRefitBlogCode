using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Polly;
using PollyRefitTest.Authentication;
using PollyRefitTest.Products;
using Refit;

namespace PollyRefitTest.PollyProducts
{
    public class ProductService : IProductService
    {
        private static readonly int NUMBER_OF_RETRIES = 3;

        private readonly IProductsApi _productsApi;
        private readonly IAuthenticationService _authenticationService;

        private AuthenticationResult _authenticationResult;

        public ProductService(IProductsApi productsApi, IAuthenticationService authenticationService)
        {
            _productsApi = productsApi;
            _authenticationService = authenticationService;
        }

        public async Task<List<Product>> GetProducts()
        {
            if (_authenticationResult == null)
                _authenticationResult = await _authenticationService.GetAccessToken().ConfigureAwait(false);

            var unauthorizedPolicy = Policy
                .Handle<ApiException>(ex => ex.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(async (exception, retryCount) =>
                {
                    _authenticationResult = await _authenticationService.GetAccessToken().ConfigureAwait(false);
                });

            var timeoutPolicy = Policy
                .Handle<ApiException>(ex => ex.StatusCode == HttpStatusCode.RequestTimeout)
                .RetryAsync(NUMBER_OF_RETRIES, async (exception, retryCount) => await Task.Delay(300).ConfigureAwait(false));

            return await unauthorizedPolicy
                .WrapAsync(timeoutPolicy)
                .ExecuteAsync(async () => await _productsApi.GetProductsAsync(_authenticationResult.access_token))
                .ConfigureAwait(false);
        }
    }
}
