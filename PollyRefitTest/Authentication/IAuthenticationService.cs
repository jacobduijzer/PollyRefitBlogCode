using System.Threading.Tasks;

namespace PollyRefitTest.Authentication
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> GetAccessToken();
    }
}
