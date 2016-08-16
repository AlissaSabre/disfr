using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace disfr.UI
{
    public class BilingualRowData : IRowData
    {
        public BilingualRowData(PairRenderer renderer, AssetData asset, ITransPair pair, int seq)
        {
            TransPair = pair;
            AssetData = asset;
            Renderer = renderer;
            Seq = seq;
        }

        protected readonly ITransPair TransPair;

        protected readonly AssetData AssetData;

        protected readonly PairRenderer Renderer;

        public bool Hidden { get { return TransPair.Serial < 0; } }

        public int Seq { get; private set; }

        public int Serial { get { return Renderer.Serial(AssetData, TransPair.Serial); } }

        public string Asset { get { return Renderer.AssetName(AssetData); } }

        public string Id { get { return Renderer.Id(AssetData, TransPair.Id); } }

        public GlossyString Source { get { return Renderer.GlossyFromInline(TransPair.Source); } }

        public GlossyString Target { get { return Renderer.GlossyFromInline(TransPair.Target); } }

        public int Serial2 { get { return 0; } }

        public string Asset2 { get { return null; } }

        public string Id2 { get { return null; } }

        public GlossyString Target2 { get { return null; } }

        public string Notes { get { return Renderer.Notes(TransPair.Notes); } }

        public string TagList { get { return Renderer.TagListFromInline(TransPair.Source); } }

        public string this[string key] { get { return TransPair[key]; } }

        public string FlatSource { get { return Renderer.FlatFromInline(TransPair.Source); } }

        public string FlatTarget { get { return Renderer.FlatFromInline(TransPair.Target); } }

        public string FlatTarget2 { get { return null; } }

        public InlineString RawSource { get { return TransPair.Source; } }

        public InlineString RawTarget { get { return TransPair.Target; } }

        public InlineString RawTarget2 { get { return null; } }

        public string SourceLang { get { return AssetData.SourceLang; } }

        public string TargetLang { get { return AssetData.TargetLang; } }
    }

    public class AssetData
    {
        public int BaseSerial { get; set; }

        private string _ShortAssetName;

        public string ShortAssetName { get { return _ShortAssetName; } }

        private string _LongAssetName;

        public string LongAssetName
        {
            get { return _LongAssetName; }
            set
            {
                _LongAssetName = value;
                _ShortAssetName = Basename(value);
            }
        }

        public int IdTrimChars { get; set; }

        private static readonly char[] SEPARATORS = { '/', '\\' };

        private string Basename(string full)
        {
            if (full == null) return full;
            var p = full.LastIndexOfAny(SEPARATORS);
            return (p < 0) ? full : full.Substring(p + 1);
        }

        public string SourceLang { get; set; }

        public string TargetLang { get; set; }
    }

    public class AdditionalPropertiesInfo
    {
        public readonly string Key;

        public readonly bool Visible;

        public AdditionalPropertiesInfo(string key, bool visible)
        {
            Key = key;
            Visible = visible;
        }
    }

}
