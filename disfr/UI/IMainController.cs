using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IColumnDesc = disfr.Writer.IColumnDesc;

namespace disfr.UI
{
    /// <summary>
    /// Defines interface that <see cref="MainController"/> provides for <see cref="MainWindow"/>.
    /// </summary>
    /// <remarks>
    /// See <see cref="MainController"/> for the semantics.
    /// </remarks>
    public interface IMainController
    {
        IEnumerable<ITableController> Tables { get; }
        string OpenFilterString { get; }
        string SaveAsFilterString { get; }
        bool Busy { get; set; }
        IEnumerable<string> PluginNames { get; }

        DelegateCommand<string[], int, bool, object> OpenCommand { get; }
        DelegateCommand<string, int, ITableController, IColumnDesc[]> SaveAsCommand { get; }
        DelegateCommand<ITableController, string[], object> OpenAltCommand { get; }
        DelegateCommand<ITableController> CloseCommand { get; }
        DelegateCommand<ITableController> CloseExceptCommand { get; }
        DelegateCommand ExitCommand { get; }
    }
}
