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
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        public static void Main(string[] args)
        {
            string command = "-s";
            if (args.Length > 0 && args[0].StartsWith("-"))
            {
                command = args[0];
                args = args.Skip(1).ToArray();
            }

            if (command == "-s")
            {
                var ipc_port = CreateUniquePort(typeof(App).Assembly);
                var uri = "Run";

                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        var server_channel = new IpcServerChannel(ipc_port);
                        ChannelServices.RegisterChannel(server_channel, true);
                        RemotingConfiguration.RegisterWellKnownServiceType(typeof(SingleInstance), uri, WellKnownObjectMode.Singleton);
                        try
                        {
                            var app = new App();
                            app.Run();
                        }
                        catch (Exception) { }
                        return;
                    }
                    catch (Exception) { }
                    Thread.Sleep(50);
                    try
                    {
                        var client_channel = new IpcClientChannel();
                        ChannelServices.RegisterChannel(client_channel, true);
                        var instance = Activator.GetObject(typeof(SingleInstance), "ipc://" + ipc_port + "/" + uri) as SingleInstance;
                        if (instance.Run(args)) return;
                    }
                    catch (Exception) { }
                    Thread.Sleep(50);
                }

                MessageBox.Show("Appliation failed: Couldn't establish remoting.", "Error - disfr");
            }
            else
            {
                var app = new App();
                app.Run();
            }
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
            new MainWindow() { DataContext = MainController }.Show();
            var args = e.Args;
            if (args.Length > 0 && args[0].StartsWith("-"))
            {
                args = args.Skip(1).ToArray();
            }
            if (args.Length > 0)
            {
                MainController.OpenCommand.Execute(args, -1, false);
            }
        }

        public bool OpenNewWindow(string[] files)
        {
            return Dispatcher.Invoke(() =>
            {
                new MainWindow() { DataContext = MainController }.Show();
                if (files.Length > 0)
                {
                    MainController.OpenCommand.Execute(files, -1, false);
                }
                return true;
            }, DispatcherPriority.ApplicationIdle);
        }

        class SingleInstance : MarshalByRefObject
        {
            public bool Run(string[] args)
            {
                return (Application.Current as App)?.OpenNewWindow(args) == true;
            }
        }
    }
}
