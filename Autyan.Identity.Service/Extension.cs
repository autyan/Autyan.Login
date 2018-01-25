using Autofac;
using Autyan.Identity.Core.Component;
using Autyan.Identity.Service.SignIn;

namespace Autyan.Identity.Service
{
    public static class Extension
    {
        public static WireUp RegisterService(this WireUp wireUp)
        {
            wireUp.ContainerBuilder.RegisterType<SignInService>().As<ISignInService>().InstancePerLifetimeScope();

            return wireUp;
        }
    }
}
