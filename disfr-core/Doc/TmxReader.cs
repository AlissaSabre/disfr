using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        public IAssetBundle Read(string filename, int filterindex)
        {
            return LoaderAssetBundle.Create(
                ReaderManager.FriendlyFilename(filename),
                () => ReadAssets(filename, filterindex));
        }

        private IEnumerable<IAsset> ReadAssets(string filename, int filterindex)
        {
            using (var stream = FileUtils.OpenRead(filename))
            {
                return ReadAssets(stream, filename);
            }
        }

        public IAssetBundle Read(Stream stream, string package, string filename)
        {
            var assets = ReadAssets(stream, package);
            if (assets == null) return null;
            return new SimpleAssetBundle(assets, filename);
        }

        private IEnumerable<IAsset> ReadAssets(Stream stream, string package)
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
                return ReadAssets(reader, package);
            }
        }

        /// <summary>The TMX namespace.</summary>
        private static readonly XNamespace TMX = XNamespace.Get("http://www.lisa.org/tmx14");

        /// <summary>The xml:lang attribute name.</summary>
        private static readonly XName XML_LANG = XNamespace.Xml + "lang";

        /// <summary>Alternative xml:lang attribute name defined by TMX spec.</summary>
        private static readonly XName TMXLANG = "lang";

        public IAssetBundle Read(XmlReader reader, string package, string filename)
        {
            var assets = ReadAssets(reader, package);
            if (assets == null) return null;
            return new SimpleAssetBundle(assets, filename);
        }

        private IEnumerable<IAsset> ReadAssets(XmlReader reader, string package)
        {
            XElement header;
            IEnumerable<XElement> tus;
            if (!ParseEnumerateTmx(reader, out header, out tus)) return null;

            var X = header.Name.Namespace;

            var locker = new object();
            var all_tus = new TuEntryList();

            AltParallel.ForEach(tus,
                // Local Init
                () => new TuEntryList(),
                // Body
                (tu, state_UNUSED, index_long, local_tus) =>
                {
                    var index = (int)index_long;
                    local_tus.Add(new TuEntry(tu, index));
                    return local_tus;
                },
                // Local Finally
                (local_tus) =>
                {
                    lock (locker)
                    {
                        all_tus.AddRange(local_tus);
                    }
                }
            );

            all_tus.Sort();

            var header_source_language = LangCode.NoAll((string)header.Attribute("srclang"));
            if (header_source_language != null)
            {
                all_tus.SourceLanguages.Add(header_source_language);
            }
            var slangs = all_tus.SourceLanguages;
            var tlangs = all_tus.TargetLanguages;
            if (slangs.Count == 0) slangs = tlangs;

            var assets = new List<IAsset>(slangs.Count * tlangs.Count);
            AltParallel.ForEach(LangPair.Enumerate(slangs, tlangs),
                (lang_pair) => 
                {
                    var asset = CreateAsset(package, lang_pair, all_tus.Tus);
                    if (asset != null)
                    {
                        lock (locker)
                        {
                            assets.Add(asset);
                        }
                    }
                }
            );
            assets.Sort((x, y) =>
            {
                var s = string.Compare(x.SourceLang, y.SourceLang, StringComparison.OrdinalIgnoreCase);
                if (s != 0) return s;
                var t = string.Compare(x.TargetLang, y.TargetLang, StringComparison.OrdinalIgnoreCase);
                return t;
            });
            return assets;
        }

        private IAsset CreateAsset(string package, LangPair lang_pair, IEnumerable<TuEntry> all_tus)
        {
            var slang = lang_pair.Source;
            var tlang = lang_pair.Target;
            var propman = new PropertiesManager(false);

            var pairs = new List<ITransPair>();
            foreach (var tu in all_tus)
            {
                var pair = tu.GetTransPair(slang, tlang, propman);
                if (pair != null)
                {
                    pairs.Add(pair);
                }
            }
            if (pairs.Count == 0) return null;

            return new TmxAsset
            {
                Package = package,
                Original = string.Format("{0} - {1}", slang, tlang),
                SourceLang = slang,
                TargetLang = tlang,
                TransPairs = pairs,
                Properties = propman.Properties,
            };
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
            for (; ; )
            {
                reader.MoveToContent();
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "tu" &&
                    reader.NamespaceURI == tmx_uri)
                {
                    yield return XNode.ReadFrom(reader) as XElement;
                }
                else if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == "body" &&
                    reader.NamespaceURI == tmx_uri)
                {
                    // We could further check that the next content node is </tmx> with no content nodes followed,
                    // though I have a strong feeling it is too much for our purpose.
                    yield break;
                }
                else
                {
                    // It is unclear from the documentation 
                    // whether an XmlReader object returned by XmlReader.Create()
                    // *always* implements IXmlLineInfo and has line info,
                    // so we need a fallback.
                    var info = reader as IXmlLineInfo;
                    if (info?.HasLineInfo() == true)
                    {
                        throw new XmlException("Not a valid TMX file.", null, info.LineNumber, info.LinePosition);
                    }
                    else
                    {
                        throw new XmlException("Not a valid TMX file.");
                    }
                }
            }
        }

        /// <summary>A pair of source and target language codes.</summary>
        private class LangPair
        {
            /// <summary>The source language code.</summary>
            public readonly string Source;

            /// <summary>The target language code.</summary>
            public readonly string Target;

            /// <summary>Creates an instance.</summary>
            /// <param name="source"></param>
            /// <param name="target"></param>
            public LangPair(string source, string target)
            {
                Source = source;
                Target = target;
            }

            /// <summary>Enumerates all possible combinations of source and target languages.</summary>
            /// <param name="slangs">Source languages.</param>
            /// <param name="tlangs">Target languages.</param>
            /// <returns>Enuemrated language pairs of possible source and target languages.</returns>
            /// <remarks>Language pairs of equivalent source and target languages are excluded.</remarks>
            public static IEnumerable<LangPair> Enumerate(IEnumerable<string> slangs, IEnumerable<string> tlangs)
            {
                var tarray = tlangs.ToArray();
                foreach (var s in slangs)
                {
                    foreach (var t in tarray)
                    {
                        if (!LangCode.Equals(s, t))
                        {
                            yield return new LangPair(s, t);
                        }
                    }
                }
            }
        }

        /// <summary>Provides several static methods to handle language codes.</summary>
        private static class LangCode
        {
            /// <summary>Checks a language code covers another.</summary>
            /// <param name="parent">A language code that may cover <paramref name="code"/>.</param>
            /// <param name="code">A language code that <paramref name="parent"/> may cover.</param>
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
            public static bool Covers(string parent, string code)
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

            /// <summary>Checks if two language codes are equivalent.</summary>
            /// <param name="x">A language code.</param>
            /// <param name="y">A language code.</param>
            /// <returns>True if the two language codes are equivalent.</returns>
            public static bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>Turns "*all*" (to mean "all/any languages" in TMX) to null.</summary>
            /// <param name="lang">A TMX language code.</param>
            /// <returns>Null if <paramref name="lang"/> equals "*all*".  Otherwise, <paramref name="lang"/>.</returns>
            public static string NoAll(string lang)
            {
                return string.Equals(lang, "*all*", StringComparison.OrdinalIgnoreCase) ? null : lang;
            }
        }

        /// <summary>A list of TU entries.</summary>
        private class TuEntryList
        {
            /// <summary>Raw list.</summary>
            public readonly List<TuEntry> Tus = new List<TuEntry>();

            /// <summary>The set of source languages taken from the TUs.</summary>
            public readonly HashSet<string> SourceLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            /// <summary>The set of target languages taken from the TUs.</summary>
            public readonly HashSet<string> TargetLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            /// <summary>Adds a TU.</summary>
            /// <param name="tu">A TU.</param>
            public void Add(TuEntry tu)
            {
                Tus.Add(tu);
                var slang = LangCode.NoAll(tu.SourceLang);
                if (slang != null) SourceLanguages.Add(slang);
                foreach (var tlang in tu.Languages)
                {
                    if (!LangCode.Equals(tlang, slang)) TargetLanguages.Add(tlang);
                }
            }

            /// <summary>Adds all TUs from another TuEntryList.</summary>
            /// <param name="another">Another TuEntryList.</param>
            public void AddRange(TuEntryList another)
            {
                Tus.AddRange(another.Tus);
                SourceLanguages.UnionWith(another.SourceLanguages);
                TargetLanguages.UnionWith(another.TargetLanguages);
            }

            /// <summary>Sort the TUs based on their indexes.</summary>
            public void Sort()
            {
                // TU index is a zero or positive integer (effectively 31 bit)
                // so we can simply make a subtraction to produce a comparison value
                // without worrying about overflow.
                Tus.Sort((x, y) => x.Index - y.Index);
            }
        }

        /// <summary>An object that corresponds to a single TMX:tu element.</summary>
        private class TuEntry
        {
            /// <summary>Information taken from a single TMX:tuv element.</summary>
            private class TuvEntry
            {
                public string Lang;

                public InlineString Inline;

                public IEnumerable<KeyValuePair<string, string>> Props;

                public IEnumerable<string> Notes;
            }

            /// <summary>Index (base 0) of this TU in the TMX file.
            /// 
            /// </summary>
            public readonly int Index;

            /// <summary>The value of tuid attribute.</summary>
            private readonly string Id;


            private readonly IEnumerable<KeyValuePair<string, string>> TuProps;

            private readonly IEnumerable<string> TuNotes;

            private readonly List<TuvEntry> Tuvs = new List<TuvEntry>();

            public TuEntry(XElement tu, int index)
            {
                var X = tu.Name.Namespace;
                Index = index;

                // collect tu info.
                Id = (string)tu.Attribute("tuid") ?? "";
                SourceLang = (string)tu.Attribute("srclang");
                TuProps = CollectProps(tu);
                TuNotes = CollectNotes(tu);

                // collect tuv's
                TagPool pool = new TagPool();
                foreach (var tuv in tu.Elements(X + "tuv"))
                {
                    // collet target tuv info.
                    Tuvs.Add(new TuvEntry
                    {
                        Lang = GetLanguage(tuv),
                        Inline = pool.AssignTagNumbers(GetInline(tuv.Element(X + "seg"))),
                        Props = CollectProps(tuv),
                        Notes = CollectNotes(tuv),
                    });
                }
            }

            public string SourceLang { get; }

            public IEnumerable<string> Languages { get { return Tuvs.Select(tuv => tuv.Lang); } }

            public TmxPair GetTransPair(string source_lang, string target_lang, PropertiesManager propman)
            {
                if (SourceLang != null && !LangCode.Covers(source_lang, SourceLang)) return null;

                var source_tuv = GetTuv(source_lang);
                if (source_tuv == null) return null;
                var target_tuv = GetTuv(target_lang);
                if (target_tuv == null) return null;

                var pair = new TmxPair
                {
                    Serial = Index + 1,
                    Id = Id,
                    Source = source_tuv.Inline,
                    Target = target_tuv.Inline,
                    SourceLang = source_lang, // or source_tuv.Lang?  FIXME.
                    TargetLang = target_lang, // or target_tuv.Lang?  FIXME.
                };
                pair.SetProps(propman, TuProps, source_tuv.Props, target_tuv.Props);
                pair.SetNotes(TuNotes, source_tuv.Notes, target_tuv.Notes);
                return pair;
            }

            /// <summary>Gets a TuvEntry whose language matches a given language.</summary>
            /// <param name="lang">A language code.</param>
            /// <returns>A TuvEntry or null if none found.</returns>
            /// <remarks>This method first looks for a same language, then for any language that is covered by <paramref name="lang"/>.</remarks>
            private TuvEntry GetTuv(string lang)
            {
                foreach (var tuv in Tuvs)
                {
                    if (LangCode.Equals(lang, tuv.Lang)) return tuv;
                }
                foreach (var tuv in Tuvs)
                {
                    if (LangCode.Covers(lang, tuv.Lang)) return tuv;
                }
                return null;
            }

            /// <summary>Get the language code of a tuv element.</summary>
            /// <param name="tuv">tuv element.</param>
            /// <returns>Language code as specified by xml:lang attribute or lang, or null if neither is specified.</returns>
            private static string GetLanguage(XElement tuv)
            {
                return (string)(tuv.Attribute(XML_LANG) ?? tuv.Attribute(TMXLANG));
            }

            /// <summary>Returns an enumerable containing all properties of a TMX:tu or TMX:tuv element.</summary>
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
                var properties = new List<KeyValuePair<string, string>>();
                foreach (var attr in elem.Attributes())
                {
                    if (attr.Name.Namespace == XNamespace.Xml) continue;
                    if (attr.Name.Namespace == XNamespace.Xmlns) continue;
                    if (attr.Name == TMXLANG) continue;
                    properties.Add(new KeyValuePair<string, string>(attr.Name.LocalName, attr.Value));
                }
                foreach (var prop in elem.Elements(X + "prop"))
                {
                    var type = (string)prop.Attribute("type");
                    if (type == null) continue;
                    properties.Add(new KeyValuePair<string, string>(type, prop.Value));
                }
                return properties;
            }

            /// <summary>Returns an enumerable containing notes of a TMX:tu or TMX:tuv element.</summary>
            /// <param name="elem">Either a TMX:tu or TMX:tuv element.</param>
            /// <returns>An enumerable containing notes.</returns>
            private static IEnumerable<string> CollectNotes(XElement elem)
            {
                var X = elem.Name.Namespace;
                var notes = elem.Elements(X + "note").Select(n => n.Value).ToList();
                return notes.Count > 0 ? notes : Enumerable.Empty<string>();
            }

            /// <summary>Returns contents of a TMX:seg element as an InlineString.</summary>
            /// <param name="elem">A seg element.</param>
            /// <returns>An inline string.</returns>
            private static InlineString GetInline(XElement elem)
            {
                var inline = new InlineBuilder();
                BuildInline(inline, elem, elem.Name.Namespace);
                return inline.ToInlineString();
            }

            /// <summary>Recursively builds an inline string from an XML element.</summary>
            /// <param name="inline">An inline builder to append partial contents.</param>
            /// <param name="elem">An XML element whose contnets are appended to <paramref name="inline"/> to build an inline string.</param>
            /// <param name="X">The namespace the TMX elements are in.</param>
            private static void BuildInline(InlineBuilder inline, XElement elem, XNamespace X)
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

            /// <summary>Builds a corresponding native code tag from a TMX inline element.</summary>
            /// <param name="type">Tag type.</param>
            /// <param name="elem">TMX element.</param>
            /// <param name="has_code">Whether the contents of <paramref name="elem"/> is a native code.</param>
            /// <returns>An inline tag.</returns>
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
        }

        private class TagPool
        {
            private Dictionary<InlineTag, int> Dict = null;

            public InlineString AssignTagNumbers(InlineString inline)
            {
                foreach (var tag in inline.Tags)
                {
                    if (Dict == null) Dict = new Dictionary<InlineTag, int>();
                    int number;
                    if (!Dict.TryGetValue(tag, out number))
                    {
                        number = Dict.Count + 1;
                        Dict.Add(tag.Clone(), number);
                    }
                    tag.Number = number;
                }
                return inline;
            }
        }

        private class TmxAsset : IAsset
        {
            public string Package { get; internal set; }

            public string Original { get; internal set; }

            public string SourceLang { get; internal set; }

            public string TargetLang { get; internal set; }

            public IEnumerable<ITransPair> TransPairs { get; internal set; }

            public IEnumerable<ITransPair> AltPairs { get { return Enumerable.Empty<ITransPair>(); } }

            public IList<PropInfo> Properties { get; internal set; }
        }

        private class TmxPair : ITransPair
        {
            public int Serial { get; internal set; }

            public string Id { get; internal set; }

            public InlineString Source { get; internal set; }

            public InlineString Target { get; internal set; }

            public string SourceLang { get; internal set; }

            public string TargetLang { get; internal set; }

            public IEnumerable<string> Notes { get; internal set; }

            private string[] _Props = null;

            public string this[int key] => (key < _Props?.Length) ? _Props[key] : null;

            internal void SetNotes(params IEnumerable<string>[] noteses)
            {
                // In practice,
                // a single tu has no more than a few comments,
                // and a primitive approach works better.
                // It also preserves the order of notes
                // in their first appearances in the file.
                var list = new List<string>();
                foreach (var notes in noteses)
                {
                    foreach (var note in notes)
                    {
                        if (!list.Contains(note))
                        {
                            list.Add(note);
                        }
                    }
                }
                if (list.Count > 0)
                {
                    var array = new string[list.Count];
                    list.CopyTo(array);
                    Notes = array;
                }
            }

            internal void SetProps(PropertiesManager manager, params IEnumerable<KeyValuePair<string, string>>[] propses)
            {
                foreach (var props in propses)
                {
                    foreach (var kvp in props)
                    {
                        manager.Put(ref _Props, kvp.Key, kvp.Value);
                    }
                }
            }
        }

        static class AltParallel
        {
#if false

        // System.Parallel versions.  Doesn't work efficiently for this program.

        public static void ForEach<TSource, TLocal>(
            IEnumerable<TSource> source,
            Func<TLocal> initial,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> final)
        {
            Parallel.ForEach(source, initial, body, final);
        }

        public static void ForEach<TSource>(
            IEnumerable<TSource> source,
            Action<TSource> body)
        {
            Parallel.ForEach(source, body);
        }

#elif false

        // Sequential versions.  Primarily for debugging.

        private const int Slice = 6000; // An arbitrary number

        public static void ForEach<TSource, TLocal>(
            IEnumerable<TSource> source, 
            Func<TLocal> initial,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> final)
        {
            TLocal local = initial();
            long index = 0;
            int slicer = 0;
            foreach (TSource item in source)
            {
                if (++slicer > Slice)
                {
                    final(local);
                    local = initial();
                    slicer = 0;
                }
                local = body(item, null, index++, local);
            }
            final(local);
        }

        public static void ForEach<TSource>(
            IEnumerable<TSource> source,
            Action<TSource> body)
        {
            foreach (TSource item in source)
            {
                body(item);
            }
        }

#elif true

            // Custom versions.

            private static readonly int QueueLength = Environment.ProcessorCount;

            private static readonly int QueueWatermark = QueueLength / 2;

            private static readonly int MaxWorkerThreads = Environment.ProcessorCount;

            private struct ItemWithIndex<T>
            {
                public readonly T Item;

                public readonly long Index;

                public ItemWithIndex(T item, long index)
                {
                    Item = item;
                    Index = index;
                }
            }

            public static void ForEach<TSource, TLocal>(
                IEnumerable<TSource> source,
                Func<TLocal> initial,
                Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
                Action<TLocal> final)
            {
                // we don't care about the consumption order of BlockingCollection
                // (we anyway sort the result after all)
                // and ConcurrentStack seemed fastest in my experiments
                using (var queue = new BlockingCollection<ItemWithIndex<TSource>>(
                    new ConcurrentStack<ItemWithIndex<TSource>>(), QueueLength))
                using (var signal = new AutoResetEvent(false))
                {
                    // define the worker tasks/threads.
                    var tasks = new List<Task>();
                    Action work = () =>
                    {
                        TLocal local = initial();
                        signal.Set();
                        foreach (var item_with_index in queue.GetConsumingEnumerable())
                        {
                            local = body(item_with_index.Item, null, item_with_index.Index, local);
                        }
                        final(local);
                    };

                    // feed items into the queue, starting new workers as needed.
                    long index = 0;
                    foreach (TSource item in source)
                    {
                        queue.Add(new ItemWithIndex<TSource>(item, index++));
                        if (queue.Count >= QueueWatermark && tasks.Count < MaxWorkerThreads)
                        {
                            tasks.Add(Task.Run(work));
                            signal.WaitOne();
                        }
                    }
                    queue.CompleteAdding();

                    // wait for works to terminate.
                    if (tasks.Count > 0)
                    {
                        Task.WaitAll(tasks.ToArray());
                    }
                    else
                    {
                        // source has less than QueueWatermark items,
                        // and no worker was run.
                        // Let the current thread do the job.
                        work();
                    }
                    Console.WriteLine($"ForEach: Tasks = {tasks.Count}");
                }
            }

            public static void ForEach<TSource>(
                IEnumerable<TSource> source,
                Action<TSource> body)
            {
                ForEach<TSource, object>(
                    source,
                    () => null,
                    (item, state, index, local) => { body(item); return null; },
                    (local) => { });
            }

#endif
        }
    }
}
