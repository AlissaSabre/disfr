using System;
using System.Collections.Generic;
using System.Text;

namespace disfr.po
{
    internal partial class PoScanner
    {
        /// <summary>Defined keywords and their Token codes.</summary>
        /// <remarks>
        /// Note that the pattern deviates on domain and msgstr;
        /// msgstr has no P_ variant (because _previous_ section never holds transalted texts),
        /// and domain has no variants at all.
        /// </remarks>
        private static readonly Dictionary<string, Token> Keywords = new Dictionary<string, Token>
    {
        { "msgctxt",      Token.MSGCTXT      },
        { "msgid",        Token.MSGID        },
        { "msgid_plural", Token.MSGID_PLURAL },
        { "msgstr",       Token.MSGSTR       },
        { "domain",       Token.DOMAIN       },
    };

        Token MsgIdToken = Token.MSGID;

        Token Keyword(string word)
        {
            if (!Keywords.TryGetValue(word, out var token))
            {
                yyerror("Unknown keyword: {0}", word);
            }
            return (token == Token.MSGID) ? MsgIdToken : token;
        }

        string TrimText(string input)
        {
            int p = 0;
            while (p < input.Length && input[p] > ' ') p++;
            while (p < input.Length && input[p] <= ' ') p++;
            return input.Substring(p).TrimEnd();
        }

        string DecodeEscaped(string input)
        {
            // Exclude the quotes at the beginning and the end.
            var sb = new StringBuilder(input.Length - 2);
            var length = input.Length - 1;
            var p = 1;

            // Scan the contents of the input string.
            while (p < length)
            {
                char c = input[p++], d;
                switch (c)
                {
                    default:
                        sb.Append(c);
                        break;
                    case '"':
                        // Lex state machine should have excluded this case already.
                        throw new ApplicationException("Internal Error: a raw quote in a string");
                    case '\\':
                        // Lex state machine guarantees p < input.Length.
                        switch (c = input[p++])
                        {
                            case 'n': sb.Append('\n'); break;
                            case 't': sb.Append('\t'); break;
                            case 'b': sb.Append('\b'); break;
                            case 'r': sb.Append('\r'); break;
                            case 'f': sb.Append('\f'); break;
                            case 'v': sb.Append('\v'); break;
                            case 'a': sb.Append('\a'); break;
                            case '\\':
                            case '"': sb.Append(c); break;
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                                p = OctalEscape(input, p - 1, out d);
                                sb.Append(d);
                                break;
                            case 'x':
                                if (p == length) yyerror("Invalid hexadecimal escape");
                                p = HexadecimalEscape(input, p, out d);
                                sb.Append(d);
                                break;
                            default:
                                sb.Append(c);
                                break;
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        private int OctalEscape(string input, int index, out char code)
        {
            // An octal escape is 1-3 chars long
            // and terminates when non-octal character is seen or after its third digit is seen.
            // Its nineth bit (from LSB) is silently ignored if presents.
            char c;
            int value = 0;
            int max = Math.Min(3, input.Length - index);
            while (max > 0 && (c = input[index]) >= '0' && c <= '7')
            {
                value = value * 8 + c - '0';
                --max;
                ++index;
            }
            code = (char)(value & 255);
            return index;
        }

        private int HexadecimalEscape(string input, int index, out char code)
        {
            // A hexadecimal escape must have at least one digit
            // and can be arbitrarily long
            // though only the last two digits are used.
            int value = 0;
            int p = index;
            while (p < input.Length)
            {
                char c = input[p++];
                if (c >= '0' && c <= '9')
                {
                    value = value * 16 + c - '0';
                }
                else if (c >= 'A' && c <= 'F')
                {
                    value = value * 16 + c - 'A';
                }
                else if (c >= 'a' && c <= 'f')
                {
                    value = value * 16 + c - 'a';
                }
                else
                {
                    break;
                }
            }
            code = (char)(value & 255);
            return p;
        }

        public int GetLineNumber()
        {
            return yyline;
        }

        public string SourceFileName { get; set; }

        public override void yyerror(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            throw new YYException($"{SourceFileName}:{yyline}:{yycol}:{msg}");
        }
    }
}
