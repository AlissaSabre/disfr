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

        private const string PREAMBLE_ALL =
            "<?xml version='1.0'?>" +
            "<tmx version='1.4'>" +
            "<header creationtool='none' creationtoolversion='0' segtype='sentence' o-tmf='x'" +
            " adminlang='en' srclang='*all*' datatype='unknown'/>" +
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
                    + POSTAMBLE), "test", "test").Assets.ToArray();
                a.Length.Is(2);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("zh-cn");
                a[0].TransPairs.Count().Is(1);
                a[1].SourceLang.Is("en");
                a[1].TargetLang.Is("zh-tw");
                a[1].TransPairs.Count().Is(1);
            }
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-cn'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-tw'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test", "test").Assets.ToArray();
                a.Length.Is(2);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("zh-cn");
                a[0].TransPairs.Count().Is(1);
                a[1].SourceLang.Is("en");
                a[1].TargetLang.Is("zh-tw");
                a[1].TransPairs.Count().Is(1);
            }
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-cn'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-tw'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test", "test").Assets.ToArray();
                // The feature to merge target language variants is removed on 71ceb78e93289b2468bf8e4f2813795269106d6c
                // a.Length.Is(1);
                // a[0].SourceLang.Is("en");
                // a[0].TargetLang.Is("zh");
                a.Length.Is(3);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("zh");
                a[0].TransPairs.Count().Is(1);
                a[1].SourceLang.Is("en");
                a[1].TargetLang.Is("zh-cn");
                a[1].TransPairs.Count().Is(1);
                a[2].SourceLang.Is("en");
                a[2].TargetLang.Is("zh-tw");
                a[2].TransPairs.Count().Is(1);
            }
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh-tw'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='zh-cn'><seg>a</seg></tuv><tuv xml:lang='en'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test", "test").Assets.ToArray();
                a.Length.Is(2);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("zh-cn");
                a[0].TransPairs.Count().Is(1);
                a[1].SourceLang.Is("en");
                a[1].TargetLang.Is("zh-tw");
                a[1].TransPairs.Count().Is(1);
            }
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en-US'><seg>a</seg></tuv><tuv xml:lang='zh'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en-GB'><seg>a</seg></tuv><tuv xml:lang='zh'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test", "test").Assets.ToArray();
                a.Length.Is(1);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("zh");
                a[0].TransPairs.Count().Is(2);
            }
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_EN
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='zh'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='de'><seg>a</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='fr'><seg>a</seg></tuv></tu>"
                    + POSTAMBLE), "test", "test").Assets.ToArray();
                a.Length.Is(3);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("de");
                a[0].TransPairs.Count().Is(1);
                a[1].SourceLang.Is("en");
                a[1].TargetLang.Is("fr");
                a[1].TransPairs.Count().Is(1);
                a[2].SourceLang.Is("en");
                a[2].TargetLang.Is("zh");
                a[2].TransPairs.Count().Is(1);
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
                    + POSTAMBLE), "test", "test").Assets.ToArray();
                a.Length.Is(2);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.ToLowerInvariant().Is("ja");
                a[1].SourceLang.Is("en");
                a[1].TargetLang.Is("ko");
            }
        }

        [TestMethod]
        public void language_detection_srclang_all_1()
        {
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_ALL
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='fr'><seg>b</seg></tuv><tuv xml:lang='de'><seg>c</seg></tuv></tu>"
                    + POSTAMBLE), "test", "test").Assets.ToArray();
                a.Length.Is(6);
                a[0].SourceLang.Is("de");
                a[0].TargetLang.Is("en");
                a[1].SourceLang.Is("de");
                a[1].TargetLang.Is("fr");
                a[2].SourceLang.Is("en");
                a[2].TargetLang.Is("de");
                a[3].SourceLang.Is("en");
                a[3].TargetLang.Is("fr");
                a[4].SourceLang.Is("fr");
                a[4].TargetLang.Is("de");
                a[5].SourceLang.Is("fr");
                a[5].TargetLang.Is("en");
            }
        }

        [TestMethod]
        public void language_detection_srclang_all_2()
        {
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_ALL
                    + "<tu srclang='en'><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='fr'><seg>b</seg></tuv><tuv xml:lang='de'><seg>c</seg></tuv></tu>"
                    + POSTAMBLE), "test", "test").Assets.ToArray();
                a.Length.Is(2);
                a[0].SourceLang.Is("en");
                a[0].TargetLang.Is("de");
                a[1].SourceLang.Is("en");
                a[1].TargetLang.Is("fr");
            }
        }

        [TestMethod]
        public void language_detection_srclang_all_3()
        {
            {
                var a = new TmxReader().Read(new StringStream(
                    PREAMBLE_ALL
                    + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='fr'><seg>b</seg></tuv><tuv xml:lang='de'><seg>c</seg></tuv></tu>"
                    + "<tu><tuv xml:lang='de'><seg>a</seg></tuv><tuv xml:lang='fr'><seg>b</seg></tuv><tuv xml:lang='de'><seg>c</seg></tuv></tu>"
                    + POSTAMBLE), "test", "test").Assets.ToArray();
                a.Length.Is(6);
                a[0].SourceLang.Is("de");
                a[0].TargetLang.Is("en");
                a[1].SourceLang.Is("de");
                a[1].TargetLang.Is("fr");
                a[2].SourceLang.Is("en");
                a[2].TargetLang.Is("de");
                a[3].SourceLang.Is("en");
                a[3].TargetLang.Is("fr");
            }
        }

        [TestMethod]
        public void language_detection_mixed_srclang_0()
        {
            // srclang is not actually mixed in this test case.
            var a = new TmxReader().Read(new StringStream(
                PREAMBLE_EN
                + "<tu><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='fr'><seg>b</seg></tuv><tuv xml:lang='de'><seg>c</seg></tuv></tu>"
                + POSTAMBLE), "test", "test").Assets.ToArray();
            a.Length.Is(2);
            a[0].SourceLang.Is("en");
            a[0].TargetLang.Is("de");
            a[1].SourceLang.Is("en");
            a[1].TargetLang.Is("fr");
        }

        [TestMethod]
        public void language_detection_mixed_srclang_1()
        {
            // srclang is not actually mixed in this test case.
            var a = new TmxReader().Read(new StringStream(
                PREAMBLE_EN
                + "<tu srclang='en'><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='fr'><seg>b</seg></tuv><tuv xml:lang='de'><seg>c</seg></tuv></tu>"
                + POSTAMBLE), "test", "test").Assets.ToArray();
            a.Length.Is(2);
            a[0].SourceLang.Is("en");
            a[0].TargetLang.Is("de");
            a[1].SourceLang.Is("en");
            a[1].TargetLang.Is("fr");
        }

        [TestMethod]
        public void language_detection_mixed_srclang_2()
        {
            // srclang is not actually mixed in this test case.
            var a = new TmxReader().Read(new StringStream(
                PREAMBLE_EN
                + "<tu srclang='de'><tuv xml:lang='en'><seg>a</seg></tuv><tuv xml:lang='fr'><seg>b</seg></tuv><tuv xml:lang='de'><seg>c</seg></tuv></tu>"
                + POSTAMBLE), "test", "test").Assets.ToArray();
            a.Length.Is(2);
            a[0].SourceLang.Is("de");
            a[0].TargetLang.Is("en");
            a[1].SourceLang.Is("de");
            a[1].TargetLang.Is("fr");
        }
    }
}
