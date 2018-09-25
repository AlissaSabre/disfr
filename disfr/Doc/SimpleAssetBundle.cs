using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    class SimpleAssetBundle : IAssetBundle
    {
        public SimpleAssetBundle(IEnumerable<IAsset> assets)
        {
            _Assets = assets;
        }

        private IEnumerable<IAsset> _Assets;

        public IEnumerable<IAsset> Assets
        {
            get { return _Assets; }
        }
    }
}
