using Autyan.Identity.Core.Cache;

namespace Autyan.Identity.Service
{
    public abstract class BaseService
    {
        protected ICacheService Cache => CacheService.Get();
    }
}
