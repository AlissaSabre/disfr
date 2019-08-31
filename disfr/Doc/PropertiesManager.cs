using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public class PropertiesManager
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="concurrent">True to create a thread safe instance, false otherwise.</param>
        public PropertiesManager(bool concurrent)
        {
            Lock = concurrent ? new object() : null;
            Indexes = concurrent
                ? (IDictionary<string, int>)new ConcurrentDictionary<string, int>()
                : (IDictionary<string, int>)new Dictionary<string, int>();
        }

        private readonly object Lock;

        private readonly IDictionary<string, int> Indexes;

        private readonly ConcurrentDictionary<string, string> Visibles = new ConcurrentDictionary<string, string>();

        public int GetIndex(string key)
        {
            int index;
            if (!Indexes.TryGetValue(key, out index))
            {
                if (Lock == null)
                {
                    index = Indexes.Count;
                    Indexes.Add(key, index);
                }
                else
                {
                    lock (Lock)
                    {
                        index = Indexes.Count;
                        if (!((ConcurrentDictionary<string, int>)Indexes).TryAdd(key, index))
                        {
                            index = Indexes[key];
                        }
                    }
                }
            }
            return index;
        }

        public void AddKeys(IEnumerable<string> keys)
        {
            bool taken = false;
            try
            {
                if (Lock != null) Monitor.Enter(Lock, ref taken);
                foreach (var key in keys)
                {
                    if (!Indexes.ContainsKey(key))
                    {
                        Indexes.Add(key, Indexes.Count);
                    }
                }
            }
            finally
            {
                if (taken) Monitor.Exit(Lock);
            }
        }

        /// <summary>
        /// Puts a property value into a property vector.
        /// </summary>
        /// <param name="vector">Property vector.</param>
        /// <param name="key">Name of the property.</param>
        /// <param name="value">Property value.</param>
        /// <remarks>
        /// This method is thread safe as long as the following two conditions are met:
        /// <list type="*">
        /// <item>This <see cref="PropertiesManager"/> instance was created with its constructor's concurrent parameter set to true.</item>
        /// <item>The same <paramref name="vector"/> is not passed to any method of <see cref="PropertiesManager"/> at the same time.</item>
        /// </list>
        /// </remarks>
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

        /// <summary>
        /// Gets a property value from a property vector.
        /// </summary>
        /// <param name="vector">Property vector.</param>
        /// <param name="key">Name of the property.</param>
        /// <param name="value">Property value.</param>
        /// <remarks>
        /// This method is thread safe as long as the following two conditions are met:
        /// <list type="*">
        /// <item>This <see cref="PropertiesManager"/> instance was created with its constructor's concurrent parameter set to true.</item>
        /// <item>The same <paramref name="vector"/> is not passed to any method of <see cref="PropertiesManager"/> at the same time.</item>
        /// </list>
        /// </remarks>
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
            Visibles.TryAdd(key, key);
        }

        public IList<PropInfo> Properties
        {
            get
            {
                var v = Visibles.Keys;
                return Indexes
                    .OrderBy(p => p.Value)
                    .Select(p => new PropInfo(p.Key, v.Contains(p.Key)))
                    .ToList().AsReadOnly();
            }
        }
    }
}
