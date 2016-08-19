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
        /// <summary>
        /// Generates files in Expected directory.
        /// </summary>
        /// <remarks>
        /// This is not really a test case, but a utility to support tests.
        /// When run (via Test Explorer), it creates (overwrites if files are present) *.dump files
        /// under the directory "Expected".
        /// Running it frequently is meaningless.
        /// Don't enable any of the #if block, unless you know what you are doing.
        /// </remarks>
        [TestMethod]
        public void GenerateFiles()
        {
            var xliff = new XliffReader();
#if false
            Dump(xliff, "Xliff1.xliff");
            Dump(xliff, "Xliff2.xliff");
            Dump(xliff, "git.html_jpn.mqxlz");
            Dump(xliff, "Configuring_Spelling_Checker.doc.sdlxliff");
            Dump(xliff, "Language_Support.doc.sdlxliff");
            Dump(xliff, "New_features.ppt.sdlxliff");
#endif

            var tmx = new TmxReader();
#if false
            Dump(tmx, "my_memory.tmx");
            Dump(tmx, "XuanZang.tmx");
#endif

            var sdltm = new SdltmReader();
#if false
            Dump(sdltm, "tea-party.sdltm");
#endif

        }
#endif

        private static void Dump(IAssetReader rd, string input)
        {
            string IDIR = @"..\..\Samples";
            string ODIR = @"..\..\Expected";

            var assets = rd.Read(Path.Combine(IDIR, input), -1);
            var visualized = new PairVisualizer().Visualize(assets);
            var output = Path.GetFileNameWithoutExtension(input) + ".dump";
            File.WriteAllText(Path.Combine(ODIR, output), visualized);
        }
    }
}
