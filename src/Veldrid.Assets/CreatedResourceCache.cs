using System.Collections.Generic;

namespace Veldrid.Assets
{
    public static class CreatedResourceCache
    {
        private static readonly Dictionary<object, object> s_cache = new Dictionary<object, object>();
        public static void ClearCache() => s_cache.Clear();

        public static bool TryGetCachedItem<TKey, TValue>(TKey key, out TValue value)
        {
            object fromCache;
            if (!s_cache.TryGetValue(key, out fromCache))
            {
                value = default(TValue);
                return false;
            }
            else
            {
                value = (TValue)fromCache;
                return true;
            }
        }

        public static void CacheItem<TKey, TValue>(TKey key, TValue value)
        {
            s_cache.Add(key, value);
        }
    }
}
