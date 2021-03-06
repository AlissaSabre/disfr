﻿using System;
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

        public string Package { get { return Renderer.PackageName(AssetData); } }

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

        public string this[int key] { get { return TransPair[AssetData.PropMapper[key]]; } }

        public string FlatSource { get { return Renderer.FlatFromInline(TransPair.Source); } }

        public string FlatTarget { get { return Renderer.FlatFromInline(TransPair.Target); } }

        public string FlatTarget2 { get { return null; } }

        public string SourceLang { get { return AssetData.SourceLang; } }

        public string TargetLang { get { return AssetData.TargetLang; } }

        public object AssetIdentity { get { return AssetData; } }

        InlineString ITransPair.Source { get { return TransPair.Source; } }

        InlineString ITransPair.Target { get { return TransPair.Target; } }

        IEnumerable<string> ITransPair.Notes { get { return TransPair.Notes; } }
    }

    public class AssetData
    {
        public int BaseSerial { get; set; }

        public string ShortPackageName { get; private set; }

        private string _LongPackageName;

        public string LongPackageName
        {
            get { return _LongPackageName; }
            set
            {
                _LongPackageName = value;
                ShortPackageName = Basename(value);
            }
        }

        public string ShortAssetName { get; private set; }

        private string _LongAssetName;

        public string LongAssetName
        {
            get { return _LongAssetName; }
            set
            {
                _LongAssetName = value;
                ShortAssetName = Basename(value);
            }
        }

        public int IdTrimChars { get; set; }

        /// <summary>
        /// Calculates the optimal <see cref="IdTrimChars"/> value for <see cref="ITransPair"/>s.
        /// </summary>
        /// <param name="pairs">A set of <see cref="ITransPair"/>s.</param>
        public void CalculateIdTrimmer(IEnumerable<ITransPair> pairs)
        {
            // Assuming an ID consists of a number optinally followed by a suffix,
            // the optimal IdTrimChars value is the minimum required length of digits
            // to hold the prefix numbers.
            // In other words,
            // this method calculates the maximum length of $2
            // when IDs are matched against a regular expression "^([0]*)([1-9][0-9]*)([^0-9].*)?$".
            // Note that we ignore intersegment contents.
            int chars = 0;
            foreach (var pair in pairs)
            {
                if (pair.Serial > 0)
                {
                    var id = pair.Id;
                    int p = 0;
                    while (p < id.Length && id[p] == '0') p++;
                    int q = p;
                    while (q < id.Length && id[q] >= '0' && id[q] <= '9') q++;
                    chars = Math.Max(chars, q - p);
                }
            }
            IdTrimChars = chars;
        }

        private static readonly char[] SEPARATORS = { '/', '\\' };

        private string Basename(string full)
        {
            if (full == null) return full;
            var p = full.LastIndexOfAny(SEPARATORS);
            return (p < 0) ? full : full.Substring(p + 1);
        }

        public string SourceLang { get; set; }

        public string TargetLang { get; set; }

        public int[] PropMapper { get; set; }
    }

    public class AdditionalPropertiesInfo
    {
        public readonly int Index;

        public readonly string Key;

        public readonly bool Visible;

        public AdditionalPropertiesInfo(int index, string key, bool visible)
        {
            Index = index;
            Key = key;
            Visible = visible;
        }
    }

}
