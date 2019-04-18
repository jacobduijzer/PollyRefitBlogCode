using System.Threading.Tasks;

namespace PollyRefitTest.Orders
{
    public interface IOrderService
    {
        Task SaveOrder(Order order);
    }
}
