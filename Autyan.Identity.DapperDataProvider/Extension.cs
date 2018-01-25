using Autofac;
using Autyan.Identity.Core.Component;
using Autyan.Identity.Model.DataProvider;

namespace Autyan.Identity.DapperDataProvider
{
    public static class Extension
    {
        public static WireUp UsrDapper(this WireUp wireUp)
        {
            //注册DataProvider
            wireUp.ContainerBuilder.RegisterType<IdentityUserProvider>().As<IIdentityUserProvider>().InstancePerLifetimeScope();

            return wireUp;
        }
    }
}
