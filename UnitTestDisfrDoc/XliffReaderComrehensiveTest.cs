using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class XliffReaderComprehensiveTest : ReaderTestBase
    {
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
