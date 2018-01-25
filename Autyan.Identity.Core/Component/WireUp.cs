using Autofac;

namespace Autyan.Identity.Core.Component
{
    public class WireUp
    {
        public ContainerBuilder ContainerBuilder { get; } = new ContainerBuilder();

        private WireUp()
        {

        }

        public static WireUp Instance = new WireUp();

        public void Build()
        {
            
        }
    }
}
