using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    /// <summary>
    /// An asset bundle built around an asset-loader delegate.
    /// </summary>
    public class LoaderAssetBundle : IAssetBundle
    {
        private LoaderAssetBundle() { }

        /// <summary>
        /// Creates an asset bundle around a loader delegate.
        /// </summary>
        /// <param name="name">(Human friendly) name of bundle.</param>
        /// <param name="loader">Delegate to (re)load assets.</param>
        /// <returns>The asset bundle.</returns>
        /// <remarks>
        /// <paramref name="loader"/> may return a null or throw an exception upon failure.
        /// This method invokes it once before returning,
        /// and it returns a null if the invokation returned a null
        /// or throws an exception if it threw one.
        /// </remarks>
        public static LoaderAssetBundle Create(string name, Func<IEnumerable<IAsset>> loader)
        {
            var assets = loader();
            if (assets == null) return null;

            return new LoaderAssetBundle()
            {
                Loader = loader,
                Name = name,
                Assets = assets,
            };
        }

        private Func<IEnumerable<IAsset>> Loader;

        public string Name { get; private set; }

        public IEnumerable<IAsset> Assets { get; private set; }

        public bool CanRefresh { get { return true; } }

        /// <summary>
        /// Refreshes the bundle by invoking the underlying loader delegate.
        /// </summary>
        /// <remarks>
        /// The loader delegate may indicate a failure by returning a null or throwing an exception.
        /// If it returned a null, this method returns silently.
        /// If the delegate threw an exception, this method doesn't try catching it.
        /// The bundle contents are unchanged in either case.
        /// </remarks>
        public void Refresh()
        {
            var asset = Loader();
            if (asset != null) Assets = asset;
        }
    }
}
