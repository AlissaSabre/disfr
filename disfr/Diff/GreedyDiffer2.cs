using System;
using System.Collections.Generic;
using System.Text;

namespace Diff
{
    /// <summary>
    /// A variation of the greedy algorithm by Eugene W. Myers (1986). O((M+N)*D).
    /// It means this algorithm works fast if given two lists are _similar_.
    /// </summary>
    public class GreedyDiffer2 : DifferBase
    {
        private struct Head
        {
            public int Src, Dst;
            public Trace Tra;
        }

        private class Trace
        {
            private readonly Trace Prev;
            private readonly int Step;

            public Trace(Trace prev, int step)
            {
                Prev = prev;
                Step = step;
            }

            public const int A = -1;
            public const int D = -2;

            public static int C(int run) { return run; }

            public static Trace operator+(Trace prev, int step)
            {
                return new Trace(prev, step);
            }

            public StringBuilder ToStringBuilder(int extra_length)
            {
                int length = 0;
                for (var p = this; p != null; p = p.Prev) length += (p.Step < 0) ? 1 : p.Step;
                var sb = new StringBuilder(length + extra_length) { Length = length };
                for (var p = this; p != null; p = p.Prev)
                {
                    switch (p.Step)
                    {
                        case A: sb[--length] = 'A'; break;
                        case D: sb[--length] = 'D'; break;
                        default:
                            for (int i = p.Step; i > 0; --i)
                            {
                                sb[--length] = 'C';
                            }
                            break;
                    }
                }
                return sb;
            }

            public override string ToString()
            {
                return ToStringBuilder(0).ToString();
            }
        }

        protected override string DoCompare<T>(IList<T> src, IList<T> dst, Comparison<T> comp)
        {
            var sn = src.Count;
            var dn = dst.Count;
            while (sn > 0 && dn > 0 && comp(src[sn - 1], dst[dn - 1]) == 0)
            {
                --sn;
                --dn;
            }
            if (sn == 0) return new StringBuilder(dst.Count).Append('A', dn).Append('C', src.Count).ToString();
            if (dn == 0) return new StringBuilder(src.Count).Append('D', sn).Append('C', dst.Count).ToString();

            var heads = new Head[sn + dn + 3];
            var z = heads.Length - 2;
            var orig = sn + 1;
            var goal = dn + 1;
            heads[orig].Tra = null;

            for (var i = 1; i < z; i++)
            {
                var min = orig - i + 1;
                var max = orig + i - 1;
                for (var p = min; p <= max; p += 2)
                {
                    if (p < 1 || p > z) continue;

                    var s = heads[p].Src;
                    var d = heads[p].Dst;
                    var r = heads[p].Tra;

                    // Find a snake.
                    var u = 0;
                    while (s + u < sn && d + u < dn && comp(src[s + u], dst[d + u]) == 0) u++;
                    if (u > 0)
                    {
                        s += u;
                        d += u;
                        r += Trace.C(u);
                    }

                    // Recognize a difference.
                    if (heads[p - 1].Src + heads[p - 1].Dst <= s + d)
                    {
                        heads[p - 1].Src = s + 1;
                        heads[p - 1].Dst = d;
                        heads[p - 1].Tra = r + Trace.D;
                    }
                    if (heads[p + 1].Src + heads[p + 1].Dst <= s + d)
                    {
                        heads[p + 1].Src = s;
                        heads[p + 1].Dst = d + 1;
                        heads[p + 1].Tra = r + Trace.A;
                    }
                }

                // See if we have reached the goal.
                if (heads[goal].Src >= sn && heads[goal].Dst >= dn)
                {
                    int posts = src.Count - sn;
                    return heads[goal].Tra.ToStringBuilder(posts).Append('C', posts).ToString();
                }
            }

            throw new ApplicationException("Edit matrix overrun.  It means this program has a bug in it.");
        }
    }
}
