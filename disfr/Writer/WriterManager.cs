using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Plugin;
using disfr.Doc;

namespace disfr.Writer
{
    public class WriterManager
    {
        private static WriterManager _Current;

        public static WriterManager Current { get { return _Current ?? CreateInstance(); } }

        private static readonly object Lock = new object();

        private static WriterManager CreateInstance()
        {
            lock (Lock)
            {
                if (_Current == null)
                {
                    var manager = new WriterManager();

                    // Add standard writers.
                    manager.Add(new TmxWriter());
                    manager.Add(new XlsxWriter());
                    manager.Add(new XmlTableWriter());

                    // Add plugin writers.
                    manager.AddRange(PluginManager.Current.Writers.Cast<IPairsWriter>());

                    // Add a debug writer.  (I want it listed last, since it is least useful for ordinary users.)
                    manager.Add(new XmlDebugTreeWriter());

                    _Current = manager;    
                }
                return _Current;
            }
        }

        private List<IPairsWriter> Writers = new List<IPairsWriter>();

        public void Add(IPairsWriter writer)
        {
            Writers.Add(writer);
            _FilterString = null;
        }

        public void AddRange(IEnumerable<IPairsWriter> writers)
        {
            Writers.AddRange(writers);
            _FilterString = null;
        }

        public IEnumerable<IPairsWriter> AsEnumerable()
        {
            return Writers;
        }

        private string _FilterString = null;

        public string FilterString { get { return _FilterString ?? (_FilterString = CreateFilter()); } }

        private string CreateFilter()
        {
            return string.Join("|", Writers.SelectMany(r => r.FilterString));
        }

        public void Write(string filename, int index, IEnumerable<ITransPair> data, IColumnDesc[] columns = null, InlineString.Render render = InlineString.RenderNormal)
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

            Writers[i].Write(filename, adjusted_index, data, columns, render);
        }
    }
}
