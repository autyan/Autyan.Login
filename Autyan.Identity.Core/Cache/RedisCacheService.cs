using System;
using System.Collections.Generic;
using Autyan.Identity.Core.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Autyan.Identity.Core.Cache
{
    public class RedisCacheService : ICacheService
    {
        private static readonly ConnectionMultiplexer ConnectionMultiplexer;

        static RedisCacheService()
        {
            ConnectionMultiplexer = ConnectionMultiplexer.Connect(BasicConfiguration.RedisServerAddress);
        }

        private IDatabase DefaultDatabase => ConnectionMultiplexer.GetDatabase();

        public T GetCacheItem<T>(object key)
        {
            var itemString = DefaultDatabase.StringGet(key.ToString());
            if (!itemString.HasValue) return default(T);
            return JsonConvert.DeserializeObject<T>(itemString.ToString());
        }

        public void SetCacheItem<T>(object key, T item, TimeSpan? expire = null)
        {
            var itemJsonString = JsonConvert.SerializeObject(item);
            DefaultDatabase.StringSet(key.ToString(), itemJsonString, expire);
        }

        public IEnumerable<T> GetCachesByGroup<T>(string group)
        {
            throw new NotImplementedException();
        }

        public void SetCachesByGroup<T>(string group, IEnumerable<KeyValuePair<object, T>> items)
        {
            throw new NotImplementedException();
        }
    }
}
