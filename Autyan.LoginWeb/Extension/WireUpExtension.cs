using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autyan.Identity.Core.Component;

namespace Autyan.LoginWeb.Extension
{
    public static class WireUpExtension
    {
        public static WireUp UseAutoFacMvc(this WireUp wireUp)
        {
            var builder = wireUp.ContainerBuilder;

            // Register your MVC controllers. (MvcApplication is the name of
            // the class in Global.asax.)
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // OPTIONAL: Register model binders that require DI.
            builder.RegisterModelBinders(typeof(MvcApplication).Assembly);
            builder.RegisterModelBinderProvider();

            // OPTIONAL: Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // OPTIONAL: Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // OPTIONAL: Enable property injection into action filters.
            builder.RegisterFilterProvider();


            // OPTIONAL: Enable action method parameter injection (RARE).
            //builder.InjectActionInvoker();
            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return wireUp;
        }
    }
}