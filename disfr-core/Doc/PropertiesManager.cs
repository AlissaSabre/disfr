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
            KeyCount = 0;
        }

        private readonly object Lock;

        private readonly IDictionary<string, int> Indexes;

        private volatile int KeyCount;

        private readonly ConcurrentDictionary<string, string> Visibles = new ConcurrentDictionary<string, string>();

        public int GetIndex(string key)
        {
            int index;
            if (!Indexes.TryGetValue(key, out index))
            {
                if (Lock == null)
                {
                    index = KeyCount++;
                    Indexes.Add(key, index);
                }
                else
                {
                    lock (Lock)
                    {
                        index = KeyCount;
                        if (((ConcurrentDictionary<string, int>)Indexes).TryAdd(key, index))
                        {
                            KeyCount++;
                        }
                        else
                        {
                            index = Indexes[key];
                        }
                    }
                }
            }
            return index;
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
                // A sort of a silly workaround... FIXME. 
                var u = new string[Math.Max(index + 1, KeyCount)];
                if (v != null)
                {
                    //Array.Copy(v, 0, u, 0, v.Length);
                    for (int i = 0; i < v.Length; i++)
                    {
                        u[i] = v[i];
                    }
                }
                vector = v = u;
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
