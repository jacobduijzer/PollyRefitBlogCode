using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PollyRefitTest.Authentication;
using Refit;

namespace PollyRefitTest.Products
{
    public class ProductService
        : IProductService
    {
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
            if(_authenticationResult == null)
                _authenticationResult = await _authenticationService.GetAccessToken().ConfigureAwait(false);

            int tryCount = 0;
            while(tryCount < 3)
            {
                try
                {
                    tryCount++;
                    return await GetProductsAsync(_authenticationResult.access_token).ConfigureAwait(false);
                }
                catch (ApiException ex)
                { 
                    switch(ex.StatusCode)
                    {
                        case System.Net.HttpStatusCode.RequestTimeout:
                            await Task.Delay(500);
                            break;
                        case System.Net.HttpStatusCode.Unauthorized:
                            _authenticationResult = await _authenticationService.GetAccessToken().ConfigureAwait(false);
                            break;
                    }
                    
                }
                catch(Exception ex)
                {

                }
            }

            throw new Exception("Something went wrong");
        }

        private async Task<List<Product>> GetProductsAsync(string accessToken) =>
            await _productsApi
            .GetProductsAsync(accessToken)
            .ConfigureAwait(false);
    }
}
