using System;
using System.Collections.Generic;
using System.Text;

namespace Diff
{
    /// <summary>
    /// The interface that all Differ algorithms should provide.
    /// </summary>
    public interface IDiffer
    {
        /// <summary>
        /// Gets the current edit path.
        /// </summary>
        /// <remarks>
        /// Changes is null unless a Compare method is called.
        /// </remarks>
        string Changes { get; }

        /// <summary>
        /// Discards any result of comparison.
        /// </summary>
        /// <returns>This instance.</returns>
        IDiffer Reset();

        /// <summary>
        /// Compares two sequences using the type's standard equality.
        /// </summary>
        /// <typeparam name="T">Type of an element.</typeparam>
        /// <param name="src">Source sequence.</param>
        /// <param name="dst">Destination sequence.</param>
        /// <returns>This instance.</returns>
        /// <remarks>
        /// The result of the comparison is kept in the object.
        /// You can extract it as an edit path through <see cref="Changes"/>.
        /// </remarks>
        IDiffer Compare<T>(IList<T> src, IList<T> dst);

        /// <summary>
        /// Compares two sequences using the specified equality.
        /// </summary>
        /// <typeparam name="T">Type of an element.</typeparam>
        /// <param name="src">Source sequence.</param>
        /// <param name="dst">Destination sequence.</param>
        /// <param name="comp">Equality.</param>
        /// <returns>This instance.</returns>
        /// <remarks>
        /// The result of the comparison is kept in the object.
        /// You can extract it as an edit path through <see cref="Changes"/>.
        /// </remarks>
        IDiffer Compare<T>(IList<T> src, IList<T> dst, Comparison<T> comp);

        /// <summary>
        /// Performs any post-processing to the results of comparison. 
        /// </summary>
        /// <returns>This instance.</returns>
        IDiffer Reorder();
    }

    /// <summary>
    /// Provides some standard implementation among Differ algorithm providers.
    /// </summary>
    /// <remarks>
    /// You don't need to subclass <see cref="DifferBase"/> to implement a new Differ algorithm.
    /// If you subclass it, you only need to override <see cref="DoCompare{T}(IList{T}, IList{T}, Comparison{T})"/> in your delived class.
    /// </remarks>
    public abstract class DifferBase : IDiffer
    {
        /// <summary>
        /// Performs an actual Differ algorithm.
        /// </summary>
        /// <typeparam name="T">Type of an element.</typeparam>
        /// <param name="src">Source sequence.</param>
        /// <param name="dst">Destination sequence.</param>
        /// <param name="comp">Equality.</param>
        /// <returns>The minimum edit path.</returns>
        /// <remarks>
        /// A subclass must override this method to provide an algorithm.
        /// </remarks>
        protected abstract string DoCompare<T>(IList<T> src, IList<T> dst, Comparison<T> comp);

        public string Changes { get; protected set; }

        public IDiffer Reset()
        {
            Changes = null;
            return this;
        }

        public IDiffer Compare<T>(IList<T> src, IList<T> dst)
        {
            Changes = DoCompare(src, dst, Comparer<T>.Default.Compare);
            return this;
        }

        public IDiffer Compare<T>(IList<T> src, IList<T> dst, Comparison<T> comp)
        {
            Changes = DoCompare(src, dst, comp);
            return this;
        }

        public IDiffer Reorder()
        {
            Changes = Reorder(Changes);
            return this;
        }

        /// <summary>
        /// Reorders an edit path so that a delete precedes add.
        /// </summary>
        /// <param name="changes">An edit path.</param>
        /// <returns>Reordered edit path.</returns>
        public static string Reorder(string changes)
        {
            //int r;
            //while ((r = changes.IndexOf("AD")) >= 0)
            //{
            //    changes = changes.Substring(0, r) + "DA" + changes.Substring(r + 2);
            //}
            //return changes;

            var sb = new StringBuilder(changes.Length);
            int a = 0;
            foreach (var c in changes)
            {
                switch (c)
                {
                    case 'A':
                        a++;
                        break;
                    case 'C':
                        if (a > 0)
                        {
                            sb.Append('A', a);
                            a = 0;
                        }
                        sb.Append(c);
                        break;
                    case 'D':
                        sb.Append(c);
                        break;
                    default:
                        throw new ArgumentException(string.Format("unknown char '{0}' (U+{0:X})", c), "changes");
                }
            }
            return sb.Append('A', a).ToString();
        }
    }
}
