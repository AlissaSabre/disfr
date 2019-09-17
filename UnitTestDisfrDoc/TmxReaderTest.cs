using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;
using System.Linq;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class TmxReaderTest : ReaderTestBase
    {
        [TestMethod]
        public void my_memory_tmx_Test()
        {
            Comprehensive(new TmxReader(), "my_memory.tmx");
        }

        [TestMethod]
        [Ignore]
        public void XuanZang_tmx_Test()
        {
            Comprehensive(new TmxReader(), "XuanZang.tmx");
        }

        private const string PREAMBLE_EN =
            "<?xml version='1.0'?>" +
            "<tmx version='1.4'>" +
            "<header creationtool='none' creationtoolversion='0' segtype='sentence' o-tmf='x'" +
            " adminlang='en' srclang='en' datatype='unknown'/>" +
            "<body>";

        private const string POSTAMBLE =
            "</body></tmx>";

        [TestMethod]
        public void language_detection_1()
        {
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-cn'><seg>a</seg></tuv><tuv xml:lang='zh-tw'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test").ToArray();
                a.Length.Is(2);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("zh-cn");
                a[1].TargetLang.Is("zh-tw");
            }
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-cn'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-tw'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test").ToArray();
                a.Length.Is(2);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("zh-cn");
                a[1].TargetLang.Is("zh-tw");
            }
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-cn'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-tw'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test").ToArray();
                a.Length.Is(1);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("zh");
            }
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-tw'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='zh-cn'><seg>a</seg></tuv><tuv xml:lang='en'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test").ToArray();
                a.Length.Is(2);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("zh-cn");
                a[1].TargetLang.Is("zh-tw");
            }
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en-US'><seg>a</seg></tuv><tuv xml:lang='zh'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en-GB'><seg>a</seg></tuv><tuv xml:lang='zh'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test").ToArray();
                a.Length.Is(1);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("zh");
            }
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='de'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='fr'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test").ToArray();
                a.Length.Is(3);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("de");
                a[1].TargetLang.Is("fr");
                a[2].TargetLang.Is("zh");
            }
        }

        [TestMethod]
        public void language_detection_2()
        {
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='ko'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>b</seg></tuv><tuv xml:lang='ja'><seg>b</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>c</seg></tuv><tuv xml:lang='JA'><seg>c</seg></tuv></tu>"
                    + POSTAMBLE), "test").ToArray();
                a.Length.Is(2);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.ToLowerInvariant().Is("ja");
                a[1].SourceLang.Is("en");
                a[1].TargetLang.Is("ko");
            }
        }
    }
}
