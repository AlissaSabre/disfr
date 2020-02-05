using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public class CombinedAssetBundle : IAssetBundle
    {
        public CombinedAssetBundle(IEnumerable<IAssetBundle> bundles, string name)
        {
            Bundles = bundles.ToArray();
            _Assets = new List<IAsset>();
            Name = name;
            FlattenBundles();
        }

        private void FlattenBundles()
        {
            _Assets.Clear();
            foreach (var b in Bundles)
            {
                _Assets.AddRange(b.Assets);
            }
        }

        public string Name { get; private set; }

        private readonly IAssetBundle[] Bundles;

        private readonly List<IAsset> _Assets;

        public IEnumerable<IAsset> Assets { get { return _Assets; } }

        public bool CanRefresh => Bundles.Any(b => b.CanRefresh);

        public void Refresh()
        {
            foreach (var b in Bundles)
            {
                b.Refresh();
            }
            FlattenBundles();
        }
    }
}
