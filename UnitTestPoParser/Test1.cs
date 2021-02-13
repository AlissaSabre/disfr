using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.po;
using System.Runtime.CompilerServices;

namespace UnitTestPoParser
{
    [TestClass]
    public class Test1
    {
        private static string SamplesDir = "../../Samples/";

        private static string ReferencesDir = "../../Expected/";

        private static string OutputDir = "./";

        [TestMethod]
        public void TestMethod1()
        {
            Run(SamplesDir + "sample1.po", ReferencesDir + "sample1.po");
        }

        [TestMethod]
        public void TestMethod2()
        {
            Run(SamplesDir + "sample2.po", ReferencesDir + "sample2.po");
        }

        [TestMethod]
        public void TestMethod3()
        {
            Run(SamplesDir + "sample3.po", ReferencesDir + "sample3.po");
        }

        private void Run(string input, string expected, [CallerMemberName] string caller = "")
        {
            var parser = new PoParser();
            var output = OutputDir + GetType().FullName + "." + caller + "." + Path.GetFileName(input);
            using (var sink = new FileSink(output))
            {
                parser.Parse(input, sink);
            }
            CompareFiles(output, expected);
        }

        private void CompareFiles(string actual, string expected)
        {
            using (var r1 = new StreamReader(actual))
            using (var r2 = new StreamReader(expected))
            {
                for (int n = 1; ; n++)
                {
                    var line1 = r1.ReadLine();
                    var line2 = r2.ReadLine();
                    if (line1 is null && line2 is null) break;
                    line1.Is(line2, $"Lines at {n} differ.");
                }
            }
        }
    }
}
