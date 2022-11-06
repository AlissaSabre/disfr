using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Plugin;
using disfr.Doc;
using disfr.Writer;

namespace disfr.po
{
    /// <summary>Writer plugin for the gettext .po (officially known as <i>portable object</i>) files.</summary>
    public class PoWriterPlugin : IWriterPlugin
    {
        public string Name => "PoWriter";

        public IWriter CreateWriter() { return new PoWriter(); }
    }


    /// <summary>An <see cref="IPairsWriter"/> implementation to write pairs as a gettext .po file.</summary>
    /// <remarks>
    /// In addition to the instance methods of <see cref="IPairsWriter"/>,
    /// this class provides some static methods to write a whole or a part of .po file.
    /// They are for application programs other than the disfr UI.
    /// </remarks>
    public class PoWriter : IPairsWriter
    {
        public string Name => "PoWriter";

        public IList<string> FilterString { get; } = new string[]
        {
            "Gettext PO File|*.po",
        };

        public void Write(string filename, int filterindex_UNUSED, IEnumerable<ITransPair> pairs, IColumnDesc[] columns_UNUSED, InlineString.Render render)
        {
            Write(filename, pairs, render);
        }

        public static void Write(string filename, IEnumerable<ITransPair> pairs, InlineString.Render render = InlineString.RenderNormal)
        {
            using (var writer = File.CreateText(filename))
            {
                WriteHeader(writer, pairs);
                WritePairs(writer, pairs, render);
            }
        }

        public static void WriteHeader(TextWriter writer, IEnumerable<ITransPair> pairs)
        {
            var slang = GetUniqueOrNull(pairs, p => p.SourceLang);
            var tlang = GetUniqueOrNull(pairs, p => p.TargetLang);

            writer.WriteLine("msgid \"\"");
            writer.WriteLine("msgstr \"\"");
            writer.WriteLine("\"PO-Revision-Date: {0}+0000\\n\"", 
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            if (!string.IsNullOrWhiteSpace(slang))
            {
                writer.WriteLine("\"Source-Language: {0}\\n\"", LangUtils.XmlLangToPosixLocaleLabel(slang));
            }
            if (!string.IsNullOrWhiteSpace(tlang))
            {
                writer.WriteLine("\"Language: {0}\\n\"", LangUtils.XmlLangToPosixLocaleLabel(tlang));
            }
            writer.WriteLine("\"MIME-Version: 1.0\\n\"");
            writer.WriteLine("\"Content-Type: text/plain; charset=utf-8\\n\"");
            writer.WriteLine("\"Content-Transfer-Encoding: 8bit\\n\"");
            writer.WriteLine("\"Generated-By: disfr PO plugin\\n\"");
        }

        public static void WritePairs(TextWriter writer, IEnumerable<ITransPair> pairs, InlineString.Render render)
        {
            var counter = new DuplicateCounter();
            foreach (var p in pairs)
            {
                writer.WriteLine();

                // TODO: write appropriate properties including obsolete, flags, and References
                // TODO: as well as Notes.
                // TODO: however, we can't assume the pairs are from another .po,
                // TODO: so we can't simply turn them into PO's comment-pretended metadata.

                var s = p.Source.ToString(render);
                var t = p.Target.ToString(render);

                var n = counter.CountDuplicates(s);
                if (n > 0)
                {
                    // Yes, using msgctxt like this is barely legal...
                    writer.WriteLine("msgctxt \"repetition {0}\"", n);
                }

                writer.WriteLine("msgid \"\"");
                WriteEscapedStringLine(writer, s);

                writer.WriteLine("msgstr \"\"");
                WriteEscapedStringLine(writer, t);
            }
        }

        public static void WriteEscapedStringLine(TextWriter writer, string text)
        {
            writer.Write("\"");
            foreach (var c in text)
            {
                switch (c)
                {
                    case '\n': writer.Write("\\n"); break;
                    case '\t': writer.Write("\\t"); break;
                    case '\b': writer.Write("\\b"); break;
                    case '\r': writer.Write("\\r"); break;
                    case '\f': writer.Write("\\f"); break;
                    case '\v': writer.Write("\\v"); break;
                    case '\a': writer.Write("\\a"); break;
                    case '\\': writer.Write("\\\\"); break;
                    case '"': writer.Write("\\\""); break;
                    default:
                        if (c < 0x20 || (c >= 0x7F && c < 0xA0))
                        {
                            // Use an octal escape for these code points,
                            // because the unicode escape is not supported by GNU tools,
                            // and the hexadecimal escape has some interoperability problems.
                            writer.Write('\\');
                            writer.Write((char)('0' + (3 & (c >> 6))));
                            writer.Write((char)('0' + (7 & (c >> 3))));
                            writer.Write((char)('0' + (7 & c)));
                        }
                        else
                        {
                            // Write everything else unescaped, even if it is a format character,
                            // a noncharacter, or anything else.
                            writer.Write(c);
                        }
                        break;
                }
            }
            writer.WriteLine("\"");
        }

        /// <summary>Returns a unique selected string or null.</summary>
        /// <typeparam name="T">The data type of the elements in the <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence of the elements.</param>
        /// <param name="selector">A function to get a string from each element.</param>
        /// <returns>A unique string if one exists or null.</returns>
        /// <remarks>
        /// <para>
        /// This method applies <paramref name="selector"/> to each element in the <paramref name="source"/>
        /// and examines how many distinctive non-null strings are returned.
        /// If there are zero, two, or more distinctive non-null strings, this method returns null.
        /// Otherwise, if there is only one distinctive non-null string, this method returns the unique string.
        /// </para>
        /// <para>
        /// The strings are compared in the case-insensitive ordinal way.
        /// </para>
        /// </remarks>
        private static string GetUniqueOrNull<T>(IEnumerable<T> source, Func<T, string> selector)
        {
            var values = source.Select(selector).Where(s => !(s is null)).Distinct(StringComparer.OrdinalIgnoreCase).Take(2).ToArray();
            return values.Length == 1 ? values[0] : null;
        }
    }

    /// <summary>Counts how many identical strings have been seen.</summary>
    /// <remarks>
    /// This class is derived from <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>.
    /// We don't need concurrency, but invoking AddOrUpdate is usually faster
    /// than invoking Dictionary.Contains followed by Dictionary.Add.
    /// </remarks>
    class DuplicateCounter : System.Collections.Concurrent.ConcurrentDictionary<string, int>
    {
        /// <summary>Returns the number of same strings that were seen previously.</summary>
        /// <param name="s">The string.</param>
        /// <returns>The number of strings seen previously.</returns>
        /// <remarks>This method returns a zero if it is the first time to see a particular string.</remarks>
        public int CountDuplicates(string s) => AddOrUpdate(s, _ => 0, (_, old) => old + 1);
    }
}
