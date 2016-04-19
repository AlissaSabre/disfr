using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

using Dragablz;
using WpfColorFontDialog;

namespace disfr.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContextChanged += this_DataContextChanged;
            tables.ClosingItemCallback = tabControl_ClosingItemCallback;
            //tabControl.Items.Clear();
        }

        /// <summary>
        /// A static instance of <see cref="InterTabClient"/> shared by all <see cref="TabablzControl"/> on all windows.
        /// </summary>
        public static readonly InterTabClient InterTabClient = new InterTabClient();


        private IMainController Controller { get { return (IMainController)DataContext; } }


        private void this_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // This code tries to cover the cases that the DataContext is changed to another backend on-the-fly.
            // However, it is not tested.  Assume it doesn't.
            if (e.OldValue != null)
            {
                ((INotifyPropertyChanged)e.OldValue).PropertyChanged -= DataContext_PropertyChanged;
            }
            if (e.NewValue != null)
            {
                ((INotifyPropertyChanged)e.NewValue).PropertyChanged += DataContext_PropertyChanged;
            }
            SyncToTables();
        }

        /// <summary>
        /// Indicates that this Window is waiting for a new Tab/Table being added on it.
        /// </summary>
        /// <remarks>
        /// No, I don't think this is a good idea. :(
        /// </remarks>
        private bool AcceptingNewTable = true;

        private bool IsClosing = false;

        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tables" && !IsClosing)
            {
                SyncToTables();
            }
        }

        /// <summary>
        /// Syncrhonize tabs to the Tables on the backend.
        /// </summary>
        private void SyncToTables()
        {
            // Remove all redundant tabs.
            foreach (var tab in tables.Items.Cast<TableView>().Where(i => !Controller.Tables.Contains(i.DataContext)).ToArray())
            {
                tables.Items.Remove(tab);
            }

            if (AcceptingNewTable || IsSoleWindow())
            {
                // Add all new tabs.
                var all_tables_in_view = TabablzControl.GetLoadedInstances().SelectMany(t => t.Items.Cast<TableView>()).Select(i => i.DataContext).ToArray();
                var all_new_tables = Controller.Tables.Except(all_tables_in_view).ToArray();
                if (all_new_tables.Count() > 0)
                {
                    TableView first_added = null;
                    foreach (var table in all_new_tables)
                    {
                        var tab = new TableView() { DataContext = table };
                        tables.Items.Add(tab);
                        if (first_added == null) first_added = tab;
                    }
                    tables.SelectedItem = first_added;
                    AcceptingNewTable = false;
                }
            }
            else if (tables.Items.Count == 0)
            {
                Close();
            }
        }

        private bool IsSoleWindow() { return !TabablzControl.GetLoadedInstances().Any(t => t != tables); }

        private void tabControl_ClosingItemCallback(ItemActionCallbackArgs<TabablzControl> args)
        {
            var tab = args.DragablzItem.Content as TableView;
            Controller.CloseCommand.Execute(tab?.Controller);
        }

        private void this_Closing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            foreach (var tab in tables.Items.Cast<TableView>().ToArray())
            {
                Controller.CloseCommand.Execute(tab.Controller);
            }
        }



        private OpenFileDialog OpenFileDialog = new OpenFileDialog() { Multiselect = true };

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Under some circumstances (that we can't control; e.g., what shortcuts the user has on his/her desktop),
            // OpenFileDialog.ShowDialog() may require very long time (more than several seconds)
            // after a user clicks on OK or Cancel button and the dialog box disappears.
            // It appears to a user that this app stops working.
            // I tried to work around it by setting Busy flag early, though it seems not very effective...

            Controller.Busy = true;
            OpenFileDialog.Filter = Controller.OpenFilterString;
            if (OpenFileDialog.ShowDialog(this) == true)
            {
                var filenames = OpenFileDialog.FileNames;
                var index = OpenFileDialog.FilterIndex - 1; // Returned index is 1-based but we expect a 0-based index.
                Controller.OpenCommand.Execute(filenames, index);
                AcceptingNewTable = true;
            }
            else
            {
                Controller.Busy = false;
            }
            e.Handled = true;
        }

        private SaveFileDialog SaveFileDialog = new SaveFileDialog();

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Controller.Busy = true;
            SaveFileDialog.Filter = Controller.SaveAsFilterString;
            if (SaveFileDialog.ShowDialog(this) == true)
            {
                var filename = SaveFileDialog.FileName;
                var index = SaveFileDialog.FilterIndex - 1; // Returned index is 1-based but we expect a 0-based index.
                Controller.SaveAsCommand.Execute(filename, index, e.Parameter as ITableController);
            }
            else
            {
                Controller.Busy = false;
            }
            e.Handled = true;

        }

        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Controller.SaveAsCommand.CanExecute(null, -1, e.Parameter as ITableController);
            e.Handled = true;
        }

        private void Font_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new ColorFontDialog();
            dlg.Font = FontInfo.GetControlFont(tables.SelectedContent as Control);
            if (dlg.ShowDialog() == true)
            {
                FontInfo.ApplyFont(tables.SelectedContent as Control, dlg.Font);
            }
            e.Handled = true;
        }

        private void Font_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (tables.SelectedContent as Control) != null;
            e.Handled = true;
        }

        private void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            new AboutDialog() { Owner = this }.ShowDialog();
            e.Handled = true;
        }

        private void Debug_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ;
        }

    }
}
