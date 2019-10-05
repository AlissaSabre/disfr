using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace disfr.UI
{
    public enum TagShowing
    {
        Name,
        Disp,
        Code,
        None,
    }

    public class PairRenderer
    {
        static PairRenderer()
        {
            InitializeSpecialCharMaps();
            SpecialCharChecker = BuildSCC(SpecialCharMapAlt.Keys);
        }

        public bool ShowLocalSerial { get; set; }

        public bool ShowLongAssetName { get; set; }

        public bool ShowRawId { get; set; }

        public TagShowing ShowTag { get; set; }

        public bool ShowSpecials { get; set; }

        private const char OPAR = '{';
        private const char CPAR = '}';

        private const string SpecialChars =
            "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007" +
            "\u0008\u0009\u000A\u000B\u000C\u000D\u000E\u000F" +
            "\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017" +
            "\u0018\u0019\u001A\u001B\u001C\u001D\u001E\u001F" +
            "\u0020\u007F" +
            "\u0080\u0081\u0082\u0083\u0084\u0085\u0086\u0087" +
            "\u0088\u0089\u008A\u008B\u008C\u008D\u008E\u008F" +
            "\u0090\u0091\u0092\u0093\u0094\u0095\u0096\u0097" +
            "\u0098\u0099\u009A\u009B\u009C\u009D\u009E\u009F" +
            "\u00A0" +
            "\u1680\u180E" +
            "\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007" +
            "\u2008\u2009\u200A\u200B\u200C\u200D" +
            "\u2028\u2029\u202F\u205F" +
            "\u2060\u2061\u2062\u2063" +
            "\u3000\u3164" +
            "\uFFA0\uFEFF";

        private static readonly Dictionary<char, string> SpecialCharMapAlt = new Dictionary<char, string>()
        {
            { '\u0009', "\u2192\t" }, /* → */
            { '\u000A', "\u21B5\n" }, /* ↵ */
            { '\u000D', "\u2190" }, /* ← */
            { '\u0020', "\u22C5\u200B" }, /* ⋅ */
            { '\u00A0', "\u00AC" }, /* ¬ */
            { '\u2029', "\u00B6\n" }, /* ¶ */
            { '\u3000', "\u2610\u200B" }, /* ☐ */
        };

        private static readonly Dictionary<char, string> SpecialCharMapRaw = new Dictionary<char, string>();

        private static void InitializeSpecialCharMaps()
        {
            foreach (char c in SpecialChars)
            {
                if (!SpecialCharMapAlt.ContainsKey(c))
                {
                    SpecialCharMapAlt[c] = string.Format("(U+{0:X4})", (int)c);
                }
            }

            foreach (char c in SpecialCharMapAlt.Keys)
            {
                SpecialCharMapRaw[c] = c.ToString();
            }
        }

        /// <summary>
        /// A sort of a direct perfect hash table to substitute SpecialCharMap.ContainsKey. 
        /// </summary>
        /// <remarks>
        /// Use <code>(SpecialCharChecker[c % SpecialCharChecker.Length] == c)</code>
        /// to see whether <code>c</code> is contained in SpecialCharMap as a key.
        /// </remarks>
        private static readonly char[] SpecialCharChecker;

        /// <summary>
        /// Builds and returns a SpecialCharChecker (SCC).
        /// </summary>
        /// <param name="map">SpecialCharMap to create SCC for.</param>
        /// <returns>The SpecialCharChecker.</returns>
        private static char[] BuildSCC(ICollection<char> specials)
        {
            for (int n = specials.Count; ; n++)
            {
                var icc = TryBuildSCC(specials, n);
                if (icc != null) return icc;
            }
        }

        /// <summary>
        /// Tries to build an SpecialCharChecker (SCC) of size <paramref name="size"/>.
        /// </summary>
        /// <param name="map">SpecialCharMap to create SCC for.</param>
        /// <param name="size">The desired size of the SpecialCharChecker.</param>
        /// <returns>The SpecialCharChecker of size <paramref name="size"/>, or null if not found.</returns>
        private static char[] TryBuildSCC(ICollection<char> specials, int size)
        {
            var icc = new char[size];
            foreach (var c in specials)
            {
                var i = c % size;
                if (i == 0 && c != 0) return null;
                if (icc[i] != 0) return null;
                icc[i] = c;
            }
            return icc;
        }

        public int Serial(AssetData asset, int serial)
        {
            if (serial <= 0) return 0;
            if (ShowLocalSerial) return serial;
            return serial + asset.BaseSerial;
        }

        public string AssetName(AssetData asset)
        {
            return ShowLongAssetName ? asset.LongAssetName : asset.ShortAssetName;
        }

        public string Id(AssetData asset, string id)
        {
            return ShowRawId ? id : TrimId(id, asset.IdTrimChars);
        }

        public GlossyString GlossyFromInline(InlineString inline, bool ignore_show_specials = false)
        {
            var g = new GlossyString();
            foreach (var run in inline.RunsWithProperties.Where(rwp => rwp.Property != InlineProperty.Del).Select(rwp => rwp.Run))
            {
                if (run is InlineText)
                {
                    var str = (run as InlineText).Text;
                    if (!ShowSpecials || ignore_show_specials)
                    {
                        g.Append(str, Gloss.None);
                    }
                    else
                    {
                        int p = 0;
                        for (int q = 0; q < str.Length; q++)
                        {
#if true
                            // The version with SpecialCharChecker.
                            var c = str[q];
                            if (SpecialCharChecker[c % SpecialCharChecker.Length] == c)
                            {
                                if (q > p) g.Append(str.Substring(p, q - p), Gloss.None);
                                g.Append(SpecialCharMapRaw[c], Gloss.SYM);
                                g.Append(SpecialCharMapAlt[c], Gloss.ALT);
                                p = q + 1;
                            }
#else
                            // The version without SpecialCharChecker.
                            string special;
                            if (SpecialCharMap.TryGetValue(str[q], out special))
                            {
                                if (q > p) g.Append(str.Substring(p, q - p), Gloss.None);
                                g.Append(SpecialCharMapRaw[c], Gloss.SYM);
                                g.Append(SpecialCharMapAlt[c], Gloss.ALT);
                                p = q + 1;
                            }
#endif
                        }
                        if (p < str.Length)
                        {
                            g.Append(str.Substring(p), Gloss.None);
                        }
                    }
                }
                else if (run is InlineTag)
                {
                    var tag = (InlineTag)run;
                    switch (ShowTag)
                    {
                        case TagShowing.None:
                            break;
                        case TagShowing.Name:
                            g.Append(BuildTagString(tag, tag.Number.ToString()), Gloss.TAG);
                            break;
                        case TagShowing.Disp:
                            g.Append(Enclose(tag.Display) ?? BuildTagString(tag, tag.Name), Gloss.TAG);
                            break;
                        case TagShowing.Code:
                            g.Append(tag.Code ?? BuildTagString(tag, "*"), Gloss.TAG);
                            break;
                        default:
                            throw new ApplicationException("internal error");
                    }
                }
                else
                {
                    throw new ApplicationException("internal error");
                }
            }
            g.Frozen = true;
            return g;
        }

        private static string Enclose(string s)
        {
            if (s == null) return null;
            if (s.StartsWith("{") && s.EndsWith("}")) return s;
            return "{" + s + "}";
        }

        private static string BuildTagString(InlineTag tag, string label)
        {
            return OPAR + label + CPAR;
        }

        public string FlatFromInline(InlineString inline)
        {
            // XXX XXX XXX
            return inline.ToString(InlineToString.Flat);
        }

        private const char FIGURE_SPACE = '\u2007';

        private string TrimId(string id, int trim)
        {
            if (trim < 0) return id;
            int len = id.Length - trim;
            var sb = new StringBuilder(id, trim, len, len);
            for (int p = 0; p < sb.Length && sb[p] == '0'; p++) sb[p] = FIGURE_SPACE;
            return sb.ToString();
        }

        public string TagListFromInline(InlineString text)
        {
            if (ShowTag != TagShowing.Name && ShowTag != TagShowing.Disp) return null;
            var sb = new StringBuilder();
            foreach (var tag in text.Tags)
            {
                if (sb.Length > 0) sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(tag.Code))
                {
                    switch (ShowTag)
                    {
                        case TagShowing.Name:
                            sb.Append(BuildTagString(tag, tag.Number.ToString()));
                            break;
                        case TagShowing.Disp:
                            sb.Append(Enclose(tag.Display) ?? BuildTagString(tag, tag.Name));
                            break;
                    }
                    sb.Append(" = ").Append(tag.Code);
                }
            }
            return sb.ToString();
        }

        public string Notes(IEnumerable<string> notes)
        {
            return notes == null ? null : string.Join("\n", notes);
        }
    }
}
