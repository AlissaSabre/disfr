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

        public int Priority { get { return 10; } }

        private static readonly string[] _FilterString = { "gettext PO files|*.po" };

        public IList<string> FilterString { get { return _FilterString; } }

        public IAssetBundle Read(string filename, int filterindex)
        {
            return LoaderAssetBundle.Create(
                ReaderManager.FriendlyFilename(filename),
                () => new[] { ReadAsset(filename) });
        }

        private IAsset ReadAsset(string filename)
        {
            var parser = new PoParser();
            var sink = new PoAssetSink() { GetLineNumber = parser.GetLineNumber };
            parser.Parse(filename, sink);
            var a = sink.GetAsset();
            a.Package = filename;
            a.Original = sink.Project;
            a.SourceLang = ""; // unkown/unspecified
            a.TargetLang = sink.TargetLanguage;
            return a;
        }
    }

    class PoAssetSink : CollectorSinkBase
    {
        private readonly PoAsset Asset = new PoAsset();

        private readonly List<PoTransPair> Pairs = new List<PoTransPair>();

        private int Serial = 0;

        private bool FirstTime = true;

        protected override void Finish()
        {
            var pid = FormPairId();
            var first_time = FirstTime;
            FirstTime = false;
            if (first_time && MessageId.Length == 0)
            {
                ProcessMetadata();
            }
            else if (!HasPlural)
            {
                Pairs.Add(CreatePair(MessageId, MessageStr, pid));
            }
            else
            {
                Pairs.Add(CreatePair(MessageId, MessageStrPlural[0], pid + ".0"));
                for (int i = 1; i < MessageStrPlural.Count; i++)
                {
                    var suffixed_pid = pid + "." + i.ToString(CultureInfo.InvariantCulture);
                    Pairs.Add(CreatePair(MessageIdPlural, MessageStrPlural[i], suffixed_pid));
                }
                if (MessageStrPlural.Count <= 1)
                {
                    Pairs.Add(CreatePair(MessageIdPlural, String.Empty, pid + "._"));
                }
            }
        }

        private PoTransPair CreatePair(string source, string target, string pid)
        {
            var pair = new PoTransPair(Asset)
            {
                Serial = ++Serial,
                Id = pid,
                Source = new InlineString(source),
                Target = new InlineString(target),
                Notes = TranslatorComments
                    .Concat(ExtractedComments.Select(s => "Extracted: " + s)),
                [PoAsset.PropObsolete] = IsObsolete ? "Obsolete" : "",
                [PoAsset.PropFlags] = string.Join(" ", Flags),
                [PoAsset.PropReferences] = string.Join("\n", References),
            };
            return pair;
        }

        private int LastLineNumber = 0;

        private int LastBranch = 0;

        private string FormPairId()
        {
            // Use the line number (in .po) as an ID.
            var id = LineNumber.ToString();

            // A single line can have multiple mdessage defs, though.
            // Use a, b, c, ... to distinguish them.
            if (LineNumber > LastLineNumber)
            {
                LastLineNumber = LineNumber;
                LastBranch = 0;
            }
            else
            {
                string branch = string.Empty;
                var n = ++LastBranch;
                while (n > 0)
                {
                    n--;
                    branch = (char)('a' + n % 26) + branch;
                    n /= 26;
                }
                id += branch;
            }

            return id;
        }

        public PoAsset GetAsset()
        {
            var a = Asset;
            a.TransPairs = Pairs.ToArray();
            return a;
        }

        public string Project { get; protected set; } = string.Empty;

        public string TargetLanguage { get; protected set; } = string.Empty;

        private void ProcessMetadata()
        {
            foreach (var line in MessageStr.Split('\n'))
            {
                if (line.StartsWith("Project-Id-Version:", StringComparison.InvariantCultureIgnoreCase))
                {
                    Project = line.Substring("Project-Id-Version:".Length).Trim();
                }
                if (line.StartsWith("Language:", StringComparison.InvariantCultureIgnoreCase))
                {
                    TargetLanguage = line.Substring("Language:".Length).Trim();
                }
            }
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
