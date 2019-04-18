using System.Collections.Generic;
using System.Threading.Tasks;

namespace PollyRefitTest.Products
{
    public interface IProductService
    {
        Task<List<Product>> GetProducts();
    }
}
