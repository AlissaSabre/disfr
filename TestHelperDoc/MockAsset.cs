using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace TestHelperDoc
{
    public class MockAsset : IAsset
    {
        public string Package
        {
            get => _Package ?? throw new InvalidOperationException($"{nameof(Package)} is not set.");
            set => _Package = value;
        }
        private string _Package;

        public string Original
        {
            get => _Original ?? throw new InvalidOperationException($"{nameof(Original)} is not set.");
            set => _Original = value;
        }
        private string _Original;

        public string SourceLang
        {
            get => _SourceLang ?? throw new InvalidOperationException($"{nameof(SourceLang)} is not set.");
            set => _SourceLang = value;
        }
        private string _SourceLang;

        public string TargetLang
        {
            get => _TargetLang ?? throw new InvalidOperationException($"{nameof(TargetLang)} is not set.");
            set => _TargetLang = value;
        }
        private string _TargetLang;

        public IEnumerable<ITransPair> TransPairs
        {
            get => _TransPairs ?? throw new InvalidOperationException($"{nameof(TransPairs)} is not set.");
            set => _TransPairs = value;
        }
        private IEnumerable<ITransPair> _TransPairs;

        public IEnumerable<ITransPair> AltPairs
        {
            get => _AltPairs ?? throw new InvalidOperationException($"{nameof(AltPairs)} is not set.");
            set => _AltPairs = value;
        }
        private IEnumerable<ITransPair> _AltPairs;

        public IList<PropInfo> Properties
        {
            get => _Properties ?? throw new InvalidOperationException($"{nameof(Properties)} is not set.");
            set => _Properties = value;
        }
        private IList<PropInfo> _Properties;
    }

    public class MockTransPair : ITransPair
    {
        public int Serial
        {
            get => _Serial ?? throw new InvalidOperationException($"{nameof(Serial)} is not set.");
            set => _Serial = value;
        }
        private int? _Serial;

        public string Id
        {
            get => _Id ?? throw new InvalidOperationException($"{nameof(Id)} is not set.");
            set => _Id = value;
        }
        private string _Id;

        public InlineString Source
        {
            get => _Source ?? throw new InvalidOperationException($"{nameof(Source)} is not set.");
            set => _Source = value;
        }
        private InlineString _Source;

        public InlineString Target
        {
            get => _Target ?? throw new InvalidOperationException($"{nameof(Target)} is not set.");
            set => _Target = value;
        }
        private InlineString _Target;

        public string SourceLang
        {
            get => _SourceLang ?? throw new InvalidOperationException($"{nameof(SourceLang)} is not set.");
            set => _SourceLang = value;
        }
        private string _SourceLang;

        public string TargetLang
        {
            get => _TargetLang ?? throw new InvalidOperationException($"{nameof(TargetLang)} is not set.");
            set => _TargetLang = value;
        }
        private string _TargetLang;

        public IEnumerable<string> Notes
        {
            get => _Notes ?? throw new InvalidOperationException($"{nameof(Notes)} is not set.");
            set => _Notes = value;
        }
        private IEnumerable<string> _Notes;

        public string this[int index]
        {
            get => _item.TryGetValue(index, out var value) ? value : throw new InvalidOperationException($"this[{index}] is not set.");
            set => _item[index] = value;
        }
        private Dictionary<int, string> _item = new Dictionary<int, string>();
    }
}
