using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Plugin;

namespace disfr.Doc
{
    public class ReaderManager
    {
        private const string ANY_FILES = "All known files";

        private const string ALL_FILES = "All files";

        private static ReaderManager _Current;

        public static ReaderManager Current { get { return _Current ?? CreateInstance(); } }

        private static ReaderManager CreateInstance()
        {
            lock (typeof(ReaderManager))
            {
                if (_Current == null)
                {
                    var manager = new ReaderManager();

                    // Add standard readers.
                    manager.Add(new XliffReader());
                    manager.Add(new TmxReader());
                    manager.Add(new SdltmReader());

                    // Add plugin readers.
                    manager.AddRange(PluginManager.Current.Readers);

                    _Current = manager;
                }
                return _Current;
            }
        }

        private List<IAssetReader> Readers = new List<IAssetReader>();

        private IAssetReader[] ReadersInPriority = null;

        public void Add(IAssetReader reader)
        {
            Readers.Add(reader);
            _FilterString = null;
            ReadersInPriority = null;
        }

        public void AddRange(IEnumerable<IAssetReader> readers)
        {
            Readers.AddRange(readers);
            _FilterString = null;
            ReadersInPriority = null;
        }

        public IEnumerable<IAssetReader> AsEnumerable()
        {
            return Readers;
        }

        private string _FilterString = null;

        public string FilterString { get { return _FilterString ?? (_FilterString = CreateFilter()); } }

        private string CreateFilter()
        {
            if (Readers.Count == 0)
            {
                return ALL_FILES + "|*.*";
            }
            else
            {
                return ANY_FILES + "|" +
                    string.Join(";", Readers.SelectMany(r => r.FilterString).SelectMany(s => s.Substring(s.IndexOf("|") + 1).Split(';', ',')).Distinct()) +
                    "|" +
                    string.Join("|", Readers.SelectMany(r => r.FilterString)) +
                    "|" + 
                    ALL_FILES + "|*.*";
            }
        }

        public IAssetBundle Read(string filename, int index = -1)
        {
            if (index < -1) throw new ArgumentOutOfRangeException("index");

            int adjusted_index = index - 1;
            int i = 0;
            while (i < Readers.Count)
            {
                int count = Readers[i].FilterString.Count;
                if (adjusted_index < count) break;
                adjusted_index -= count;
                i++;
            }

            if (i < Readers.Count && adjusted_index >= 0)
            {
                // An explicit file type (= reader) was specified.
                var assets = Readers[i].Read(filename, adjusted_index);
                if (assets == null)
                {
                    var name = Path.GetFileName(filename);
                    var type = Readers[i].FilterString[adjusted_index];
                    type = type.Substring(0, type.IndexOf('|'));
                    var message = string.Format("\"{0}\" is incompatible with the specified file format ({1}).", name, type);
                    throw new IOException(message);
                }
                return assets;
            }

            if (i >= Readers.Count && adjusted_index > 0)
            {
                // The specified index number was too large.
                throw new ArgumentOutOfRangeException("index");
            }

            // Index values -1, 0 and the maximum (last in the filters)
            // is for auto-detection.
            // Oh, the three values are for different user experience, but
            // we behave in an exactly same way once specified.

            if (ReadersInPriority == null)
            {
                ReadersInPriority = Readers.Where(r => r.Priority > 0).OrderBy(r => -r.Priority).ToArray();
            }
            foreach (var reader in ReadersInPriority)
            {
                var assets = reader.Read(filename, -1);
                if (assets != null) return assets;
            }

            throw new IOException(string.Format("Can't read \"{0}\"; file format is incompatible.", Path.GetFileName(filename)));
        }

        public string FriendlyFilename(string filename)
        {
            return Path.GetFileNameWithoutExtension(filename);
        }
    }
}
