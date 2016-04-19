using System;
using System.Collections.Generic;

namespace Diff
{
    /// <summary>
    /// Provides static methods to compare two sequences for a minimum edit path.
    /// </summary>
    /// <remarks>
    /// Our Differ library has a strange protocol for some historical reasons.
    /// This class provides a wrapper for a more reasonable API. 
    /// </remarks>
    public static class Diff
    {
        private static IDiffer Differ() { return new GreedyDiffer2(); } 

        public static string Compare<T>(IList<T> src, IList<T> dst) { return Differ().Compare(src, dst).Reorder().Changes; }

        public static string Compare<T>(IList<T> src, IList<T> dst, Comparison<T> comp) { return Differ().Compare(src, dst, comp).Reorder().Changes; }
    }
}
