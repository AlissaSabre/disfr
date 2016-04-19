using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace disfr.UI
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
            var version = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location);
            Title = string.Format("About {0}", version.ProductName);
            appName.Text = string.Format("{0} version {1}", version.ProductName, version.ProductVersion);
            appDesc.Text = string.Format("{0}", version.Comments);
            appAuth.Text = string.Format("Produced by {0}", version.CompanyName);
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
