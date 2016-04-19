using System;
using System.Collections.Generic;
using System.Text;

namespace Diff
{
    public interface IDiffer
    {
        string Changes { get; }

        IDiffer Reset();

        IDiffer Compare<T>(IList<T> src, IList<T> dst);

        IDiffer Compare<T>(IList<T> src, IList<T> dst, Comparison<T> comp);

        IDiffer Reorder();
    }

    public abstract class DifferBase : IDiffer
    {
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
