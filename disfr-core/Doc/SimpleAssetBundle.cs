﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public class SimpleAssetBundle : IAssetBundle
    {
        public SimpleAssetBundle(IEnumerable<IAsset> assets, string name)
        {
            Assets = assets;
            Name = name;
        }

        public string Name { get; private set; }

        public IEnumerable<IAsset> Assets { get; private set; }

        public bool CanRefresh { get { return false; } }

        public void Refresh() { }
    }
}
