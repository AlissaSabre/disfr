using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disfr.UI
{
    /// <summary>
    /// Represents display decorations for a character.
    /// </summary>
    /// <see cref="GlossyString"/>
    /// <remarks>
    /// <para>Diff/Edits</para>
    /// <list type="table">
    /// <item><term>COM</term><description>Common/Unchanged.</description></item>
    /// <item><term>INS</term><description>Right-only/Iserted.</description></item>
    /// <item><term>DEL</term><description>Left-only/Deleted.</description></item>
    /// </list>
    /// <para>Tags</para>
    /// <list type="table">
    /// <item><term>NOR</term><description>Normal text.</description></item>
    /// <item><term>TAG</term><description>Inline tag.</description></item>
    /// <item><term>SYM</term><description>Special symbol.</description></item>
    /// <item><term>ALT</term><description>Special symbol in visible (alternative) presentation.</description></item>
    /// </list> 
    /// </remarks>
    [Flags]
    public enum Gloss
    {
        /// <summary>Indicates no glosses are applied.</summary>
        None = 0,

        // Diff/edits/emphasis.
        /// <summary>Indicates common/unchantes parts.</summary>
        COM = 0,
        /// <summary>Indicates right-only/inserted parts.</summary>
        INS = 1,
        /// <summary>Indicates left-only/deleted parts.</summary>
        DEL = 2,

        // Tags
        /// <summary>Indicates normal texts.</summary>
        NOR = 0,
        /// <summary>Indicates inline tags.</summary>
        TAG = 4,
        /// <summary>Indicates special symbols.</summary>
        SYM = 8,
        /// <summary>Indicates special symbols in alternative presentation.</summary>
        ALT = 16,

        // To be used by diagnostic tools.
        /// <summary>Indicates emphasized texts.</summary>
        EMP = 32,

        // Other
        /// <summary>Possibly used for hit test in the future.</summary>
        HIT = 64,
    }

    /// <summary>
    /// A string-ish type that can represent some display attribution for each character.
    /// </summary>
    /// <remarks>
    /// <para>
    /// GlossyString is like <see cref="string"/> type,
    /// but it can also holds some display attribution for each character.
    /// </para>
    /// <para>
    /// A display attribution is dedicated for disfr and is something like
    /// "this is a tag" or "this is a special symbol", 
    /// or "this is an inserted fragment" or "this is a deleted fragment".
    /// </para>
    /// <para>A display attribution is represented by <see cref="Gloss"/> type.
    /// You can show a <see cref="GlossyString"/> data to a user by using <see cref="GlossyTextBlock"/>.
    /// </para>
    /// </remarks>
    public class GlossyString : System.Collections.IEnumerable
    {
        public struct Pair
        {
            public string Text;
            public Gloss Gloss;

            public Pair(string text, Gloss gloss) { Text = text; Gloss = gloss; }
        }

        /// <summary>
        /// Create an empty unfrozen instance.
        /// </summary>
        public GlossyString()
        {
            Pairs = new List<Pair>();
            _Frozen = false;
        }

        /// <summary>
        /// Create a frozen instance equivalent to a single string.
        /// </summary>
        /// <param name="text"></param>
        public GlossyString(string text) : this(text, Gloss.None) {}

        /// <summary>
        /// Create a frozen instance of a single string in a uniform gloss.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="gloss"></param>
        public GlossyString(string text, Gloss gloss)
        {
            if (text == null) throw new ArgumentNullException("text");
            if (text.Length == 0)
            {
                Pairs = new Pair[0];
            }
            else
            {
                Pairs = new Pair[] { new Pair(text, gloss) };
            }
            _Frozen = true;
        }

        /// <summary>
        /// Create a frozen copy of a GlossyString.
        /// </summary>
        /// <param name="original"></param>
        public GlossyString(GlossyString original)
        {
            Pairs = original.Pairs.ToArray();
            _Frozen = true;
        }

        public static readonly GlossyString Empty = new GlossyString("");

        private readonly IList<Pair> Pairs;

        private bool _Frozen;

        public bool Frozen
        {
            get { return _Frozen; }
            set
            {
                if (value == _Frozen) return;
                if (value == false) throw new InvalidOperationException("Unfreezing a frozen GlossyString.");
                _Frozen = true;
            }
        }

        private int LastAdded = -1;

        public GlossyString Append(string text) { return Append(text, Gloss.None); }

        public GlossyString Append(string text, Gloss gloss)
        {
            if (text == null) throw new ArgumentNullException("text");
            if (_Frozen) throw new InvalidOperationException("Appending to a frozen GlossyString.");

            LastAdded = text.Length;
            if (text.Length > 0)
            {
                if (Pairs.Count > 0 && Pairs[Pairs.Count - 1].Gloss == gloss)
                {
                    Pairs[Pairs.Count - 1] = new Pair(Pairs[Pairs.Count - 1].Text + text, gloss);
                }
                else
                {
                    Pairs.Add(new Pair(text, gloss));
                }
            }
            return this;
        }

        /// <summary>
        /// <i>Add</i> a bool value to <see cref="Frozen"/> property.
        /// This method is intended for use with Collection Initializers.
        /// </summary>
        /// <param name="frozen"></param>
        public void Add(bool frozen)
        {
            if (_Frozen) throw new InvalidOperationException("Adding Frozen flag to a frozen GlossyString.");
            Frozen = frozen;
        }

        /// <summary>
        /// Add a string segment without any <see cref="Gloss"/>.
        /// </summary>
        /// <param name="text"></param>
        public void Add(string text) { Append(text, Gloss.None); }

        /// <summary>
        /// Change the <see cref="Gloss"/> for the last added segment.
        /// This method is intended for use with Collection Initializers. 
        /// </summary>
        /// <param name="gloss"></param>
        public void Add(Gloss gloss)
        {
            if (_Frozen) throw new InvalidOperationException("Modifying gloss of a frozen GlossyString.");
            if (LastAdded < 0) throw new InvalidOperationException("Modifying gloss of a GlossyString without a valid segment.");

            var last = LastAdded;
            LastAdded = -1;
            if (last == 0) return;

            var p = Pairs[Pairs.Count - 1];
            if (p.Text.Length == last)
            {
                Pairs[Pairs.Count - 1] = new Pair(p.Text, gloss);
            }
            else
            {
                Pairs[Pairs.Count - 1] = new Pair(p.Text.Substring(0, p.Text.Length - last), p.Gloss);
                Pairs.Add(new Pair(p.Text.Substring(p.Text.Length - last), gloss));
            }
        }

        /// <summary>
        /// Get String (text) portion of this GlossyString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// Get String (text) portion of this GlossyString using alternate symbol presentation. 
        /// </summary>
        /// <returns></returns>
        public string ToString(bool altsym)
        {
            if (Pairs.Count == 0) return "";
            if (Pairs.Count == 1) return Pairs[0].Text;

            var to_drop = altsym ? Gloss.SYM : Gloss.ALT;
            var sb = new StringBuilder();
            foreach (var p in Pairs)
            {
                if (!p.Gloss.HasFlag(to_drop)) sb.Append(p.Text);
            }
            return sb.ToString();
        }

        public IEnumerator GetEnumerator()
        {
            return Pairs.GetEnumerator();
        }

        /// <summary>
        /// Enumerate over all Text-Gloss pairs.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Currently this method returns GlossyString's internal object directly,
        /// so it could be dangerous if the return value is casted and abused.
        /// Consider wrapping if provided for general public.
        /// </remarks>
        public ICollection<Pair> AsCollection()
        {
            return Pairs;
        }

        /// <summary>
        /// Get a frozen version of this GlossyString.
        /// </summary>
        /// <returns>This object if it is already frozen, or a new frozen instance otherwise.</returns>
        public GlossyString ToFrozen()
        {
            return this.Frozen ? this : new GlossyString(this);
        }

        public static bool IsNullOrEmpty(GlossyString glossy)
        {
            return glossy == null || glossy.Pairs.Count == 0;
        }
    }
}
