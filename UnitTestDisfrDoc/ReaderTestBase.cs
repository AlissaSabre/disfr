using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    /// <summary>
    /// The base class of Test Classes for IAssetReader implementation.
    /// </summary>
    abstract public class ReaderTestBase
    {
        protected const string IDIR = @"..\..\Samples";
        protected const string ODIR = @"..\..\Expected";

        protected static void Comprehensive(IAssetReader reader, string filename)
        {
            var assets = reader.Read(Path.Combine(IDIR, filename), -1);
            var got = new PairVisualizer().Visualize(assets);

            var dumpname = Path.GetFileNameWithoutExtension(filename) + ".dump";
            var expected = File.ReadAllText(Path.Combine(ODIR, dumpname));

            got.Is(expected);
        }
    }
}
