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

        private static readonly XNamespace TMX = XNamespace.Get("http://www.lisa.org/tmx14");

        private static readonly XName XML_LANG = XNamespace.Xml + "lang";

        private static readonly XName LANG = "lang";

        public IAssetBundle Read(XmlReader reader, string package, string filename)
        {
            var assets = ReadAssets(reader, package);
            if (assets == null) return null;
            return new SimpleAssetBundle(assets, filename);
        }

        class LanguagePair
        {
            public readonly string Source;
            public readonly string Target;

            private LanguagePair(string source, string target)
            {
                Source = source;
                Target = target;
            }

            public static IEnumerable<LanguagePair> Enumerate(string[] slangs, string[] tlangs)
            {
                foreach (var s in slangs)
                {
                    foreach (var t in tlangs)
                    {
                        if (!LangCode.Equals(s, t))
                        {
                            yield return new LanguagePair(s, t);
                        }
                    }
                }
            }
        }

        private IEnumerable<IAsset> ReadAssets(XmlReader reader, string package)
        {
            XElement header;
            IEnumerable<XElement> tus;
            if (!ParseEnumerateTmx(reader, out header, out tus)) return null;

            var X = header.Name.Namespace;

            //if (slang == null) return null;

            var locker = new object();
            var all_tus = new List<TuEntry>();

            Local.ForEach(tus,
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 },
                // Local Init
                () => new List<TuEntry>(),
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

            var tlangs = DetectTargetLanguages(all_tus);
            var slangs = DetectSourceLanguages(all_tus, header);
            if (slangs.Length == 0) slangs = tlangs;

            var assets = new List<IAsset>(slangs.Length * tlangs.Length);
            Local.ForEach(LanguagePair.Enumerate(slangs, tlangs),
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 },
                (lang_pair) => 
                {
                    var asset = CreateAsset(package, lang_pair, all_tus);
                    if (asset != null)
                    {
                        lock (locker)
                        {
                            assets.Add(asset);
                        }
                    }
                }
            );

            return assets;
        }

        private IAsset CreateAsset(string package, LanguagePair lang_pair, List<TuEntry> all_tus)
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

        /// <summary>Detects one or more source languages of a TMX file.</summary>
        /// <param name="header">The TMX:header element.</param>
        /// <param name="tus">List of TU entries.</param>
        /// <returns>List of source language code.</returns>
        /// <remarks>The return value may be an empty array but won't be null.</remarks>
        private string[] DetectSourceLanguages(List<TuEntry> tus, XElement header)
        {
            var langs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tu in tus)
            {
                if (tu.SourceLang != null) langs.Add(tu.SourceLang);
            }

            var header_slang = LangCode.NoAll((string)header.Attribute("srclang"));
            if (header_slang != null) langs.Add(header_slang);

            // We should reduce the languages by eliminating more specific language code covered by a generic language code in langs.  FIXME.
            return langs.ToArray();
        }

        private string[] DetectTargetLanguages(List<TuEntry> tus)
        {
            var langs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tu in tus)
            {
                foreach (var lang in tu.Languages)
                {
                    if (!LangCode.Equals(lang, tu.SourceLang)) langs.Add(lang);
                }
            }
            return langs.ToArray();
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
            for (; ; )
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
    }

    static class LangCode
    {
        /// <summary>Checks a language code covers another.</summary>
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

    class TuEntry
    {
        private static readonly XName XML_LANG = XNamespace.Xml + "lang";

        private static readonly XName TMXLANG = "lang"; // Alternative xml:lang attribute defined by TMX spec.

        private class TuvEntry
        {
            public string Lang;

            public InlineString Inline;

            public IEnumerable<KeyValuePair<string, string>> Props;

            public IEnumerable<string> Notes;
        }

        private int Index;

        private string Id;

        private IEnumerable<KeyValuePair<string, string>> TuProps;

        private IEnumerable<string> TuNotes;

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
            foreach (var tuv in tu.Elements(X + "tuv"))
            {
                // collet target tuv info.
                Tuvs.Add(new TuvEntry
                {
                    Lang = GetLanguage(tuv),
                    Inline = GetInline(tuv.Element(X + "seg")),
                    Props = CollectProps(tuv),
                    Notes = CollectNotes(tuv),
                });
            }
        }

        public string SourceLang { get; }

        public IEnumerable<string> Languages { get { return Tuvs.Select(tuv => tuv.Lang); } }

        public TmxPair GetTransPair(string source_lang, string target_lang, PropertiesManager propman)
        {
            if (SourceLang != null && SourceLang != source_lang) return null;

            var source_tuv = GetTuv(source_lang);
            if (source_tuv == null) return null;
            var target_tuv = GetTuv(target_lang);
            if (target_tuv == null) return null;

            TagPool pool;
            var source_inline = NumberTags(source_tuv.Inline, out pool);
            var target_inline = MatchTags(target_tuv.Inline, pool);

            var pair = new TmxPair
            {
                Serial = Index + 1,
                Id = Id,
                Source = source_inline,
                Target = target_inline,
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

        private class TagPool : Dictionary<InlineTag, int>
        {
        }

        /// <summary>Assigns local numbers to tags in an inline string, producing a tag pool.</summary>
        /// <param name="source">An inline string.</param>
        /// <param name="pool">The produced tag pool.</param>
        /// <returns>An inline string with tag numbers assigned.</returns>
        /// <remarks>
        /// This method never updates <paramref name="source"/>.
        /// If it contained any tags, a new inline string containing tag numbers would be produced and returned.
        /// If it contained no tags, <paramref name="source"/> would be returned.
        /// <paramref name="pool"/> would be null when <paramref name="source"/> contained no tags.
        /// </remarks>
        private static InlineString NumberTags(InlineString source, out TagPool pool)
        {
            if (source.HasTags)
            {
                var inline = new InlineString(source);
                var p = new TagPool();
                int n = 0;
                foreach (var tag in inline.Tags)
                {
                    p[tag] = tag.Number = ++n;
                }
                pool = p;
                return inline;
            }
            else
            {
                pool = null;
                return source;
            }
        }

        /// <summary>Assigns local numbers to matching tags in an inline string, referring to a tag pool.</summary>
        /// <param name="target">An inline string.</param>
        /// <param name="pool">The tag pool.</param>
        /// <returns>An inline string with tag numberes assigned.</returns>
        /// <remarks>
        /// This method never updates <paramref name="target"/>.
        /// If it contained any tags, a new inline string containing tag numbers would be produced and returned.
        /// If it contained no tags, <paramref name="target"/> would be returned.
        /// </remarks>
        private static InlineString MatchTags(InlineString target, TagPool pool)
        {
            if (target.HasTags)
            {
                var inline = new InlineString(target);
                if (pool != null)
                {
                    foreach (var tag in inline.Tags)
                    {
                        int n;
                        pool.TryGetValue(tag, out n);
                        tag.Number = n;
                    }
                }
                return inline;
            }
            else
            {
                return target;
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

    static class Local
    {
#if false

        public static void ForEach<TSource, TLocal>(
            IEnumerable<TSource> source,
            ParallelOptions options,
            Func<TLocal> initial,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> final)
        {
            Parallel.ForEach(source, options, initial, body, final);
        }

        public static void ForEach<TSource>(
            IEnumerable<TSource> source,
            ParallelOptions options,
            Action<TSource> body)
        {
            Parallel.ForEach(source, options, body);
        }

#elif false

        public static void ForEach<TSource, TLocal>(
            IEnumerable<TSource> source, 
            ParallelOptions options,
            Func<TLocal> initial,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> final)
        {
            TLocal local = initial();
            long index = 0;
            foreach (TSource item in source)
            {
                local = body(item, null, index++, local);
            }
            final(local);
        }

        public static void ForEach<TSource>(
            IEnumerable<TSource> source,
            ParallelOptions options,
            Action<TSource> body)
        {
            foreach (TSource item in source)
            {
                body(item);
            }
        }

#elif true
        
        struct IndexAndItem<T>
        {
            public readonly T Item;

            public readonly long Index;

            public IndexAndItem(T item, long index)
            {
                Item = item;
                Index = index;
            }
        }

        public static void ForEach<TSource, TLocal>(
            IEnumerable<TSource> source,
            ParallelOptions options,
            Func<TLocal> initial,
            Func<TSource, ParallelLoopState, long, TLocal, TLocal> body,
            Action<TLocal> final)
        {
            using (var queue = new BlockingCollection<IndexAndItem<TSource>>(Environment.ProcessorCount))
            {
                var task = Task.Run(() =>
                {
                    var tasks = new Task[Environment.ProcessorCount];
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        tasks[i] = Task.Run(() =>
                        {
                            TLocal local = initial();
                            while (!queue.IsCompleted)
                            {
                                try
                                {
                                    var index_and_item = queue.Take();
                                    local = body(index_and_item.Item, null, index_and_item.Index, local);
                                }
                                catch (InvalidOperationException)
                                {
                                }
                            }
                            final(local);
                        });
                    }
                    Task.WaitAll(tasks);
                });

                long index = 0;
                foreach (TSource item in source)
                {
                    queue.Add(new IndexAndItem<TSource>(item, index++));
                }
                queue.CompleteAdding();

                task.Wait();
            }
        }

        public static void ForEach<TSource>(
            IEnumerable<TSource> source,
            ParallelOptions options,
            Action<TSource> body)
        {
            ForEach<TSource, object>(
                source,
                options,
                () => null,
                (item, state, index, local) => { body(item); return null; },
                (local) => { });
        }

#endif
    }
}
