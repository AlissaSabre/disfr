using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    // Yes, we really need to make a concurrent test

    [TestClass]
    public class PropertiesManagerTest
    {
        [TestMethod]
        public void PutOne_sequential()
        {
            PutOne(new PropertiesManager(false));
        }

        [TestMethod]
        public void PutOne_concurrent()
        {
            PutOne(new PropertiesManager(true));
        }

        private void PutOne(PropertiesManager manager)
        {
            string[] props1 = null;
            manager.Put(ref props1, "one", "one-1");
            manager.Get(props1, "one").Is("one-1");

            string[] props2 = null;
            manager.Put(ref props2, "one", "one-2");
            manager.Get(props1, "one").Is("one-1");
            manager.Get(props2, "one").Is("one-2");

            manager.Properties.Select(info => info.Key).Is(new[] { "one" });
            props1[0].Is("one-1");
            props2[0].Is("one-2");
        }

        [TestMethod]
        public void PutTwo_sequential()
        {
            PutTwo(new PropertiesManager(false));
        }

        [TestMethod]
        public void PutTwo_concurrent()
        {
            PutTwo(new PropertiesManager(true));
        }

        private void PutTwo(PropertiesManager manager)
        {
            string[] props1 = null;
            manager.Put(ref props1, "one", "one-1");
            manager.Put(ref props1, "two", "two-1");
            manager.Get(props1, "one").Is("one-1");
            manager.Get(props1, "two").Is("two-1");

            string[] props2 = null;
            manager.Put(ref props2, "one", "one-2");
            manager.Put(ref props2, "two", "two-2");
            manager.Get(props1, "one").Is("one-1");
            manager.Get(props1, "two").Is("two-1");
            manager.Get(props2, "one").Is("one-2");
            manager.Get(props2, "two").Is("two-2");

            string[] props3 = null;
            manager.Put(ref props3, "two", "two-3");
            manager.Put(ref props3, "one", "one-3");
            manager.Get(props1, "one").Is("one-1");
            manager.Get(props1, "two").Is("two-1");
            manager.Get(props2, "one").Is("one-2");
            manager.Get(props2, "two").Is("two-2");
            manager.Get(props3, "one").Is("one-3");
            manager.Get(props3, "two").Is("two-3");

            manager.Properties.Select(info => info.Key).Is(new[] { "one", "two" });
            props1[0].Is("one-1");
            props1[1].Is("two-1");
            props2[0].Is("one-2");
            props2[1].Is("two-2");
            props3[0].Is("one-3");
            props3[1].Is("two-3");
        }

        [TestMethod]
        public void Overwriting_sequential()
        {
            Overwriting(new PropertiesManager(false));
        }

        [TestMethod]
        public void Overwriting_concurrent()
        {
            Overwriting(new PropertiesManager(true));
        }

        private void Overwriting(PropertiesManager manager)
        {
            string[] props = null;
            manager.Put(ref props, "one", "one-A");
            manager.Put(ref props, "two", "two-A");
            manager.Get(props, "one").Is("one-A");
            manager.Get(props, "two").Is("two-A");

            manager.Put(ref props, "one", "one-B");
            manager.Get(props, "one").Is("one-B");
            manager.Get(props, "two").Is("two-A");

            manager.Put(ref props, "two", "two-B");
            manager.Get(props, "one").Is("one-B");
            manager.Get(props, "two").Is("two-B");
        }

        [TestMethod]
        public void VisibleBefore_sequential()
        {
            VisibleBefore(new PropertiesManager(false));
        }

        [TestMethod]
        public void VisibleBefore_concurrent()
        {
            VisibleBefore(new PropertiesManager(true));
        }

        [TestMethod]
        public void VisibleAfter_sequential()
        {
            VisibleAfter(new PropertiesManager(false));
        }

        [TestMethod]
        public void VisibleAfter_concurrent()
        {
            VisibleAfter(new PropertiesManager(true));
        }

        private void VisibleBefore(PropertiesManager manager)
        {
            manager.MarkVisible("two");
            manager.MarkVisible("ten");
            manager.Properties.Where(info => info.Visible).Select(info => info.Key).Is(Enumerable.Empty<string>());

            string[] props = null;

            manager.Put(ref props, "one", "one-1");
            manager.Properties.Where(info => info.Visible).Select(info => info.Key).Is(Enumerable.Empty<string>());

            manager.Put(ref props, "two", "two-1");
            manager.Properties.Where(info => info.Visible).Select(info => info.Key).Is(new[] { "two" });

            manager.Put(ref props, "three", "three-1");
            manager.Properties.Where(info => info.Visible).Select(info => info.Key).Is(new[] { "two" });
        }

        private void VisibleAfter(PropertiesManager manager)
        {
            string[] props = null;
            manager.Put(ref props, "one", "one-1");
            manager.Put(ref props, "two", "two-1");
            manager.Put(ref props, "three", "three-1");
            manager.Properties.Where(info => info.Visible).Select(info => info.Key).Is(Enumerable.Empty<string>());

            manager.MarkVisible("two");
            manager.Properties.Where(info => info.Visible).Select(info => info.Key).Is(new[] { "two" });

            manager.MarkVisible("one");
            manager.Properties.Where(info => info.Visible).Select(info => info.Key).Is(new[] { "one", "two" });

            manager.MarkVisible("ten");
            manager.Properties.Where(info => info.Visible).Select(info => info.Key).Is(new[] { "one", "two" });
        }
    }
}
