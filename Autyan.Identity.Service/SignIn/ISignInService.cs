using System.Threading.Tasks;
using Autyan.Identity.Model;

namespace Autyan.Identity.Service.SignIn
{
    public interface ISignInService
    {
        Task<SignInResult> RegisterAsync(IdentityUser user);

        Task<SignInResult> PasswordSignInAsync(string userName, string password);

        Task<SignInResult> UserLoginLockAsync(long userId);
    }
}
