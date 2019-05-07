using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    /// <summary>
    /// Represents a single bilingual entry.
    /// </summary>
    /// <remarks>
    /// A <see cref="ITransPair"/> instance typically corresponds to a trans-unit element in XLIFF
    /// or (a bilingual portion of) a tu element in TMX. 
    /// </remarks>
    public interface ITransPair
    {
        /// <summary>
        /// Non-negative serial number in an asset.
        /// </summary>
        /// <remarks>
        /// Pairs with a positive value should be in their increasing order in the pairs' natural ordering.
        /// Zero or negative value means the pair represents an inter-segment content.
        /// </remarks>
        int Serial { get; }

        /// <summary>
        /// Identifier of a pair.
        /// </summary>
        /// <value>
        /// This is usually taken from the file, so the value depends on the CAT software that created it.
        /// </value>
        string Id { get; }

        /// <summary>
        /// The source text of this pair.
        /// </summary>
        /// <remarks>
        /// The value is always non-null.
        /// </remarks>
        InlineString Source { get; }

        /// <summary>
        /// The target text of this pair.
        /// </summary>
        /// <remarks>
        /// The value is always non-null.
        /// It means, even if the underlying file format (such as XLIFF) allows a distinction between the two cases
        /// that the corresponding translation doesn't exist and
        /// that the corresponding translation exists and is an empty text,
        /// disfr can't distinguish them.
        /// </remarks>
        InlineString Target { get; }

        /// <summary>
        /// The source language code as defined in RFC 4646.
        /// </summary>
        /// <remarks>
        /// The value can be null if the source language information is missing in the file.
        /// (Many bilingual file formats, e.g. XLIFF or TMX, mandate source language code,
        /// but it is still possible to create a violating file that doesn't provide source language code.
        /// disfr-core doesn't work around for the case by itself.)
        /// </remarks>
        string SourceLang { get; }

        /// <summary>
        /// The target language code as defined in RFC 4646.
        /// </summary>
        /// <remarks>
        /// The value can be null if the target language information is missing in the file.
        /// </remarks>
        string TargetLang { get; }

        /// <summary>
        /// A sequence of notes that are associated with this pair.
        /// </summary>
        /// <remarks>
        /// The value can be null if no note is assosicated with a pair.
        /// The value can also be an empty enumerable.
        /// The behavior depends on each implementation of this interface.
        /// </remarks>
        IEnumerable<string> Notes { get; }

        /// <summary>
        /// Additional properties of a pair.
        /// </summary>
        /// <param name="index">The index of a property to get the value of in <see cref="IAsset.Properties"/></param>
        /// <returns>The property value, or null if no property presents for this pair.</returns>
        string this[int index] { get; }
    }
}
