using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace disfr.Doc
{
    public class TmxReader : IAssetReader
    {
        private const int MINIMUM_ENTRIES_PER_THREAD = 400;

        private static readonly string[] _FilterString = { "TMX Translation Memory|*.tmx" };

        public IList<string> FilterString { get { return _FilterString; } }

        public string Name { get { return "TmxReader"; } }

        public int Priority { get { return 7; } }

        public IAssetBundle Read(string filename, int filterindex)
        {
            using (var stream = File.OpenRead(filename))
            {
                return Read(stream, filename, ReaderManager.FriendlyFilename(filename));
            }
        }

        private static readonly XNamespace TMX = XNamespace.Get("http://www.lisa.org/tmx14");

        private static readonly XName XML_LANG = XNamespace.Xml + "lang";

        private static readonly XName LANG = "lang";

        public IAssetBundle Read(Stream stream, string package, string filename)
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

            var tus = tmx.Element(X + "body").Elements(X + "tu").ToList();
            var pool = new StringPool();

            string[] langs = DetectLanguages(tmx);
            if (langs == null) return null;

            var assets = new TmxAsset[langs.Length];
            for (int i = 1; i < langs.Length; i++)
            {
                var asset = new TmxAsset()
                {
                    Package = package,
                    Original = string.Format("{0} - {1}", langs[0], langs[i]),
                    SourceLang = langs[0],
                    TargetLang = langs[i],
                };
                assets[i] = asset;
            }

#if true
            var thread_count = Environment.ProcessorCount - 1;
            if (thread_count * MINIMUM_ENTRIES_PER_THREAD > tus.Count)
            {
                thread_count = tus.Count / MINIMUM_ENTRIES_PER_THREAD;
            }
            if (thread_count < 1) thread_count = 1;
#else
            var thread_count = 1;
#endif
            var threads = new Thread[thread_count];
            var entries_per_thread = (int)Math.Ceiling((double)tus.Count / thread_count);

            // When accessing array_of_array_of_list_of_pairs[x][y][z], 
            // x is a thread index, 
            // y is a language index, 
            // z is a serial number within the thread-language group.
            var array_of_array_of_list_of_pairs = new List<TmxPair>[threads.Length][];

            for (int thread = 0; thread < threads.Length; thread++)
            {
                var array_of_list_of_pairs = new List<TmxPair>[langs.Length];
                for (int i = 0; i < array_of_list_of_pairs.Length; i++) array_of_list_of_pairs[i] = new List<TmxPair>();
                array_of_array_of_list_of_pairs[thread] = array_of_list_of_pairs;

                int min = thread * entries_per_thread;
                int max = Math.Min(min + entries_per_thread, tus.Count); 
                threads[thread] = new Thread((ThreadStart)delegate
                {
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

                    for (int index = min; index < max; index++)
                    {
                        var tu = tus[index];

                        CollectProps(tu_props, tu);
                        CollectNotes(tu_notes, tu);

                        for (int i = 0; i < segs.Length; i++) segs[i] = null;
                        foreach (var tuv in tu.Elements(X + "tuv"))
                        {
                            for (int i = 0; i < langs.Length; i++)
                            {
                                if (Covers(langs[i], Lang(tuv)))
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
                            var source_lang = Lang(segs[0].Parent);

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
                                        TargetLang = Lang(segs[i].Parent),
                                    };
                                    SetProps(assets[i].PropMan, pair, tu_props, pool);
                                    SetProps(assets[i].PropMan, pair, props[0], pool);
                                    SetProps(assets[i].PropMan, pair, props[i], pool);
                                    pair.AddNotes(tu_notes.Concat(notes[0]).Concat(notes[i]));
                                    array_of_list_of_pairs[i].Add(pair);
                                }
                            }
                        }
                    }
                });
                threads[thread].Start();
            }
            for (int thread = 0; thread < threads.Length; thread++) threads[thread].Join();

            for (int i = 1; i < langs.Length; i++)
            {
                var pairs = new TmxPair[array_of_array_of_list_of_pairs.Sum(x => x[i].Count)];
                int index = 0;
                for (int t = 0; t < thread_count; t++)
                {
                    array_of_array_of_list_of_pairs[t][i].CopyTo(pairs, index);
                    index += array_of_array_of_list_of_pairs[t][i].Count;
                }
                assets[i].TransPairs = pairs;
            }

            return new SimpleAssetBundle(assets.Skip(1), filename);
        }

        /// <summary>
        /// Detect the source language and target languages. 
        /// </summary>
        /// <param name="tmx">The &lt;tmx&gt; element.</param>
        /// <returns>An array of language codes, whose element at [0] is the source language.</returns>
        private string[] DetectLanguages(XElement tmx)
        {
            var X = tmx.Name.Namespace;

            var slang = (string)tmx.Element(X + "header").Attribute("srclang");

            var tlang_variants = tmx.Element(X + "body").Elements(X + "tu").Elements(X + "tuv").Select(Lang).Distinct().Where(t => !Covers(slang, t)).ToList();
            var tlangs = tlang_variants.Where(t => !tlang_variants.Any(v => v != t && Covers(v, t))).ToList();
            if (tlangs.Count == 0) return null;

            if (string.Equals(slang, "*all*", StringComparison.OrdinalIgnoreCase))
            {
                if (tlangs.Count <= 1) return null;

                var freq = new int[tlangs.Count];
                for (int i = 0; i < freq.Length; i++)
                {
                    var lang = tlangs[i];
                    freq[i] = tmx.Element(X + "body").Elements(X + "tu").Count(tu => tu.Elements(X + "tuv").Select(Lang).Any(n => Covers(lang, n)));
                }

                var max = freq.Max();
                var candidates = freq.Select((f, i) => (f == max) ? tlangs[i] : null).Where(s => s != null).ToList();
                if (candidates.Count > 1)
                {
                    var en = candidates.Where(n => Covers("en", n)).ToList();
                    if (en.Count > 0) candidates = en;
                }

                slang = candidates[0];
                tlangs.Remove(slang);
            }

            tlangs.Insert(0, slang);
            return tlangs.ToArray();
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
                return StringComparer.OrdinalIgnoreCase.Equals(parent, code);
            }
            else if (parent.Length < code.Length)
            {
                return code[parent.Length] == '-' && StringComparer.OrdinalIgnoreCase.Equals(parent, code.Substring(0, parent.Length));
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


        private static void SetProps(PropertiesManager manager, TmxPair pair, IEnumerable<KeyValuePair<string, string>> props, StringPool pool)
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

        internal readonly PropertiesManager PropMan = new PropertiesManager();

        public IList<PropInfo> Properties { get { return PropMan.Infos.ToList().AsReadOnly(); } }
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
