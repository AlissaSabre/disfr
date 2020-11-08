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
using disfr.Plugin;
using disfr.Writer;

namespace disfr.UI
{

    public class MainController : IMainController, INotifyPropertyChanged
    {
        public MainController()
        {
            DelegateCommandHelper.GetHelp(this, StaleExceptionEventHandler);
            Scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private readonly TaskScheduler Scheduler;

        private void StaleExceptionEventHandler(object sender, DelegateCommand.StaleExceptionEventArgs e)
        {
            throw new AggregateException(e.Exception);
        }

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

        public IEnumerable<string> PluginNames { get { return PluginManager.Current.PluginNames; } }

        #region OpenCommand

        public ReaderManager ReaderManager { get { return ReaderManager.Current; } }

        public string OpenFilterString { get { return ReaderManager.FilterString; } }

        public DelegateCommand<string[], int, bool, object> OpenCommand { get; private set; }

        private async Task OpenCommand_ExecuteAsync(string[] filenames, int index, bool single_tab, object tag)
        {
            Busy = true;
            try
            {
                IAssetBundle[] bundles = await Task.Run(() =>
                    (single_tab && filenames.Length > 1)
                        ? new[] { ReaderManager.Read(filenames, index) }
                        : filenames.Select(f => ReaderManager.Read(f, index)).ToArray());
                var new_tables = bundles.Select(b => TableController.LoadBilingualAssets(b)).ToArray();
                Array.ForEach(new_tables, t => { t.Tag = tag; });
                _Tables.AddRange(new_tables);
                RaisePropertyChanged("Tables");
            }
            finally
            {
                Busy = false;
            }
        }

        #endregion

        #region SaveAsCommand

        public WriterManager WriterManager { get { return WriterManager.Current; } }

        public string SaveAsFilterString { get { return WriterManager.FilterString; } }

        public DelegateCommand<string, int, ITableController, IColumnDesc[]> SaveAsCommand { get; private set; }

        private async Task SaveAsCommand_ExecuteAsync(string filename, int index, ITableController table, IColumnDesc[] columns)
        {
            Busy = true;
            try
            {
                await Task.Run(() =>
                    WriterManager.Write(filename, index, table.Rows, columns, (InlineString.Render)table.InlineStringRenderMode));
            }
            finally
            {
                Busy = false;
            }
        }

        private bool SaveAsCommand_CanExecute(string filename, int index, ITableController table, IColumnDesc[] columns)
        {
            return table != null;
        }

        #endregion

        #region CloseCommand

        public DelegateCommand<ITableController> CloseCommand { get; private set; }

        private Task CloseCommand_ExecuteAsync(ITableController table)
        {
            _Tables.Remove(table);
            RaisePropertyChanged("Tables");
            return Task.FromResult<object>(null);
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

        private Task CloseExceptCommand_ExecuteAsync(ITableController table)
        {
            _Tables.Clear();
            _Tables.Add(table);
            RaisePropertyChanged("Tables");
            return Task.FromResult<object>(0);
        }

        private bool CloseExceptCommand_CanExecute(ITableController table)
        {
            //return _Tables.Contains(table) && _Tables.Count >= 2;
            return table != null && _Tables.Count >= 2;
        }

        #endregion

        #region OpenAltCommand

        public DelegateCommand<ITableController, string[], object> OpenAltCommand { get; private set; }

        private async Task OpenAltCommand_ExecuteAsync(ITableController table, string[] origins, object tag)
        {
            Busy = true;
            try
            {
                // It is questionable to invoke LoadAltAssets by a worker thread,
                // though that's what disfr has been doing for several years already.
                // FIXME!
                var result = await Task.Run(() => table.LoadAltAssets(origins));
                result.Tag = tag;
                _Tables.Add(result);
                RaisePropertyChanged("Tables");
            }
            finally
            {
                Busy = false;
            }
        }

        private bool OpenAltCommand_CanExecute(ITableController table, string[] origins, object tag)
        {
            return table != null && table.HasAltAssets;
        }

        #endregion

        #region ExitCommand

        public DelegateCommand ExitCommand { get; private set; }

        private Task ExitCommand_ExecuteAsync()
        {
            App.Current.Shutdown();
            // I'm not sure the Shutdown method ever returns...
            return Task.FromResult<object>(null);
        }

        #endregion

    }
}
