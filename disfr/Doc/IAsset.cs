using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disfr.Doc
{
    /// <summary>
    /// Represents an asset, which corresponds to a file element in XLIFF.
    /// </summary>
    public interface IAsset
    {
        /// <summary>
        /// Local identifier of a pckage in which this asset is read from. 
        /// </summary>
        /// <remarks>
        /// This is usually a local full path name of the package file.
        /// </remarks>
        string Package { get; }

        /// <summary>
        /// The identifier of this asset as in the originating environment.
        /// </summary>
        /// <remarks>
        /// It is the value of /xliff/file/@original if this asset is from a standard XLIFF 1.2 file.
        /// </remarks>
        string Original { get; }
        
        /// <summary>
        /// The source language code as defined by RFC 4646.
        /// </summary>
        string SourceLang { get; }
        
        /// <summary>
        /// The target language code as defined by RFC 4646.
        /// </summary>
        string TargetLang { get; }

        /// <summary>
        /// Sequence of translation pairs that make up this asset.
        /// </summary>
        IEnumerable<ITransPair> TransPairs { get; }

        /// <summary>
        /// Sequence of alternative translation pairs.
        /// </summary>
        /// <remarks>
        /// <see cref="AltPairs"/> is primarity to represent alt-trans elements in XLIFF,
        /// collectively which are often called <i>project translation memory</i>.
        /// </remarks>
        IEnumerable<ITransPair> AltPairs { get; }

        /// <summary>
        /// List of properties that <see cref="ITransPair"/>s of this asset may have.
        /// </summary>
        /// <remarks>
        /// The returned object will be readonly.
        /// The integer index value in the list is important;
        /// it will be used to access <see cref="P:ITransPair.Item(Int32)"/>.
        /// </remarks>
        IList<PropInfo> Properties { get; }
    }

    /// <summary>
    /// Represents an additional property an <see cref="ITransPair"/> instance may have.
    /// </summary>
    public class PropInfo
    {
        /// <summary>
        /// The name of this property.
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// Whether this property should be initially visible to the user.
        /// </summary>
        public readonly bool Visible;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="key">The <see cref="Key"/> value.</param>
        /// <param name="visible">The <see cref="Visible"/> value.</param>
        public PropInfo(string key, bool visible = false)
        {
            Key = key;
            Visible = visible;
        }
    }
}
