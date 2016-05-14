using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace disfr.Doc
{
    public class XliffParser
    {
        private static readonly XNamespace XLIFF = XNamespace.Get("urn:oasis:names:tc:xliff:document:1.2");
        private static readonly XNamespace SDL = XNamespace.Get("http://sdl.com/FileTypes/SdlXliff/1.0");
        private static readonly XNamespace IWS = XNamespace.Get("http://www.idiominc.com/ws/asset");
        private static readonly XNamespace MQ = XNamespace.Get("MQXliff"); // This is NOT a placeholder; it's real!  :(

        public string Filename;

        public XliffReader.Flavour Flavour;

        public IEnumerable<IAsset> Read(Stream stream)
        {
            XElement xliff;
            try
            {
                // I experienced that some import filter (used with some CAT software) 
                // produces some illegal entities, e.g., "&#x1F;".
                // Although it is NOT a wellformed XML in theory, we need to take care of them.
                // Another issue is that some XML file includes DOCTYPE declaration
                // with a system identifier,
                // that XmlReader tries to access to to get a DTD, 
                // so we need to instruct explicitly not to do so.
                var settings = new XmlReaderSettings()
                {
                    CheckCharacters = false,
                    IgnoreWhitespace = false,
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null,
                    CloseInput = true
                };
                using (var rd = XmlReader.Create(stream, settings))
                {
                    xliff = XElement.Load(rd);
                }
            }
            catch (XmlException)
            {
                // This is usually thrown when the given file was not an XML document,
                // or it was meant to be an XML but contained some error.
                // The first case is normal, since we try to parse everything as an XML.
                // The latter case needs diagnostic,
                // but I don't know how I can identify the cases.
                // We need some diagnostic logging feature.  FIXME.
                return null;
            }
            catch (IOException)
            {
                // This must be an indication of an issue in the underlying file access.
                throw;
            }
            catch (Exception)
            {
                // If we get an exception of other type,
                // I believe it is an indication of some unexpected error.
                // However, the API document of XElement.Load(Strream) is too vague,
                // and I don't know what we should do.
                return null;
            }

            // We are probably seeing a wellformed XML document.
            // Check if it is an XLIFF document.
            var X = xliff.Name.Namespace;
            if (X != XLIFF && X != XNamespace.None) return null;
            if (xliff.Name.LocalName != "xliff") return null;

            // OK.  It seems an XLIFF.  Try to detect a flavour if set to Auto.
            if (Flavour == XliffReader.Flavour.Auto)
            {
                Flavour = XliffReader.Flavour.Generic;
                var ns = xliff.Descendants().Select(e => e.Name.Namespace).FirstOrDefault(n => n == SDL || n == IWS || n == MQ);
                if (ns == SDL) Flavour = XliffReader.Flavour.Trados;
                if (ns == IWS) Flavour = XliffReader.Flavour.Idiom;
                if (ns == MQ) Flavour = XliffReader.Flavour.MemoQ;
            }

            return xliff.Elements(X + "file").Select(CreateAsset);
        }

        private XliffAsset CreateAsset(XElement file)
        {
            XliffAsset asset;
            switch (Flavour)
            {
                case XliffReader.Flavour.Generic:
                    asset = new XliffAsset(file);
                    break;
                case XliffReader.Flavour.Trados:
                    asset = new TradosXliffAsset(file);
                    break;
                case XliffReader.Flavour.Idiom:
                    asset = new IdiomXliffAsset(file);
                    break;
                case XliffReader.Flavour.MemoQ:
                    asset = new MemoQXliffAsset(file);
                    break;
                default:
                    throw new ApplicationException("internal error");
            }
            asset.Package = Filename;
            return asset;
        }

        class XliffAsset : IAsset
        {
            public XliffAsset(XElement file)
            {
                X = file.Name.Namespace;

                Original = (string)file.Attribute("original") ?? "";
                SourceLang = (string)file.Attribute("source-language");
                TargetLang = (string)file.Attribute("target-language");
                TransPairs = file.Descendants(X + "trans-unit").SelectMany(ExtractPairs).Select(SerialPatcher);
                AltPairs = file.Descendants(X + "trans-unit").Elements(X + "alt-trans").SelectMany(ExtractAltPairs);
            }

            protected readonly XNamespace X;

            protected bool SegmentationAllowed = true;

            private int _UniqueNumber = 0;

            protected int UniqueNumber { get { return ++_UniqueNumber; } }

            public string Package { get; internal set; }

            public string Original { get; protected set; }

            public string SourceLang { get; protected set; }

            public string TargetLang { get; protected set; }

            public IEnumerable<ITransPair> TransPairs { get; protected set; }

            public IEnumerable<ITransPair> AltPairs { get; protected set; }

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

            private static int CompareRuns(object x, object y)
            {
                var xx = x as XElement;
                var yy = y as XElement;
                if (xx == null && yy == null) return 0;
                if (xx == null || yy == null) return -1;
                if ((string)xx.Attribute("mid") == (string)yy.Attribute("mid")) return 0;
                return -1;
            } 

            protected bool IsMrkSeg(XNode node)
            {
                if (!(node is XElement)) return false;
                var element = node as XElement;
                if (element.Name.Namespace != X) return false;
                if (element.Name.LocalName != "mrk") return false;
                if ((string)element.Attribute("mtype") != "seg") return false;
                return true;
            }

            protected virtual IEnumerable<XliffTransPair> SegmentAndExtractPairs(XElement tu)
            {
                var slang = GetLang(tu.Element(X + "seg-source")) ?? GetLang(tu.Element(X + "source")) ?? SourceLang;
                var tlang = GetLang(tu.Element(X + "target")) ?? TargetLang;

                var ssegs = GetInlineSegments(tu.Element(X + "seg-source"));
                var tsegs = GetInlineSegments(tu.Element(X + "target"));
                var diff = Diff.Diff.Compare(ssegs, tsegs, CompareRuns);
                int s = 0, t = 0;
                foreach (char c in diff)
                {
                    switch (c)
                    {
                        case 'A': yield return ExtractSegmentedPair(null, tsegs[t++], slang, tlang); break;
                        case 'D': yield return ExtractSegmentedPair(ssegs[s++], null, slang, tlang); break;
                        case 'C': yield return ExtractSegmentedPair(ssegs[s++], tsegs[t++], slang, tlang); break;
                    }
                }
            }

            protected virtual XliffTransPair ExtractSegmentedPair(object src, object tgt, string slang, string tlang)
            {
                var selem = src as XElement;
                var telem = tgt as XElement;

                var pair = new XliffTransPair()
                {
                    Serial = (selem != null) || (telem != null) ? 1 : -1,
                    Id = (string)selem?.Attribute("mid") ?? (string)telem?.Attribute("mid") ?? "*",
                    Source = (selem != null) ? GetInline(selem) : (src as InlineString ?? new InlineString()),
                    Target = (telem != null) ? GetInline(telem) : (tgt as InlineString ?? new InlineString()),
                    SourceLang = slang,
                    TargetLang = tlang,
                };
                MatchTags(pair.Source, pair.Target);
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
                    Source = GetInline(source),
                    Target = GetInline(target),
                    SourceLang = GetLang(source) ?? SourceLang,
                    TargetLang = GetLang(target) ?? TargetLang,
                };
                MatchTags(pair.Source, pair.Target);
                foreach (var attr in tu.Attributes().Where(a => a.Name != "id"))
                {
                    pair.AddProp(attr.Name.LocalName, attr.Value);
                }
                return pair;
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
                    Source = GetInline(source),
                    Target = GetInline(target),
                    SourceLang = GetLang(source) ?? SourceLang,
                    TargetLang = GetLang(target) ?? TargetLang,
                };
                pair.AddProp("origin", (string)alt.Attribute("origin"));
                MatchTags(pair.Source, pair.Target);
                return pair;
            }

            protected InlineString GetInline(XElement element)
            {
                var seq = new ContentParser(this, false).Parse(element).GetSequence();
                if (seq.Count == 0) return new InlineString();
                if (seq.Count != 1) throw new Exception("Internal Error");
                return (InlineString)seq[0];
            }

            protected List<object> GetInlineSegments(XElement element)
            {
                return new ContentParser(this, true).Parse(element).GetSequence();
            }

            protected class ContentParser
            {
                public ContentParser(XliffAsset xliff, bool allow_segmentation)
                {
                    Xliff = xliff;
                    X = xliff.X;
                    AllowSegmentation = allow_segmentation;
                }

                private readonly XliffAsset Xliff;

                private readonly XNamespace X;

                private readonly bool AllowSegmentation;

                private readonly List<object> _Sequence = new List<object>();

                public List<object> GetSequence()
                {
                    FlushInline();
                    return _Sequence;
                }

                private InlineString _Inline = null;

                private InlineString Inline
                {
                    get { return _Inline ?? (_Inline = new InlineString()); }
                }

                private void FlushInline()
                {
                    if (_Inline != null)
                    {
                        _Sequence.Add(_Inline);
                        _Inline = null;
                    }
                }

                private void Add(XElement mrk)
                {
                    GetSequence().Add(mrk);
                }

                public ContentParser Parse(XElement elem)
                {
                    if (elem == null) return this;
                    foreach (var n in elem.Nodes())
                    {
                        if (n.NodeType == XmlNodeType.Text)
                        {
                            Inline.Append(((XText)n).Value);
                        }
                        else if (n.NodeType == XmlNodeType.Element)
                        {
                            var e = (XElement)n;
                            var ns = e.Name.Namespace;
                            var name = e.Name.LocalName;
                            if (ns == X && name == "mrk")
                            {
                                // mrk tags are _markers_ for CAT tools.
                                // We take care of the XLIFF segmentation spec.
                                // Uses of other mrk tags are CAT software dependent.
                                // Most of their uses are
                                // invisible to translators, although there MAY BE some that
                                // are visible.  For the moment, we ignore the start end end tags, 
                                // leaving the content.
                                if (AllowSegmentation && (string)e.Attribute("mtype") == "seg")
                                {
                                    Add(e);
                                }
                                else
                                {
                                    Parse(e);
                                }
                            }
                            else if (ns == X && (name == "x" || name == "ph"))
                            {
                                // Replace a standalone native code element with a standalone inline tag.
                                Inline.Append(Xliff.BuildNativeCodeTag(Tag.S, e, name == "ph"));
                            }
                            else if (ns == X && (name == "bx" || name == "bpt"))
                            {
                                // Replace a beginning native code element with a beginning inline tag.
                                Inline.Append(Xliff.BuildNativeCodeTag(Tag.B, e, name == "bpt"));
                            }
                            else if (ns == X && (name == "ex" || name == "ept"))
                            {
                                // Replace an ending native code element with an ending inline tag.
                                Inline.Append(Xliff.BuildNativeCodeTag(Tag.E, e, name == "ept"));
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
                                Inline.Append(Xliff.BuildNativeCodeTag(type, e, true));
                            }
                            else if (ns == X && name == "g")
                            {
                                // If this is an XLIFF g element, 
                                // replace start and end tags with inline tags,
                                // and keep converting its content,
                                // because the g holds instructions in its attributes,
                                // and its content is a part of translatable text.
                                Inline.Append(Xliff.BuildNativeCodeTag(Tag.B, e, false));
                                Parse(e);
                                Inline.Append(Xliff.BuildNativeCodeTag(Tag.E, e, false));
                            }
                            else
                            {
                                // Uunknown element, i.e., some external (no XLIFF) element or a 
                                // misplaced XLIFF element.
                                // OH, I have no good idea how to handle it.  FIXME.
                                Inline.Append(Xliff.HandleUnknownTag(e));
                            }
                        }
                        else
                        {
                            // Silently discard any other nodes; e.g., comment or pi. 
                        }
                    }
                    return this;
                }
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

            protected virtual InlineString HandleUnknownTag(XElement element)
            {
                var id = "*";
                var rid = UniqueNumber.ToString();
                var name = element.Name.ToString();
                if (string.IsNullOrEmpty(element.Value))
                {
                    return new InlineString() { new InlineTag(Tag.S, id, rid, name, null, null, null) };
                }
                else
                {
                    // Assume the contents of this element is a translatable text.
                    return new InlineString()
                        .Append(new InlineTag(Tag.B, id, rid, name, null, null, null))
                        .Append(GetInline(element))
                        .Append(new InlineTag(Tag.E, id, rid, name, null, null, null));
                }
            }

            private static readonly XName XML_LANG = XNamespace.Xml + "lang";

            protected string GetLang(XElement element)
            {
                if (element == null) return null;
                return (string)element.Attribute(XML_LANG);
            }

            protected void MatchTags(InlineString source, InlineString target)
            {
                var dict = new Dictionary<InlineTag, int>();
                int i = 0;
                foreach (var tag in source.OfType<InlineTag>())
                {
                    dict[tag] = tag.Number = ++i;
                }
                foreach (var tag in target.OfType<InlineTag>())
                {
                    int j;
                    dict.TryGetValue(tag, out j);
                    tag.Number = j;
                }
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

        class TradosXliffAsset : XliffAsset
        {
            public TradosXliffAsset(XElement file) : base(file)
            {
                var comments = file.Parent.Elements(SDL + "doc-info").Descendants(SDL + "cmt-def")
                    .ToDictionary(c => (string)c.Attribute("id"), c => c.Descendants(SDL + "Comment").Select(s => s.Value.TrimEnd()).ToArray());
                SdlComments = comments.Count == 0 ? null : comments;

                var tags = file.Element(X + "header")?.Element(SDL + "tag-defs")?.Elements(SDL + "tag")?.ToArray();
                PhTags = tags?.Where(tag => tag.Element(SDL + "ph") != null)?.ToDictionary(tag => (string)tag.Attribute("id"), tag => tag);
                PtTags = tags?.Where(tag => tag.Element(SDL + "bpt") != null)?.ToDictionary(tag => (string)tag.Attribute("id"), tag => tag);
                StTags = tags?.Where(tag => tag.Element(SDL + "st") != null)?.ToDictionary(tag => (string)tag.Attribute("id"), tag => tag);
            }

            protected readonly Dictionary<string, string[]> SdlComments;

            protected readonly Dictionary<string, XElement> PhTags, PtTags, StTags;

            protected override IEnumerable<XliffTransPair> SegmentAndExtractPairs(XElement tu)
            {
                var seginfos = tu.Elements(SDL + "seg-defs").Elements(SDL + "seg").ToDictionary(seg => (string)seg.Attribute("id"));
                var pairs = base.SegmentAndExtractPairs(tu).ToArray();
                foreach (var pair in pairs)
                {
                    XElement seg;
                    if (seginfos.TryGetValue(pair.Id, out seg))
                    {
                        foreach (var attr in seg.Attributes().Where(a => a.Name != "id"))
                        {
                            pair.AddProp(attr.Name.LocalName, attr.Value);
                        }
                        foreach (var value in seg.Elements(SDL + "value"))
                        {
                            pair.AddProp((string)value.Attribute("key"), value.Value);
                        }
                    }
                }
                return pairs;
            }

            protected override XliffTransPair ExtractSinglePair(XElement tu)
            {
                var p = base.ExtractSinglePair(tu);
                p.Serial = -1; // Trados Studio always segments (via X:mrk), so an unsegmented tu is never for translation. 
                return p;
            }

            protected override InlineTag BuildNativeCodeTag(Tag type, XElement element, bool has_code)
            {
                var tag_name = element.Name.LocalName;
                var id = (string)element.Attribute("id");
                if (tag_name == "g")
                {
                    XElement tag;
                    PtTags.TryGetValue(id, out tag);
                    var pt = tag?.Element(SDL + (type == Tag.B ? "bpt" : "ept"));

                    return new InlineTag(
                        type: type,
                        id: id,
                        rid: (string)element.Attribute("rid") ?? id,
                        name: tag_name,
                        ctype: (string)element.Attribute("ctype"),
                        display: (string)pt?.Attribute("name"),
                        code: (string)pt?.Value);
                }
                else if (tag_name == "x" && (string)element.Ancestors(X + "trans-unit").FirstOrDefault()?.Attribute("translate") == "no")
                {
                    // Oh, I'm not very sure whether translate="no" is the correct way to distinguish <st> tags...
                    XElement tag;
                    StTags.TryGetValue(id, out tag);
                    var st = tag?.Element(SDL + "st");

                    return new InlineTag(
                        type: type,
                        id: id,
                        rid: (string)element.Attribute("rid") ?? id,
                        name: tag_name,
                        ctype: (string)element.Attribute("ctype"),
                        display: (string)st?.Attribute("name"),
                        code: (string)st?.Value);
                }
                else if (tag_name == "x")
                {
                    XElement tag;
                    PhTags.TryGetValue(id, out tag);
                    var ph = tag?.Element(SDL + "ph");

                    return new InlineTag(
                        type: type,
                        id: id,
                        rid: (string)element.Attribute("rid") ?? id,
                        name: tag_name,
                        ctype: (string)element.Attribute("ctype"),
                        display: (string)ph?.Attribute("name"),
                        code: (string)ph?.Value);
                }
                else
                {
                    return base.BuildNativeCodeTag(type, element, has_code);
                }
            }
        }

        class IdiomXliffAsset : XliffAsset
        {
            public IdiomXliffAsset(XElement file) : base(file)
            {
                // IWS package doesn't support XLIFF segmentation.
                SegmentationAllowed = false;

                // Some IWS package contains an XLIFF whose file element
                // has no original attribute,
                // storing that information in another place...
                if (Original == "") Original = (string)file.Descendants(IWS + "ais_src_path").FirstOrDefault() ?? "";
            }

            protected override IEnumerable<XliffTransPair> ExtractPairs(XElement tu)
            {
                return base.ExtractPairs(tu)
                    .Concat(tu.Elements(IWS + "markup-seg").Select(ExtractMsegPair))
                    .OrderBy(pair => pair.Id);
            }

            private XliffTransPair ExtractMsegPair(XElement mseg)
            {
                var markup = new InlineString() { mseg.Value };
                return new XliffTransPair()
                {
                    Serial = -1,
                    Id = (string)mseg.Attribute("sequence"),
                    Source = markup,
                    Target = markup,
                    SourceLang = SourceLang,
                    TargetLang = TargetLang,
                };
            }

            protected override XliffTransPair ExtractSinglePair(XElement tu)
            {
                var pair = base.ExtractSinglePair(tu);
                var metadata = tu.Element(IWS + "segment-metadata");
                if (metadata != null)
                {
                    foreach (var attr in metadata.Attributes())
                    {
                        pair.AddProp(attr.Name.LocalName, attr.Value);
                    }
                    foreach (var attr in metadata.Elements(IWS + "status").Attributes())
                    {
                        pair.AddProp(attr.Name.LocalName, attr.Value);
                    }
                }
                return pair;
            }

            protected override InlineTag BuildNativeCodeTag(Tag type, XElement element, bool has_code)
            {
                // WorldServer appears to place the real native code in x attribute,
                // making the contents of an element a display string, if any.
                return new InlineTag(
                    type: type,
                    id: (string)element.Attribute("id"),
                    rid: (string)element.Attribute("rid") ?? (string)element.Attribute("id"),
                    name: element.Name.LocalName,
                    ctype: (string)element.Attribute("ctype"),
                    display: has_code ? element.Value : null,
                    code: (string)element.Attribute("x"));
            }
        }

        class MemoQXliffAsset : XliffAsset
        {
            public MemoQXliffAsset(XElement file) : base(file)
            {
                // memoQ bilingual files appear not to support XLIFF segmentation.
                SegmentationAllowed = false;  
            }
        }

        class XliffTransPair : ITransPair
        {
            private static readonly IReadOnlyDictionary<string, string> EmptyProps
                = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()); 

            public int Serial { get; internal set; }

            public string Id { get; internal set; }

            public InlineString Source { get; internal set; }

            public InlineString Target { get; internal set; }

            public string SourceLang { get; internal set; }

            public string TargetLang { get; internal set; }

            public IEnumerable<string> Notes { get; internal set; }

            internal Dictionary<string, string> _Props = null;

            public IReadOnlyDictionary<string, string> Props
            {
                get { return _Props ?? EmptyProps; }
            }

            internal void AddProp(string key, string value)
            {
                if (value == null) return;
                if (_Props == null) _Props = new Dictionary<string, string>();
                _Props[key] = value;
            }
        }
    }
}
