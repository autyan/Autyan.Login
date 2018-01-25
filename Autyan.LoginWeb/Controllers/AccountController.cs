using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Autyan.Identity.Model;
using Autyan.Identity.Service.SignIn;
using Autyan.LoginWeb.Models;

namespace Autyan.LoginWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISignInService _signInService;

        public AccountController(ISignInService signInService)
        {
            _signInService = signInService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Register()
        {
            return await Task.FromResult(View());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(UserRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var newUser = new IdentityUser
            {
                LoginName = model.UserName,
                PasswordHash = model.Password
            };

            var result = await _signInService.RegisterAsync(newUser);
            if (result.Succeed)
            {
                DoLogin(new SignInViewModel
                {
                    UserName = model.UserName
                });
                return Redirect("/");
            }

            foreach (var error in result.SignInErrors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Login()
        {
            return await Task.FromResult(View());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(SignInViewModel model)
        {
            var result = await _signInService.PasswordSignInAsync(model.UserName, model.Password);

            if (!result.Succeed)
            {
                foreach (var error in result.SignInErrors)
                {
                    ModelState.AddModelError("", error.Description);
                    return View(model);
                }
            }

            DoLogin(model);
            return Redirect("/");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return Redirect("/");
        }

        private static void DoLogin(SignInViewModel model)
        {
            FormsAuthentication.SetAuthCookie(model.UserName, false);
        }
    }
}