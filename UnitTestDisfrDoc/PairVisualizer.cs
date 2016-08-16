using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    public class PairVisualizer
    {
        public string Visualize(IEnumerable<IAsset> assets)
        {
            var sb = new StringBuilder();
            Visualize(sb, assets, FindProps(assets).OrderBy(s => s, StringComparer.Ordinal).ToArray());
            return sb.ToString();
        }

        private IEnumerable<string> FindProps(IEnumerable<IAsset> assets)
        {
            return assets.SelectMany(FindProps).Distinct();
        }

        private IEnumerable<string> FindProps(IAsset asset)
        {
            return asset.Properties.Select(p => p.Key);
        }

        private void Visualize(StringBuilder sb, IEnumerable<IAsset> assets, string[] props)
        {
            sb.AppendLine("<Package>");
            foreach (var asset in assets)
            {
                Visualize(sb, asset, props);
            }
            sb.AppendLine("</Package>");
        }

        private void Visualize(StringBuilder sb, IAsset asset, string[] props)
        {
            sb.AppendFormat("<Asset package=\"{0}\" original=\"{1}\" source-lang=\"{2}\" target-lang=\"{3}\">",
                asset.Package, asset.Original, asset.SourceLang, asset.TargetLang);
            sb.AppendLine();
            foreach (var pair in asset.TransPairs)
            {
                Visualize(sb, pair, props);
                sb.AppendLine();
            }
            var alts = asset.AltPairs.ToList();
            if (alts.Count > 0)
            {
                sb.AppendLine("<AltPairs>");
                foreach (var alt in alts)
                {
                    sb.Append("  ");
                    Visualize(sb, alt, props);
                    sb.AppendLine();
                }
                sb.AppendLine("</AltPairs>");
            }
            sb.AppendLine("</Asset>");
        }

        private void Visualize(StringBuilder sb, ITransPair pair, string[] props)
        {
            sb.Append("<Pair>");
            Print(sb, "Serial", pair.Serial);
            Print(sb, "Id", pair.Id);
            Print(sb, "Source", pair.Source.ToString());
            Print(sb, "Target", pair.Target.ToString());
            Print(sb, "SourceLang", pair.SourceLang);
            Print(sb, "TargetLang", pair.TargetLang);
            if (pair.Notes != null)
            {
                foreach (var note in pair.Notes)
                {
                    Print(sb, "Note", note);
                }
            }
            foreach (var key in props)
            {
                var prop = pair[key];
                if (prop != null && prop.Length > 0)
                {
                    Print(sb, "Prop", "name", key, prop);
                }
            }
            sb.Append("</Pair>");
        }

        private static void Print(StringBuilder sb, string tagname, int content)
        {
            sb.AppendFormat("<{0}>", tagname);
            sb.Append(content);
            sb.AppendFormat("</{0}>", tagname);
        }

        private static void Print(StringBuilder sb, string tagname, string content)
        {
            sb.AppendFormat("<{0}>", tagname);
            Escape(sb, content);
            sb.AppendFormat("</{0}>", tagname);
        }

        private static void Print(StringBuilder sb, string tagname, string attrname, string attrvalue, string content)
        {
            sb.AppendFormat("<{0} {1}=\"{2}\">", tagname, attrname, attrvalue);
            Escape(sb, content);
            sb.AppendFormat("</{0}>", tagname);
        }

        private static void Escape(StringBuilder sb, string content)
        {
            foreach (var c in content)
            {
                if (c == '&')
                {
                    sb.Append("&amp;");
                }
                else if (c == '<')
                {
                    sb.Append("&lt;");
                }
                else if (c == '>')
                {
                    sb.Append("&gt;");
                }
                else if ((c >= 0x00 && c <= 0x1F) || (c >= 0x7F && c <= 0x9F))
                {
                    sb.AppendFormat("&#{0};", (int)c);
                }
                else
                {
                    sb.Append(c);
                }
            }
        }
    }
}
