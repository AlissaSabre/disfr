using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace disfr.Doc
{
    public class TmxReader : IAssetReader
    {
        private static readonly string[] _FilterString = { "TMX Translation Memory|*.tmx" };

        public IList<string> FilterString { get { return _FilterString; } }

        public string Name { get { return "TmxReader"; } }

        public int Priority { get { return 7; } }

        public IEnumerable<IAsset> Read(string filename, int filterindex)
        {
            using (var stream = File.OpenRead(filename))
            {
                return Read(stream, filename);
            }
        }

        private static readonly XNamespace TMX = XNamespace.Get("http://www.lisa.org/tmx14");

        private static readonly XName XML_LANG = XNamespace.Xml + "lang";

        public IEnumerable<IAsset> Read(Stream stream, string package)
        {
            XElement tmx;
            try
            {
                tmx = XElement.Load(stream, LoadOptions.PreserveWhitespace);
            }
            catch (Exception)
            {
                return null;
            }

            var X = tmx.Name.Namespace;
            if (tmx.Name.LocalName != "tmx" || (X != TMX && X != XNamespace.None))
            {
                return null;
            }

            var tus = tmx.Element(X + "body").Elements(X + "tu");

            string[] langs;
            {
                var slang = (string)tmx.Element(X + "header").Attribute("srclang");

                var tlang_variants = tus.Elements(X + "tuv").Attributes(XML_LANG).Select(a => (string)a).Distinct().Where(t => !Covers(slang, t)).ToArray();
                var tlangs = tlang_variants.Where(t => !tlang_variants.Any(v => v != t && Covers(v, t))).ToArray();
                if (tlangs.Length == 0) return null;

                langs = new[] { slang }.Concat(tlangs).ToArray();
            }

            var lists = new List<TmxPair>[langs.Length];
            for (int i = 0; i < lists.Length; i++) lists[i] = new List<TmxPair>();

            var segs = new XElement[langs.Length];
            var props = new List<KeyValuePair<string, string>>[langs.Length];
            var notes = new List<string>[langs.Length];
            for (int i = 0; i < segs.Length; i++)
            {
                segs[i] = null;
                props[i] = new List<KeyValuePair<string, string>>();
                notes[i] = new List<string>();
            }

            var tu_props = new List<KeyValuePair<string, string>>();
            var tu_notes = new List<string>();
            var tag_pool = new Dictionary<InlineTag, int>();

            foreach (var tu in tus)
            {
                CollectProps(tu_props, tu);
                CollectNotes(tu_notes, tu);

                for (int i = 0; i < segs.Length; i++) segs[i] = null;
                foreach (var tuv in tu.Elements(X + "tuv"))
                {
                    for (int i = 0; i < langs.Length; i++)
                    {
                        if (Covers(langs[i], (string)tuv.Attribute(XML_LANG)))
                        {
                            segs[i] = tuv.Element(X + "seg");
                            CollectProps(props[i], tuv);
                            CollectNotes(notes[i], tuv);
                            break;
                        }
                    }
                }

                if (segs[0] != null)
                {
                    var id = (string)tu.Attribute("tuid") ?? "";
                    var source = NumberTags(tag_pool, GetInline(segs[0], X));
                    var source_lang = (string)segs[0].Attribute(XML_LANG) ?? langs[0];

                    for (int i = 1; i < segs.Length; i++)
                    {
                        if (segs[i] != null)
                        {
                            var pair = new TmxPair()
                            {
                                Serial = 0, // XXX
                                Id = id,
                                Source = source,
                                Target = MatchTags(tag_pool, GetInline(segs[i], X)),
                                SourceLang = source_lang,
                                TargetLang = (string)segs[i].Attribute(XML_LANG) ?? langs[i]
                            };
                            pair.SetProps(tu_props.Concat(props[0]).Concat(props[i]));
                            pair.AddNotes(tu_notes.Concat(notes[0]).Concat(notes[i]));
                            lists[i].Add(pair);
                        }
                    }
                }
            }

            var assets = new IAsset[langs.Length - 1];
            for (int i = 1; i < langs.Length; i++)
            {
                var asset = new TmxAsset()
                {
                    Package = package,
                    Original = string.Format("{0} - {1}", langs[0], langs[i]),
                    SourceLang = langs[0],
                    TargetLang = langs[i],
                    TransPairs = lists[i],
                };
                assets[i - 1] = asset;
            }

            return assets;
        }

        /// <summary>
        /// Checks a language code covers another.
        /// </summary>
        /// <param name="x">A language code.</param>
        /// <param name="y">A language code.</param>
        /// <returns>
        /// True if <paramref name="x"/> covers <paramref name="y"/>.  False otherwise.
        /// A langauge code covers itself.
        /// A langauge code covers another langauge code if all subtags are included in the other.
        /// Cases are insignificant (and the casing is via so-called ordinal manner.
        /// </returns>
        /// <example>
        /// "en" covers "en" itself, "en-GB", "en-US", and "en-US-VA", but it doesn't cover "fr".
        /// "en-US" covers "en-US-VA" but doesn't cover "en" or "en-GB".
        /// </example>
        private static bool Covers(string x, string y)
        {
            if (x.Length == y.Length)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(x, y);
            }
            else if (x.Length < y.Length)
            {
                return y[x.Length] == '-' && StringComparer.OrdinalIgnoreCase.Equals(x, y.Substring(0, x.Length));
            }
            else
            {
                return false;
            }
        }

        private static InlineString GetInline(XElement elem, XNamespace X)
        {
            var inline = new InlineString();
            BuildInline(inline, elem, X);
            return inline;
        }

        private static void BuildInline(InlineString inline, XElement elem, XNamespace X)
        {
            foreach (var node in elem.Nodes())
            {
                if (node is XText)
                {
                    inline.Append((node as XText).Value);
                }
                else if (node is XElement)
                {
                    var e = (XElement)node;
                    var ns = e.Name.Namespace;
                    var name = e.Name.LocalName;
                    if (ns == X && name == "bpt")
                    {
                        inline.Append(BuildNativeCodeTag(Tag.B, e, true));
                    }
                    else if (ns == X && name == "ept")
                    {
                        inline.Append(BuildNativeCodeTag(Tag.E, e, true));
                    }
                    else if (ns == X && name == "hi")
                    {
                        inline.Append(BuildNativeCodeTag(Tag.B, e, false));
                        BuildInline(inline, e, X);
                        inline.Append(BuildNativeCodeTag(Tag.E, e, false));
                    }
                    else if (ns == X && name == "it")
                    {
                        Tag type;
                        switch ((string)e.Attribute("pos"))
                        {
                            case "open": type = Tag.B; break;
                            case "close": type = Tag.E; break;
                            default: type = Tag.S; break;
                        }
                        inline.Append(BuildNativeCodeTag(type, e, true));
                    }
                    else if (ns == X && (name == "ph" || name == "ut"))
                    {
                        // Replace a standalone native code element with a standalone inline tag.
                        inline.Append(BuildNativeCodeTag(Tag.S, e, true));
                    }
                    else
                    {
                        // Uunknown element.
                        // OH, I have no good idea how to handle it.  FIXME.
                        if (string.IsNullOrEmpty((string)e) && !e.HasElements)
                        {
                            inline.Append(BuildNativeCodeTag(Tag.S, e, false));
                        }
                        else
                        {
                            inline.Append(BuildNativeCodeTag(Tag.B, e, false));
                            BuildInline(inline, e, X);
                            inline.Append(BuildNativeCodeTag(Tag.E, e, false));
                        }
                    }
                }
                else
                {
                    // Silently discard any other nodes; e.g., comment or pi. 
                }
            }
        }

        private static InlineTag BuildNativeCodeTag(Tag type, XElement elem, bool has_code)
        {
            return new InlineTag(
                type: type,
                id: (string)elem.Attribute("x") ?? "*",
                rid: (string)elem.Attribute("i") ?? "*",
                name: elem.Name.LocalName,
                ctype: (string)elem.Attribute("type"),
                display: null,
                code: has_code ? elem.Value : null);
        }

        private static InlineString NumberTags(Dictionary<InlineTag, int> pool, InlineString source)
        {
            pool.Clear();
            int n = 0;
            foreach (var tag in source.OfType<InlineTag>())
            {
                pool[tag] = tag.Number = ++n;
            }
            return source;
        }

        private static InlineString MatchTags(Dictionary<InlineTag, int> pool, InlineString target)
        {
            foreach (var tag in target.OfType<InlineTag>())
            {
                int m;
                pool.TryGetValue(tag, out m);
                tag.Number = m;
            }
            return target;
        }

        // <paramref name="elem"/> should either be TMX:tu or TMX:tuv.
        private static void CollectProps(List<KeyValuePair<string, string>> props, XElement elem)
        {
            var X = elem.Name.Namespace;
            props.Clear();
            props.AddRange(elem.Attributes().Where(a => a.Name.Namespace != XNamespace.Xml).Select(a => new KeyValuePair<string, string>(a.Name.LocalName, (string)a)));
            props.AddRange(elem.Elements(X + "prop").Select(p => new KeyValuePair<string, string>((string)p.Attribute("type"), (string)p)));
        }

        private static void CollectNotes(List<string> notes, XElement elem)
        {
            var X = elem.Name.Namespace;
            notes.Clear();
            notes.AddRange(elem.Elements(X + "note").Select(n => (string)n));
        }
    }

    class TmxAsset : IAsset
    {
        public string Package { get; set; }

        public string Original { get; set; }

        public string SourceLang { get; set; }

        public string TargetLang { get; set; }

        public IEnumerable<ITransPair> TransPairs { get; set; }

        public IEnumerable<ITransPair> AltPairs { get { return Enumerable.Empty<ITransPair>(); } }
    }

    class TmxPair : ITransPair
    {
        public int Serial { get; set; }

        public string Id { get; set; }

        public InlineString Source { get; set; }

        public InlineString Target { get; set; }

        public string SourceLang { get; set; }

        public string TargetLang { get; set; }

        private HashSet<string> _Notes = null;

        public IEnumerable<string> Notes { get { return _Notes; } }

        public void AddNotes(IEnumerable<string> notes) { (_Notes ?? (_Notes = new HashSet<string>())).UnionWith(notes); }

        private Dictionary<string, string> _Props = null;

        private static readonly Dictionary<string, string> EmptyProps = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> Props { get { return _Props ?? EmptyProps; } }

        public void SetProps(IEnumerable<KeyValuePair<string, string>> props)
        {
            if (props.Any())
            {
                _Props = props.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }
    }
}
