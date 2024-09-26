using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace disfr.po
{

    class PoAssetSink : CollectorSinkBase
    {
        private readonly PoAsset Asset = new PoAsset();

        private readonly List<PoTransPair> Pairs = new List<PoTransPair>();

        private int Serial = 0;

        public bool Collected { get; private set; }

        protected override void Finish()
        {
            var pid = FormPairId();
            var collected = Collected;
            Collected = true;
            if (!collected && MessageId.Length == 0)
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
                    .Concat(ExtractedComments.Select(s => "Extracted: " + s))
                    .ToArray(),
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
                    TargetLanguage = LangUtils.PosixLocaleLabelToXmlLang(line.Substring("Language:".Length).Trim());
                }
            }
        }
    }
}
