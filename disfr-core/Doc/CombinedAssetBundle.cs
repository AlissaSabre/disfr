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
            foreach (var b in Bundles)
            {
                _Assets.AddRange(b.Assets);
            }
            Name = name;
        }

        public string Name { get; private set; }

        private readonly IAssetBundle[] Bundles;

        private List<IAsset> _Assets;

        public IEnumerable<IAsset> Assets { get { return _Assets; } }
    }
}
