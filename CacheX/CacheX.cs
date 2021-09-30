using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;

namespace CacheX
{
    public class CacheX<TKey, TValue>
    {
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _sync;
        private readonly ISystemClock _systemClock;

        public CacheX(ISystemClock systemClock = null)
        {
            _systemClock = systemClock ?? new SystemClock();
            _cache = new MemoryCache(new MemoryCacheOptions { Clock = _systemClock });
            _sync = new();
        }

        public async Task<TValue> GetOrAdd(TKey key, Func<Task<TValue>> createValueFactory)
        {
            return await GetOrAdd(key, createValueFactory, default);
        }

        public async Task<TValue> GetOrAdd(TKey key, Func<Task<TValue>> createValueFactory,
            TimeSpan absoluteExpiration = default)
        {
            if (_cache.TryGetValue(key, out TValue cachedValue))
                return cachedValue;

            var semaphore = _sync.GetOrAdd(key, x => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                if (_cache.TryGetValue(key, out cachedValue))
                    return cachedValue;

                cachedValue = await createValueFactory();

                return absoluteExpiration == default ?
                    _cache.Set(key, cachedValue) :
                    _cache.Set(key, cachedValue, absoluteExpiration);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
