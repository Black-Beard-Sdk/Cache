using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bb.Caching
{

    /// <summary>
    /// cache policies for regions
    /// </summary>
    public class CachePolicies
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CachePolicies"/> class.
        /// </summary>
        /// <param name="policies">The policies.</param>
        public CachePolicies(List<CachePolicy> policies)
        {
            this._policies = policies.ToDictionary(c => c.Name);
            this._default = this._policies.FirstOrDefault(c => c.Value.IsDefault).Value ?? CachePolicy.Default;
        }


        /// <summary>
        /// Resolves the policy for the specified policy name.
        /// </summary>
        /// <param name="policyName">The region.</param>
        /// <returns></returns>
        internal CachePolicy ResolvePolicy(string policyName)
        {

            CachePolicy result;

            if (!this._policies.TryGetValue(policyName, out result))
                result = this._default;

            return result;

        }

        /// <summary>
        /// Add a new cache retention policy 
        /// This method is not thread safe
        /// </summary>
        /// <param name="policy"></param>
        public void Add(CachePolicy policy)
        {
            this._policies.Add(policy.Name, policy);
        }

        private readonly Dictionary<string, CachePolicy> _policies;
        private readonly CachePolicy _default;

    }


}
