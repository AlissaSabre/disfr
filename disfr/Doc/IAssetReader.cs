using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public interface IAssetReader
    {
        string Name { get; }

        int Priority { get; }

        IList<string> FilterString { get; }

        IEnumerable<IAsset> Read(string filename, int filterindex);
    }
}
