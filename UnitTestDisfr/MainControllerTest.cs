using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.UI;

namespace UnitTestDisfr
{
    [TestClass]
    public class MainControllerTest
    {
        private const string IDIR = @"..\..\..\UnitTestDisfrDoc\Samples";

        private readonly string[] ALT_ORIGINS =
        {
            "generic.tmx",
            "v11/messages.xml",
        };

        [TestMethod]
        public void OpenAltCommand_Execute_1()
        {
            RunDispatched(OpenAltCommand_Execute_1_body);
        }

        private void OpenAltCommand_Execute_1_body()
        {
            var controller = new MainController();
            (controller as INotifyPropertyChanged).PropertyChanged += controller_PropertyChanged;

            controller.Tables.Count().Is(0);

            PrepareForWaiting();
            controller.OpenCommand.Execute(new[] { Path.Combine(IDIR, "Xliff2.xliff") }, -1, true, "XYZZY");
            Wait();

            controller.Tables.Count().Is(1);
            controller.Tables.ElementAt(0).Tag.Is("XYZZY");

            var table = controller.Tables.ElementAt(0);
            PrepareForWaiting();
            controller.OpenAltCommand.Execute(table, ALT_ORIGINS, "42");
            Wait();

            controller.Tables.Count().Is(2);
            controller.Tables.ElementAt(1).Tag.Is("42");
            controller.Tables.ElementAt(1).AllRows.Count().Is(2);
        }

        [TestMethod]
        public void OpenAltCommand_Execute_2()
        {
            RunDispatched(OpenAltCommand_Execute_2_body);
        }

        private void OpenAltCommand_Execute_2_body()
        {
            var controller = new MainController();
            (controller as INotifyPropertyChanged).PropertyChanged += controller_PropertyChanged;

            controller.Tables.Count().Is(0);

            PrepareForWaiting();
            controller.OpenCommand.Execute(new[] { Path.Combine(IDIR, "Xliff2.xliff") }, -1, true, "Deep One");
            Wait();

            controller.Tables.Count().Is(1);
            controller.Tables.ElementAt(0).Tag.Is("Deep One");

            var table = controller.Tables.ElementAt(0);
            PrepareForWaiting();
            controller.OpenAltCommand.Execute(table, new[] { ALT_ORIGINS[0] }, "Cthulhu");
            Wait();

            controller.Tables.Count().Is(2);
            controller.Tables.ElementAt(1).Tag.Is("Cthulhu");
            controller.Tables.ElementAt(1).AllRows.Count().Is(1);
        }

        [TestMethod]
        public void OpenAltCommand_Execute_3()
        {
            RunDispatched(OpenAltCommand_Execute_3_body);
        }

        private void OpenAltCommand_Execute_3_body()
        {
            var controller = new MainController();
            (controller as INotifyPropertyChanged).PropertyChanged += controller_PropertyChanged;

            controller.Tables.Count().Is(0);

            PrepareForWaiting();
            controller.OpenCommand.Execute(new[] { Path.Combine(IDIR, "Xliff2.xliff") }, -1, true, "foo");
            Wait();

            controller.Tables.Count().Is(1);
            controller.Tables.ElementAt(0).Tag.Is("foo");

            var table = controller.Tables.ElementAt(0);
            PrepareForWaiting();
            controller.OpenAltCommand.Execute(table, new[] { "nothing" }, "bar");
            Wait();

            controller.Tables.Count().Is(2);
            controller.Tables.ElementAt(1).Tag.Is("bar");
            controller.Tables.ElementAt(1).AllRows.Count().Is(0);
        }

        [TestMethod]
        public void AltAssetOrigins_1()
        {
            RunDispatched(AltAssetOrigins_1_body);
        }

        private void AltAssetOrigins_1_body()
        {
            var controller = new MainController();
            (controller as INotifyPropertyChanged).PropertyChanged += controller_PropertyChanged;

            controller.Tables.Count().Is(0);

            PrepareForWaiting();
            controller.OpenCommand.Execute(new[] { Path.Combine(IDIR, "Xliff2.xliff") }, -1, true, null);
            Wait();

            controller.Tables.Count().Is(1);
            var origins = controller.Tables.First().AltAssetOrigins.ToList();
            origins.Count.Is(ALT_ORIGINS.Length);
            origins.Except(ALT_ORIGINS).Is(Enumerable.Empty<string>());
            ALT_ORIGINS.Except(origins).Is(Enumerable.Empty<string>());
        }

        // a quck-n-dirty sort of a Didpatcher framework simulator...

        private volatile bool TableChanged = false;

        private volatile DispatcherFrame WaitingFrame = null;

        private void PrepareForWaiting()
        {
            TableChanged = false;
        }

        private void Wait()
        {
            while (!TableChanged)
            {
                WaitingFrame = new DispatcherFrame();
                Dispatcher.PushFrame(WaitingFrame);
                WaitingFrame = null;
            }
        }

        private void controller_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tables")
            {
                TableChanged = true;
                if (WaitingFrame != null) WaitingFrame.Continue = false;
            }
        }

        private static void RunDispatched(Action body)
        {
            Exception exception = null;
            var t = new Thread((ThreadStart)delegate ()
            {
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
                Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate ()
                {
                    try
                    {
                        body();
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                    finally
                    {
                        Dispatcher.CurrentDispatcher.InvokeShutdown();
                    }
                });
                Dispatcher.Run();
            });
            t.Start();
            t.Join();
            if (exception != null) ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }
}
