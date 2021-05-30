using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelperDoc;

using disfr.Doc;
using disfr.UI;

namespace UnitTestDisfrUI
{
    [TestClass]
    public class PairRendererIdTest
    {
        [TestMethod]
        public void IdTrimmer_10()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                    new MockTransPair { Serial = 1, Id = "" },
            });
            ad.IdTrimChars.Is(0);
        }

        [TestMethod]
        public void IdTrimmer_11()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                    new MockTransPair { Serial = 1, Id = "0" },
            });
            ad.IdTrimChars.Is(0);
        }

        [TestMethod]
        public void IdTrimmer_12()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                    new MockTransPair { Serial = 1, Id = "1" },
            });
            ad.IdTrimChars.Is(1);
        }

        [TestMethod]
        public void IdTrimmer_13()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                    new MockTransPair { Serial = 1, Id = "9" },
            });
            ad.IdTrimChars.Is(1);
        }

        [TestMethod]
        public void IdTrimmer_14()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                    new MockTransPair { Serial = 1, Id = "a" },
            });
            ad.IdTrimChars.Is(0);
        }

        [TestMethod]
        public void IdTrimmer_15()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                    new MockTransPair { Serial = 1, Id = "000" },
            });
            ad.IdTrimChars.Is(0);
        }

        [TestMethod]
        public void IdTrimmer_16()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                    new MockTransPair { Serial = 1, Id = "001" },
            });
            ad.IdTrimChars.Is(1);
        }

        [TestMethod]
        public void IdTrimmer_17()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                    new MockTransPair { Serial = 1, Id = "100" },
            });
            ad.IdTrimChars.Is(3);
        }

        [TestMethod]
        public void IdTrimmer_18()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                    new MockTransPair { Serial = 1, Id = "10a" },
            });
            ad.IdTrimChars.Is(2);
        }

        [TestMethod]
        public void IdTrimmer_20()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                new MockTransPair { Serial = 1, Id =    "0"   },
                new MockTransPair { Serial = 2, Id =    "1"   },
                new MockTransPair { Serial = 3, Id =   "10a"  },
                new MockTransPair { Serial = 4, Id = "0020"   },
                new MockTransPair { Serial = 5, Id = "1000"   },
                new MockTransPair { Serial = 6, Id = "1000a"  },
                new MockTransPair { Serial = 7, Id = "1234-2" },
                new MockTransPair { Serial = 8, Id = "9999"   },
            });
            ad.IdTrimChars.Is(4);
        }

        [TestMethod]
        public void IdTrimmer_21()
        {
            var ad = new AssetData();
            ad.CalculateIdTrimmer(new MockTransPair[]
            {
                new MockTransPair { Serial = 1, Id =   "1" },
                new MockTransPair { Serial = 2, Id =  "10" },
                new MockTransPair { Serial = 0, Id = "100" },
                new MockTransPair { Serial = 4, Id =  "20" },
            });
            ad.IdTrimChars.Is(2);
        }

        [TestMethod]
        public void IdPresentation_01()
        {
            var renderer = new PairRenderer();
            var ad = new AssetData { IdTrimChars = 4 };
            renderer.Id(ad, "0").Is(P(3) + "0"); // GitHub issue #6.
            renderer.Id(ad, "1").Is(P(3) + "1");
            renderer.Id(ad, "9").Is(P(3) + "9");
        }

        [TestMethod]
        public void IdPresentation_02()
        {
            var renderer = new PairRenderer();
            var ad = new AssetData { IdTrimChars = 3 };
            renderer.Id(ad, "0000").Is(P(2) + "0");
            renderer.Id(ad, "0001").Is(P(2) + "1");
            renderer.Id(ad, "0009").Is(P(2) + "9");
            renderer.Id(ad, "0010").Is(P(1) + "10");
            renderer.Id(ad, "0099").Is(P(1) + "99");
            renderer.Id(ad, "0100").Is("100");
            renderer.Id(ad, "0999").Is("999");
        }

        [TestMethod]
        public void IdPresentation_03()
        {
            var renderer = new PairRenderer();
            var ad = new AssetData { IdTrimChars = 3 };
            renderer.Id(ad,  "1000").Is( "1000");
            renderer.Id(ad, "10000").Is("10000");
        }

        [TestMethod]
        public void IdPresentation_04()
        {
            // TableController uses CalculateIdTrimmer to determine IdTrimChars,
            // so the case like this test occurs only on intersegment pairs.

            var renderer = new PairRenderer();
            var ad = new AssetData { IdTrimChars = 3 };
            renderer.Id(ad, "01000").Is( "1000");
            renderer.Id(ad, "10000").Is("10000");
        }

        [TestMethod]
        public void IdPresentation_11()
        {
            var renderer = new PairRenderer();
            var ad = new AssetData { IdTrimChars = 3 };
            renderer.Id(ad,   "1a" ).Is(P(2) +  "1a" );
            renderer.Id(ad,  "10a" ).Is(P(1) + "10a" );
            renderer.Id(ad,  "10ab").Is(P(1) + "10ab");
            renderer.Id(ad, "100a" ).Is(      "100a" );
        }

        [TestMethod]
        public void IdPresentation_12()
        {
            var renderer = new PairRenderer();
            var ad = new AssetData { IdTrimChars = 3 };
            renderer.Id(ad,     "a").Is("a");
            renderer.Id(ad,    "0a").Is(P(2) + "0a");
            renderer.Id(ad,   "00a").Is(P(2) + "0a");
            renderer.Id(ad,  "000a").Is(P(2) + "0a");
            renderer.Id(ad, "0000a").Is(P(2) + "0a");
        }

        private static string P(int n)
        {
            return new string('\u2007', n);
        }
    }
}
