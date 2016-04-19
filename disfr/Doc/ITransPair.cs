using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public interface ITransPair
    {
        int Serial { get; }

        string Id { get; }

        InlineString Source { get; }

        InlineString Target { get; }

        string SourceLang { get; }

        string TargetLang { get; }

        IEnumerable<string> Notes { get; }

        IReadOnlyDictionary<string, string> Props { get; }
    }
}
