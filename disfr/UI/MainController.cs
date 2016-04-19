using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        #region OpenCommand

        public ReaderManager ReaderManager { get { return ReaderManager.Current; } }

        public string OpenFilterString { get { return ReaderManager.FilterString; } }

        public DelegateCommand<string, int> OpenCommand { get; private set; }

        private void OpenCommand_Execute(string filename, int index)
        {
            Busy = true;
            var bw = new BackgroundWorker();
            bw.DoWork += OpenCommand_Worker_DoWork;
            bw.RunWorkerCompleted += OpenCommand_Worker_RunWorkerCompleted;
            bw.RunWorkerAsync(Tuple.Create(filename, index));
        }

        private void OpenCommand_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var parameter = e.Argument as Tuple<string, int>;
            var filename = parameter.Item1;
            var index = parameter.Item2;
            var table = TableController.LoadBilingualAssets(
                name: ReaderManager.FriendlyFilename(filename),
                assets: ReaderManager.Read(filename, index));
            e.Result = table;
        }

        private void OpenCommand_Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ((IDisposable)sender).Dispose();
            if (e.Error != null)
            {
                // Make some feedback.
            }
            if (e.Result != null)
            {
                _Tables.Add(e.Result as ITableController);
                RaisePropertyChanged("Tables");
            }
            Busy = false;
        }

        #endregion

        #region SaveAsCommand

        public WriterManager WriterManager { get { return WriterManager.Current; } }

        public string SaveAsFilterString { get { return WriterManager.FilterString; } }

        public DelegateCommand<string, int, ITableController> SaveAsCommand { get; private set; }

        private void SaveAsCommand_Execute(string filename, int index, ITableController table)
        {
            Busy = true;
            var bw = new BackgroundWorker();
            bw.DoWork += SaveAsCommand_Worker_DoWork;
            bw.RunWorkerCompleted += SaveAsCommand_RunWorkerCompleted;
            bw.RunWorkerAsync(Tuple.Create(filename, index, table));
        }

        private void SaveAsCommand_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var parameter = e.Argument as Tuple<string, int, ITableController>;
            var filename = parameter.Item1;
            var index = parameter.Item2;
            var table = parameter.Item3;
            WriterManager.Write(filename, index, table.Rows, null);
        }

        private void SaveAsCommand_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ((IDisposable)sender).Dispose();
            if (e.Error != null)
            {
                // Make some feedback.
            }
            Busy = false;
        }

        private bool SaveAsCommand_CanExecute(string filename, int index, ITableController table)
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

        public DelegateCommand<ITableController> OpenAltCommand { get; private set; }

        private void OpenAltCommand_Execute(ITableController table)
        {
            Busy = true;
            var bw = new BackgroundWorker();
            bw.DoWork += OpenAltCommand_Worker_DoWork;
            bw.RunWorkerCompleted += OpenAltCommand_Worker_RunWorkerCompleted;
            bw.RunWorkerAsync(table);
        }

        private void OpenAltCommand_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var base_table = e.Argument as ITableController;
            var alt_table = base_table.LoadAltAssets();
            e.Result = alt_table;
        }

        private void OpenAltCommand_Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ((IDisposable)sender).Dispose();
            if (e.Error != null)
            {
                // Make some feedback.
            }
            if (e.Result != null)
            {
                _Tables.Add(e.Result as ITableController);
                RaisePropertyChanged("Tables");
            }
            Busy = false;
        }

        private bool OpenAltCommand_CanExecute(ITableController table)
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
    }
}
