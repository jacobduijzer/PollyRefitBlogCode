using System.Threading.Tasks;
using Refit;

namespace PollyRefitTest.Orders
{
    public interface IOrderApi
    {
        [Post("/order")]
        Task SaveOrder(Order order);
    }
}
