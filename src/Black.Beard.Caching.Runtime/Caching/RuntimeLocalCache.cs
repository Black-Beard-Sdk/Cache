using Bb.Caching;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Runtime.CompilerServices;

namespace Black.Beard.Caching.Runtime
{

    /// <summary>
    /// Implementation From <see cref="ILocalCache"/> with <see cref="IMemoryCache"/>
    /// </summary>
    /// <example>
    /// <code lang="C#">
    /// 
    /// </code>
    /// </example>
    public class RuntimeLocalCache : LocalCacheBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="OptimisedCache"/> class.
        /// </summary>
        /// <param name="cache">The cache <see cref="IMemoryCache"/>.</param>
        public RuntimeLocalCache(IMemoryCache cache) : base(cache)
        {

        }

        /// <summary>
        /// Gets the value for the specified key and region.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetValue(object key, out CacheValue value)
        {
            var _value = (cache as IMemoryCache).Get(key);
            value = (cache as IMemoryCache).Get(key) as CacheValue;
            return value != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void RemoveValue(object key)
        {
            (cache as IMemoryCache).Remove(key);            
        }



        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="policyName">Name of the policy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetValue(object key, CacheValue value, string policyName = null)
        {
            var entry = (cache as IMemoryCache).CreateEntry(key);
            entry.SetValue(value);
            entry.Dispose();
        }

        private MemoryCacheEntryOptions policy(string policyName)
        {

            var policy = new MemoryCacheEntryOptions();

            var _p = ResolvePolicy(policyName);

            var time = TimeSpan.FromSeconds(_p.Duration);

            switch (_p.Mode)
            {

                case PolicyMode.Default:
                    var _t = DateTime.Now.Add(new TimeSpan());
                    policy.AbsoluteExpiration = new DateTimeOffset(_t);
                    policy.Priority = CacheItemPriority.Normal;
                    break;

                case PolicyMode.Slided:
                    policy.Priority = CacheItemPriority.Normal;
                    policy.SlidingExpiration = time;
                    break;

                case PolicyMode.NotRemovable:
                    policy.Priority = CacheItemPriority.NeverRemove;
                    break;

            }

            return policy;

        }

    }

}
