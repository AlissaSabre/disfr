using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        DelegateCommand<string, int> OpenCommand { get; }
        DelegateCommand<string, int, ITableController> SaveAsCommand { get; }
        DelegateCommand<ITableController> CloseCommand { get; }
        DelegateCommand<ITableController> CloseExceptCommand { get; }
        DelegateCommand ExitCommand { get; }
    }
}
