using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class ReaderManagerAutoDetectTest : ReaderTestBase
    {
        [TestMethod]
        public void ReaderManager_AutoDetect_xliff_1()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "Crafted00.xliff"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_xliff_2()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "Crafted04.xliff"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_xliff_3()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "Xliff1.xliff"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_xliff_4()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "Xliff2.xliff"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_sdlxliff_1()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "ChangeTracking_Trados_1.sdlxliff"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_sdlxliff_2()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "Configuring_Spelling_Checker.doc.sdlxliff"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_sdlxliff_3()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "Language_Support.doc.sdlxliff"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_sdlxliff_4()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "New_features.ppt.sdlxliff"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_mqxliff_1()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "ChangeTracking_memoQ_1.mqxliff"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_mqxlz_1()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "git.html_jpn.mqxlz"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_tmx_1()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "XuanZang.tmx"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_tmx_2()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "multilanguage.tmx"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_tmx_3()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "my_memory.tmx"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_tmx_4()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "tmx_sample_document.tmx"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_sdltm_1()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "tea-party.sdltm"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_po_1()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "c-strings.po"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_po_2()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "issue24.po"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_txlf_1()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "The Man in the Moon_txt.txlf"), -1).IsNot(null);
        }

        [TestMethod]
        public void ReaderManager_AutoDetect_ExcelGlossary_2()
        {
            ReaderManager.Current.Read(Path.Combine(IDIR, "Example_Excel_Glossary.xlsx"), -1).IsNot(null);
        }

    }
}
