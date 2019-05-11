using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace disfr.Doc
{
    /// <summary>
    /// Creates an <see cref="InlineString"/>.
    /// </summary>
    /// <remarks>
    /// This is the <see cref="InlineString"/> counterpart of <see cref="StringBuilder"/>,
    /// though its functionality is limited.
    /// </remarks>
    public class InlineBuilder : IEnumerable<InlineElement>
    {
        /// <summary>
        /// The contents of the <see cref="InlineString"/> being built.
        /// </summary>
        private readonly List<InlineElement> _Contents = new List<InlineElement>();

        /// <summary>
        /// Implements <see cref="IEnumerable.GetEnumerator()"/>.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Contents.GetEnumerator();
        }

        /// <summary>
        /// Implements <see cref="IEnumerable{InlineElement}.GetEnumerator()"/>
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<InlineElement> GetEnumerator()
        {
            return _Contents.GetEnumerator();
        }

        /// <summary>
        /// Gets or sets an inline property to be applied to the inline elements to be added.
        /// </summary>
        public InlineProperty Property
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Adds a <see cref="String"/> at the end of the <see cref="InlineString"/> being built.
        /// </summary>
        /// <param name="text">The string to add.</param>
        public void Add(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            AddString(text, null);
        }

        /// <summary>
        /// Adds an <see cref="InlineText"/> at the end of the <see cref="InlineString"/> being built.
        /// </summary>
        /// <param name="text">The <see cref="InlineText"/> to add.</param>
        public void Add(InlineText text)
        {
            if (text == null) throw new ArgumentNullException("text");
            AddString(text.Text, text);
        }

        /// <summary>
        /// Actually adds a string text or an inline text.
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
        /// Adds an inline tag at the end of the <see cref="InlineString"/> being built.
        /// </summary>
        /// <param name="tag"><see cref="InlineTag"/> to add.</param>
        public void Add(InlineTag tag)
        {
            if (tag == null) throw new ArgumentNullException("tag");
            _Contents.Add(tag);
        }

        internal void InternalAddRange(IEnumerable<InlineElement> contents)
        {
            foreach (var element in contents)
            {
                if (element is InlineText)
                {
                    var inline_text = (InlineText)element;
                    AddString(inline_text.Text, inline_text);
                }
                else if (element is InlineTag)
                {
                    _Contents.Add((InlineTag)element);
                }
                else
                {
                    throw new ApplicationException("internal error");
                }
            }
        }

        /// <summary>
        /// Adds a string, returning this instance.
        /// </summary>
        /// <param name="text">string to add.</param>
        /// <returns>this instance.</returns>
        public InlineBuilder Append(string text) { Add(text); return this; }

        /// <summary>
        /// Adds an inline text, returning this instance.
        /// </summary>
        /// <param name="text">InlineText to add.</param>
        /// <returns>this instance.</returns>
        public InlineBuilder Append(InlineText text) { Add(text); return this; }

        /// <summary>
        /// Adds an inline tag, returning this instance.
        /// </summary>
        /// <param name="tag">Inline tag to add.</param>
        /// <returns>this instance.</returns>
        public InlineBuilder Append(InlineTag tag) { Add(tag); return this; }

        /// <summary>
        /// Add the contents of another InlineString, returning this instance.
        /// </summary>
        /// <param name="inline">InlineString whose contents are added.</param>
        /// <returns>this instance.</returns>
        public InlineBuilder Append(InlineString inline) { Add(inline); return this; }

        /// <summary>
        /// Adds (the contents of) an <see cref="InlineString"/> at the end of the <see cref="InlineString"/> being built.
        /// </summary>
        /// <param name="inline"><see cref="InlineString"/> to add.</param>
        public void Add(InlineString inline)
        {
            if (inline == null) throw new ArgumentNullException("inline");

            foreach (var x in inline.Elements)
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
        /// Gets the contents of the <see cref="InlineString"/> being built.
        /// </summary>
        public IEnumerable<InlineElement> Contents { get { return _Contents; } }

        /// <summary>
        /// Gets whether this InlineString represents an empty string.
        /// </summary>
        public bool IsEmpty { get { return _Contents.Count == 0; } }

        /// <summary>
        /// Returns an <see cref="InlineString"/> that corresponds to the contents of this instance.
        /// </summary>
        /// <returns></returns>
        public InlineString ToInlineString()
        {
            return new InlineString(_Contents);
        }

        /// <summary>
        /// Removes all contents in this instance.
        /// </summary>
        public void Clear()
        {
            _Contents.Clear();
        }
    }

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
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class InlineString
    {
        private static readonly InlineElement[] EMPTY_CONTENTS = new InlineElement[0]; // Array.Empty<InlineElement>()

        /// <summary>
        /// Creates an empty inline string.
        /// </summary>
        /// <remarks>
        /// Use of this constructor is generally not recommended;
        /// it is usually better to use <see cref="Empty"/> instead,
        /// like <see cref="string.Empty"/> is preferred over <see cref="string()"/>.
        /// </remarks>
        public InlineString()
        {
            _Contents = EMPTY_CONTENTS;
        }

        /// <summary>
        /// Returns an empty inline string.
        /// </summary>
        /// <remarks>
        /// This object can be shared safely,
        /// because there is no way to alter its value/contents,
        /// like an array of length zero.
        /// </remarks>
        public static readonly InlineString Empty = new InlineString();

        public InlineString(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            if (text.Length == 0)
            {
                _Contents = EMPTY_CONTENTS;
            }
            else
            {
                _Contents = new InlineText[] { text };
            }
        }

        public InlineString(params InlineElement[] contents) : this(contents as IEnumerable<InlineElement>) { }

        public InlineString(IEnumerable<InlineElement> contents)
        {
            var array = contents as InlineElement[] ?? contents.ToArray();
            if (array == null || array.Length == 0)
            {
                _Contents = EMPTY_CONTENTS;
            }
            else if (array.Length == 1)
            {
                _Contents = array;
            }
            else
            {
                var builder = new InlineBuilder();
                builder.InternalAddRange(contents);
                _Contents = builder.Contents.ToArray();
            }
        }

        internal InlineString(List<InlineElement> contents)
        {
            _Contents = contents.ToArray();
        }

        private readonly InlineElement[] _Contents;

        /// <summary>
        /// Gets all elements with their properties.
        /// </summary>
        public IEnumerable<InlineElementWithProperty> ElementsWithProperties { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Gets all elements in this inline string.
        /// </summary>
        public IEnumerable<InlineElement> Elements { get { return _Contents; } }

        /// <summary>
        /// Gets all tags in this inline string.
        /// </summary>
        public IEnumerable<InlineTag> Tags { get { return _Contents.OfType<InlineTag>(); } }

        /// <summary>
        /// Gets whether this InlineString represents an empty string.
        /// </summary>
        public bool IsEmpty { get { return _Contents.Length == 0; } }

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
            if (that._Contents.Length != _Contents.Length) return false;
            for (int i = 0; i < _Contents.Length; i++)
            {
                if (!that._Contents[i].Equals(_Contents[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// Get a string representation of this InlineString.
        /// </summary>
        /// <returns>The string representation.</returns>
        /// <remarks>
        /// This is equivalent to call <see cref="ToString(InlineToString)"/> with <see cref="InlineToString.Normal"/>.
        /// </remarks>
        public override string ToString()
        {
            return string.Concat<InlineElement>(_Contents);
        }

        /// <summary>
        /// Get a string representation of this InlineString.
        /// </summary>
        /// <param name="options">A set of options to control the string representation.</param>
        /// <returns>The string representation.</returns>
        public string ToString(InlineToString options)
        {
            return string.Concat(_Contents.Select(inline => inline.ToString(options)));
        }

        /// <summary>
        /// Gets presentation of this object.
        /// </summary>
        /// <remarks>
        /// It is for VisualStudio debuggers and for testing.
        /// </remarks>
        public string DebuggerDisplay => ToString(InlineToString.Debug);
    }

    public struct InlineElementWithProperty
    {
        public InlineElement Element;

        public InlineProperty Property;
    }

    /// <summary>
    /// Represents a property designated for a part of an <see cref="InlineString"/>.
    /// </summary>
    public enum InlineProperty
    {
        None = 0,

        Ins,

        Del,

        Emp,
    }

    /// <summary>
    /// A type (direction/association) of an inline tag.
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
    /// <remarks>
    /// InlineTag is <i>mostly</i> immutable.
    /// An exception is that the <see cref="Number"/> property can be
    /// assigned a value after an instance has been created,
    /// but its value can't be changed once assigned.
    /// In other words, an <see cref="InlineTag"/> instance has only two states:
    /// whether <see cref="Number"/> is not assigned yet or already assigned,
    /// and the transition of the state is one-way.
    /// </remarks>
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
                type.GetHashCode() +
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
        /// Gets a string representation of this InlineTag.
        /// </summary>
        /// <param name="options">A set of options to control the string representation.</param>
        /// <returns>The string representation.</returns>
        /// <remarks>
        /// This method of <see cref="InlineTag"/> only cares <see cref="InlineToString.TagMask"/> flags.
        /// </remarks>
        public override string ToString(InlineToString options)
        {
            switch (options & InlineToString.TagMask)
            {
                case InlineToString.TagDebug:
                    return string.Format("{0}{1};{2}{3}", '{', Name, Id, '}');
                case InlineToString.TagHidden:
                    return "";
                case InlineToString.TagCode:
                    return Code;
                case InlineToString.TagNumber:
                    return string.Format("{0}{1}{2}", '{', Number, '}');
                case InlineToString.TagLabel:
                    return string.Format("{0}{1}{2}", '{', Name, '}');
                default:
                    throw new ApplicationException("Internal Error");
            }
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

        /// <summary>
        /// Gets a string representation of this InlineText.
        /// </summary>
        /// <param name="options">A set of options to control the string representation.</param>
        /// <returns>The string representation.</returns>
        /// <remarks>
        /// This method of <see cref="InlineText"/> only cares <see cref="InlineToString.TextMask"/> flags.
        /// </remarks>
        public override string ToString(InlineToString options)
        {
            return Text;
        }
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    public abstract class InlineElement
    {
        /// <summary>
        /// Gets a string representation of this InlineElement.
        /// </summary>
        /// <returns>A string representation.</returns>
        /// <remarks>
        /// This is equivalent to call <see cref="ToString(InlineToString)"/> with <see cref="InlineToString.Normal"/>
        /// </remarks>
        public override string ToString()
        {
            return ToString(InlineToString.Normal);
        }

        /// <summary>
        /// Gets a string representation of this InlineElement.
        /// </summary>
        /// <param name="options">A set of options to control the string representation.</param>
        /// <returns>The string representation.</returns>
        public abstract string ToString(InlineToString options);

        /// <summary>
        /// Gets presentation of this object.
        /// </summary>
        /// <remarks>
        /// It is for VisualStudio debuggers and for testing.
        /// </remarks>
        public string DebuggerDisplay => ToString(InlineToString.Debug);
    }

    /// <summary>
    /// Options to control <see cref="InlineElement.ToString(InlineToString)"/> and <see cref="InlineString.ToString(InlineToString)"/>.
    /// </summary>
    [Flags]
    public enum InlineToString
    {
        /// <summary>
        /// A representation suitable for normal uses.
        /// </summary>
        Normal = TagCode | TextLatest,

        /// <summary>
        /// A representation suitable for debugging.
        /// </summary>
        Debug = TagDebug | TextDebug,

        /// <summary>
        /// Any tag is hidden completely.
        /// </summary>
        TagHidden = 0,

        /// <summary>
        /// Any tag is replaced by its code.
        /// </summary>
        TagCode = 1,

        /// <summary>
        /// Any tag is replaced by a representation based on a local matching number.
        /// </summary>
        TagNumber = 2,

        /// <summary>
        /// Any tag is replaced by its label.
        /// </summary>
        TagLabel = 3,

        /// <summary>
        /// Any tag is replaced by a representation suitable for debugging.
        /// </summary>
        TagDebug = 4,

        /// <summary>
        /// Mask for Tag-controlling options.
        /// </summary>
        TagMask = TagDebug | TagHidden | TagCode | TagNumber | TagLabel,

        TextLatest = 16 * 0,

        TextOld = 16 * 1,

        TextAll = 16 * 2,

        TextDebug = 16 * 3,

        TextMask = TextDebug | TextLatest | TextOld | TextAll,
    }
}
