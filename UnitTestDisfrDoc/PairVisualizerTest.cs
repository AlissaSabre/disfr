using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class PairVisualizerTest
    {
#if false
        private const string IDIR = @"..\..\Samples";
        private const string ODIR = @"..\..\Expected";

        [TestMethod]
        public void TestMethod1()
        {
            var rd = new XliffReader();
            Dump(rd, "Xliff1.xliff", "Xliff1.dump");
            Dump(rd, "Xliff2.xliff", "Xliff2.dump");
            Dump(rd, "git.html_jpn.mqxlz", "git.html_jpn.dump");
            Dump(rd, "Configuring_Spelling_Checker.doc.sdlxliff", "Configuring_Spelling_Checker.doc.dump");
            Dump(rd, "Language_Support.doc.sdlxliff", "Language_Support.doc.dump");
            Dump(rd, "New_features.ppt.sdlxliff", "New_features.ppt.dump");
        }

        private static void Dump(IAssetReader rd, string input, string output)
        {
            var package = rd.Read(Path.Combine(IDIR, input), -1);
            var vis = new PairVisualizer();
            File.WriteAllText(Path.Combine(ODIR, output), vis.Visualize(package));
        }
#endif
    }
}
