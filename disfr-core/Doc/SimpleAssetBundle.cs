using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public class SimpleAssetBundle : IAssetBundle
    {
        private readonly Func<IEnumerable<IAsset>> Reloader;

        public SimpleAssetBundle(IEnumerable<IAsset> assets, string name, Func<IEnumerable<IAsset>> reloader = null)
        {
            Assets = assets;
            Name = name;
            Reloader = reloader;
        }

        public string Name
        {
            get; private set;
        }

        public IEnumerable<IAsset> Assets
        {
            get; private set;
        }

        public bool CanRefresh { get { return Reloader != null; } }

        public void Refresh()
        {
            if (Reloader != null)
            {
                Assets = Reloader();
            }
        }
    }
}
