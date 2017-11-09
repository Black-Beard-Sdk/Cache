using System;

namespace Black.Beard.Caching.Contracts
{

    /// <summary>
    /// Abstract cache
    /// </summary>
    public interface ILocalCache
    {

        /// <summary>
        /// Return the value for the specified key.
        /// if the cache not contains value, it is initialized with the return of fetcher delegate method
        /// </summary>
        /// <param name="key">The key is the cache key</param>
        /// <param name="fetcher">The fetcher.</param>
        /// <param name="policyName">Name of the policy.</param>
        /// <returns>cache value</returns>
        object GetOrResolve(object key, Func<object> fetcher, string policyName = null);

        /// <summary>
        /// Return the value for the specified key. If the value is missing, null is returned.
        /// </summary>
        /// <param name="key">The key is the cache key</param>
        /// <returns>cache value</returns>
        object Get(object key);

        /// <summary>
        /// Deletes the specified key from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        void Del(object key);

        /// <summary>
        /// Sets the specified key with the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="policyName">Name of the policy.</param>
        void Set(object key, object value, string policyName = null);

    }


}
