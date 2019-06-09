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
    /// <para>
    /// This is the <see cref="InlineString"/> counterpart of <see cref="StringBuilder"/>,
    /// though its functionality is limited.
    /// </para>
    /// <para>
    /// This class implements <see cref="IEnumerable{T}"/> primarily to allow use of collection initializer.
    /// The actual enumeration of <see cref="InlineRunWithProperty"/> instances is not intended for use by
    /// application codes.
    /// (Test and debug codes use it, though.)
    /// </para>
    /// </remarks>
    public class InlineBuilder : IEnumerable<InlineRunWithProperty>
    {
        /// <summary>
        /// The contents of the <see cref="InlineString"/> being built.
        /// </summary>
        private readonly List<InlineRunWithProperty> _Contents = new List<InlineRunWithProperty>();

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
        public IEnumerator<InlineRunWithProperty> GetEnumerator()
        {
            return _Contents.GetEnumerator();
        }

        /// <summary>
        /// Gets or sets an inline property to be applied to the inline elements to be added.
        /// </summary>
        public InlineProperty Property { get; set; }

        /// <summary>
        /// Sets an inline property to <see cref="Property"/>.
        /// </summary>
        /// <param name="property">New inline property.</param>
        /// <remarks>
        /// This method is intended for use by collection initializers.
        /// </remarks>
        public void Add(InlineProperty property)
        {
            Property = property;
        }

        /// <summary>
        /// Adds a <see cref="String"/> at the end of the <see cref="InlineString"/> being built.
        /// </summary>
        /// <param name="text">The string to add.</param>
        public void Add(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            InternalAddText(text, null);
        }

        /// <summary>
        /// Adds an <see cref="InlineText"/> at the end of the <see cref="InlineString"/> being built.
        /// </summary>
        /// <param name="text">The <see cref="InlineText"/> to add.</param>
        public void Add(InlineText text)
        {
            if (text == null) throw new ArgumentNullException("text");
            InternalAddText(text.Text, text);
        }

        /// <summary>
        /// Actually adds a string text or an inline text.
        /// </summary>
        /// <param name="text">A string version of the text.</param>
        /// <param name="inline_text">An InlineText version of the text if available, or null otherwise.</param>
        internal void InternalAddText(string text, InlineText inline_text)
        {
            if (text.Length == 0) return;
            if (_Contents.Count > 0
                && _Contents[_Contents.Count - 1].Property == Property
                && _Contents[_Contents.Count - 1].Run is InlineText)
            {
                var x = _Contents[_Contents.Count - 1].Run as InlineText;
                _Contents[_Contents.Count - 1] = new InlineRunWithProperty(Property, new InlineText(x.Text + text));
            }
            else
            {
                _Contents.Add(new InlineRunWithProperty(Property, inline_text ?? new InlineText(text)));
            }
        }

        /// <summary>
        /// Adds an inline tag at the end of the <see cref="InlineString"/> being built.
        /// </summary>
        /// <param name="tag"><see cref="InlineTag"/> to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tag"/> is null.</exception>
        public void Add(InlineTag tag)
        {
            if (tag == null) throw new ArgumentNullException("tag");
            InternalAddTag(tag);
        }

        /// <summary>
        /// Creates ands add an <see cref="InlineTag"/>.
        /// </summary>
        /// <param name="type">Tag type.</param>
        /// <param name="id">"id" identifier of tag.</param>
        /// <param name="rid">"rid" identifier of tag.</param>
        /// <param name="name">Name of this tag, which is usually a local part of the tag name in the bilingual markup language.</param>
        /// <param name="ctype">A string to indicates a purpose of this tag, as in ctype attribute of XLIFF inline tags.  It may be null.</param>
        /// <param name="display">A user friendly label of this tag.  It may be null.</param>
        /// <param name="code">A string representation of underlying code in the source of this tag.  It may be null.</param>
        /// <exception cref="ArgumentNullException">Any of <paramref name="id"/>, <paramref name="rid"/> or <paramref name="name"/> is null.</exception>
        public void Add(Tag type, string id, string rid, string name, string ctype = null, string display = null, string code = null)
        {
            InternalAddTag(new InlineTag(type, id, rid, name, ctype, display, code));
        }

        internal void InternalAddTag(InlineTag tag)
        {
            _Contents.Add(new InlineRunWithProperty(Property, tag));
        }

        internal void InternalAddRange(IEnumerable<InlineRun> contents)
        {
            foreach (var element in contents)
            {
                var inline_text = element as InlineText;
                if (!(inline_text is null))
                {
                    InternalAddText(inline_text.Text, inline_text);
                    continue;
                }

                var inline_tag = element as InlineTag;
                if (!(inline_tag is null))
                {
                    InternalAddTag(inline_tag);
                    continue;
                }

                throw new ApplicationException("internal error");
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

        ///// <summary>
        ///// Add the contents of another InlineString, returning this instance.
        ///// </summary>
        ///// <param name="inline">InlineString whose contents are added.</param>
        ///// <returns>this instance.</returns>
        //public InlineBuilder Append(InlineString inline) { Add(inline); return this; }

        ///// <summary>
        ///// Adds (the contents of) an <see cref="InlineString"/> at the end of the <see cref="InlineString"/> being built.
        ///// </summary>
        ///// <param name="inline"><see cref="InlineString"/> to add.</param>
        //public void Add(InlineString inline)
        //{
        //    if (inline == null) throw new ArgumentNullException("inline");

        //    foreach (var x in inline.Contents)
        //    {
        //        if (x is InlineText)
        //        {
        //            Add((x as InlineText).Text);
        //        }
        //        else if (x is InlineTag)
        //        {
        //            Add(x as InlineTag);
        //        }
        //        else
        //        {
        //            throw new ApplicationException("internal error");
        //        }
        //    }
        //}

        /// <summary>
        /// Gets the internal contents being built.
        /// </summary>
        internal List<InlineRunWithProperty> InternalContents { get { return _Contents; } }

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
            return IsEmpty ? InlineString.Empty : new InlineString(_Contents);
        }

        /// <summary>
        /// Removes all contents in this instance.
        /// </summary>
        /// <param name="keep_property">
        /// True to keep the current value of <see cref="Property"/> after clear.
        /// False to reset <see cref="Property"/> to its default value during clear.
        /// </param>
        public void Clear(bool keep_property = false)
        {
            _Contents.Clear();
            if (!keep_property) Property = default(InlineProperty);
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
    /// This version of <see cref="InlineString"/> doesn't implement <see cref="IEnumerable{T}"/>.
    /// Use <see cref="RunsWithProperties"/>, <see cref="Contents"/> or <see cref="Enumerate(InlineToString)"/> to enumerate contents.
    /// </para>
    /// <para>
    /// Note that we used to support <i>special characters</i> in InlineString.
    /// I now consider they blong to presentation but infoset,
    /// so the support for special characters in InlineString has been removed.
    /// It is now included in <see cref="disfr.UI.PairRenderer"/>.
    /// </para>
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class InlineString : IEquatable<InlineString>
    {
        private static readonly InlineRunWithProperty[] EMPTY_CONTENTS = new InlineRunWithProperty[0]; // Array.Empty<InlineRunWithProperty>()

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
        /// Gets an empty inline string.
        /// </summary>
        /// <remarks>
        /// This object can be shared safely,
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
                _Contents = new InlineRunWithProperty[] { new InlineRunWithProperty(text) };
            }
        }

        public InlineString(params InlineRun[] contents) : this(contents as IEnumerable<InlineRun>) { }

        public InlineString(IEnumerable<InlineRun> contents)
        {
            if (contents == null) throw new ArgumentNullException("contents");
            if (contents.Any())
            {
                var builder = new InlineBuilder();
                builder.InternalAddRange(contents);
                _Contents = builder.InternalContents.ToArray();
            }
            else
            {
                _Contents = EMPTY_CONTENTS;
            }
        }

        internal InlineString(List<InlineRunWithProperty> contents)
        {
            _Contents = contents.ToArray();
        }

        private readonly InlineRunWithProperty[] _Contents;

        /// <summary>
        /// Gets all contents with their properties.
        /// </summary>
        /// <remarks>
        /// This property enumerates all contents, including those usually not presented to the user,
        /// e.g., a deleted section in a change-tracked document.
        /// You need to filter runs with properties appropriately 
        /// </remarks>
        public IEnumerable<InlineRunWithProperty> RunsWithProperties { get { return _Contents; } }

        /// <summary>
        /// Gets all tags in this inline string.
        /// </summary>
        /// <remarks>
        /// This property enumerates all tags, including those usually not presented to the user,
        /// e.g., a deleted tag in a change-tracked document.
        /// </remarks>
        public IEnumerable<InlineTag> Tags { get { return _Contents.Select(rwp => rwp.Run as InlineTag).Where(t => !(t is null)); } }

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
            int m = 263; // a number chosen arbitrarily;
            int h = _Contents.Length ^ 0x5ab0e273; // a magic number chosen arbitrarily.
            for (int i = _Contents.Length - 1, d = 0; i >= 0; i -= ++d)
            {
                h = h * m + _Contents[i].Run.GetHashCode();
            }
            return h;
        }

        /// <summary>
        /// Provides contents based equality.
        /// </summary>
        /// <param name="inline">Another <see cref="InlineString"/> to compare.</param>
        /// <returns>True if equal.</returns>
        public bool Equals(InlineString inline)
        {
            if (inline == null) return false;
            if (inline._Contents.Length != _Contents.Length) return false;
            for (int i = 0; i < _Contents.Length; i++)
            {
                if (!inline._Contents[i].Equals(_Contents[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// Provides contents based equality.
        /// </summary>
        /// <param name="obj">Another object to test equality.</param>
        /// <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as InlineString);
        }

        public static bool operator ==(InlineString x, InlineString y)
        {
            return (x is null) ? (y is null) : x.Equals(y);
        }

        public static bool operator !=(InlineString x, InlineString y)
        {
            return !(x == y);
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
            return ToString(InlineToString.Normal);
        }

        /// <summary>
        /// Get a string representation of this InlineString.
        /// </summary>
        /// <param name="options">A set of options to control the string representation.</param>
        /// <returns>The string representation.</returns>
        public string ToString(InlineToString options)
        {
            var debug = options.HasFlag(InlineToString.ShowProp);
            var hide = (InlineProperty)(options & InlineToString.HideMask);
            var b = new StringBuilder();
            var prop = InlineProperty.None;
            foreach (var rwp in _Contents)
            {
                if ((rwp.Property & hide) == 0)
                {
                    var s = rwp.Run.ToString(options);
                    if (debug && s.Length > 0 && prop != rwp.Property)
                    {
                        prop = rwp.Property;
                        b.Append("{" + prop + "}");
                    }
                    b.Append(s);
                }
            }
            return b.ToString();
        }

        /// <summary>
        /// Gets presentation of this object.
        /// </summary>
        /// <remarks>
        /// It is for VisualStudio debuggers and for testing.
        /// </remarks>
        public string DebuggerDisplay => ToString(InlineToString.Debug);
    }

    /// <summary>
    /// Represents a run with a property in an <see cref="InlineString"/>.
    /// </summary>
    public struct InlineRunWithProperty : IEquatable<InlineRunWithProperty>
    {
        public readonly InlineProperty Property;

        public readonly InlineRun Run;

        public InlineRunWithProperty(InlineProperty property, InlineRun run)
        {
            if (run is null) throw new ArgumentNullException("run");
            Property = property;
            Run = run;
        }

        public InlineRunWithProperty(InlineRun run)
        {
            Property = InlineProperty.None;
            Run = run;
        }

        public InlineRunWithProperty(string text)
        {
            Property = InlineProperty.None;
            Run = new InlineText(text);
        }

        public override int GetHashCode()
        {
            return (int)Property + Run.GetHashCode();
        }

        public bool Equals(InlineRunWithProperty rwp)
        {
            return Property == rwp.Property && Run.Equals(rwp.Run); 
        }

        public override bool Equals(object obj)
        {
            return (obj is InlineRunWithProperty) && Equals((InlineRunWithProperty)obj);
        }

        public static bool operator ==(InlineRunWithProperty x, InlineRunWithProperty y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(InlineRunWithProperty x, InlineRunWithProperty y)
        {
            return !x.Equals(y);
        }
    }

    /// <summary>
    /// Represents a property designated for a part of an <see cref="InlineString"/>.
    /// </summary>
    /// <remarks>
    /// The codes using <see cref="InlineProperty"/> assume that <see cref="None"/> is the default value (i.e., 0).
    /// You should not change it.
    /// Also, other values relate to values of <see cref="InlineToString"/>.
    /// Be careful when you are changing them.
    /// </remarks>
    public enum InlineProperty
    {
        None = 0,

        Ins = 0x01,

        Del = 0x02,

        Emp = 0x04,
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
    public class InlineTag : InlineRun, IEquatable<InlineTag>
    {
        private const string OPAR = "{";

        private const string CPAR = "}";

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
        }

        /// <summary>
        /// Returns a hash value.
        /// </summary>
        /// <returns>The hash value.</returns>
        /// <remarks>
        /// Only a subset of members is considered, primarily for performance.
        /// </remarks>
        public override int GetHashCode()
        {
            return _EqualityComparer.GetHashCode(this);
        }

        /// <summary>
        /// Tests equality to an <see cref="InlineTag"/> object.
        /// </summary>
        /// <param name="tag">Another <see cref="InlineTag"/> object.</param>
        /// <returns>True if and only if all (public) members of <paramref name="tag"/> have values equal to this object.</returns>
        public bool Equals(InlineTag tag)
        {
            if (tag is null) return false;
            if (ReferenceEquals(this, tag)) return true;
            return
                tag.TagType == TagType &&
                tag.Id == Id &&
                tag.Rid == Rid &&
                tag.Name == Name &&
                tag.Ctype == Ctype &&
                tag.Display == Display &&
                tag.Code == Code &&
                tag._Number == _Number;
        }

        /// <summary>
        /// Tests equality using <see cref="InlineTag.Equals(InlineTag)"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as InlineTag);
        }

        public static bool operator ==(InlineTag x, InlineTag y)
        {
            return (x is null) ? (y is null) : x.Equals(y);
        }

        public static bool operator !=(InlineTag x, InlineTag y)
        {
            return !(x == y);
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
                    return string.Format("{0}{1};{2}{3}", OPAR, Name, Id, CPAR);
                case InlineToString.TagHidden:
                    return string.Empty;
                case InlineToString.TagCode:
                    return Code ?? string.Empty;
                case InlineToString.TagNumber:
                    return Enclose(Number);
                case InlineToString.TagDisplay:
                    return Enclose(Display, Name);
                default:
                    throw new ArgumentException("options");
            }
        }

        /// <summary>
        /// Produces an enclosed presentation of a tag based on the given candidate properties.
        /// </summary>
        /// <param name="items">List of candidate properties.</param>
        /// <returns>A string of form "{xxx}", where xxx is the string presentation of the first non-null item.</returns>
        /// <remarks>
        /// IF the string presentation of the appropriate item is already enclosed in '{' and '}', we don't add our own encloser. 
        /// </remarks>
        private static string Enclose(params object[] items)
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    var s = item.ToString();
                    if (s.StartsWith(OPAR) && s.EndsWith(CPAR))
                    {
                        return s;
                    }
                    else
                    {
                        return OPAR + s + CPAR;
                    }
                }
            }

            return OPAR + CPAR;
        }

        /// <summary>
        /// Compares two tags by a more loose way than <see cref="InlineTag.Equals(InlineTag)"/>,
        /// which should be suitable for matching tags in source and target inline strings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// </para>
        /// It considers <see cref="InlineTag.TagType"/>, <see cref="InlineTag.Id"/>, 
        /// <see cref="InlineTag.Rid"/> and <see cref="InlineTag.Name"/> only.
        /// <para>
        /// A thread-safe immutable instance of this class is available from <see cref="InlineTag.LooseEqualityComparer"/>.
        /// </para>
        /// </remarks>
        protected class LooseInlineTagEqualityComparer : IEqualityComparer<InlineTag>
        {
            public bool Equals(InlineTag x, InlineTag y)
            {
                if (ReferenceEquals(x, y)) return true;
                return
                    x.TagType == y.TagType &&
                    x.Id == y.Id &&
                    x.Rid == y.Rid &&
                    x.Name == y.Name;
            }

            public int GetHashCode(InlineTag x)
            {
                return
                    x.TagType.GetHashCode() +
                    x.Id.GetHashCode() * 3 +
                    x.Rid.GetHashCode() * 5 +
                    x.Name.GetHashCode() * 17;
            }
        }

        private static readonly LooseInlineTagEqualityComparer _EqualityComparer = new LooseInlineTagEqualityComparer();

        /// <summary>
        /// Gets an <see cref="IEqualityComparer"/> instance that compares tags 
        /// by a more loose way than <see cref="InlineTag.Equals(InlineTag)"/>,
        /// which should be suitable for matching tags in source and target inline strings.
        /// </summary>
        public static IEqualityComparer<InlineTag> LooseEqualityComparer { get { return _EqualityComparer; } }
    }

    /// <summary>
    /// Represents a text portion in <see cref="InlineString"/>.
    /// </summary>
    /// <remarks>
    /// InlineText is immutable.
    /// </remarks>
    public class InlineText : InlineRun, IEquatable<InlineText>
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

        public bool Equals(InlineText text)
        {
            return Text == text?.Text;
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

        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// Gets a string representation of this InlineText.
        /// </summary>
        /// <param name="options">Not used in <see cref="InlineText.ToString(InlineToString)"/>.</param>
        /// <returns>The string representation.</returns>
        /// <remarks>
        /// This method always returns its <see cref="Text"/>, ignoring <paramref name="options"/>.
        /// </remarks>
        public override string ToString(InlineToString options)
        {
            return Text;
        }
    }

    /// <summary>
    /// Represents a run in an <see cref="InlineString"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an abstract class with two subclasses: <see cref="InlineText"/> and <see cref="InlineTag"/>.
    /// This class as well as its subclasses provide value-based equality via
    /// overridden <see cref="Object.Equals(object)"/>, == and !=.
    /// (<see cref="InlineRun"/> doesn't override <see cref="Object.Equals(object)"/>,
    /// but it is OK.
    /// An abstract class has no instance of its own, 
    /// and all its subclasses overrride <see cref="Object.Equals(object)"/> appropriately.
    /// </para>
    /// </remarks>
