using System.Threading.Tasks;

namespace PollyRefitTest.Authentication
{
    public interface IAuthenticationApi
    {
        Task<AuthenticationResult> GetAccessToken(string clientId, string secret);
    }
}
