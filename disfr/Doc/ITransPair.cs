using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public interface ITransPair
    {
        /// <summary>
        /// Non-negative serial number in an asset.
        /// </summary>
        /// <value>
        /// Zero means the pair represents an inter-segment content.
        /// Pairs with a non-zero value should be in their increasing order in the pairs' natural ordering, except for zero.
        /// </value>
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
