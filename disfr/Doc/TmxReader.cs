using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

        public IEnumerable<IAsset> Read(Stream stream, string package)
        {
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings()
            {
                CheckCharacters = false,
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null,
                CloseInput = false,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = false,
            }))
            {
                return Read(reader, package);
            }
        }

        private static readonly XNamespace TMX = XNamespace.Get("http://www.lisa.org/tmx14");

        private static readonly XName XML_LANG = XNamespace.Xml + "lang";

        private static readonly XName LANG = "lang";

        private class PairStore
        {
            private readonly Dictionary<string, List<ITransPair>> Store
                = new Dictionary<string, List<ITransPair>>(StringComparer.OrdinalIgnoreCase);

            public void Add(int index, string tlang, ITransPair pair)
            {
                List<ITransPair> list;
                if (!Store.TryGetValue(tlang, out list))
                {
                    list = new List<ITransPair>();
                    Store.Add(tlang, list);
                }
                list.Add(pair);
            }

            public void AddAll(PairStore another)
            {
                List<ITransPair> list;
                foreach (var kvp in another.Store)
                {
                    var tlang = kvp.Key;
                    if (Store.TryGetValue(tlang, out list))
                    {
                        list.AddRange(kvp.Value);
                    }
                    else
                    {
                        list = new List<ITransPair>(kvp.Value);
                        Store.Add(tlang, list);
                    }
                }
            }

            private IEnumerable<string> TargetLanguages;

            public IEnumerable<string> GetTargetLanguages()
            {
                if (TargetLanguages != null) return TargetLanguages;

                var all_tlangs = Store.Keys.ToArray();
                var unique_tlangs = all_tlangs.Where(t => !all_tlangs.Any(v => v != t && Covers(v, t))).ToList();
                unique_tlangs.Sort(StringComparer.OrdinalIgnoreCase);

                foreach (var u in unique_tlangs)
                {
                    foreach (var v in all_tlangs)
                    {
                        if (u != v && Covers(u, v))
                        {
                            Store[u].AddRange(Store[v]);
                            Store.Remove(v);
                        }
                    }
                }

                foreach (var list in Store.Values)
                {
                    list.Sort((x, y) => x == y
                        ? StringComparer.OrdinalIgnoreCase.Compare(x.TargetLang, y.TargetLang)
                        : Comparer<int>.Default.Compare(x.Serial, y.Serial));
                }

                TargetLanguages = unique_tlangs;
                return TargetLanguages;
            }

            public IEnumerable<ITransPair> GetPairs(string tlang)
            {
                return Store[tlang];
            }
        }

        public IEnumerable<IAsset> Read(XmlReader reader, string package)
        {
            XElement header;
            IEnumerable<XElement> tus;
            if (!ParseEnumerateTmx(reader, out header, out tus)) return null;

            var X = header.Name.Namespace;
            var pool = new ConcurrentStringPool();

            var slang = DetectSourceLanguage(header);
            if (slang == null) return null;

            var propman = new PropertiesManager(true);
            var pair_store_pool = new ConcurrentStack<PairStore>();

            Parallel.ForEach(tus,
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                // Local Init
                () =>
                {
                    PairStore pairs;
                    return pair_store_pool.TryPop(out pairs) ? pairs : new PairStore();
                },
                // Body
                (tu, state, index_long, pairs) =>
                {
                    var index = (int)index_long;

                    // collect tu info.
                    var id = (string)tu.Attribute("tuid") ?? "";
                    var tu_props = CollectProps(tu);
                    var tu_notes = CollectNotes(tu);

                    var source_tuv = tu.Elements(X + "tuv").FirstOrDefault(tuv => Covers(slang, Lang(tuv)));
                    if (source_tuv != null)
                    {
                        // collec source tuv info.
                        var source_lang = Lang(source_tuv);
                        var source_inline = GetInline(source_tuv.Element(X + "seg"), X);
                        var source_props = CollectProps(source_tuv);
                        var source_notes = CollectNotes(source_tuv);

                        var tag_pool = NumberTags(source_inline);

                        foreach (var target_tuv in tu.Elements(X + "tuv"))
                        {
                            if (Covers(slang, Lang(target_tuv))) continue;

                            // collet target tuv info.
                            var target_lang = Lang(target_tuv);
                            var target_inline = MatchTags(tag_pool, GetInline(target_tuv.Element(X + "seg"), X));
                            var target_props = CollectProps(target_tuv);
                            var target_notes = CollectNotes(target_tuv);

                            var pair = new TmxPair()
                            {
                                Serial = index + 1,
                                Id = id,
                                Source = source_inline,
                                Target = target_inline,
                                SourceLang = source_lang,
                                TargetLang = target_lang,
                            };

                            SetProps(propman, pair, tu_props, pool);
                            SetProps(propman, pair, source_props, pool);
                            SetProps(propman, pair, target_props, pool);
                            pair.AddNotes(tu_notes, source_notes, target_notes);

                            pairs.Add(index, target_lang, pair);
                        }
                    }
                    return pairs;
                },
                // Local Finally
                pairs =>
                {
                    pair_store_pool.Push(pairs);
                }
            );

            // Merge pairs in pair_store_pool into a single PairStore.
            PairStore all_pairs = null;
            for(;;)
            {
                PairStore pairs;
                if (!pair_store_pool.TryPop(out pairs)) break;
                if (all_pairs == null)
                {
                    all_pairs = pairs;
                }
                else
                {
                    all_pairs.AddAll(pairs);
                }
            }
            if (all_pairs == null)
            {
                return Enumerable.Empty<IAsset>();
            }

            return all_pairs.GetTargetLanguages().Select(tlang => new TmxAsset()
            {
                Package = package,
                Original = string.Format("{0} - {1}", slang, tlang),
                SourceLang = slang,
                TargetLang = tlang,
                TransPairs = all_pairs.GetPairs(tlang),
                Properties = propman.Properties,
            }).ToList();
        }

        /// <summary>
        /// Detect the source language and target languages. 
        /// </summary>
        /// <param name="tmx">The &lt;tmx&gt; element.</param>
        /// <returns>An array of language codes, whose element at [0] is the source language.</returns>
        /// <remarks>
        /// Language codes are defined being case insensitive.
        /// This method takes care of that feature.
        /// </remarks>
        private string DetectSourceLanguage(XElement header)
        {
            var slang = (string)header.Attribute("srclang");
            if (string.Equals(slang, "*all*", StringComparison.OrdinalIgnoreCase)) return null;
            return slang;
        }

        private bool ParseEnumerateTmx(XmlReader reader, out XElement header, out IEnumerable<XElement> tus)
        {
            header = null;
            tus = null;

            try
            {
                reader.MoveToContent();
                if (reader.NodeType != XmlNodeType.Element ||
                    reader.LocalName != "tmx" ||
                    (reader.NamespaceURI != "" &&
                     reader.NamespaceURI != TMX.NamespaceName)) return false;
            }
            catch (XmlException)
            {
                // 
                return false;
            }

            var tmx_uri = reader.NamespaceURI;

            if (!reader.ReadToDescendant("header", tmx_uri)) return false;
            header = XNode.ReadFrom(reader) as XElement;
            if (header == null) return false;

            reader.MoveToContent();
            if (reader.NodeType != XmlNodeType.Element ||
                reader.LocalName != "body" ||
                reader.NamespaceURI != tmx_uri) return false;

            reader.ReadToDescendant("tu", tmx_uri);
            tus = EnumerateTUs(reader, tmx_uri);
            return true;
        }

        private IEnumerable<XElement> EnumerateTUs(XmlReader reader, string tmx_uri)
        {
            for (;;)
            {
                reader.MoveToContent();
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "tu" &&
                    reader.NamespaceURI == tmx_uri)
                {
                    yield return XNode.ReadFrom(reader) as XElement;
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    yield break;
                }
                else
                {
                    throw new XmlException("Not a valid TMX file.");
                }
            }
        }

        /// <summary>
        /// Checks a language code covers another.
        /// </summary>
        /// <param name="parent">A language code that may cover <paramref name="code"/>.</param>
        /// <param name="code">A language code that may be covered by <paramref name="parent"/>.</param>
        /// <returns>
        /// True if <paramref name="parent"/> covers <paramref name="code"/>.  False otherwise.
        /// </returns>
        /// <remarks>
        /// A langauge code covers itself.
        /// A langauge code covers another langauge code if all subtags are included in the other.
        /// Cases are insignificant (and the casing is via so-called ordinal manner.
        /// </remarks>
        /// <example>
        /// "en" covers "en" itself, "en-GB", "en-US", and "en-US-VA", but it doesn't cover "fr".
        /// "en-US" covers "en-US-VA" but doesn't cover "en" or "en-GB".
        /// </example>
        private static bool Covers(string parent, string code)
        {
            if (parent.Length == code.Length)
            {
                return code.Equals(parent, StringComparison.OrdinalIgnoreCase);
            }
            else if (parent.Length < code.Length)
            {
                return code.StartsWith(parent, StringComparison.OrdinalIgnoreCase) && code[parent.Length] == '-';
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the language code of a tuv element.
        /// </summary>
        /// <param name="tuv">tuv element.</param>
        /// <returns>Language code as specified by xml:lang attribute or lang.</returns>
        private static string Lang(XElement tuv)
        {
            return (string)tuv.Attribute(XML_LANG) ?? (string)tuv.Attribute(LANG);
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
                    inline.Add((node as XText).Value);
                }
                else if (node is XElement)
                {
                    var e = (XElement)node;
                    var ns = e.Name.Namespace;
                    var name = e.Name.LocalName;
                    if (ns == X && name == "bpt")
                    {
                        inline.Add(BuildNativeCodeTag(Tag.B, e, true));
                    }
                    else if (ns == X && name == "ept")
                    {
                        inline.Add(BuildNativeCodeTag(Tag.E, e, true));
                    }
                    else if (ns == X && name == "hi")
                    {
                        inline.Add(BuildNativeCodeTag(Tag.B, e, false));
                        BuildInline(inline, e, X);
                        inline.Add(BuildNativeCodeTag(Tag.E, e, false));
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
                        inline.Add(BuildNativeCodeTag(type, e, true));
                    }
                    else if (ns == X && (name == "ph" || name == "ut"))
                    {
                        // Replace a standalone native code element with a standalone inline tag.
                        inline.Add(BuildNativeCodeTag(Tag.S, e, true));
                    }
                    else
                    {
                        // Uunknown element.
                        // OH, I have no good idea how to handle it.  FIXME.
                        if (string.IsNullOrEmpty((string)e) && !e.HasElements)
                        {
                            inline.Add(BuildNativeCodeTag(Tag.S, e, false));
                        }
                        else
                        {
                            inline.Add(BuildNativeCodeTag(Tag.B, e, false));
                            BuildInline(inline, e, X);
                            inline.Add(BuildNativeCodeTag(Tag.E, e, false));
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

        private static Dictionary<InlineTag, int> NumberTags(InlineString source)
        {
            if (!source.HasTags) return null;
            var pool = new Dictionary<InlineTag, int>();
            int n = 0;
            foreach (var tag in source.OfType<InlineTag>())
            {
                pool[tag] = tag.Number = ++n;
            }
            return pool;
        }

        private static InlineString MatchTags(Dictionary<InlineTag, int> pool, InlineString target)
        {
            if (pool != null)
            {
                foreach (var tag in target.OfType<InlineTag>())
                {
                    int m;
                    pool.TryGetValue(tag, out m);
                    tag.Number = m;
                }
            }
            return target;
        }

        /// <summary>
        /// Returns an enumerable containing all properties of a TMX:tu or TMX:tuv element.
        /// </summary>
        /// <param name="elem">Either a TMX:tu or TMX:tuv element.</param>
        /// <returns>An enumerable containing all properties.</returns>
        /// <remarks>
        /// Returned enumerable contains both standard and non-standard (per TMX spec.) properties
        /// expressed both as attribute and as TMX:prop element,
        /// but it excludes attributes defined by XML spec. and those for language codes.
        /// </remarks>
        private static IEnumerable<KeyValuePair<string, string>> CollectProps(XElement elem)
        {
            var X = elem.Name.Namespace;
            foreach (var attr in elem.Attributes())
            {
                if (attr.Name.Namespace == XNamespace.Xml) continue;
                if (attr.Name.Namespace == XNamespace.Xmlns) continue;
                if (attr.Name == LANG) continue;
                yield return new KeyValuePair<string, string>(attr.Name.LocalName, attr.Value);
            }
            foreach (var prop in elem.Elements(X + "prop"))
            {
                var type = (string)prop.Attribute("type");
                if (type == null) continue;
                yield return new KeyValuePair<string, string>(type, prop.Value);
            }
        }

        private static IEnumerable<string> CollectNotes(XElement elem)
        {
            var X = elem.Name.Namespace;
            foreach (var note in elem.Elements(X + "note"))
            {
                yield return note.Value;
            }
        }

        private static void SetProps(PropertiesManager manager, TmxPair pair, IEnumerable<KeyValuePair<string, string>> props, IStringPool pool)
        {
            foreach (var kvp in props)
            {
                manager.Put(ref pair._Props, kvp.Key, pool.Intern(kvp.Value));
            }
        }
    }

    class TmxAsset : IAsset
    {
        public string Package { get; internal set; }

        public string Original { get; internal set; }

        public string SourceLang { get; internal set; }

        public string TargetLang { get; internal set; }

        public IEnumerable<ITransPair> TransPairs { get; internal set; }

        public IEnumerable<ITransPair> AltPairs { get { return Enumerable.Empty<ITransPair>(); } }

        public IList<PropInfo> Properties { get; internal set; }
    }

    class TmxPair : ITransPair
    {
        public int Serial { get; internal set; }

        public string Id { get; internal set; }

        public InlineString Source { get; internal set; }

        public InlineString Target { get; internal set; }

        public string SourceLang { get; internal set; }

        public string TargetLang { get; internal set; }

        private HashSet<string> _Notes = null;

        public IEnumerable<string> Notes { get { return _Notes; } }

        internal void AddNotes(params IEnumerable<string>[] noteses)
        {
            if (_Notes == null) _Notes = new HashSet<string>();
            foreach (var notes in noteses)
            {
                _Notes.UnionWith(notes);
            }
        }

        internal string[] _Props = null;

        public string this[int key]
        {
            get
            {
                return (key < _Props?.Length) ? _Props[key] : null;
            }
        }
    }
}
