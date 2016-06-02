using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
        public static void Main()
        {
            disfr.App app = new disfr.App();
            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            base.OnStartup(e);

            MainWindow = new MainWindow();
            MainWindow.DataContext = new MainController();
            MainWindow.Show();
        }
    }
}
