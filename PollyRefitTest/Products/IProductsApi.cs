using System.Collections.Generic;
using System.Threading.Tasks;
using PollyRefitTest.Products;
using Refit;

namespace PollyRefitTest
{
    public interface IProductsApi
    {
        [Get("/products")]
        Task<List<Product>> GetProductsAsync(string accessToken);
    }
}
