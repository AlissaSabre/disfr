using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.po;

namespace UnitTestPoWriter
{
    [TestClass]
    public class WriteEscapedStringLineTest
    {
        [TestMethod]
        public void Test_Ordinary_strings_are_written_unescaped()
        {
            TestUnchanged("");
            TestUnchanged("Hello, world!");
            TestUnchanged("Do you know the way to San Jose?");
        }

        [TestMethod]
        public void Test_Most_ASCII_charcters_are_written_unchanged()
        {
            TestUnchanged(" ! #$%&'()*+,-./");
            TestUnchanged("0123456789:;<=>?");
            TestUnchanged("@ABCDEFGHIJKLMNO");
            TestUnchanged("PQRSTUVEXYZ[ ]^_");
            TestUnchanged("`abcdefghijklmno");
            TestUnchanged("pqrstuvexyz{|}~");
        }

        [TestMethod]
        public void Test_Special_symbols_are_escaped()
        {
            Test("\"", "\\\"");
            Test("\\", "\\\\");
        }

        [TestMethod]
        public void Test_C0_ontrols_are_escaped()
        {
            // Note that PoWriter uses octal escapes rather than hexadecimal escapes,
            // and its octal escapes always have three octal digits.

            Test("\x00\x01\x02\x03\x04\x05\x06", @"\000\001\002\003\004\005\006");
            Test("\x07\x08\x09\x0A\x0B\x0C\x0D", @"\a\b\t\n\v\f\r");
            Test("\x0E\x0F", @"\016\017");

            Test("\x10\x11\x12\x13\x14\x15\x16\x17", @"\020\021\022\023\024\025\026\027");
            Test("\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F", @"\030\031\032\033\034\035\036\037");
        }

        [TestMethod]
        public void Test_DEL_is_escaped()
        {
            Test("\x7F", @"\177");
        }

        [TestMethod]
        public void Test_C1_controls_are_escaped()
        {
            Test("\x80\x81\x82\x83\x84\x85\x86\x87", @"\200\201\202\203\204\205\206\207");
            Test("\x88\x89\x8A\x8B\x8C\x8D\x8E\x8F", @"\210\211\212\213\214\215\216\217");

            Test("\x90\x91\x92\x93\x94\x95\x96\x97", @"\220\221\222\223\224\225\226\227");
            Test("\x98\x99\x9A\x9B\x9C\x9D\x9E\x9F", @"\230\231\232\233\234\235\236\237");
        }

        [TestMethod]
        public void Test_Usual_non_ASCII_characters_are_handled_unchanged()
        {
            // Latin-1
            TestUnchanged("\u00C1\u00E7\u00FF");

            // Latin Extended B
            TestUnchanged("\u0100\u0133\u0166\u01F6");

            // Greek
            TestUnchanged("\u0391\u0392\u0393\u03B1\u03B2\u03B3");

            // Cyrillic
            TestUnchanged("\u0410\u0411\u0412\u0430\u0431\u0432");

            // Hebrew
            TestUnchanged("\u05D0\u05D1\u05D2\u05D3\u05D4");

            // Arabic
            TestUnchanged("\u0627\u0628\u062A\u062B\u062C");

            // Hiragana
            TestUnchanged("\u3042\u3044\u3046\u3048\u304A");

            // Katakana
            TestUnchanged("\u30A2\u30A4\u30A6\u30A8\u30AA");

            // CJK Ideograph
            TestUnchanged("\u7532\u4E59\u4E19\u4E01\u620A\u5DF1\u5E9A\u8F9B\u58EC\u7678");

            // Hangle Jamo
            TestUnchanged("\u1100\u1161\u1102\u1161\u1103\u1161\u1105\u1161");

            // Hangle Syllable
            TestUnchanged("\uAC00\uB098\uB2E4\uB77C");
        }

        [TestMethod]
        public void Test_Surrogate_pairs_are_handled_unchagned()
        {
            // Surrogate pairs
            TestUnchanged("\uD800\uDC00\uD840\uDC00\uD880\uDC00");
        }

        [TestMethod]
        public void Test_Mixed_graphic_and_control_characters_are_handled()
        {
            var template = "Abcd efgh{0}Ijkl{0}{0}Mnop qrst uvwx{0}Yz{0}";

            var s1 = string.Format(template, "\n");
            var t1 = string.Format(template, "\\n");
            Test(s1, t1);

            var s2 = string.Format(template, "\r\n");
            var t2 = string.Format(template, "\\r\\n");
            Test(s2, t2);
        }

        private static void TestUnchanged(string content) => Test(content, content);

        private static void Test(string content, string result)
        {
            using (var wr = new StringWriter())
            {
                PoWriter.WriteEscapedStringLine(wr, content);
                var s = wr.ToString();

                // The result should be enclosed in a pair of double quotes
                // and followed by a newline. 
                // Strip them off before comparing against the expected result
                // (after making sure they are double quotes and a newline.)
                (s.Length >= Environment.NewLine.Length + 2).IsTrue();
                s.StartsWith("\"").IsTrue();
                s.EndsWith("\"" + Environment.NewLine).IsTrue();
                s.Substring(1, s.Length - (Environment.NewLine.Length + 2)).Is(result);
            }
        }
    }
}
