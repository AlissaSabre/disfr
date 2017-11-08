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
    /// Many bilingual file supports a notion of <i>tags</i> within text data.
    /// Some file also supports a notion of <i>special character</i>.
    /// <see cref="InlineString"/> is a substitution of a <see cref="string"/> type,
    /// whose contents include not just ordinary characters but also tags and special characters. 
    /// </remarks>
    public class InlineString : IEnumerable<object>
    {
        private static readonly char[] SpecialChars = new char[]
        {
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', 
            '\u0008', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F',
            '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017',
            '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F',
            '\u0020', '\u007F', 
            '\u0080', '\u0081', '\u0082', '\u0083', '\u0084', '\u0085', '\u0086', '\u0087',
            '\u0088', '\u0089', '\u008A', '\u008B', '\u008C', '\u008D', '\u008E', '\u008F',
            '\u0090', '\u0091', '\u0092', '\u0093', '\u0094', '\u0095', '\u0096', '\u0097',
            '\u0098', '\u0099', '\u009A', '\u009B', '\u009C', '\u009D', '\u009E', '\u009F',
            '\u00A0', 
            '\u1680', '\u180E',
            '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007',
            '\u2008', '\u2009', '\u200A', '\u200B', '\u200C', '\u200D',
            '\u2028', '\u2029', '\u202F', '\u205F',
            '\u2060', '\u2061', '\u2062', '\u2063',
            '\u3000', '\u3164',
            '\uFFA0', '\uFEFF',
        };

        private static readonly Dictionary<char, InlineChar> InlineChars = new Dictionary<char, InlineChar>();

        static InlineString()
        {
            foreach (char c in SpecialChars)
            {
                InlineChars[c] = new InlineChar(c);
            }
        }

        private readonly List<object> _Contents = new List<object>();

        /// <summary>
        /// An accumulated hash value of this object.
        /// </summary>
        /// <remarks>
        /// The initial value is a magic number determined by me (chosen at random and fixed.)
        /// </remarks>
        private int _HashCode = 0x5ab0e273;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Contents.GetEnumerator();
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _Contents.GetEnumerator();
        }

        public IEnumerable<object> Contents { get { return _Contents; } }

        public void Add(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            if (text == "") return;
            int p = 0, q;
            while ((q = text.IndexOfAny(SpecialChars, p)) >= 0)
            {
                if (q > p) AddString(text.Substring(p, q - p));
                AddChar(InlineChars[text[q]]);
                p = q + 1;
            }
            if (p < text.Length) AddString(text.Substring(p));
        }

        private void AddString(string text)
        {
            if (_Contents.Count > 0 && _Contents[_Contents.Count - 1] is string)
            {
                var x = _Contents[_Contents.Count - 1] as string;
                var y = x + text;
                _Contents[_Contents.Count - 1] = y;
                _HashCode = _HashCode - x.GetHashCode() + y.GetHashCode();
            }
            else
            {
                _Contents.Add(text);
                _HashCode += (_HashCode >> 9) + text.GetHashCode();
            }
        }

        private void AddChar(InlineChar c)
        {
            _Contents.Add(c);
            _HashCode += (_HashCode >> 1) + c.GetHashCode();
        }

        public void Add(InlineTag tag)
        {
            if (tag == null) throw new ArgumentNullException("tag");
            _Contents.Add(tag);
            _HashCode += (_HashCode >> 7) + tag.GetHashCode();
        }

        public InlineString Append(string text) { Add(text); return this; }

        public InlineString Append(InlineTag tag) { Add(tag); return this; }

        public InlineString Append(InlineString inline)
        {
            if (inline == null) throw new ArgumentNullException("inline");
            IEnumerable<object> contents;
            if (Object.ReferenceEquals(inline._Contents, _Contents))
            {
                contents = inline._Contents.ToArray();
            }
            else
            {
                contents = inline._Contents;
            }
            foreach (var x in contents)
            {
                if (x is string)
                {
                    Add((string)x);
                }
                else if (x is InlineTag)
                {
                    Add((InlineTag)x);
                }
                else
                {
                    throw new ApplicationException("internal error");
                }
            }
            return this;
        }

        public bool IsEmpty { get { return _Contents.Count == 0; } }

        public override int GetHashCode() { return _HashCode; }

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

    public class InlineTag
    {
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

        public InlineChar(char char_data)
        {
            Char = char_data;
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
            return Char.ToString();
        }
    }

}
