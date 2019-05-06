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
    /// <para>
    /// Many bilingual files support a notion of <i>tags</i> within text data.
    /// Some also support some additional properties for emphasis, insertion or deletion.
    /// <see cref="InlineString"/> is a substitution of a <see cref="string"/> type,
    /// whose contents include not just ordinary characters but also tags with some properties. 
    /// </para>
    /// <para>
    /// Note that we used to support <i>special characters</i> in InlineString.
    /// I now consider they blong to presentation but infoset,
    /// so the support for special characters in InlineString has been removed.
    /// It is now included in <see cref="disfr.UI.PairRenderer"/>.
    /// </para>
    /// </remarks>
    public class InlineString : IEnumerable<InlineElement>
    {
        /// <summary>
        /// The contents of this InlineString.
        /// </summary>
        private readonly List<InlineElement> _Contents = new List<InlineElement>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Contents.GetEnumerator();
        }

        /// <summary>
        /// Implements IEnumerator{InlineElement}.GetEnumerator().
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<InlineElement> GetEnumerator()
        {
            return _Contents.GetEnumerator();
        }

        /// <summary>
        /// Gets the contents of this InlineString as an enumerable.
        /// </summary>
        public IEnumerable<InlineElement> Contents { get { return _Contents; } }

        /// <summary>
        /// Adds a text at the end to this InlineString.
        /// </summary>
        /// <param name="text">The text to add.</param>
        public void Add(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            AddString(text, null);
        }

        /// <summary>
        /// Adds an inline text at the end.
        /// </summary>
        /// <param name="text">The inline text to add.</param>
        public void Add(InlineText text)
        {
            if (text == null) throw new ArgumentNullException("text");
            AddString(text.Text, text);
        }

        /// <summary>
        /// Adds a string text or an inline text at the end of this InlineString.
        /// </summary>
        /// <param name="text">A string version of the text.</param>
        /// <param name="inline_text">An InlineText version of the text if available, or null otherwise.</param>
        private void AddString(string text, InlineText inline_text)
        {
            if (text.Length == 0) return;
            if (_Contents.Count > 0 && _Contents[_Contents.Count - 1] is InlineText)
            {
                var x = _Contents[_Contents.Count - 1] as InlineText;
                var y = x.Text + text;
                _Contents[_Contents.Count - 1] = new InlineText(y);
            }
            else
            {
                _Contents.Add(inline_text ?? new InlineText(text));
            }
        }

        /// <summary>
        /// Adds an inline tag at the end of this inline string.
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
        /// Adds an inline text, returning this instance.
        /// </summary>
        /// <param name="text">InlineText to add.</param>
        /// <returns>this instance.</returns>
        public InlineString Append(InlineText text) { Add(text); return this; }

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
        /// <remarks>
        /// This method works like a concatenation, though the first InlineString is updated.
        /// </remarks>
        public void Add(InlineString inline)
        {
            if (inline == null) throw new ArgumentNullException("inline");
            if (Object.ReferenceEquals(inline._Contents, _Contents))
            {
                throw new ArgumentException("Can't add contents of an InlineString to itself.", "inline");
            }

            foreach (var x in inline._Contents)
            {
                if (x is InlineText)
                {
                    Add((x as InlineText).Text);
                }
                else if (x is InlineTag)
                {
                    Add(x as InlineTag);
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
            var shift = 9; // an arbitrarily chosen number.
            var h = 0x5ab0e273; // a magic number.
            foreach (var c in _Contents)
            {
                h += (h << shift) + c.GetHashCode();
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
    public class InlineTag : InlineElement
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

    /// <summary>
    /// Represents a text portion in <see cref="InlineString"/>.
    /// </summary>
    /// <remarks>
    /// InlineText is immutable.
    /// </remarks>
    public class InlineText : InlineElement
    {
        /// <summary>
        /// The Text data.
        /// </summary>
        public readonly string Text;

        public InlineText(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            Text = text;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Text == (obj as InlineText)?.Text;
        }

        public override string ToString()
        {
            return Text;
        }

        public static bool operator ==(InlineText x, InlineText y)
        {
            return x?.Text == y?.Text;
        }

        public static bool operator !=(InlineText x, InlineText y)
        {
            return x?.Text != y?.Text;
        }

        public static explicit operator string(InlineText x)
        {
            return x?.Text;
        }

        public static implicit operator InlineText(string s)
        {
            return s == null ? null : new InlineText(s);
        }
    }

    public class InlineElement
    {

    }

}
