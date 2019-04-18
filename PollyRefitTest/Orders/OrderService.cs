using System;
using System.Net;
using System.Threading.Tasks;
using Polly;
using PollyRefitTest.Logging;
using PollyRefitTest.Queue;
using Refit;

namespace PollyRefitTest.Orders
{
    public class OrderService : IOrderService
    {
        private static readonly int NUMBER_OF_RETRIES = 3;
        private const int EXCEPTIONS_ALLOWED_BEFORE_BREAKING_CIRCUIT = 10;

        private readonly IOrderApi _orderApi;
        private readonly IQueueService _queueService;
        private readonly ILogger _logger;
        private readonly IAsyncPolicy _circuitBreaker;

        public OrderService(
            IOrderApi orderApi,
            IQueueService queueService,
            ILogger logger
            )
        {
            _orderApi = orderApi;
            _queueService = queueService;

            _logger = logger;

            _circuitBreaker = Policy.Handle<Exception>()
                .CircuitBreakerAsync(
                    EXCEPTIONS_ALLOWED_BEFORE_BREAKING_CIRCUIT,
                    TimeSpan.FromMilliseconds(5000),
                    (exception, things) =>
                    {
                        logger.Debug("Breaking circuit");
                    }, 
                    () =>
                    {
                    });                    
        }

        public async Task SaveOrder(Order order)
        {
            var retryPolicy = Policy
                .Handle<ApiException>(ex => ex.StatusCode == HttpStatusCode.RequestTimeout)
                .RetryAsync(NUMBER_OF_RETRIES, async (exception, retryCount) =>
                {
                    _logger.Debug("Retrying, waiting first");
                    await Task.Delay(500).ConfigureAwait(false);
                });

            var fallbackPolicy = Policy
                .Handle<Exception>()
                .FallbackAsync(async (cancellationToken) =>
                {
                    _logger.Debug("Handling fallback");
                    await SaveOrderInQueue(order).ConfigureAwait(false);
                });

            await fallbackPolicy
                .WrapAsync(retryPolicy)
                .WrapAsync(_circuitBreaker)
                .ExecuteAsync(async () => await _orderApi.SaveOrder(order).ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        private async Task SaveOrderInQueue(Order order) =>
            await _queueService.SaveOrder(order)
                .ConfigureAwait(false);
    }
}