#pragma warning disable CS0660, CS0661
    [DebuggerDisplay("{DebuggerDisplay}")]
    public abstract class InlineRun
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

        public static bool operator ==(InlineRun x, InlineRun y)
        {
            return (x is null) ? (y is null) : x.Equals(y);
        }

        public static bool operator !=(InlineRun x, InlineRun y)
        {
            return !(x == y);
        }
    }
#pragma warning restore CS0660, CS0661

    /// <summary>
    /// Options to control <see cref="InlineRun.ToString(InlineToString)"/> and <see cref="InlineString.ToString(InlineToString)"/>.
    /// </summary>
    /// <remarks>
    /// The default value of <see cref="InlineToString"/> is not useful.
    /// <see cref="Normal"/> designates a practical default behaviour.
    /// </remarks>
    [Flags]
    public enum InlineToString
    {
        /// <summary>
        /// A representation suitable for normal uses.
        /// </summary>
        /// <remarks>
        /// This option shows the latest version of texts with original representation of tags.
        /// </remarks>
        Normal = TagCode | HideDel,

        /// <summary>
        /// A variant of <see cref="Normal"/> to show before-change texts. 
        /// </summary>
        /// <remarks>
        /// This option shows the before-change version of texts with original representation of tags.
        /// </remarks>
        Older = TagCode | HideIns,

        /// <summary>
        /// A representation suitable for flat applications.
        /// </summary>
        /// <remarks>
        /// This options shows the latest texts with all tags removed.
        /// </remarks>
        Flat = TagHidden | HideDel,

        /// <summary>
        /// A representation suitable for debugging and testing.
        /// </summary>
        Debug = TagDebug | ShowProp,

        /// <summary>
        /// Any tag is replaced by its code.
        /// </summary>
        TagCode = 0x0100,

        /// <summary>
        /// Any tag is replaced by a representation based on a local matching number.
        /// </summary>
        TagNumber = 0x0200,

        /// <summary>
        /// Any tag is replaced by its label.
        /// </summary>
        TagDisplay = 0x0400,

        /// <summary>
        /// Any tag is replaced by a representation suitable for debugging.
        /// </summary>
        TagDebug = 0x8000,

        /// <summary>
        /// Any tag is hidden completely.
        /// </summary>
        TagHidden = 0x0000,

        /// <summary>
        /// Mask for Tag-controlling options.
        /// </summary>
        TagMask = 0xFF00,

        /// <summary>
        /// Hides runs with <see cref="InlineProperty.Ins"/>.
        /// </summary>
        HideIns = InlineProperty.Ins,

        /// <summary>
        /// Hides runs with <see cref="InlineProperty.Del"/>.
        /// </summary>
        HideDel = InlineProperty.Del,

#if false // For the moment, we have no use case to hide an emphisized run.
        /// <summary>
        /// Hides runs with <see cref="InlineProperty.Emp"/>.
        /// </summary>
        HideEmp = InlineProperty.Emp,
#endif

        /// <summary>
        /// Mask for run hiding options.
        /// </summary>
        HideMask = InlineProperty.Ins | InlineProperty.Del /* | InlineProperty.Emp */,

        /// <summary>
        /// Shows effective <see cref="InlineProperty"/> values.
        /// </summary>
        /// <remarks>
        /// This is for testing/debugging purpose.
        /// </remarks>
        ShowProp = 0x80,
    }
}
