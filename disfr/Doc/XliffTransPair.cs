using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    class XliffTransPair : ITransPair
    {
        public int Serial { get; internal set; }

        public string Id { get; internal set; }

        public InlineString Source { get; internal set; }

        public InlineString Target { get; internal set; }

        public string SourceLang { get; internal set; }

        public string TargetLang { get; internal set; }

        private HashSet<string> _Notes = null; 

        public IEnumerable<string> Notes { get { return _Notes; } }

        internal void AddNotes(IEnumerable<string> notes)
        {
            if (notes == null) return;
            var valid_notes = notes.Where(s => !string.IsNullOrWhiteSpace(s));
            if (!valid_notes.Any()) return;
            if (_Notes == null) _Notes = new HashSet<string>();
            _Notes.UnionWith(valid_notes);
        }

        internal string[] _Props = null;

        public string this[int key]
        {
            get
            {
                return (key < _Props?.Length) ? _Props[key] : null;
            }
        }
    }
}
