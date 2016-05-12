using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for ExceptionDialog.xaml
    /// </summary>
    public partial class ExceptionDialog : Window
    {
        public ExceptionDialog()
        {
            InitializeComponent();
        }

        private Exception _Exception;

        public Exception Exception
        {
            get { return _Exception; }
            set
            {
                _Exception = value;
                Message.Text = value.Message;
                Details.Text = value.ToString();
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
