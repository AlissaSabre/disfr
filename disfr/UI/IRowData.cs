using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace disfr.UI
{
    public interface IRowData : ITransPair
    {
        bool Hidden { get; }

        /// <summary>
        /// Internal sequential number of all rows.
        /// </summary>
        /// <remarks>
        /// This property is used for stabilizing sorting.
        /// </remarks>
        int Seq { get; }

        /// <summary>
        /// Asset name.
        /// </summary>
        string Asset { get; }

        new GlossyString Source { get; }

        new GlossyString Target { get; }

        int Serial2 { get; }

        string Asset2 { get; }

        string Id2 { get; }

        GlossyString Target2 { get; }

        new string Notes { get; }

        string TagList { get; }

        string FlatSource { get; }

        string FlatTarget { get; }

        string FlatTarget2 { get; }

        object AssetIdentity { get; }
    }
}
