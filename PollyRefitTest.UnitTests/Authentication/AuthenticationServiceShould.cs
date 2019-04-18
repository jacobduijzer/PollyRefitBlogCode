using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PollyRefitTest.Authentication;
using Refit;
using Xunit;

namespace PollyRefitTest.UnitTests.Authentication
{
    public class AuthenticationServiceShould
    {
        private const string CORRECT_CLIENT_ID = "correct_client_id";
        private const string CORRECT_CLIENT_SECRET = "correct_client_secret";
        private const string INVALID_CLIENT_ID = "invalid_client_id";

        private const string TEST_ACCESS_TOKEN = "someaccesstoken";

        private readonly Mock<IAuthenticationApi> _authApi;

        public AuthenticationServiceShould()
        {
            _authApi = new Mock<IAuthenticationApi>(MockBehavior.Strict);
            _authApi
                .Setup(x => x.GetAccessToken(CORRECT_CLIENT_ID, CORRECT_CLIENT_SECRET))
                .ReturnsAsync(new AuthenticationResult { access_token = TEST_ACCESS_TOKEN })
                .Verifiable();
            _authApi
                .Setup(x => x.GetAccessToken(INVALID_CLIENT_ID, CORRECT_CLIENT_SECRET))
                .ThrowsAsync(TestHelper.CreateRefitException(HttpMethod.Post, HttpStatusCode.Forbidden))
                .Verifiable();
        }

        [Fact]
        public void Construct() =>
            new AuthenticationService(_authApi.Object, CORRECT_CLIENT_ID, CORRECT_CLIENT_SECRET)
                .Should().BeOfType<AuthenticationService>();

        [Fact]
        public void ThrowWhenApiIsNull() =>
            new Action(() => new AuthenticationService(null, CORRECT_CLIENT_ID, CORRECT_CLIENT_SECRET))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("authenticationApi");

        [Theory]
        [InlineData(null, CORRECT_CLIENT_SECRET, "clientId")]
        [InlineData(CORRECT_CLIENT_ID, null, "clientSecret")]
        public void ThrowWhenInputIsNull(string clientId, string clientSecret, string paramName)
        {
            new Action(() => new AuthenticationService(_authApi.Object, clientId, clientSecret))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(paramName);
        }

        [Theory]
        [InlineData("", CORRECT_CLIENT_SECRET, "clientId")]
        [InlineData(CORRECT_CLIENT_ID, "", "clientSecret")]
        public void ThrowWhenInputIsEmpty(string clientId, string clientSecret, string paramName)
        {
            new Action(() => new AuthenticationService(_authApi.Object, clientId, clientSecret))
                .Should().Throw<ArgumentException>().And.ParamName.Should().Be(paramName);
        }

        [Fact]
        public async Task ReturnAuthenticationResult()
        {
            var service = new AuthenticationService(_authApi.Object, CORRECT_CLIENT_ID, CORRECT_CLIENT_SECRET);
            var authenticationResult = await service.GetAccessToken().ConfigureAwait(false);

            authenticationResult.Should().NotBeNull();
            authenticationResult.access_token.Should().NotBeNullOrEmpty().And.Be(TEST_ACCESS_TOKEN);
        }

        [Fact]
        public void ThrowWhenCredentialsAreInvalid()
        {
            var service = new AuthenticationService(_authApi.Object, INVALID_CLIENT_ID, CORRECT_CLIENT_SECRET);

             new Func<Task>(async () => await service.GetAccessToken().ConfigureAwait(false))
                .Should().Throw<ApiException>().Where(ex => ex.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task RetryWhenReceivingTimeout()
        {
            // ARRANGE
            var mockApi = new Mock<IAuthenticationApi>(MockBehavior.Strict);
            mockApi.SetupSequence(x => x.GetAccessToken(CORRECT_CLIENT_ID, CORRECT_CLIENT_SECRET))
                .Throws(TestHelper.CreateRefitException(HttpMethod.Post, HttpStatusCode.RequestTimeout))
                .Throws(TestHelper.CreateRefitException(HttpMethod.Post, HttpStatusCode.RequestTimeout))
                .ReturnsAsync(new AuthenticationResult { access_token = TEST_ACCESS_TOKEN });

            var service = new AuthenticationService(mockApi.Object, CORRECT_CLIENT_ID, CORRECT_CLIENT_SECRET);

            // ACT
            var authResult = await service.GetAccessToken().ConfigureAwait(false);

            // ASSERT
            authResult.Should().NotBeNull();
            authResult.access_token.Should().NotBeNullOrEmpty().And.Be(TEST_ACCESS_TOKEN);

            // The API Should have called GetAccessToken exactly 3 times: 2 times it received an exception,
            // the third time a correct result
            mockApi.Verify(x => x.GetAccessToken(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        }
    }
}
