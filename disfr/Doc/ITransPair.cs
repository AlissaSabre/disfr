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
        /// Zero or negative value means the pair represents an inter-segment content.
        /// Pairs with a non-zero value should be in their increasing order in the pairs' natural ordering, except for zeroes and negatives.
        /// </value>
        int Serial { get; }

        /// <summary>
        /// Identifier of a pair.
        /// </summary>
        /// <value>
        /// This is usually taken from the file, so the value depends on the CAT software that created it.
        /// </value>
        string Id { get; }

        InlineString Source { get; }

        InlineString Target { get; }

        string SourceLang { get; }

        string TargetLang { get; }

        IEnumerable<string> Notes { get; }

        /// <summary>
        /// Additional properties of a pair.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string this[string key] { get; }

        /// <summary>
        /// Returns all keys of additional properties that this pair <b>may</b> have.
        /// </summary>
        /// <remarks>
        /// This property is tentative; it should be removed soon.
        /// </remarks>
        IEnumerable<string> PropKeys { get; }
    }
}
