namespace Autyan.Identity.Core.Cache
{
    public static class CacheService
    {
        public static ICacheService Get()
        {
            return new RedisCacheService();
        }
    }
}
