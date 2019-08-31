using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    /// <summary>
    /// Provides a local/private equivalent of the CLR's intern pool.
    /// </summary>
    public interface IStringPool
    {
        /// <summary>
        /// Returns a locally pooled version of a string.
        /// </summary>
        /// <param name="s">A string to look in the pool.</param>
        /// <returns>A pooled string that is equal to <paramref name="s"/>.</returns>
        /// <remarks>
        /// This method may or may not be thread safe, depending on the implementation.
        /// </remarks>
        string Intern(string s);
    }

    /// <summary>
    /// Provides a local/private equivalent of the CLR's intern pool.
    /// </summary>
    /// <remarks>
    /// This class is not thread safe.
    /// Use <see cref="ConcurrentStringPool"/> for concurrent access.
    /// </remarks>
    public class StringPool : IStringPool
    {
        private readonly Dictionary<string, string> Pool = new Dictionary<string, string>();

        /// <summary>
        /// Returns a locally pooled version of a string.
        /// </summary>
        /// <param name="s">A string to look in the pool.</param>
        /// <returns>A pooled string that is equal to <paramref name="s"/>.</returns>
        /// <remarks>
        /// This method is not thread safe.
        /// Use <see cref="ConcurrentStringPool"/> for concurrent access.
        /// </remarks>
        public string Intern(string s)
        {
            if (s == null) return null;
            string t;
            if (Pool.TryGetValue(s, out t)) return t;
            if ((t = string.IsInterned(s)) != null) s = t;
            Pool.Add(s, s);
            return s;
        }
    }

    /// <summary>
    /// Provides a local/private equivalent of the CLR's intern pool.
    /// </summary>
    /// <remarks>
    /// This class is thread safe.
    /// It has a slight overhead over <see cref="StringPool"/> if used singlethreadedly.
    /// </remarks>
    public class ConcurrentStringPool : IStringPool
    {
        private readonly ConcurrentDictionary<string, string> Pool = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Returns a locally pooled version of a string.
        /// </summary>
        /// <param name="s">A string to look in the pool.</param>
        /// <returns>A pooled string that is equal to <paramref name="s"/>.</returns>
        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        public string Intern(string s)
        {
            if (s == null) return null;
            string t;
            if (Pool.TryGetValue(s, out t)) return t;
            if ((t = string.IsInterned(s)) != null) s = t;
            if (Pool.TryAdd(s, s)) return s;
            return Pool[s];
        }
    }
}
