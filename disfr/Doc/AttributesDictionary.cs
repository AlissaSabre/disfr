using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace disfr.Doc
{
    public class AttributesDictionary : IReadOnlyDictionary<string, string>
    {
        private readonly XElement Element;

        public AttributesDictionary(XElement metadata)
        {
            Element = metadata;
        }

        public string this[string key]
        {
            get
            {
                string value;
                if (!TryGetValue(key, out value)) throw new KeyNotFoundException(key);
                return value;
            }
        }

        public int Count
        {
            get
            {
                return Keys.Count();
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return Element.Attributes().Select(a => a.Name.ToString()).Distinct();
            }
        }

        public IEnumerable<string> Values
        {
            get
            {
                return Keys.Select(key => (string)Element.Attribute(key));
            }
        }

        public bool ContainsKey(string key)
        {
            if (key == null) throw new ArgumentNullException("key");
            return Element.Attribute(key) != null;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Keys.Select(key => new KeyValuePair<string, string>(key, this[key])).GetEnumerator();
        }

        public bool TryGetValue(string key, out string value)
        {
            if (key == null) throw new ArgumentNullException("key");
            var attr = Element.Attribute(key);
            value = (string)attr;
            return attr != null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
