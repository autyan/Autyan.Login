using System.Web;
using System.Web.Mvc;
using Autyan.LoginWeb.Filters;

namespace Autyan.LoginWeb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute());
            filters.Add(new ModelValidationFilter());
        }
    }
}
