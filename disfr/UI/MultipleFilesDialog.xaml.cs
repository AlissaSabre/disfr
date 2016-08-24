using System;
using System.Collections.Generic;
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
    /// Interaction logic for MultipleFilesDialog.xaml
    /// </summary>
    public partial class MultipleFilesDialog : Window
    {
        public MultipleFilesDialog()
        {
            InitializeComponent();
        }

        public bool SingleTab
        {
            get { return singleTabRadioButton.IsChecked == true; }
            set { singleTabRadioButton.IsChecked = value; }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
