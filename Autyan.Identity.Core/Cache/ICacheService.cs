using System;
using System.Collections.Generic;

namespace Autyan.Identity.Core.Cache
{
    public interface ICacheService
    {
        T GetCacheItem<T>(object key);

        void SetCacheItem<T>(object key, T item, TimeSpan? expire);

        IEnumerable<T> GetCachesByGroup<T>(string group);

        void SetCachesByGroup<T>(string group, IEnumerable<KeyValuePair<object, T>> items);
    }
}
