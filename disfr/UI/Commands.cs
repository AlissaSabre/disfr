using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace disfr.UI
{
    public static class Commands
    {
        /// <summary>
        /// A command to show an About dialog.  Handled in <see cref="MainWindow"/>.
        /// </summary>
        public static readonly RoutedCommand About = new RoutedCommand("About", typeof(Commands));

        public static readonly RoutedCommand Compare = new RoutedCommand("Compare", typeof(Commands));

        public static readonly RoutedCommand Debug = new RoutedCommand("Debug", typeof(Commands));

        public static readonly RoutedCommand Font = new RoutedCommand("Font", typeof(Commands));

        public static readonly RoutedCommand SaveAlt = new RoutedCommand("SaveAlt", typeof(Commands));

        /// <summary>
        /// A command to make selection empty.  Handled in <see cref="DataGrid"/> control on <see cref="TableView"/>.
        /// </summary>
        public static readonly RoutedCommand UnselectAll = new RoutedCommand("UnselectAll", typeof(Commands));
    }
}
