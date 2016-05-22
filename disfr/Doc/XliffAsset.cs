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

        protected virtual XliffTransPair ExtractSegmentedPair(object src, object tgt, string slang, string tlang, IEnumerable<string> notes)
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
                Source = GetInline(source),
                Target = GetInline(target),
                SourceLang = GetLang(source) ?? SourceLang,
                TargetLang = GetLang(target) ?? TargetLang,
            };
            MatchTags(pair.Source, pair.Target);
            pair.AddNotes(GetNotes(tu));
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
            pair.AddNotes(GetNotes(alt));
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
