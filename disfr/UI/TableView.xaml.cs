using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;

using ColumnDesc = disfr.Writer.ColumnDesc;

namespace disfr.UI
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : TabItem
    {
        public TableView()
        {
            InitializeComponent();
            StandardColumns = CreateStandardColumns();
            DataContextChanged += this_DataContextChanged;
        }

        #region ColumnInUse attached property

        public static readonly DependencyProperty ColumnInUseProperty =
            DependencyProperty.Register("ColumnInUse", typeof(bool), typeof(TableView));

        public static void SetColumnInUse(DependencyObject target, bool in_use)
        {
            target.SetValue(ColumnInUseProperty, in_use);
        }

        public static bool GetColumnInUse(DependencyObject target)
        {
            return (bool)target.GetValue(ColumnInUseProperty);
        }

        #endregion

        #region StandardColumns local array

        private class StandardColumnInfo
        {
            public readonly DataGridColumn Column;
            public readonly Func<IRowData, bool> InUse;

            public StandardColumnInfo(DataGridColumn column, Func<IRowData, bool> in_use)
            {
                Column = column;
                InUse = in_use;
            }
        }

        private StandardColumnInfo[] CreateStandardColumns()
        {
            return new[]
            {
                new StandardColumnInfo(Serial, r => true),
                new StandardColumnInfo(Asset, r => true),
                new StandardColumnInfo(Id, r => !string.IsNullOrEmpty(r.Id)),
                new StandardColumnInfo(Source, r => true),
                new StandardColumnInfo(Target, r => true),
                new StandardColumnInfo(Asset2, r => !string.IsNullOrEmpty(r.Asset2)),
                new StandardColumnInfo(Id2, r => !string.IsNullOrEmpty(r.Id2)),
                new StandardColumnInfo(Target2, r => !GlossyString.IsNullOrEmpty(r.Target2)),
                new StandardColumnInfo(Notes, r => !string.IsNullOrEmpty(r.Notes)),
            };
        }

        private readonly StandardColumnInfo[] StandardColumns;

        #endregion

        #region Controller and dynamic column generation

        /// <summary>
        /// A typed alias of <see cref="DataContext"/>.
        /// </summary>
        /// <value>
        /// An <see cref="ITableController"/> to show on the grid.
        /// </value>
        public ITableController Controller
        {
            get { return DataContext as ITableController; }
            set { DataContext = value; }
        }

        private void this_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Reset the column organization.
            // This should be useless under the current scenario.
            foreach (var dc in dataGrid.Columns.Except(StandardColumns.Select(sc => sc.Column)).ToArray())
            {
                dataGrid.Columns.Remove(dc);
            }
            foreach (var dc in dataGrid.Columns)
            {
                dc.SetValue(ColumnInUseProperty, false);
            }

            // Our DataContext should always be an ITable, but null should be allowed (though we don't need.)
            var table = e.NewValue as ITableController;
            if (table == null) return;

            // Show / hide standard columns.
            // Unused standard columns are kept in the list, but users can't view them.
            // We use a false value of ColumnInUse atatched property to tell the case. 
            foreach (var colInfo in StandardColumns)
            {
                var in_use = table.Rows.Any(colInfo.InUse);
                colInfo.Column.Visibility = in_use ? Visibility.Visible : Visibility.Collapsed;
                colInfo.Column.SetValue(ColumnInUseProperty, in_use);
            }

            // A special handling of Asset column visibility.
            // There are many cases that a file contains only one Asset,
            // and Asset column is just redundant in the cases, so hide it in the cases.
            // (but users can still view it if they want.)
            if (Asset.Visibility == Visibility.Visible && 
                Asset2.Visibility != Visibility.Visible &&
                !table.Rows.Select(r => r.Asset).Distinct().Skip(1).Any())
            {
                Asset.Visibility = Visibility.Collapsed;
            }

            // Create columns for additional properties.
            // They are initially hidden but users can view them.
            // ColumnInUse attached property is set to true for the purpose. 
            foreach (var key in table.Rows.SelectMany(r => r.Keys).Distinct())
            {
                var column = new DataGridTextColumn()
                {
                    Header = key.Replace("_", " "), // XXX: No, we should not do this!
                    Binding = new Binding("[" + key + "]"),
                    Visibility = Visibility.Collapsed,
                };
                column.SetValue(ColumnInUseProperty, true);
                dataGrid.Columns.Add(column);
            }
        }

        #endregion

        public IEnumerable<DataGridColumn> Columns { get { return dataGrid.Columns; } }

        private void UnselectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            dataGrid.UnselectAllCells();
            e.Handled = true;
        }

        public ColumnDesc[] VisibleColumnDescs
        {
            get
            {
                return dataGrid.Columns
                    .Where(c => c.Visibility == Visibility.Visible)
                    .OrderBy(c => c.DisplayIndex)
                    .Select(c => new ColumnDesc(
                        c.Header.ToString(),
                        (c.ClipboardContentBinding as Binding)?.Path?.Path))
                    .ToArray();
            }
        }

        private void dataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            // We manage DataGrid sorting in a slightly different way than usual:
            //  - When a column header is clicked, the table is sorted via the column (as usual),
            //    but Seq (invisible to user) is always automatically specified as the second sort key.
            //  - The second clicking on a same column reverses the sort order (this is normal),
            //    but the third clicking on a sort key column resets the sorting,
            //    as opposed to reverses the sort order again.
            //    The fourth clicking is same as the first clicking.
            //  - Shift key is ignored; users can't specify his/her own secondary sort keys.

            var dataGrid = sender as DataGrid;
            foreach (var c in dataGrid.Columns) c.SortDirection = null;
            var cv = dataGrid.Items as CollectionView;

            var path = e.Column.SortMemberPath;
            if (cv.SortDescriptions.Count == 0 || cv.SortDescriptions[0].PropertyName != path)
            {
                // This is the first clicking on a column.
                cv.SortDescriptions.Clear();
                cv.SortDescriptions.Add(new SortDescription(path, ListSortDirection.Ascending));
                cv.SortDescriptions.Add(new SortDescription("Seq", ListSortDirection.Ascending));
                e.Column.SortDirection = ListSortDirection.Ascending;
            }
            else if (cv.SortDescriptions[0].Direction == ListSortDirection.Ascending)
            {
                // This is the second clicking on a column.
                cv.SortDescriptions[0] = new SortDescription(path, ListSortDirection.Descending);
                e.Column.SortDirection = ListSortDirection.Descending;
            }
            else
            {
                // This should be the third clicking on a column.
                cv.SortDescriptions.Clear();
            }

            e.Handled = true;
        }

        #region QuickFilter dependency property

        public static readonly DependencyProperty QuickFilterProperty =
            DependencyProperty.Register("QuickFilter", typeof(bool), typeof(TableView),
                new FrameworkPropertyMetadata(QuickFilterChangedCallback) { BindsTwoWayByDefault = true });
        
        public bool QuickFilter
        {
            get { return (bool)GetValue(QuickFilterProperty); }
            set { SetValue(QuickFilterProperty, value); }
        }

        private static void QuickFilterChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TableView).OnQuickFilterChanged(true.Equals(e.NewValue));
        }

        private void OnQuickFilterChanged(bool value)
        {
            var visible = value ? Visibility.Visible : Visibility.Collapsed;
            foreach (var c in dataGrid.Columns)
            {
                var h = c.Header;
            }
        }

        #endregion

        #region FilterBox backends

        /// <summary>
        /// Identifire of the private attached property Column.
        /// </summary>
        private static readonly DependencyProperty ColumnProperty
            = DependencyProperty.Register("Column", typeof(DataGridColumn), typeof(TableView));

        /// <summary>
        /// Identifier of the private attached property FIlterRegex.
        /// </summary>
        private static readonly DependencyProperty FilterRegexProperty
            = DependencyProperty.Register("FilterRegex", typeof(Regex), typeof(TableView));

        private void FilterBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // We want to hook into a PART_EditableTextBox component of a ComboBox.
            // I first thought we can do so in Loaded event handler, but I was wrong.
            // The visual tree appears to be created only after the ComboBox becomes visible,
            // and IsVisibleChanged event handler is raised just before the visual tree creation.
            // So, we need some tricky maneuver.
            if ((bool)e.NewValue)
            {
                // We have nothing to do upon every Visibility changes.
                // We only need one-time bootstrap.
                var filterbox = sender as ComboBox;
                filterbox.IsVisibleChanged -= FilterBox_IsVisibleChanged;

                // WPF is about to make this ComboBox visible,
                // so next time its dispatcher becomes idle, it should have been mde visible.
                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)delegate ()
                {
                    // The header text of a column that we are filtering with
                    // is stored in the TextBlock next to this ComboBox.
                    var header = FindVisualChild<TextBlock>(VisualTreeHelper.GetParent(filterbox), "HeaderText").Text;

                    // It is a UI text, so we can't compare it against a fixed string,
                    // but we can surely compare them each other (assuming there are no duplicates...)
                    var column = dataGrid.Columns.FirstOrDefault(c => (c.Header as string) == header);

                    // Next find a TextBox portion of the ComboBox and hook into it.
                    var textbox = FindVisualChild<TextBox>(filterbox, "PART_EditableTextBox");
                    textbox.SetValue(ColumnProperty, column);
                    textbox.TextChanged += FilterBox_TextBox_TextChanged;
                });
            }
        }

        private bool FilterUpdating = false;

        private void FilterBox_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var column = textbox.GetValue(ColumnProperty) as DataGridColumn;

            var text = textbox.Text;
            column.SetValue(FilterRegexProperty, string.IsNullOrEmpty(text) ? null : new Regex(Regex.Escape(textbox.Text), RegexOptions.IgnoreCase));

            // For the moment, we need to enumerate all RowData whenever the ContentsFilter is changed.
            // It takes some significant time.
            // We postpone updating the filter when the user is typing the filter text quickly,
            // so that the typed texts appear on the screen as soon as possible.
            // Well, I think the core cause of this problem is that the filtering-enumeration is performed by the UI thread.
            // Hence I have a feeling we should run the filtering operation in a separate thread.  FIXME. 
            if (!FilterUpdating)
            {
                FilterUpdating = true;
                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)delegate ()
                {
                    FilterUpdating = false;
                    var matchers = dataGrid.Columns.Select(CreateMatcher).Where(m => m != null).ToArray();
                    Controller.ContentsFilter = row =>
                    {
                        foreach (var m in matchers)
                        {
                            if (!m(row)) return false;
                        }
                        return true;
                    };
                });
            }
        }

        private static Func<IRowData, bool> CreateMatcher(DataGridColumn column)
        {
            var regex = column.GetValue(FilterRegexProperty) as Regex;
            if (regex == null) return null;
            var grabber = CreateGrabber(column.SortMemberPath);
            return row => regex.IsMatch(grabber(row));
        }

        private static Func<IRowData, string> CreateGrabber(string path)
        {
            if (path.StartsWith("[") && path.EndsWith("]"))
            {
                var key = path.Substring(1, path.Length - 2);
                return r => r[key];
            }

            var property = typeof(IRowData).GetProperty(path);
            if (property.PropertyType == typeof(string))
            {
                return r => property.GetValue(r) as string;
            }
            else
            {
                return r => property.GetValue(r).ToString(); 
            }
        }

        /// <summary>
        /// Find a node of a particular type and optinally a name in a visual tree.
        /// </summary>
        /// <typeparam name="ChildType">Type of a node to find.</typeparam>
        /// <param name="obj">An object which forms a visual tree to find a child in.</param>
        /// <param name="name">name of a child to find, or null if name is not important.</param>
        /// <returns>Any one of the children of the type and the name, or null if none found.</returns>
        private static ChildType FindVisualChild<ChildType>(DependencyObject obj, string name = null) where ChildType : FrameworkElement
        {
            if (obj == null) return null;
            if (obj is ChildType &&
                (name == null || (obj as FrameworkElement).Name == name)) return obj as ChildType;
            for (int i = VisualTreeHelper.GetChildrenCount(obj) - 1; i >= 0; --i)
            {
                var child = FindVisualChild<ChildType>(VisualTreeHelper.GetChild(obj, i), name);
                if (child != null) return child;
            }
            return null;
        }

        private static ParentType FindVisualParent<ParentType>(DependencyObject obj) where ParentType : DependencyObject
        {
            while (obj != null)
            {
                if (obj is ParentType) return obj as ParentType;
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Convert a Boolean value to and from a <see cref="Visibility"/> value.
    /// </summary>
    /// <seealso cref="BooleanToVisibilityConverter"/>
    /// <remarks>
    /// This class supports an opposite direction of conversion/binding than <see cref="BooleanToVisibilityConverter"/>. 
    /// </remarks>
    [ValueConversion(typeof(Visibility), typeof(bool))]
    public class VisibilityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Visibility?) == Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as bool?) == true ? Visibility.Visible : Visibility.Collapsed;
        }
    }


    public class SubtractingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as double?) - (parameter as IConvertible)?.ToDouble(culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as double?) + (parameter as IConvertible)?.ToDouble(culture);
        }
    }

    /// <summary>
    /// A StyleSelector that selects one of two styles to see whether a row is at an Asset boundary.
    /// </summary>
    public class RowStyleSelector : StyleSelector
    {
        /// <summary>
        /// Style to apply to a row at an Asset boundary.
        /// </summary>
        public Style BoundaryStyle { get; set; }

        /// <summary>
        /// Style to apply to a row other than at an Asset boundary.
        /// </summary>
        public Style MiddleStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var info = item as IRowData;
            if (info == null) return null;

            // We can't simply check info.SerialInAsset == 1,
            // since the DataGrid may be sorted by Source or Target contents. 

            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            var index = itemsControl.ItemContainerGenerator.IndexFromContainer(container);
            if (index == 0) return BoundaryStyle;

            var prev = itemsControl.Items[index - 1] as IRowData;
            return (prev == null || prev.Asset != info.Asset) ? BoundaryStyle : MiddleStyle;
        }
    }
}
