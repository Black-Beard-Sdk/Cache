using Black.Beard.Caching.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bb.Caching
{


    /// <summary>
    /// Implementation from <see cref="ILocalCache"/>
    /// the class provide optimized synch lock for managing a cache
    /// </summary>
    public abstract class LocalCacheBase : ILocalCache
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="concurrencyLevel">concurrency level.</param>
        protected LocalCacheBase(object cache, int concurrencyLevel = -1)
        {

            this.cache = cache;

            if (concurrencyLevel <= 0)
                concurrencyLevel = Environment.ProcessorCount * 2;

            this._concurrencyLevel = concurrencyLevel;

            // scale locks objects on x items for best locking performance.
            this._locks = new lockItem[concurrencyLevel];
            for (int i = 0; i < concurrencyLevel; i++)
            {
                this._locks[i] = new lockItem();
            }

        }

        /// <summary>
        /// Return the value for the specified key.
        /// if the cache not contains value, it is initialized with the return of fetcher method
        /// </summary>
        /// <param name="key">The key is the cache key</param>
        /// <param name="fetcher">The fetcher.</param>
        /// <returns>cache value</returns>
        public object GetOrResolve(object key, Func<object> fetcher, string policyName = null)
        {

            CacheValue value;

            if (!GetValue(key, out value))
            {

                // scale locks objects on x items for best locking performance.
                // Resolve lock index to use.
                int hashcode = key.GetHashCode();
                int lockNo = (hashcode & 0x7fffffff) % _concurrencyLevel;
                Debug.Assert(lockNo >= 0 && lockNo < _concurrencyLevel);

                var l = this._locks[lockNo];
                lock (l._syncLock)
                {
                    if (!GetValue(key, out value))
                        SetValue(key, (value = new CacheValue()), policyName);

                }

            }

            object result = value.GetValue(fetcher);

            return result;
        }

        /// <summary>
        /// Return the value for the specified key.        
        /// if the cache not contains value, it is initialized with the return of fetcher method
        /// </summary>
        /// <param name="key">The key is the cache key</param>
        /// <param name="fetcher">The fetcher.</param>
        /// <returns>cache value</returns>
        public void Set(object key, object value, string policyName = null)
        {

            CacheValue _value;

            if (!GetValue(key, out _value))
            {

                // scale locks objects on x items for best locking performance.
                // Resolve lock index to use.
                int hashcode = key.GetHashCode();
                int lockNo = (hashcode & 0x7fffffff) % _concurrencyLevel;
                Debug.Assert(lockNo >= 0 && lockNo < _concurrencyLevel);

                var l = this._locks[lockNo];
                lock (l._syncLock)
                {
                    if (!GetValue(key, out _value))
                        SetValue(key, (_value = new CacheValue()), policyName);

                }

            }

            _value.SetValue(value);

        }


        /// <summary>
        /// Return the value for the specified key.        
        /// </summary>
        /// <param name="key">The key is the cache key</param>
        /// <returns>cache value</returns>
        public object Get(object key)
        {

            object result = null;

            CacheValue value;
            if (GetValue(key, out value))
                result = value.GetValue(null);

            return result;
        }

        /// <summary>
        /// Remove the value of the cache
        /// </summary>
        /// <param name="key">The key is the cache key</param>
        /// <returns>cache value</returns>
        public void Del(object key)
        {

            RemoveValue(key);

        }

        /// <summary>
        /// Removes the value ov the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void RemoveValue(object key);


        /// <summary>
        /// Gets the value for the specified key and region.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool GetValue(object key, out CacheValue value);


        /// <summary>
        /// Sets the value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="policyName">Name of the policy for duration .</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetValue(object key, CacheValue value, string policyName = null);

        /// <summary>
        /// Gets or sets the policies list.
        /// </summary>
        /// <value>
        /// The policies.
        /// </value>
        public CachePolicies Policies { get; set; }

        /// <summary>
        /// Resolves the policy for the specified name.
        /// </summary>
        /// <param name="policyName">Name of the policy.</param>
        /// <returns></returns>
        protected CachePolicy ResolvePolicy(string policyName)
        {

            CachePolicy _p;
            if (this.Policies != null)
                _p = this.Policies.ResolvePolicy(policyName);
            else
                _p = CachePolicy.Default;
            return _p;
        }

        private class lockItem
        {
            public volatile object _syncLock = new Object();
        }

        /// <summary>
        /// embedded class object for just lock the key
        /// </summary>
        protected class CacheValue
        {

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheValue"/> class.
            /// </summary>
            /// <param name="key">The key.</param>
            public CacheValue()
            {
            }

            /// <summary>
            /// if cache not initialized, resolve value from fetcher and store it in cache value
            /// </summary>
            /// <param name="fetcher">The fetcher.</param>
            /// <returns>Gets the value from cache value.</returns>
            internal object GetValue(Func<object> fetcher)
            {

                if (fetcher != null)
                    this.fetcher = fetcher;

                if (!intialized && this.fetcher != null)
                    lock (_syncLock)
                        if (!intialized)
                        {
                            this.value = this.fetcher();
                            this.intialized = true;
                        }

                return this.value;

            }

            internal void SetValue(object value)
            {

                lock (_syncLock)
                {
                    this.value = value;
                    this.intialized = true;
                }

            }

            private bool intialized;
            private volatile object _syncLock = new object();
            private object value;
            private Func<object> fetcher;

        }

        /// <summary>
        /// instance of the cache
        /// </summary>
        protected readonly object cache;
        private readonly lockItem[] _locks;
        private readonly int _concurrencyLevel;

    }

}

/*
        /// <param name="policy">The policy. function that return the policy <see cref="CacheItemPolicy"/></param>
 */
