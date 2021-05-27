using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Plugin;
using disfr.Doc;
using System.Globalization;

namespace disfr.po
{
    public class PoReaderPlugin : IReaderPlugin
    {
        public string Name { get { return "PoReader"; } }
        public IReader CreateReader() { return new PoReader(); }
    }

    public class PoReader : IAssetReader
    {
        public string Name { get { return "PoReader"; } }

        public int Priority { get { return 7; } }

        private static readonly string[] _FilterString = { "gettext PO files|*.po" };

        public IList<string> FilterString { get { return _FilterString; } }

        public IAssetBundle Read(string filename, int filterindex)
        {
            return LoaderAssetBundle.Create(
                ReaderManager.FriendlyFilename(filename),
                () => ReadAsset(filename, filterindex < 0));
        }

        private IAsset ReadAsset(string filename, bool suppress_early_exception)
        {
            var parser = new PoParser();
            var sink = new PoAssetSink() { GetLineNumber = parser.GetLineNumber };
            try
            {
                parser.Parse(filename, sink);
            }
            catch (YYException)
            {
                if (suppress_early_exception && !sink.Collected) return null;
                throw;
            }
            var a = sink.GetAsset();
            a.Package = filename;
            a.Original = sink.Project;
            a.SourceLang = ""; // unkown/unspecified
            a.TargetLang = sink.TargetLanguage;
            return a;
        }
    }

    internal class PoAsset : IAsset
    {
        public string Package { get; internal set; }

        public string Original { get; internal set; }

        public string SourceLang { get; internal set; }

        public string TargetLang { get; internal set; }

        public IEnumerable<ITransPair> TransPairs { get; internal set; }

        public IEnumerable<ITransPair> AltPairs { get { return Enumerable.Empty<ITransPair>(); } }

        public IList<PropInfo> Properties { get { return _Properties; } }

        private static readonly PropInfo[] _Properties =
        {
            new PropInfo("obsolete", true),
            new PropInfo("flags"),
            new PropInfo("References"),
        };

        internal const int PropObsolete = 0;

        internal const int PropFlags = 1;

        internal const int PropReferences = 2;

        internal const int PropCount = 3;
    }

    internal class PoTransPair : ITransPair
    {
        internal PoTransPair(PoAsset asset) { Asset = asset; }

        private readonly PoAsset Asset;

        public int Serial { get; internal set; }

        public string Id { get; internal set; }

        public string SourceLang { get { return Asset.SourceLang; } }

        public string TargetLang { get { return Asset.TargetLang; } }

        public InlineString Source { get; internal set; }

        public InlineString Target { get; internal set; }

        public IEnumerable<string> Notes { get; internal set; }

        private readonly string[] _Props = new string[PoAsset.PropCount];

        public string this[int index]
        {
            get { return _Props[index]; }
            internal set { _Props[index] = value; }
        }
    }
}
