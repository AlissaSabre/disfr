using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    /// <summary>
    /// Provides a local/private version of the CLR's intern pool.
    /// </summary>
    public class StringPool
    {
        private readonly Dictionary<string, string> Pool = new Dictionary<string, string>();

        private readonly object Lock = new object();

        /// <summary>
        /// Returns a locally pooled version of a string.
        /// </summary>
        /// <param name="s">A string to look in the pool.</param>
        /// <returns>A pooled string that is equal to <paramref name="s"/>.</returns>
        /// <remarks>
        /// This method is thread safe, but it is achieved by internal locking.
        /// Performance may be degrade if mulitple threads accesses it simultaneously.
        /// </remarks>
        public string Intern(string s)
        {
            if (s == null) return null;
            lock(Lock)
            {
                string t;
                if (Pool.TryGetValue(s, out t)) return t;
                if ((t = string.IsInterned(s)) != null) s = t;
                return (Pool[s] = s);
            }
        }
    }
}
