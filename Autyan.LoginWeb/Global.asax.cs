using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autyan.Identity.Core.Component;
using Autyan.Identity.Core.DataConfig;
using Autyan.Identity.DapperDataProvider;
using Autyan.Identity.Model;
using Autyan.Identity.Service;
using Autyan.LoginWeb.Extension;

namespace Autyan.LoginWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            RegisterComponents();
        }

        private static void RegisterComponents()
        {
            var wireUp = WireUp.Instance;
            wireUp.WormUpModel()
                .RegisterService()
                .UseDapper()
                .UseAutoFacMvc();
            MetadataContext.Instance.Initilize(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
