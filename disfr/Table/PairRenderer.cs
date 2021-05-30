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
        Name = InlineString.Render.TagNumber,
        Disp = InlineString.Render.TagDisplay,
        Code = InlineString.Render.TagCode,
        None = InlineString.Render.TagNone,
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

        public TagShowing TagShowing { get; set; }

        public bool ShowChanges { get; set; }

        public InlineString.Render InlineStringRenderingMode
        {
            get
            {
                InlineString.Render mode = (InlineString.Render)TagShowing;
                if (!ShowChanges) mode |= InlineString.Render.HideDel;
                return mode;
            }
        }

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
            foreach (var rwp in inline.RunsWithProperties)
            {
                Gloss gloss;
                var prop = rwp.Property;
                if (ShowChanges)
                {
                    switch (prop)
                    {
                        case InlineProperty.None: gloss = Gloss.None; break;
                        case InlineProperty.Ins: gloss = Gloss.INS; break;
                        case InlineProperty.Del: gloss = Gloss.DEL; break;
                        case InlineProperty.Emp: gloss = Gloss.EMP; break;
                        default:
                            throw new ApplicationException("internal error: unknown InlineProperty value in RWP.");
                    }
                }
                else
                {
                    switch (prop)
                    {
                        case InlineProperty.None: gloss = Gloss.None; break;
                        case InlineProperty.Ins: gloss = Gloss.None; break;
                        case InlineProperty.Del: continue; // XXX
                        case InlineProperty.Emp: gloss = Gloss.EMP; break;
                        default:
                            throw new ApplicationException("internal error: unknown InlineProperty value in RWP.");
                    }
                }

                var run = rwp.Run;
                if (run is InlineText)
                {
                    var str = (run as InlineText).Text;
                    if (!ShowSpecials || ignore_show_specials)
                    {
                        g.Append(str, gloss);
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
                                g.Append(SpecialCharMapRaw[c], gloss | Gloss.SYM);
                                g.Append(SpecialCharMapAlt[c], gloss | Gloss.ALT);
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
                            g.Append(str.Substring(p), gloss);
                        }
                    }
                }
                else if (run is InlineTag)
                {
                    var tag = (InlineTag)run;
                    var text = tag.ToString((InlineString.Render)TagShowing);
                    if (!string.IsNullOrEmpty(text))
                    {
                        g.Append(text, gloss | Gloss.TAG);
                    }
                }
                else
                {
                    throw new ApplicationException("internal error: unknown type in RWP.Run");
                }
            }
            g.Frozen = true;
            return g;
        }

        public string FlatFromInline(InlineString inline)
        {
            return inline.ToString(InlineString.RenderFlat);
        }

        /// <summary>The character to pad numeric IDs.</summary>
        /// <remarks>It is Unicode U+2007 FIGURE SPACE.</remarks>
        private const char ID_PADDING_CHAR = '\u2007';

        private string TrimId(string id, int trim)
        {
            if (id.Length == 0) return id;
            if (id[0] < '0' || id[0] > '9') return id;

            int p = 0;
            while (p < id.Length && id[p] == '0') p++;
            int q = p;
            while (q < id.Length && id[q] >= '0' && id[q] <= '9') q++;

            // See issue #6 (https://github.com/AlissaSabre/disfr/issues/6)
            if (p > 0 && p == q) --p;

            int pad = trim - (q - p);
            if (pad <= 0)
            {
                return id.Substring(p);
            }
            else
            {
                var sb = new StringBuilder(id.Length - q + trim);
                sb.Append(ID_PADDING_CHAR, pad);
                sb.Append(id, p, id.Length - p);
                return sb.ToString();
            }
        }

        public string TagListFromInline(InlineString text)
        {
            if (TagShowing != TagShowing.Name && TagShowing != TagShowing.Disp) return null;
            var sb = new StringBuilder();
            foreach (var tag in text.Tags)
            {
                if (sb.Length > 0) sb.AppendLine();
                var label = tag.ToString((InlineString.Render)TagShowing);
                var code = tag.ToString(InlineString.Render.TagCode);
                sb.Append(label).Append(" = ").Append(code);
            }
            return sb.ToString();
        }

        public string Notes(IEnumerable<string> notes)
        {
            return notes == null ? null : string.Join(Environment.NewLine, notes);
        }

        public PairRenderer Clone()
        {
            // Currently, a shallow copy is enough to create an independent copy of a PairRenderer.
            return (PairRenderer)MemberwiseClone();
        }
    }
}
