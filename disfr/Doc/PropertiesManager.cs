using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public class PropertiesManager
    {
        private readonly Dictionary<string, int> Indexes = new Dictionary<string, int>();

        private readonly List<bool> VisibleVector = new List<bool>();

        public void Put(ref string[] vector, string key, string value, bool Visible)
        {
            int index;
            if (!Indexes.TryGetValue(key, out index))
            {
                index = Indexes[key] = Indexes.Count;
                while (VisibleVector.Count <= index) VisibleVector.Add(false);
            }
            if (Visible) VisibleVector[index] = true;

            var v = vector;
            if (v == null || v.Length <= index)
            {
                var u = new string[Indexes.Count]; // XXX: Or, should we allocate [Index + 1] or in-between?  
                if (v != null) Array.Copy(v, u, v.Length);
                v = vector = u;
            }
            v[index] = value;
        }

        public string Get(string[] values, string key)
        {
            int index;
            if (!Indexes.TryGetValue(key, out index)) return null;
            if (index >= values.Length) return null;
            return values[index];
        }

        public IEnumerable<PropInfo> Infos
        {
            get { return Indexes.OrderBy(p => p.Value).Select(p => new PropInfo(p.Key, VisibleVector[p.Value])); }
        }
    }
}
