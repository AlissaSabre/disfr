using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace disfr.Doc
{
    class XliffAsset : IAsset
    {
        public static readonly XNamespace XLIFF = XNamespace.Get("urn:oasis:names:tc:xliff:document:1.2");

        /// <summary>
        /// Creates a base <see cref="XliffAsset"/> instance from an xliff file element.  
        /// </summary>
        /// <param name="file">An xliff file element.</param>
        /// <remarks>
        /// This constructor initializes <see cref="TransPairs"/> and <see cref="AltPairs"/> to
        /// LINQ-style deferred execution objects.
        /// Subclass constructors can perform any additional initialization that may affect them.
        /// </remarks>
        public XliffAsset(XElement file)
        {
            X = file.Name.Namespace;

            Original = (string)file.Attribute("original") ?? "";
            SourceLang = (string)file.Attribute("source-language");
            TargetLang = (string)file.Attribute("target-language");
            _TransPairs = file.Descendants(X + "trans-unit").SelectMany(ExtractPairs).Select(SerialPatcher);
            _AltPairs = file.Descendants(X + "trans-unit").Elements(X + "alt-trans").SelectMany(ExtractAltPairs);
        }

        protected StringPool Pool = new StringPool();

        protected readonly XNamespace X;

        protected bool SegmentationAllowed = true;

        private int _UniqueNumber = 0;

        protected int UniqueNumber { get { return ++_UniqueNumber; } }

        public string Package { get; internal set; }

        public string Original { get; protected set; }

        public string SourceLang { get; protected set; }

        public string TargetLang { get; protected set; }

        protected IEnumerable<ITransPair> _TransPairs; 

        public IEnumerable<ITransPair> TransPairs { get { Materialize(); return _TransPairs; } }

        protected IEnumerable<ITransPair> _AltPairs;

        public IEnumerable<ITransPair> AltPairs { get { Materialize(); return _AltPairs; } }

        protected readonly PropertiesManager PropMan = new PropertiesManager(false); 

        public IList<PropInfo> Properties { get { Materialize(); return PropMan.Properties; } }

        protected bool Materialized = false;

        protected virtual void Materialize()
        {
            if (Materialized) return;
            _TransPairs = _TransPairs.ToList();
            _AltPairs = _AltPairs.ToList();
            Materialized = true;
        }

        protected virtual void AddProp(XliffTransPair pair, string key, string value)
        {
            PropMan.Put(ref pair._Props, key, value);
        }

        /// <summary>
        /// Extracts a series of <see cref="XliffTransPair"/>'s from a single XLIFF tu element.
        /// </summary>
        /// <param name="tu">XLIFF trans-unit element.</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// In the standard XLIFF, a trans-unit element may or may not be segmented using mrk[@mtype='seg'] elements.
        /// This method detects the case and returns a series of trans-pairs based on the segments.
        /// Otherwise, a single trans-pair is returned for a single trans-unit element.
        /// </para>
        /// <para>
        /// Note that a subclass may <i>add</i>more trans-pairs to those from standard XLIFF specificataion,
        /// based on any CAT-tool specific extensions.
        /// </para>
        /// </remarks>
        protected virtual IEnumerable<XliffTransPair> ExtractPairs(XElement tu)
        {
            if (SegmentationAllowed &&
                (string)tu.Attribute("translate") != "no" &
                tu.Elements(X + "seg-source").Descendants(X + "mrk").Any(mrk => (string)mrk.Attribute("mtype") == "seg"))
            {
                return SegmentAndExtractPairs(tu);
            }
            else
            {
                return new[] { ExtractSinglePair(tu) };
            }
        }

        // This method will be cast to Comparison{Segment} delegate,
        // but it doesn't perform real comparison.
        // It is actually like EqualityComparer{Segment}.Equals(Segment, Segment).
        // Current Diff.Diff.Compare(IList{Segment}, IList{Segment}, Comparison{Segment}) works fine with it.
        private static int CompareRuns(SegmentData x, SegmentData y)
        {
            var xx = x.Element;
            var yy = y.Element;
            if (xx == null && yy == null) return 0;
            if (xx == null || yy == null) return -1;
            if ((string)xx.Attribute("mid") == (string)yy.Attribute("mid")) return 0;
            return -1;
        }

        protected virtual IEnumerable<XliffTransPair> SegmentAndExtractPairs(XElement tu)
        {
            var slang = GetLang(tu.Element(X + "seg-source")) ?? GetLang(tu.Element(X + "source")) ?? SourceLang;
            var tlang = GetLang(tu.Element(X + "target")) ?? TargetLang;
            var notes = GetNotes(tu); 

            var ssegs = GetInlineSegments(tu.Element(X + "seg-source"));
            var tsegs = GetInlineSegments(tu.Element(X + "target"));
            var diff = Diff.Diff.Compare(ssegs, tsegs, CompareRuns);
            int s = 0, t = 0;
            foreach (char c in diff)
            {
                switch (c)
                {
                    case 'A': yield return ExtractSegmentedPair(null, tsegs[t++], slang, tlang, notes); break;
                    case 'D': yield return ExtractSegmentedPair(ssegs[s++], null, slang, tlang, notes); break;
                    case 'C': yield return ExtractSegmentedPair(ssegs[s++], tsegs[t++], slang, tlang, notes); break;
                }
            }
        }

        protected virtual XliffTransPair ExtractSegmentedPair(SegmentData src, SegmentData tgt, string slang, string tlang, IEnumerable<string> notes)
        {
            var selem = src.Element;
            var telem = tgt.Element;

            var pair = new XliffTransPair()
            {
                Serial = (selem != null) || (telem != null) ? 1 : -1,
                Id = (string)selem?.Attribute("mid") ?? (string)telem?.Attribute("mid") ?? "*",
                Source = GetInline(selem) ?? src.InlineString ?? InlineString.Empty,
                Target = GetInline(telem) ?? tgt.InlineString ?? InlineString.Empty,
                SourceLang = slang,
                TargetLang = tlang,
            };

            MatchTags(pair.Source, pair.Target);
            pair.AddNotes(notes);
            return pair;
        }

        protected virtual XliffTransPair ExtractSinglePair(XElement tu)
        {
            var source = tu.Element(X + "source");
            var target = tu.Element(X + "target");
            var pair = new XliffTransPair()
            {
                Serial = ((string)tu.Attribute("translate") == "no") ? -1 : 1,
                Id = (string)tu.Attribute("id"),
                Source = GetInline(source) ?? InlineString.Empty,
                Target = GetInline(target) ?? InlineString.Empty,
                SourceLang = GetLang(source) ?? SourceLang,
                TargetLang = GetLang(target) ?? TargetLang,
            };
            MatchTags(pair.Source, pair.Target);
            pair.AddNotes(GetNotes(tu));
            AddTuAttrProps(pair, tu);
            AddContextProps(pair, tu);
            if (source != null) AddSourceAttrProps(pair, source);
            if (target != null) AddTargetAttrProps(pair, target);
            return pair;
        }

        protected virtual void AddTuAttrProps(XliffTransPair pair, XElement tu)
        {
            foreach (var attr in tu.Attributes().Where(a => a.Name != "id"))
            {
                AddProp(pair, attr.Name.LocalName, attr.Value);
            }
        }

        protected virtual void AddContextProps(XliffTransPair pair, XElement tu)
        {
            foreach (var context in tu.Elements(X + "context-group").Elements(X + "context"))
            {
                AddProp(pair, "context/" + ((string)context.Attribute("context-type") ?? ""), (string)context ?? "");
            }
        }

        protected virtual void AddSourceAttrProps(XliffTransPair pair, XElement source)
        {
            foreach (var attr in source.Attributes()
                .Where(a => a.Name.Namespace != XNamespace.Xml && a.Name.Namespace != XNamespace.Xmlns))
            {
                AddProp(pair, attr.Name.LocalName, attr.Value);
            }
        }

        protected virtual void AddTargetAttrProps(XliffTransPair pair, XElement target)
        {
            foreach (var attr in target.Attributes()
                .Where(a => a.Name.Namespace != XNamespace.Xml && a.Name.Namespace != XNamespace.Xmlns))
            {
                AddProp(pair, attr.Name.LocalName, attr.Value);
            }
        }

        protected virtual IEnumerable<ITransPair> ExtractAltPairs(XElement alt)
        {
            // For the moment, we only take care of unsegmented cases.
            yield return ExtractSingleAltPair(alt);
        }

        protected virtual XliffTransPair ExtractSingleAltPair(XElement alt)
        {
            var source =
                alt.Element(X + "seg-source") ??
                alt.Element(X + "source") ??
                alt.Parent.Element(X + "seg-source") ??
                alt.Parent.Element(X + "source");
            var target = alt.Element(X + "target");
            var pair = new XliffTransPair()
            {
                Id = (string)alt.Parent.Attribute("id"),
                Source = GetInline(source) ?? InlineString.Empty,
                Target = GetInline(target) ?? InlineString.Empty,
                SourceLang = GetLang(source) ?? SourceLang,
                TargetLang = GetLang(target) ?? TargetLang,
            };
            AddProp(pair, "origin", (string)alt.Attribute("origin"));
            MatchTags(pair.Source, pair.Target);
            pair.AddNotes(GetNotes(alt));
            return pair;
        }

        protected InlineString GetInline(XElement element)
        {
            if (element == null) return null;
            var builder = new SegmentedContentBuilder();
            SegmentInlineContent(builder, element, false);
            var seq = builder.GetSequence();
            if (seq.Count == 0) return InlineString.Empty;
            if (seq.Count != 1) throw new Exception("Internal Error");
            if (seq[0]?.InlineString == null) throw new Exception("Internal Error");
            return seq[0].InlineString;
        }

        protected IList<SegmentData> GetInlineSegments(XElement element)
        {
            var builder = new SegmentedContentBuilder();
            SegmentInlineContent(builder, element, true);
            return builder.GetSequence();
        }

        /// <summary>
        /// Represents a segment or an inter-segment inline content in a trans-unit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If XLIFF standard segmentation (using mrk[@stype='seg']) is NOT used,
        /// The entire contents of a source or target element is a single segment.
        /// If XLIFF standard segmentation is used,
        /// the contents of each mrk[@mtype='seg'] element corresponds to a <see cref="SegmentData"/> instance,
        /// as well as each of remaining contents between such mrk elements does.
        /// </para>
        /// <para>
        /// One and only one of <see cref="Element"/> and <see cref="InlineString"/> is non-null.
        /// If <see cref="Element"/> is non-null, this <see cref="SegmentData"/> corresponds to an mrk[@mtype='seg'] element.
        /// If <see cref="InlineString"/> is non-null, this <see cref="SegmentData"/> corresponds to
        /// the contents outside of mrk element (inter segment).
        /// </para>
        /// </remarks>
        protected class SegmentData
        {
            public readonly XElement Element;

            public readonly InlineString InlineString;

            public SegmentData(XElement element) { Element = element; }

            public SegmentData(InlineString inline) { InlineString = inline; }
        }

        /// <summary>
        /// Analyzes contents of an XML element as a (possibly segmented) XLIFF inline text. 
        /// </summary>
        /// <param name="builder">Results are stored in this object.</param>
        /// <param name="elem">The element whose contents is analyzed.</param>
        /// <param name="allow_segmentation">true if the standard XLIFF segmentation is allowed.</param>
        protected void SegmentInlineContent(SegmentedContentBuilder builder, XElement elem, bool allow_segmentation)
        {
            // Why shouldn't we throw ArgumentNullException in this case?  FIXME.
            if (elem == null) return;

            foreach (var n in elem.Nodes())
            {
                if (n is XText)
                {
                    builder.Add((n as XText).Value);
                }
                else if (n is XElement)
                {
                    var e = (XElement)n;
                    var ns = e.Name.Namespace;
                    var name = e.Name.LocalName;
                    if (ns == X && name == "mrk")
                    {
                        var handled = HandleMarkElement(builder, e, allow_segmentation);
                        if (!handled)
                        {
                            SegmentInlineContent(builder, e, allow_segmentation);
                        }
                    }
                    else if (ns == X && (name == "x" || name == "ph"))
                    {
                        // Replace a standalone native code element with a standalone inline tag.
                        builder.Add(BuildNativeCodeTag(Tag.S, e, name == "ph"));
                    }
                    else if (ns == X && (name == "bx" || name == "bpt"))
                    {
                        // Replace a beginning native code element with a beginning inline tag.
                        builder.Add(BuildNativeCodeTag(Tag.B, e, name == "bpt"));
                    }
                    else if (ns == X && (name == "ex" || name == "ept"))
                    {
                        // Replace an ending native code element with an ending inline tag.
                        builder.Add(BuildNativeCodeTag(Tag.E, e, name == "ept"));
                    }
                    else if (ns == X && name == "it")
                    {
                        // Replace an isolated native code element with an appropriate inline tag.
                        Tag type;
                        switch ((string)e.Attribute("pos"))
                        {
                            case "open": type = Tag.B; break;
                            case "close": type = Tag.E; break;
                            default: type = Tag.S; break;
                        }
                        builder.Add(BuildNativeCodeTag(type, e, true));
                    }
                    else if (ns == X && name == "g")
                    {
                        // If this is an XLIFF g element, 
                        // replace start and end tags with inline tags,
                        // and keep converting its content,
                        // because the g holds instructions in its attributes,
                        // and its content is a part of translatable text.
                        builder.Add(BuildNativeCodeTag(Tag.B, e, false));
                        SegmentInlineContent(builder, e, allow_segmentation);
                        builder.Add(BuildNativeCodeTag(Tag.E, e, false));
                    }
                    else
                    {
                        // Unknown element, i.e., some external (no XLIFF) element or a 
                        // misplaced XLIFF element.
                        // OH, I have no good idea how to handle it.  FIXME.
                        var handled = HandleUnknownTag(builder, e);
                        if (!handled)
                        {
                            var id = "*";
                            var rid = UniqueNumber.ToString();
                            if (string.IsNullOrEmpty(e.Value))
                            {
                                builder.Add(new InlineTag(Tag.S, id, rid, name, null, null, null));
                            }
                            else
                            {
                                // Assume the contents of this element is a translatable text.
                                builder.Add(new InlineTag(Tag.B, id, rid, name, null, null, null));
                                SegmentInlineContent(builder, e, allow_segmentation);
                                builder.Add(new InlineTag(Tag.E, id, rid, name, null, null, null));
                            }
                        }
                    }
                }
                else
                {
                    // Silently discard any other nodes, i.e., comment or pi, entirely.
                }
            }
        }

        /// <summary>
        /// A container and builder for a sequence of <see cref="SegmentData"/>.
        /// </summary>
        protected class SegmentedContentBuilder
        {
            private readonly List<SegmentData> Sequence = new List<SegmentData>();

            public List<SegmentData> GetSequence()
            {
                Flush();
                return Sequence;
            }

            private readonly InlineBuilder InterSegment = new InlineBuilder();

            private readonly Stack<InlineProperty> PropStack = new Stack<InlineProperty>();

            public void Flush()
            {
                if (!InterSegment.IsEmpty)
                {
                    Sequence.Add(new SegmentData(InterSegment.ToInlineString()));
                    InterSegment.Clear(true);
                }
            }

            public void Add(XElement element)
            {
                Flush();
                Sequence.Add(new SegmentData(element));
            }

            public void Add(string text)
            {
                InterSegment.Add(text);
            }

            public void Add(InlineTag tag)
            {
                InterSegment.Add(tag);
            }

            public void PushProp(InlineProperty prop)
            {
                PropStack.Push(InterSegment.Property);
                InterSegment.Property = prop;
            }

            public void PopProp()
            {
                InterSegment.Property = PropStack.Pop();
            }
        }

        /// <summary>
        /// Try to handle an mrk element.
        /// </summary>
        /// <param name="builder">The builder the contents are fed to.</param>
        /// <param name="mrk">The mrk element to be handled.</param>
        /// <param name="allow_segmentation">true if segmentation is allowed at this level.</param>
        /// <returns>true if <paramref name="mrk"/> has been handled, false otherwise.</returns>
        protected virtual bool HandleMarkElement(SegmentedContentBuilder builder, XElement mrk, bool allow_segmentation)
        {
            // mrk tags are _markers_ for CAT tools.
            // We take care of the XLIFF segmentation spec.
            // In the spec.,
            // mrk[@mtype='seg'] is used for segmentation.
            // Other standard @mtype are mostly linguistic annotations. 
            // mrk elements with non-standard @mtype are CAT software dependent.
            // Most of their uses are invisible to translators,
            // although there MAY BE some that are visible.
            // For the moment,
            // we ignore the start and end tags for all mrk elements, 
            // leaving the content,
            // except for those for segmentation.

            if (allow_segmentation && (string)mrk.Attribute("mtype") == "seg")
            {
                builder.Add(mrk);
                return true;
            }

            return false;
        }

        protected virtual InlineTag BuildNativeCodeTag(Tag type, XElement element, bool has_code)
        {
            return new InlineTag(
                type: type,
                id: (string)element.Attribute("id"),
                rid: (string)element.Attribute("rid") ?? (string)element.Attribute("id"),
                name: element.Name.LocalName,
                ctype: (string)element.Attribute("ctype"),
                display: null,
                code: has_code ? element.Value : null);
        }

        protected virtual bool HandleUnknownTag(SegmentedContentBuilder builder, XElement element)
        {
            return false;
        }

        private static readonly XName XML_LANG = XNamespace.Xml + "lang";

        protected string GetLang(XElement element)
        {
            if (element == null) return null;
            return (string)element.Attribute(XML_LANG);
        }

        protected void MatchTags(InlineString source, InlineString target)
        {
            var dict = new Dictionary<InlineTag, int>(InlineTag.LooseEqualityComparer);
            int i = 0;
            foreach (var tag in source.Tags)
            {
                dict[tag] = tag.Number = ++i;
            }
            foreach (var tag in target.Tags)
            {
                int j;
                dict.TryGetValue(tag, out j);
                tag.Number = j;
            }
        }

        /// <summary>
        /// Gets notes from either trans-unit or alt-trans element.
        /// </summary>
        /// <param name="unit">trans-unit or alt-trans element.</param>
        /// <returns>List of notes.</returns>
        protected virtual IEnumerable<string> GetNotes(XElement unit)
        {
            return unit.Elements(X + "note").Select(note => (string)note);
        }

        protected Func<XliffTransPair, int, XliffTransPair> SerialPatcher
        {
            get
            {
                int serial = 0;
                return (pair, index) =>
                {
                    if (index == 0) serial = 0; // XXX: Any better idea?
                    if (pair.Serial > 0) pair.Serial = ++serial;
                    return pair;
                };
            }
        }
    }
}
