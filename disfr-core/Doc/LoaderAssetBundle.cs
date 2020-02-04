using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public class LoaderAssetBundle : IAssetBundle
    {
        public LoaderAssetBundle(string name, Func<IEnumerable<IAsset>> loader)
        {
            Loader = loader;
            Name = name;
            Assets = loader();
        }

        private readonly Func<IEnumerable<IAsset>> Loader;

        public string Name { get; private set; }

        public IEnumerable<IAsset> Assets { get; private set; }

        public bool CanRefresh { get { return true; } }

        public void Refresh() { Assets = Loader() ?? Assets; }
    }
}
