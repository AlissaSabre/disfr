using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

using disfr.UI;

namespace disfr
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public class App : Application
    {
        /// <summary>
        /// Represents command line options to this app.
        /// </summary>
        public class Options
        {
            /// <summary>
            /// Indicates the application must run in a single-instance.
            /// </summary>
            public bool SingleInstance = true;

            /// <summary>
            /// Indicates all specified files should be read into a single tab.
            /// </summary>
            public bool ReadIntoSingleTab = false;

            /// <summary>
            /// List of files to read.
            /// </summary>
            public string[] Files;

            private Options() { }

            /// <summary>
            /// Parses the command line arguments.
            /// </summary>
            /// <param name="args">Command line arguments as passed to the program entry point Main(string[]).</param>
            /// <returns>An <see cref="Options"/> instance holding parsed options.</returns>
            /// <exception cref="ArgumentException">
            /// <paramref name="args"/> includes an unknown option designator.
            /// </exception>
            public static Options Parse(string[] args)
            {
                var options = new Options();
                var a = new List<string>(args);
                while (a.Count > 0 && a[0].StartsWith("-"))
                {
                    switch (a[0])
                    {
                        case "-t": options.ReadIntoSingleTab = true; break;
                        case "-m": options.ReadIntoSingleTab = false; break;
                        case "-s": options.SingleInstance = true; break;
                        case "-i": options.SingleInstance = false; break;
                        default:
                            throw new ArgumentException("Unknown option: " + a[0], "args");
                    }
                    a.RemoveAt(0);
                }
                options.Files = a.ToArray();
                return options;
            }
        }

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            Options options;
            try
            {
                options = Options.Parse(args);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error in command line - disfr");
                return;
            }

            if (!options.SingleInstance)
            {
                var app = new App();
                app.Run();
            }
            else
            {
                var ipc_port = CreateUniquePort(typeof(App).Assembly);
                var uri = "Run";

                // If two or more process started executing this program at some critical timing,
                // or an existing App (running as IPC server) is terminating when a new process tried to start,
                // the IPC channel connection may fail.
                // We need to detect the case and try again,
                // but don't retry too much, since the failure may be by other reason (e.g., system resource exhaust.)

                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        // Try to be an IPC server.
                        var server_channel = new IpcServerChannel(null, ipc_port);
                        try
                        {
                            ChannelServices.RegisterChannel(server_channel, true);
                            try
                            {
                                RemotingConfiguration.RegisterWellKnownServiceType(typeof(SingleInstance), uri, WellKnownObjectMode.Singleton);

                                // We've got a server-side IPC channel.  Run as a server.
                                try
                                {
                                    var app = new App();
                                    app.Exit += (s, e) => server_channel.StopListening(null);
                                    app.Run();
                                }
                                catch (Exception)
                                {
                                    // The program may got an exception and terminates, due to a bug.
                                    // We SHOULD NOT retry IPC connection in the case.  Just terminate.
                                }
                                return;
                            }
                            finally
                            {
                                ChannelServices.UnregisterChannel(server_channel);
                            }
                        }
                        finally
                        {
                            server_channel.StopListening(null);
                        }
                    }
                    catch (Exception) { }

                    // When we got here, it is very likely that an IPC server
                    // (another disfr process with UI) is already running.
                    Thread.Sleep(50);

                    try
                    {
                        // Try to be an IPC client.
                        var client_channel = new IpcClientChannel((string)null, null);
                        ChannelServices.RegisterChannel(client_channel, true);
                        try
                        {
                            var instance = Activator.GetObject(typeof(SingleInstance), "ipc://" + ipc_port + "/" + uri) as SingleInstance;
                            if (instance?.Run(args) == true) return;
                        }
                        finally
                        {
                            ChannelServices.UnregisterChannel(client_channel);
                        }
                    }
                    catch (Exception) { }

                    // When we got here, it is likely that an IPC server was running but terminated.
                    Thread.Sleep(50);
                }

                MessageBox.Show("Appliation failed: Couldn't establish remoting.", "Error - disfr");
            }
        }

        private static void App_Exit(object sender, ExitEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static string CreateUniquePort(Assembly assembly)
        {
            var guid = GetAttribute<GuidAttribute>(assembly).Value;
            var version = GetAttribute<AssemblyFileVersionAttribute>(assembly).Version;
            var session = Process.GetCurrentProcess().SessionId.ToString();
            return guid + "+" + version + "+" + session;
        }

        private static T GetAttribute<T>(Assembly assembly) where T: Attribute
        {
            return assembly.GetCustomAttributes(typeof(T), false)[0] as T;
        }

        public App()
        {
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }

        public IMainController MainController;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainController = new MainController();
            OpenNewWindow(e.Args);
        }

        /// <summary>
        /// Open a new application window as if invoked with command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments as passed to program entry point <see cref="Main(string[])"/>.</param>
        /// <returns><c>true</c> if successfully opened.</returns>
        public bool OpenNewWindow(string[] args)
        {
            var options = Options.Parse(args);
            MainWindow window = null;
            Dispatcher.Invoke(() =>
            {
                window = new MainWindow() { DataContext = MainController };
                window.Topmost = true;
                window.OpenFiles(options.Files, options.ReadIntoSingleTab);
                window.Show();
            }, DispatcherPriority.ApplicationIdle);
            Dispatcher.Invoke(() =>
            {
                window.Topmost = false;
                //window.Activate();
            }, DispatcherPriority.ApplicationIdle);
            return true;
        }

        /// <summary>
        /// The remote server object.
        /// </summary>
        class SingleInstance : MarshalByRefObject
        {
            public bool Run(string[] args)
            {
                return (Application.Current as App)?.OpenNewWindow(args) == true;
            }
        }
    }

}
