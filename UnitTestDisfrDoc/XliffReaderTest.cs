using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class XliffReaderTest : ReaderTestBase
    {
        [TestMethod]
        public void Read_Basic_Xliff1()
        {
            var path = Path.Combine(IDIR, "Xliff1.xliff");
            var bundle = new XliffReader().Read(path, -1);

            bundle.Assets.Count().Is(1);

            var asset0 = bundle.Assets.ElementAt(0);
            asset0.Package.Is(path);
            asset0.Original.Is(@"Graphic Example.psd");
            asset0.SourceLang.Is("en-US");
            asset0.TargetLang.Is("ja-JP");
            asset0.TransPairs.Count().Is(3);
            asset0.AltPairs.Count().Is(0);

            var pairs0 = asset0.TransPairs.ToArray();

            pairs0[0].Serial.Is(1);
            pairs0[0].Id.Is("1");
            pairs0[0].Source.ToString().Is("Quetzal");
            pairs0[0].Target.ToString().Is("Quetzal");
            pairs0[0].SourceLang.Is("en-US");
            pairs0[0].TargetLang.Is("ja-JP");

            pairs0[1].Serial.Is(2);
            pairs0[1].Id.Is("3");
            pairs0[1].Source.ToString().Is("An application to manipulate and process XLIFF documents");
            pairs0[1].Target.ToString().Is("XLIFF 文書を編集し処理するアプリケーション");
            pairs0[1].SourceLang.Is("en-US");
            pairs0[1].TargetLang.Is("ja-JP");

            pairs0[2].Serial.Is(3);
            pairs0[2].Id.Is("4");
            pairs0[2].Source.ToString().Is("XLIFF Data Manager");
            pairs0[2].Target.ToString().Is("XLIFF データ・マネージャ");
            pairs0[2].SourceLang.Is("en-US");
            pairs0[2].TargetLang.Is("ja-JP");
        }

        [TestMethod]
        public void Read_Basic_Xliff2()
        {
            var path = Path.Combine(IDIR, "Xliff2.xliff");
            var bundle = new XliffReader().Read(path, -1);

            bundle.Assets.Count().Is(1);

            var asset0 = bundle.Assets.ElementAt(0);
            asset0.Package.Is(path);
            asset0.Original.Is(@"v12/messages.xml");
            asset0.SourceLang.Is("en-US");
            asset0.TargetLang.Is("fr-FR");
            asset0.TransPairs.Count().Is(7);
            asset0.AltPairs.Count().Is(2);
        }

        [TestMethod]
        public void Read_Comprehensive_Xliff1()
        {
            Comprehensive(new XliffReader(), @"Xliff1.xliff");
        }

        [TestMethod]
        public void Read_Comprehensive_Xliff2()
        {
            Comprehensive(new XliffReader(), @"Xliff2.xliff");
        }

        [TestMethod]
        public void Read_Comprehensive_ConfiguringSpellingChecker()
        {
            Comprehensive(new XliffReader(), @"Configuring_Spelling_Checker.doc.sdlxliff");
        }

        [TestMethod]
        public void Read_Comprehensive_LanguageSupport()
        {
            Comprehensive(new XliffReader(), @"Language_Support.doc.sdlxliff");
        }

        [TestMethod]
        public void Read_Comprehensive_NewFeatures()
        {
            Comprehensive(new XliffReader(), @"New_features.ppt.sdlxliff");
        }

        [TestMethod]
        public void Read_Comprehensive_git()
        {
            Comprehensive(new XliffReader(), @"git.html_jpn.mqxlz");
        }
    }
}
