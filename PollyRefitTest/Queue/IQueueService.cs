using System.Threading.Tasks;
using PollyRefitTest.Orders;

namespace PollyRefitTest.Queue
{
    public interface IQueueService
    {
        Task SaveOrder(Order order);
    }
}
