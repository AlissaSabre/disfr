using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class ReaderManagerTest
    {
        [TestMethod]
        public void Current_1()
        {
            var c1 = ReaderManager.Current;
            (c1 != null).Is(true);
            var c2 = ReaderManager.Current;
            object.ReferenceEquals(c1, c2).Is(true);
        }

        [TestMethod]
        public void Add_1()
        {
            var rm = new ReaderManager();
            rm.AsEnumerable().Count().Is(0);
            rm.Add(new Reader1());
            rm.AsEnumerable().Count().Is(1);
            rm.Add(new Reader2());
            rm.AsEnumerable().Count().Is(2);
        }

        [TestMethod]
        public void FilterString_1()
        {
            var rm = new ReaderManager();
            rm.FilterString.Is("All files|*.*");
            rm.Add(new Reader1());
            rm.FilterString.Is("All known files|*.foo;*.bar|Foo file|*.foo|Bar file|*.bar|All files|*.*");
            rm.Add(new Reader2());
            rm.FilterString.Is("All known files|*.foo;*.bar;*.j;*.john;*.doe|Foo file|*.foo|Bar file|*.bar|john doe|*.j;*.john;*.doe|All files|*.*");
        }

        [TestMethod]
        public void Reader_FilterIndex_1()
        {
            var rm = new ReaderManager();
            rm.Add(new Reader1());
            rm.Add(new Reader2());

            // Reader1 and Reader2 have priorities 1 and 2 respectively, so
            // Reader1 will be listed first in the filter, but
            // Reader2 will be considered first when auto-detecting.

            rm.Read("filename").ElementAt(0).Original.Is("Reader2.-1");
            rm.Read("filename", 0).ElementAt(0).Original.Is("Reader2.-1");
            rm.Read("filename", 1).ElementAt(0).Original.Is("Reader1.0");
            rm.Read("filename", 2).ElementAt(0).Original.Is("Reader1.1");
            rm.Read("filename", 3).ElementAt(0).Original.Is("Reader2.0");
            rm.Read("filename", 4).ElementAt(0).Original.Is("Reader2.-1");
            AssertEx.Catch<ArgumentOutOfRangeException>(() => rm.Read("filename", -2));
            AssertEx.Catch<ArgumentOutOfRangeException>(() => rm.Read("filename", 6));
        }



        private class Reader1 : IAssetReader
        {
            public IList<string> FilterString { get { return new string[] { "Foo file|*.foo", "Bar file|*.bar" }; } }
            public string Name { get { return "Foo bar reader"; } }
            public int Priority { get { return 1; } }

            public IEnumerable<IAsset> Read(string filename, int filterindex)
            {
                return new IAsset[] { new DummyAsset() { Package = filename, Original = "Reader1." + filterindex } };
            }
        }

        private class Reader2 : IAssetReader
        {
            public IList<string> FilterString { get { return new string[] { "john doe|*.j;*.john;*.doe" }; } }
            public string Name { get { return "John doe reader"; } }
            public int Priority { get { return 2; } }

            public IEnumerable<IAsset> Read(string filename, int filterindex)
            {
                return new IAsset[] { new DummyAsset() { Package = filename, Original = "Reader2." + filterindex } };
            }
        }

        private class DummyAsset : IAsset
        {
            public string Package { get; set; }
            public string Original { get; set; }
            public string SourceLang { get { return "en-US"; } }
            public string TargetLang { get { return "fr-FR"; } }
            public IEnumerable<ITransPair> TransPairs { get { return new ITransPair[0]; } }
            public IEnumerable<ITransPair> AltPairs { get { return null; } }
            public IList<PropInfo> Properties { get { return new List<PropInfo>(0).AsReadOnly(); } }
        }
    }
}
