using System;
using System.Collections.Generic;

namespace Diff
{
    /// <summary>
    /// Provides static methods to compare two sequences for a minimum edit path.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Our underlying "Differ" library has a strange protocol for some historical reasons.
    /// This class provides a wrapper for a slightly more reasonable API. 
    /// </para>
    /// <para>
    /// In this class, an <i>edit path</i> is represented by a string consisting of three alphabets: A, D and C.
    /// Each letter represents a single edit operation:
    /// <list type="bullet">
    /// <item>A represents an <i>add</i> operation, i.e., a new element is added into the current sequence.</item>
    /// <item>D represents a <i>delete</i> operation, i.e., an existing element is deleted from the current sequence.</item>
    /// <item>C represents a <i>common</i> element, that corresponds to an operation to advance the edit position over one element without changing the contents of the current sequence.</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static class Diff
    {
        /// <summary>
        /// The <see cref="IDiffer"/> instance we are using.
        /// </summary>
        /// <returns></returns>
        private static IDiffer Differ() { return new GreedyDiffer2(); }

        /// <summary>
        /// Compares two sequences using the type's standard equality.
        /// </summary>
        /// <typeparam name="T">Type of an element to be compared.</typeparam>
        /// <param name="src">Source sequence of elements.</param>
        /// <param name="dst">Destination sequence of elements.</param>
        /// <returns>The <i>edit path</i> from <paramref name="src"/> to <paramref name="dst"/>.</returns>
        /// <remarks>This method is NOT thread safe.</remarks>
        public static string Compare<T>(IList<T> src, IList<T> dst) { return Differ().Compare(src, dst).Reorder().Changes; }

        /// <summary>
        /// Compares two sequences using the user-specified equality.
        /// </summary>
        /// <typeparam name="T">Type of an element to be compared.</typeparam>
        /// <param name="src">Source sequence of elements.</param>
        /// <param name="dst">Destination sequence of elements.</param>
        /// <param name="comp">Equality for use to produce <i>edit path</i>.</param>
        /// <returns>The <i>edit path</i> from <paramref name="src"/> to <paramref name="dst"/>.</returns>
        /// <remarks>This method is NOT thread safe.</remarks>
        public static string Compare<T>(IList<T> src, IList<T> dst, Comparison<T> comp) { return Differ().Compare(src, dst, comp).Reorder().Changes; }
    }
}
