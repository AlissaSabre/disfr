using System;
using System.Collections.Generic;
using System.Globalization;
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

        private static readonly DependencyProperty ColumnInUseProperty =
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
