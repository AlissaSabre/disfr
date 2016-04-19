using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.UI;
using System.Windows.Input;

namespace UnitTestDisfr
{
    [TestClass]
    public class DelegateCommandTest
    {
        [TestMethod]
        public void ZeroParameter_1()
        {
            int x, y;
            bool z = false;

            var dc = new DelegateCommand(() => { x = 1; }, () => { y = 2; return z; });

            x = y = 0; z = false;
            dc.CanExecute().Is(false);
            x.Is(0);
            y.Is(2);

            x = y = 0; z = true;
            dc.CanExecute().Is(true);
            x.Is(0);
            y.Is(2);

            x = y = 0; z = false;
            dc.Execute();
            x.Is(1);
            y.Is(0);
        }

        [TestMethod]
        public void ZeroParameter_2()
        {
            int x;

            var dc = new DelegateCommand(() => { x = 1; }, null);

            x = 0;
            dc.CanExecute().Is(true);
            x.Is(0);

            x = 0;
            dc.Execute();
            x.Is(1);
        }

        [TestMethod]
        public void ZeroParameter_3()
        {
            int x;

            var dc = new DelegateCommand(() => { x = 1; });

            x = 0;
            dc.CanExecute().Is(true);
            x.Is(0);

            x = 0;
            dc.Execute();
            x.Is(1);
        }

        [TestMethod]
        public void ZeroParameter_4()
        {
            int x, y;
            bool z = false;

            ICommand ic = new DelegateCommand(() => { x = 1; }, () => { y = 2; return z; });

            x = y = 0; z = false;
            ic.CanExecute(null).Is(false);
            x.Is(0);
            y.Is(2);

            x = y = 0; z = true;
            ic.CanExecute(null).Is(true);
            x.Is(0);
            y.Is(2);

            x = y = 0; z = false;
            ic.Execute(null);
            x.Is(1);
            y.Is(0);

            AssertEx.Catch<ArgumentException>(() => ic.CanExecute(0));
            AssertEx.Catch<ArgumentException>(() => ic.Execute(0));

            AssertEx.Catch<ArgumentException>(() => ic.CanExecute(""));
            AssertEx.Catch<ArgumentException>(() => ic.Execute(""));

            AssertEx.Catch<ArgumentException>(() => ic.CanExecute("0"));
            AssertEx.Catch<ArgumentException>(() => ic.Execute("0"));

            AssertEx.Catch<ArgumentException>(() => ic.CanExecute("1"));
            AssertEx.Catch<ArgumentException>(() => ic.Execute("1"));
        }

        [TestMethod]
        public void OneParameter_1()
        {
            int x, y;
            bool z = false;

            var dc = new DelegateCommand<int>(p => { x = p; }, p => { y = p; return z; });

            x = y = 0; z = false;
            dc.CanExecute(33).Is(false);
            x.Is(0);
            y.Is(33);

            x = y = 0; z = true;
            dc.CanExecute(44).Is(true);
            x.Is(0);
            y.Is(44);

            x = y = 0; z = false;
            dc.Execute(55);
            x.Is(55);
            y.Is(0);
        }

        [TestMethod]
        public void OneParameter_2()
        {
            int x;

            var dc = new DelegateCommand<int>(p => { x = p; }, null);

            x = 0;
            dc.CanExecute(33).Is(true);
            x.Is(0);

            x = 0;
            dc.Execute(44);
            x.Is(44);
        }

        [TestMethod]
        public void OneParameter_3()
        {
            int x;

            var dc = new DelegateCommand<int>(p => { x = p; });

            x = 0;
            dc.CanExecute(33).Is(true);
            x.Is(0);

            x = 0;
            dc.Execute(44);
            x.Is(44);
        }

        [TestMethod]
        public void OneParameter_4()
        {
            int x, y;
            bool z = false;

            ICommand ic = new DelegateCommand<int>(p => { x = p; }, p => { y = p; return z; });

            x = y = 0; z = false;
            ic.CanExecute(33).Is(false);
            x.Is(0);
            y.Is(33);

            x = y = 0; z = true;
            ic.CanExecute(44).Is(true);
            x.Is(0);
            y.Is(44);

            x = y = 0; z = false;
            ic.Execute(55);
            x.Is(55);
            y.Is(0);

            AssertEx.Catch<NullReferenceException>(() => ic.CanExecute(null));
            AssertEx.Catch<NullReferenceException>(() => ic.Execute(null));

            AssertEx.Catch<InvalidCastException>(() => ic.CanExecute(1.0));
            AssertEx.Catch<InvalidCastException>(() => ic.Execute(1.0));

            AssertEx.Catch<InvalidCastException>(() => ic.CanExecute((short)1));
            AssertEx.Catch<InvalidCastException>(() => ic.Execute((short)1));

            AssertEx.Catch<InvalidCastException>(() => ic.CanExecute("1"));
            AssertEx.Catch<InvalidCastException>(() => ic.Execute("1"));
        }

        [TestMethod]
        public void TwoParameters_1()
        {
            string s, t;
            int x, y;
            bool z = false;

            var dc = new DelegateCommand<string, int>((p, q) => { s = p; x = q; }, (p, q) => { t = p; y = q; return z; });

            s = t = ""; x = y = 0; z = false;
            dc.CanExecute("a", 33).Is(false);
            s.Is("");
            t.Is("a");
            x.Is(0);
            y.Is(33);

            s = t = ""; x = y = 0; z = true;
            dc.CanExecute("b", 44).Is(true);
            s.Is("");
            t.Is("b");
            x.Is(0);
            y.Is(44);

            s = t = ""; x = y = 0; z = false;
            dc.Execute("c", 55);
            s.Is("c");
            t.Is("");
            x.Is(55);
            y.Is(0);
        }

        [TestMethod]
        public void TwoParameters_2()
        {
            string s;
            int x;

            var dc = new DelegateCommand<string, int>((p, q) => { s = p; x = q; }, null);

            s = ""; x = 0;
            dc.CanExecute("a", 33).Is(true);
            s.Is("");
            x.Is(0);

            s = ""; x = 0;
            dc.Execute("b", 44);
            s.Is("b");
            x.Is(44);
        }

        [TestMethod]
        public void TwoParameters_3()
        {
            string s;
            int x;

            var dc = new DelegateCommand<string, int>((p, q) => { s = p; x = q; });

            s = ""; x = 0;
            dc.CanExecute("a", 33).Is(true);
            s.Is("");
            x.Is(0);

            s = ""; x = 0;
            dc.Execute("b", 44);
            s.Is("b");
            x.Is(44);
        }

        [TestMethod]
        public void TwoParameters_4()
        {
            string s, t;
            int x, y;
            bool z = false;

            ICommand ic = new DelegateCommand<string, int>((p, q) => { s = p; x = q; }, (p, q) => { t = p; y = q; return z; });

            s = t = ""; x = y = 0; z = false;
            ic.CanExecute(new object[] { "a", 33 }).Is(false);
            s.Is("");
            t.Is("a");
            x.Is(0);
            y.Is(33);

            s = t = ""; x = y = 0; z = true;
            ic.CanExecute(new object[] { "b", 44 }).Is(true);
            s.Is("");
            t.Is("b");
            x.Is(0);
            y.Is(44);

            s = t = ""; x = y = 0; z = false;
            ic.Execute(new object[] { "c", 55 });
            s.Is("c");
            t.Is("");
            x.Is(55);
            y.Is(0);

            AssertEx.Catch<NullReferenceException>(() => ic.CanExecute(null));
            AssertEx.Catch<NullReferenceException>(() => ic.Execute(null));
        }
    }
}
