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
        public bool ShowLocalSerial { get; set; }

        public bool ShowLongAssetName { get; set; }

        public bool ShowRawId { get; set; }

        public TagShowing ShowTag { get; set; }

        public bool ShowSpecials { get; set; }

        private const char OPAR = '\u00AB'; /* « */
        private const char CPAR = '\u00BB'; /* » */

        private static Dictionary<char, string> SpecialCharMap = new Dictionary<char, string>()
        {
            { '\u0009', "\u2192\t" }, /* → */
            { '\u000A', "\u21B5\n" }, /* ↵ */
            { '\u0020', "\u22C5\u200B" }, /* ⋅ */
            { '\u00A0', "\u00AC" }, /* ¬ */
            { '\u2029', "\u00B6\n" }, /* ¶ */
            { '\u3000', "\u2610\u200B" }, /* ☐ */
        };

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

        public GlossyString GlossyFromInline(InlineString text)
        {
            var g = new GlossyString();
            foreach (var obj in text.Contents)
            {
                if (obj is string)
                {
                    g.Append((string)obj, Gloss.None);
                }
                else if (obj is InlineChar)
                {
                    string visual;
                    var c = ((InlineChar)obj).Char;
                    if (!ShowSpecials)
                    {
                        g.Append(c.ToString(), Gloss.None);
                    }
                    else if (SpecialCharMap.TryGetValue(c, out visual))
                    {
                        g.Append(visual, Gloss.SYM);
                    }
                    else
                    {
                        g.Append(string.Format("(U+{0:X4})", (int)c), Gloss.SYM);
                    }
                }
                else if (obj is InlineTag)
                {
                    var tag = (InlineTag)obj;
                    switch (ShowTag)
                    {
                        case TagShowing.None:
                            break;
                        case TagShowing.Name:
                            g.Append(BuildTagString(tag, tag.Number.ToString()), Gloss.TAG);
                            break;
                        case TagShowing.Disp:
                            g.Append(Enclose(tag.Display) ?? BuildTagString(tag, tag.Name, '{', '}'), Gloss.TAG);
                            break;
                        case TagShowing.Code:
                            g.Append(tag.Code ?? BuildTagString(tag, "*", '(', ')'), Gloss.TAG);
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

        private static string BuildTagString(InlineTag tag, string label, char opar = OPAR, char cpar = CPAR)
        {
            return opar + label + cpar;
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



    }
}
