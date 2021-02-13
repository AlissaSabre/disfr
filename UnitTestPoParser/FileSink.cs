using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.po;

namespace UnitTestPoParser
{
    class FileSink : CollectorSinkBase, IDisposable
    {
        private readonly TextWriter Writer;

        public FileSink(string filename)
        {
            Writer = new StreamWriter(filename);
        }

        public void Dispose()
        {
            Writer.Dispose();
        }

        protected override void Finish()
        {
            Writer.WriteLine();
            EmitLines("# ", TranslatorComments);
            EmitLines("#. ", ExtractedComments);
            EmitLines("#: ", References);
            EmitLines("#, ", Flags);

            var previous_prefix = IsObsolete ? "#~| " : "#| ";
            EmitString(previous_prefix, "msgctxt",      PreviousMessageContext);
            EmitString(previous_prefix, "msgid",        PreviousMessageId);
            EmitString(previous_prefix, "msgid_plural", PreviousMessageIdPlural);

            var prefix = IsObsolete ? "#~ " : "";
            EmitString(prefix, "msgctxt", MessageContext);
            if (MessageIdPlural == null)
            {
                EmitString(prefix, "msgid",  MessageId);
                EmitString(prefix, "msgstr", MessageStr);
            }
            else
            {
                EmitString(prefix, "msgid",        MessageId);
                EmitString(prefix, "msgid_plural", MessageIdPlural);
                for (int i = 0; i < MessageStrPlural.Count; i++)
                {
                    EmitString(prefix, $"msgstr[{i}]", MessageStrPlural[i]);
                }
            }
        }

        private void EmitLines(string prefix, IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                Writer.Write(prefix);
                Writer.WriteLine(item);
            }
        }

        private static readonly char[] SingleLetterEscape = { 'a', 'b', 't', 'N', 'v', 'f', 'r' };

        private void EmitString(string prefix, string keyword, string item)
        {
            if (item != null)
            {
                int p = item.IndexOf('\n');
                bool multiline = (p >= 0 && p < item.Length - 1);

                Writer.Write(prefix);
                Writer.Write(keyword);
                Writer.Write(' ');
                if (multiline || item.Length == 0) Writer.WriteLine("\"\"");
                bool quoted = false;
                foreach (var c in item)
                {
                    if (!quoted)
                    {
                        Writer.Write('"');
                        quoted = true;
                    }
                    if (c == '\n')
                    {
                        Writer.WriteLine("\\n\"");
                        quoted = false;
                    }
                    else if (c == '"' || c == '\\')
                    {
                        Writer.Write('\\');
                        Writer.Write(c);
                    }
                    else if (c >= '\a' && c <= '\r')
                    {
                        Writer.Write('\\');
                        Writer.Write(SingleLetterEscape[c - '\a']);
                    }
                    else if (c < ' ')
                    {
                        Writer.Write(@"\x{0:X2}", (int)c);
                    }
                    else
                    {
                        Writer.Write(c);
                    }
                }
                if (quoted) Writer.WriteLine("\"");
            }
        }
    }
}
