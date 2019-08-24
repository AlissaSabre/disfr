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

        private readonly HashSet<string> Visibles = new HashSet<string>();

        public int GetIndex(string key)
        {
            int index;
            if (!Indexes.TryGetValue(key, out index))
            {
                index = Indexes.Count;
                Indexes.Add(key, index);
            }
            return index;
        }

        public void AddKeys(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (!Indexes.ContainsKey(key))
                {
                    Indexes.Add(key, Indexes.Count); 
                }
            }
        }

        public void Put(ref string[] vector, string key, string value)
        {
            var index = GetIndex(key);
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
            if (values == null) return null;
            int index;
            if (!Indexes.TryGetValue(key, out index)) return null;
            if (index >= values.Length) return null;
            return values[index];
        }

        public void MarkVisible(string key)
        {
            Visibles.Add(key);
        }

        public IEnumerable<PropInfo> Infos
        {
            get { return Indexes.OrderBy(p => p.Value).Select(p => new PropInfo(p.Key, Visibles.Contains(p.Key))); }
        }
    }
}
