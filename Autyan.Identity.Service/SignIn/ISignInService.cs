using System.Threading.Tasks;
using Autyan.Identity.Model;

namespace Autyan.Identity.Service.SignIn
{
    public interface ISignInService
    {
        SignInResult Register(IdentityUser user);

        Task<SignInResult> RegisterAsync(IdentityUser user);

        SignInResult PasswordSignIn(string userName, string password);

        Task<SignInResult> PasswordSignInAsync(string userName, string password);

        Task<SignInResult> UserLoginLock(long userId);
    }
}
