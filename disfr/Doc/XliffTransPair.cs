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
        private static readonly IReadOnlyDictionary<string, string> EmptyProps
            = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

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

        private Dictionary<string, string> _Props = null;

        public IReadOnlyDictionary<string, string> Props
        {
            get { return _Props ?? EmptyProps; }
        }

        internal void AddProp(string key, string value)
        {
            if (value == null) return;
            if (_Props == null) _Props = new Dictionary<string, string>();
            _Props[key] = value;
        }
    }
}
