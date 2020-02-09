using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// Interaction logic for OriginChooserDialog.xaml
    /// </summary>
    public partial class OriginChooserDialog : Window
    {
        public OriginChooserDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// List of origins to show to the user.
        /// </summary>
        public IEnumerable<string> AllOrigins
        {
            get { return list.Items.Cast<string>(); }
            set
            {
                list.Items.Clear();
                foreach (var origin in value) list.Items.Add(origin);
                list.SelectAll();
            }
        }

        /// <summary>
        /// List of origins that the user has selected.
        /// </summary>
        public string[] SelectedOrigins { get { return list.SelectedItems.Cast<string>().ToArray(); } }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            // Make this dialog box to appear near the upper left corner of the owner window like OpenFileDialog.
            if (Owner != null)
            {
                var cl = Owner.Content as FrameworkElement;
                Top = Owner.Top + (Owner.ActualHeight - cl.ActualHeight);
                Left = Owner.Left + (Owner.ActualWidth - cl.ActualWidth);
            }
        }

    }
}
