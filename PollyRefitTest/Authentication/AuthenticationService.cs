using System.Net;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Polly;
using Refit;

namespace PollyRefitTest.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private static readonly int NUMBER_OF_RETRIES = 3;

        private readonly IAuthenticationApi _authenticationApi;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public AuthenticationService(
            IAuthenticationApi authenticationApi,
            string clientId,
            string clientSecret
            )
        {
            Guard.Against.Null(authenticationApi, nameof(authenticationApi));
            Guard.Against.NullOrEmpty(clientId, nameof(clientId));
            Guard.Against.NullOrEmpty(clientSecret, nameof(clientSecret));

            _authenticationApi = authenticationApi;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task<AuthenticationResult> GetAccessToken() =>
            await Policy
                .Handle<ApiException>(ex => ex.StatusCode == HttpStatusCode.RequestTimeout)
                .RetryAsync(NUMBER_OF_RETRIES, async (exception, retryCount) => await Task.Delay(500))
                .ExecuteAsync(async () => await _authenticationApi.GetAccessToken(_clientId, _clientSecret).ConfigureAwait(false))
                .ConfigureAwait(false);
    }
}
