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
        
        string SourceLang { get; }
        
        string TargetLang { get; }

        IEnumerable<ITransPair> TransPairs { get; }

        IEnumerable<ITransPair> AltPairs { get; }

        /// <summary>
        /// List of properties that <see cref="ITransPair"/>s of this asset may have.
        /// </summary>
        /// <remarks>
        /// The returned object will be readonly.
        /// </remarks>
        IList<PropInfo> Properties { get; }
    }

    /// <summary>
    /// Represents an additional property of an <see cref="ITransPair"/>.
    /// </summary>
    public class PropInfo
    {
        /// <summary>
        /// A key to pass to <see cref="ITransPair[string]"/> to get the property value.
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// Whether this property should be initially visible to the user.
        /// </summary>
        public readonly bool Visible;

        public PropInfo(string key, bool visible = false)
        {
            Key = key;
            Visible = visible;
        }
    }
}
