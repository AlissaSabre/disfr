using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disfr.Doc
{
    /// <summary>
    /// Represents a sort of a <i>rich</i> text.
    /// </summary>
    /// <remarks>
    /// Many bilingual files support a notion of <i>tags</i> within text data.
    /// Some also support some additional properties for emphasis, insertion or deletion.
    /// <see cref="InlineString"/> is a substitution of a <see cref="string"/> type,
    /// whose contents include not just ordinary characters but also tags with some properties. 
    /// Note that we used to support <i>special characters</i> in InlineString.
    /// I now consider they blongs to presentation but infoset,
    /// so the support for special characters in InlineString has been removed.
    /// </remarks>
    public class InlineString : IEnumerable<object>
    {
        //private static readonly char[] SpecialChars = new char[]
        //{
        //    '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', 
        //    '\u0008', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F',
        //    '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017',
        //    '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F',
        //    '\u0020', '\u007F', 
        //    '\u0080', '\u0081', '\u0082', '\u0083', '\u0084', '\u0085', '\u0086', '\u0087',
        //    '\u0088', '\u0089', '\u008A', '\u008B', '\u008C', '\u008D', '\u008E', '\u008F',
        //    '\u0090', '\u0091', '\u0092', '\u0093', '\u0094', '\u0095', '\u0096', '\u0097',
        //    '\u0098', '\u0099', '\u009A', '\u009B', '\u009C', '\u009D', '\u009E', '\u009F',
        //    '\u00A0', 
        //    '\u1680', '\u180E',
        //    '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007',
        //    '\u2008', '\u2009', '\u200A', '\u200B', '\u200C', '\u200D',
        //    '\u2028', '\u2029', '\u202F', '\u205F',
        //    '\u2060', '\u2061', '\u2062', '\u2063',
        //    '\u3000', '\u3164',
        //    '\uFFA0', '\uFEFF',
        //};

        //private static readonly Dictionary<char, InlineChar> InlineChars;

        //static InlineString()
        //{
        //    InlineChars = new Dictionary<char, InlineChar>(SpecialChars.Length * 2);
        //    foreach (char c in SpecialChars)
        //    {
        //        InlineChars[c] = new InlineChar(c);
        //    }
        //    InlineCharChecker = BuildICC();
        //}

        ///// <summary>
        ///// A sort of a direct perfect hash table to substitute InlineChars.ContainsKey. 
        ///// </summary>
        //private static readonly char[] InlineCharChecker;

        ///// <summary>
        ///// Builds an InlineCharChecker.
        ///// </summary>
        ///// <returns>The InlineCharChecker.</returns>
        //private static char[] BuildICC()
        //{
        //    for (int n = SpecialChars.Length; ; n++)
        //    {
        //        var icc = TryBuildICC(n);
        //        if (icc != null) return icc;
        //    }
        //}

        ///// <summary>
        ///// Tries to build an InlineCharChecker of size <paramref name="size"/>.
        ///// </summary>
        ///// <param name="size">The desired size of the InlineCharChecker.</param>
        ///// <returns>The InlineCharChecker of size <paramref name="size"/>, or null if not found.</returns>
        //private static char[] TryBuildICC(int size)
        //{
        //    var icc = new char[size];
        //    foreach (var c in SpecialChars)
        //    {
        //        var i = c % size;
        //        if (i == 0 && c != 0) return null;
        //        if (icc[i] != 0) return null;
        //        icc[i] = c;
        //    }
        //    return icc;
        //}

        /// <summary>
        /// The contents of this InlineString.
        /// </summary>
        /// <remarks>
        /// Although the element type is declared being object, 
        /// the list can actually contain elements either of the three types:
        /// string, InlineTag and InlineChar.
        /// </remarks>
        private readonly List<object> _Contents = new List<object>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Contents.GetEnumerator();
        }

        /// <summary>
        /// Implements IEnumerator{object}.GetEnumerator().
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<object> GetEnumerator()
        {
            return _Contents.GetEnumerator();
        }

        /// <summary>
        /// Gets the contents of this InlineString as an enumerable.
        /// </summary>
        public IEnumerable<object> Contents { get { return _Contents; } }

        /// <summary>
        /// Add a string at the end to this InlineString.
        /// </summary>
        /// <param name="text">The string to add.</param>
        /// <remarks>
        /// Any special characters in <paramref name="text"/> will be searched and isolated.
        /// </remarks>
        public void Add(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
//#if !UNSAFE
//            int mod = InlineCharChecker.Length;
//            for (int p = 0, q; p < text.Length; p = q + 1)
//            {
//                for (q = p; q < text.Length && text[q] != InlineCharChecker[text[q] % mod]; q++) ;
//                if (p < q) AddString(text.Substring(p, q - p));
//                if (q < text.Length) AddChar(InlineChars[text[q]]);
//            }
//#else
//            unsafe
//            {
//                int mod = InlineCharChecker.Length;
//                fixed (char* s = text, icc = InlineCharChecker)
//                {
//                    char* r = s + text.Length;
//                    for (char* p = s, q; p < r; p = q + 1)
//                    {
//                        char c;
//                        for (q = p; q < r && (c = *q) != icc[c % mod]; q++) ;
//                        if (p < q) AddString(text.Substring((int)(p - s), (int)(q - p)));
//                        if (q < r) AddChar(InlineChars[*q]);
//                    }
//                }
//            }
//#endif
            AddString(text);
        }

        /// <summary>
        /// Add a string to this InlineString.
        /// </summary>
        /// <param name="text">string to add.</param>
        /// <remarks>
        /// The string is added as it is; no special characters must occur in <paramref name="text"/>. 
        /// </remarks>
        private void AddString(string text)
        {
            if (_Contents.Count > 0 && _Contents[_Contents.Count - 1] is string)
            {
                var x = _Contents[_Contents.Count - 1] as string;
                var y = x + text;
                _Contents[_Contents.Count - 1] = y;
            }
            else
            {
                _Contents.Add(text);
            }
        }

        private void AddChar(InlineChar c)
        {
            _Contents.Add(c);
        }

        /// <summary>
        /// Add an inline tag at the end of this inline string.
        /// </summary>
        /// <param name="tag"></param>
        public void Add(InlineTag tag)
        {
            if (tag == null) throw new ArgumentNullException("tag");
            _Contents.Add(tag);
        }

        /// <summary>
        /// Adds a string, returning this instance.
        /// </summary>
        /// <param name="text">string to add.</param>
        /// <returns>this instance.</returns>
        public InlineString Append(string text) { Add(text); return this; }

        /// <summary>
        /// Adds an inline tag, returning this instance.
        /// </summary>
        /// <param name="tag">Inline tag to add.</param>
        /// <returns>this instance.</returns>
        public InlineString Append(InlineTag tag) { Add(tag); return this; }

        /// <summary>
        /// Add the contents of another InlineString, returning this instance.
        /// </summary>
        /// <param name="inline">InlineString whose contents are added.</param>
        /// <returns>this instance.</returns>
        public InlineString Append(InlineString inline) { Add(inline); return this; }

        /// <summary>
        /// Add the contents of another InlineString at the end of this InlineString.
        /// </summary>
        /// <param name="inline">InlineString whose contents are added.</param>
        public void Add(InlineString inline)
        {
            if (inline == null) throw new ArgumentNullException("inline");
            if (Object.ReferenceEquals(inline._Contents, _Contents))
            {
                throw new ArgumentException("Can't add contents of an InlineString to itself.", "inline");
            }

            foreach (var x in inline._Contents)
            {
                if (x is string)
                {
                    AddString((string)x);
                }
                else if (x is InlineTag)
                {
                    Add((InlineTag)x);
                }
                else if (x is InlineChar)
                {
                    AddChar((InlineChar)x);
                }
                else
                {
                    throw new ApplicationException("internal error");
                }
            }
        }

        /// <summary>
        /// Gets whether this InlineString represents an empty string.
        /// </summary>
        public bool IsEmpty { get { return _Contents.Count == 0; } }

        /// <summary>
        /// Calculates and returns a content based hash code. 
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            var h = 0x5ab0e273; // a magic number.
            foreach (var c in _Contents)
            {
                h += (h << 9) + c.GetHashCode();
            }
            return h;
        }

        /// <summary>
        /// Provides contents based equality.
        /// </summary>
        /// <param name="obj">Another object to test equality.</param>
        /// <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            var that = obj as InlineString;
            if (that == null) return false;
            if (that._Contents.Count != _Contents.Count) return false;
            for (int i = 0; i < _Contents.Count; i++)
            {
                if (!that._Contents[i].Equals(_Contents[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// Get a string representation of this InlineString.
        /// </summary>
        /// <returns>
        /// This method is primarily for debug purposes.
        /// </returns>
        public override string ToString()
        {
            return String.Concat(_Contents);
        }
    }

    /// <summary>
    /// A type of an inline tag.
    /// </summary>
    public enum Tag
    {
        /// <summary>
        /// Indicates a tag that begins something, e.g., &lt;em>.
        /// </summary>
        B,

        /// <summary>
        /// Indicates a tag that ends something, e.g., &lt;/em>.
        /// </summary>
        E,

        /// <summary>
        /// Indicates a tag that has no specific beginning/ending semantics, e.g., &lt;image ... />
        /// </summary>
        S,
    };

    /// <summary>
    /// Represents a tag in an <see cref="InlineString"/>.
    /// </summary>
    public class InlineTag
    {
        /// <summary>
        /// A type of a tag.
        /// </summary>
        public readonly Tag TagType;

        public readonly string Id;

        public readonly string Rid;

        public readonly string Name;

        public readonly string Ctype;

        public readonly string Display;

        public readonly string Code;

        private int _Number = 0;

        public int Number
        {
            get { return _Number; }
            set
            {
                if (_Number != 0) throw new InvalidOperationException("Number is already assigned.");
                _Number = value;
            }
        }

        private readonly int _HashCode;

        private static readonly int[] TagHash =
        {
            0x494096b1,
            0x75003b1a,
            0x62c97b81,
        };

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="type">Tag type.</param>
        /// <param name="id">"id" identifier of tag.</param>
        /// <param name="rid">"rid" identifier of tag.</param>
        /// <param name="name">Name of this tag, which is usually a local part of the tag name in the bilingual markup language.</param>
        /// <param name="ctype">A string to indicates a purpose of this tag, as in ctype attribute of XLIFF inline tags.  It may be null.</param>
        /// <param name="display">A user friendly label of this tag.  It may be null.</param>
        /// <param name="code">A string representation of underlying code in the source of this tag.  It may be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/>, <paramref name="rid"/> or <paramref name="name"/> is null.</exception>
        public InlineTag(Tag type, string id, string rid, string name, string ctype, string display, string code)
        {
            if (id == null) throw new ArgumentNullException("id");
            if (rid == null) throw new ArgumentNullException("rid");
            if (name == null) throw new ArgumentNullException("name");

            TagType = type;
            Id = id;
            Rid = rid;
            Name = name;
            Ctype = ctype;
            Display = display;
            Code = code;

            _HashCode =
                TagHash[(int)type] +
                id.GetHashCode() * 3 +
                rid.GetHashCode() * 5 +
                name.GetHashCode() * 17;
        }

        /// <summary>
        /// Returns a hash value from <see cref="TagType"/>, <see cref="Id"/>, <see cref="Rid"/> and <see cref="Name"/>.
        /// </summary>
        /// <returns>The hash value.</returns>
        public override int GetHashCode() { return _HashCode; }

        /// <summary>
        /// Tests equality to an <see cref="InlineTag"/> object.
        /// </summary>
        /// <param name="obj">Another <see cref="InlineTag"/> object.</param>
        /// <returns>True if <paramref name="obj"/> is an equal <see cref="InlineTag"/> object.</returns>
        /// <remarks>
        /// Two <see cref="InlineTag"/> objects are considered equal if and only if
        /// they have equal <see cref="TagType"/>, <see cref="Id"/>, <see cref="Rid"/> and <see cref="Name"/>.
        /// Other members, i.e., <see cref="Ctype"/>, <see cref="Display"/>, <see cref="Code"/> and <see cref="Number"/>
        /// are not considered.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj)) return true;
            var that = obj as InlineTag;
            return !object.ReferenceEquals(that, null) &&
                that.TagType == TagType &&
                that.Id == Id &&
                that.Rid == Rid &&
                that.Name == Name;
        }

        /// <summary>
        /// Get a string representation of this InlineTag.
        /// </summary>
        /// <returns>
        /// A string primarily for debug purposes.
        /// </returns>
        public override string ToString()
        {
            return "{" + Name + ";" + Id + "}";
        }
    }

    public class InlineChar
    {
        public readonly char Char;

        private readonly string String;

        public InlineChar(char char_data)
        {
            Char = char_data;
            String = Char.ToString();
        }

        public override bool Equals(object obj)
        {
            return (obj is InlineChar) && ((InlineChar)obj).Char == Char;
        }

        public override int GetHashCode()
        {
            return Char.GetHashCode();
        }

        public override string ToString()
        {
            return String;
        }
    }

}
