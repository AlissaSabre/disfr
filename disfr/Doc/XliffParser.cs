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
            if (X != XliffAsset.XLIFF && X != XNamespace.None) return null;
            if (xliff.Name.LocalName != "xliff") return null;

            // OK.  It seems an XLIFF.  Try to detect a flavour if set to Auto.
            if (Flavour == XliffReader.Flavour.Auto)
            {
                Flavour = XliffReader.Flavour.Generic;
                var ns = xliff.Descendants().Select(e => e.Name.Namespace).FirstOrDefault(
                    n => n == TradosXliffAsset.SDL ||
                         n == IdiomXliffAsset.IWS ||
                         n == MemoQXliffAsset.MQ);
                if (ns == TradosXliffAsset.SDL) Flavour = XliffReader.Flavour.Trados;
                if (ns == IdiomXliffAsset.IWS) Flavour = XliffReader.Flavour.Idiom;
                if (ns == MemoQXliffAsset.MQ) Flavour = XliffReader.Flavour.MemoQ;
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
