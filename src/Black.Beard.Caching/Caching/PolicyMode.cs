using System;
using System.Collections.Generic;
using System.Text;

namespace Bb.Caching
{

    /// <summary>
    /// Specifies how items are prioritized for preservation during a memory pressure triggered cleanup.
    /// </summary>
    public enum PolicyMode
    {

        /// <summary>
        /// Indicates that there is no priority for removing the cache entry.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Indicates that a cache entry should never be removed from the cache.
        /// </summary>
        NotRemovable = 1,

        /// Indicates that a cache entry should be removed from the cache if never again used.
        Slided = 2,

    }

}
