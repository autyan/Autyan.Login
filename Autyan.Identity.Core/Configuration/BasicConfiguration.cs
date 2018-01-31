using System.Configuration;

namespace Autyan.Identity.Core.Configuration
{
    public static class BasicConfiguration
    {
        public static string RedisServerAddress => ConfigurationManager.AppSettings["RedisServer"];
    }
}
