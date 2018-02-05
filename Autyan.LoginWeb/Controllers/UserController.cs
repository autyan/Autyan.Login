using System.Threading.Tasks;
using System.Web.Mvc;
using Autyan.Identity.Service.SignIn;
using Autyan.LoginWeb.Models;

namespace Autyan.LoginWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly ISignInService _signInService;

        public UserController(ISignInService signInService)
        {
            _signInService = signInService;
        }

        [AllowAnonymous]
        public async Task<ActionResult> Login(SignInViewModel model)
        {
            var result = await _signInService.PasswordSignInAsync(model.UserName, model.Password);

            if (!result.Succeed)
            {
                foreach (var error in result.ErrorMessages)
                {
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }

            //DoLogin(model);
            return Json("OK");
        }

        [AllowAnonymous]
        public ActionResult Get(int id)
        {
            return Json("");
        }
    }
}