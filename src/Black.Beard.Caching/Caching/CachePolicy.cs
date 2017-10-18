using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bb.Caching
{

    /// <summary>
    /// Cache policy retention
    /// </summary>
    public class CachePolicy
    {

        static CachePolicy()
        {

            CachePolicy.Default = new CachePolicy()
            {
                IsDefault = true,
                Duration  = (long)TimeSpan.FromMinutes(10).TotalSeconds,
                Mode = PolicyMode.Default,
                Name = "Default",
            };

        }

        /// <summary>
        /// Provide a default policy
        /// </summary>
        public static CachePolicy Default { get; internal set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public PolicyMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the length time duration.
        /// </summary>
        /// <value>
        /// The length time in seconds.
        /// </value>
        public long Duration { get;  set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is default.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is default; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the region name where the current policy must be apply.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        public string Name { get;  set; }

    }

}
