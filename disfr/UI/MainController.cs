using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using Dragablz;

using disfr.Doc;
using disfr.Writer;

namespace disfr.UI
{

    public class MainController : IMainController, INotifyPropertyChanged
    {
        public MainController()
        {
            DelegateCommandHelper.GetHelp(this);
            Scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private readonly TaskScheduler Scheduler;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Tables

        private readonly List<ITableController> _Tables = new List<ITableController>();

        public IEnumerable<ITableController> Tables { get { return _Tables; } }

        #endregion

        #region Busy

        private bool _Busy = false;

        public bool Busy
        {
            get { return _Busy; }
            set
            {
                if (value != _Busy)
                {
                    _Busy = value;
                    RaisePropertyChanged("Busy");
                }
            }
        }

        #endregion

        #region OpenCommand

        public ReaderManager ReaderManager { get { return ReaderManager.Current; } }

        public string OpenFilterString { get { return ReaderManager.FilterString; } }

        public DelegateCommand<string[], int, bool, object> OpenCommand { get; private set; }

        private void OpenCommand_Execute(string[] filenames, int index, bool single_tab, object tag)
        {
            Busy = true;
            Task.Run(() =>
            {
                ITableController[] result;
                var started = DateTime.UtcNow;
                if (single_tab && filenames.Length > 1)
                {
                    result = new[]
                    {
                        TableController.LoadBilingualAssets(
                            name: "(multiple files)",
                            assets: filenames.SelectMany(f => ReaderManager.Read(f, index).Assets))
                    };
                }
                else
                {
                    result = filenames.Select(f =>
                        TableController.LoadBilingualAssets(
                            name: ReaderManager.FriendlyFilename(f),
                            assets: ReaderManager.Read(f, index).Assets)
                    ).ToArray();
                }
                Console.WriteLine("Elapsed: {0} ms", (DateTime.UtcNow - started).TotalMilliseconds);
                Array.ForEach(result, tc => { tc.Tag = tag; });
                return result;
            }).ContinueWith(worker =>
            {
                if (worker.IsFaulted)
                {
                    InvokeThrowTaskException(worker);
                }
                else
                {
                    _Tables.AddRange(worker.Result);
                    RaisePropertyChanged("Tables");
                }
                Busy = false;
            }, Scheduler);
        }

        #endregion

        #region SaveAsCommand

        public WriterManager WriterManager { get { return WriterManager.Current; } }

        public string SaveAsFilterString { get { return WriterManager.FilterString; } }

        public DelegateCommand<string, int, ITableController, ColumnDesc[]> SaveAsCommand { get; private set; }

        private void SaveAsCommand_Execute(string filename, int index, ITableController table, ColumnDesc[] columns)
        {
            Busy = true;
            Task.Run(() =>
            {
                WriterManager.Write(filename, index, table.Rows, columns);
            }).ContinueWith(worker =>
            {
                if (worker.IsFaulted)
                {
                    InvokeThrowTaskException(worker);
                }
                Busy = false;
            }, Scheduler);
        }

        private bool SaveAsCommand_CanExecute(string filename, int index, ITableController table, ColumnDesc[] columns)
        {
            return table != null;
        }

        #endregion

        #region CloseCommand

        public DelegateCommand<ITableController> CloseCommand { get; private set; }

        private void CloseCommand_Execute(ITableController table)
        {
            _Tables.Remove(table);
            RaisePropertyChanged("Tables");
        }

        private bool CloseCommand_CanExecute(ITableController table)
        {
            //return _Tables.Contains(table);

            // if table is not null, it must be contained in _Tables
            // unless this app got crazy.
            // We don't need to test for containment every time can-execute is evaluated.
            return table != null;
        }

        #endregion

        #region CloseExceptCommand

        public DelegateCommand<ITableController> CloseExceptCommand { get; private set; }

        private void CloseExceptCommand_Execute(ITableController table)
        {
            _Tables.Clear();
            _Tables.Add(table);
            RaisePropertyChanged("Tables");
        }

        private bool CloseExceptCommand_CanExecute(ITableController table)
        {
            //return _Tables.Contains(table) && _Tables.Count >= 2;
            return table != null && _Tables.Count >= 2;
        }

        #endregion

        #region OpenAltCommand

        public DelegateCommand<ITableController, string[], object> OpenAltCommand { get; private set; }

        private void OpenAltCommand_Execute(ITableController table, string[] origins, object tag)
        {
            Busy = true;
            Task.Run(() =>
            {
                var result = table.LoadAltAssets(origins);
                result.Tag = tag;
                return result;
            }).ContinueWith(worker =>
            {
                if (worker.IsFaulted)
                {
                    InvokeThrowTaskException(worker);
                }
                else
                {
                    _Tables.Add(worker.Result);
                    RaisePropertyChanged("Tables");
                }
                Busy = false;
            }, Scheduler);
        }

        private bool OpenAltCommand_CanExecute(ITableController table, string[] origins, object tag)
        {
            return table != null && table.HasAltAssets;
        }

        #endregion

        #region ExitCommand

        public DelegateCommand ExitCommand { get; private set; }

        private void ExitCommand_Execute()
        {
            App.Current.Shutdown();
        }

        #endregion

        private static void InvokeThrowTaskException(Task worker)
        {
            var e = worker.Exception;
            Dispatcher.FromThread(Thread.CurrentThread)?.BeginInvoke((Action)delegate { throw e; });
        }

    }
}
