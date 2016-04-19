using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Plugin;
using disfr.UI;

namespace disfr.Writer
{
    public class WriterManager
    {
        private static WriterManager _Current;

        public static WriterManager Current { get { return _Current ?? CreateInstance(); } }

        private static WriterManager CreateInstance()
        {
            lock (typeof(WriterManager))
            {
                if (_Current == null)
                {
                    var manager = new WriterManager();

                    // Add standard writers.
                    manager.Add(new TmxWriter());

                    // Add plugin writers.
                    foreach (var wr in PluginManager.Current.Writers)
                    {
                        manager.Add(wr);
                    }

                    _Current = manager;    
                }
                return _Current;
            }
        }

        private List<IRowsWriter> Writers = new List<IRowsWriter>();

        public void Add(IRowsWriter writer)
        {
            Writers.Add(writer);
            _FilterString = null;
        }

        public IEnumerable<IRowsWriter> AsEnumerable()
        {
            return Writers;
        }

        private string _FilterString = null;

        public string FilterString { get { return _FilterString ?? (_FilterString = CreateFilter()); } }

        private string CreateFilter()
        {
            return string.Join("|", Writers.SelectMany(r => r.FilterString));
        }

        public void Write(string filename, int index, IEnumerable<IRowData> data, object write_params)
        {
            // Unlike ReaderManager, WriterManager doesn't support auto-detection,
            // so the write selection is simpler.

            if (index < 0) throw new ArgumentOutOfRangeException("index");

            int adjusted_index = index;
            int i = 0;
            while (i < Writers.Count)
            {
                int count = Writers[i].FilterString.Count;
                if (adjusted_index < count) break;
                adjusted_index -= count;
                i++;
            }

            if (i >= Writers.Count) throw new ArgumentOutOfRangeException("index");

            Writers[i].Write(filename, adjusted_index, data, write_params);
        }
    }
}
