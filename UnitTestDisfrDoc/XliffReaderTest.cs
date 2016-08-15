using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class XliffReaderTest
    {
        private const string IDIR = @"..\..\Samples";
        private const string ODIR = @"..\..\Expected";

        [TestMethod]
        public void Read_Basic_Xliff1()
        {
            var assets = new XliffReader().Read(Path.Combine(IDIR, "Xliff1.xliff"), -1);

            assets.Count().Is(1);

            var asset0 = assets.ElementAt(0);
            asset0.Package.Is(@"..\..\Samples\Xliff1.xliff");
            asset0.Original.Is(@"Graphic Example.psd");
            asset0.SourceLang.Is("en-US");
            asset0.TargetLang.Is("ja-JP");
            asset0.TransPairs.Count().Is(3);
            asset0.AltPairs.Count().Is(0);

            var pairs0 = asset0.TransPairs.ToArray();

            pairs0[0].Serial.Is(1);
            pairs0[0].Id.Is("1");
            pairs0[0].Source.Is("Quetzal");
            pairs0[0].Target.Is("Quetzal");
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
            var assets = new XliffReader().Read(Path.Combine(IDIR, "Xliff2.xliff"), -1);

            assets.Count().Is(1);

            var asset0 = assets.ElementAt(0);
            asset0.Package.Is(@"..\..\Samples\Xliff2.xliff");
            asset0.Original.Is(@"v12/messages.xml");
            asset0.SourceLang.Is("en-US");
            asset0.TargetLang.Is("fr-FR");
            asset0.TransPairs.Count().Is(7);
            asset0.AltPairs.Count().Is(2);
        }

        [TestMethod]
        public void Read_Comprehensive_Xliff1()
        {
            Comprehensive(@"Xliff1.xliff");
        }

        [TestMethod]
        public void Read_Comprehensive_Xliff2()
        {
            Comprehensive(@"Xliff2.xliff");
        }

        [TestMethod]
        public void Read_Comprehensive_ConfiguringSpellingChecker()
        {
            Comprehensive(@"Configuring_Spelling_Checker.doc.sdlxliff");
        }

        [TestMethod]
        public void Read_Comprehensive_LanguageSupport()
        {
            Comprehensive(@"Language_Support.doc.sdlxliff");
        }

        [TestMethod]
        public void Read_Comprehensive_NewFeatures()
        {
            Comprehensive(@"New_features.ppt.sdlxliff");
        }

        [TestMethod]
        public void Read_Comprehensive_git()
        {
            Comprehensive(@"git.html_jpn.mqxlz");
        }

        private static void Comprehensive(string filename)
        {
            var assets = new XliffReader().Read(Path.Combine(IDIR, filename), -1);
            var got = new PairVisualizer().Visualize(assets);

            var dumpname = Path.GetFileNameWithoutExtension(filename) + ".dump";
            var expected = File.ReadAllText(Path.Combine(ODIR, dumpname));

            got.Is(expected);
        } 
    }
}
