using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace disfr.UI
{
    /// <summary>
    /// A set of application commands.
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// A command to show an About dialog.  Handled in <see cref="MainWindow"/>.
        /// </summary>
        public static readonly RoutedCommand About = new RoutedCommand("About", typeof(Commands));

        /// <summary>
        /// A command to compare a bilingual file against another.  NOT IMPLEMENTED YET.
        /// </summary>
        public static readonly RoutedCommand Compare = new RoutedCommand("Compare", typeof(Commands));

        /// <summary>
        /// A command to invoke a debug feature.  Handled in <see cref="MainWindow"/>.
        /// </summary>
        public static readonly RoutedCommand Debug = new RoutedCommand("Debug", typeof(Commands));

        /// <summary>
        /// A command to show a Font dialog to change font.  Handled in <see cref="MainWindow"/>.
        /// </summary>
        public static readonly RoutedCommand Font = new RoutedCommand("Font", typeof(Commands));

        /// <summary>
        /// A command to extract complementary TM (alt-trans contents).  Handled in <see cref="MainWindow"/>.
        /// </summary>
        public static readonly RoutedCommand OpenAlt = new RoutedCommand("OpenAlt", typeof(Commands));

        /// <summary>
        /// A command to make selection empty.  Handled in <see cref="System.Windows.Controls.DataGrid"/> on <see cref="TableView"/>.
        /// </summary>
        public static readonly RoutedCommand UnselectAll = new RoutedCommand("UnselectAll", typeof(Commands));
    }
}
