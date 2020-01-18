using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace disfr.UI
{
    public interface IRowData
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
        /// Serial number of a row.
        /// </summary>
        /// <remarks>
        /// This property is visible to users.
        /// It was originally intended to simulate memoQ's segment numbering when it reads a foreign XLIFF.
        /// </remarks>
        int Serial { get; }

        /// <summary>
        /// Asset name.
        /// </summary>
        string Asset { get; }

        /// <summary>
        /// Row (segment) ID as assigned in the original file.
        /// </summary>
        string Id { get; }

        GlossyString Source { get; }

        GlossyString Target { get; }

        int Serial2 { get; }

        string Asset2 { get; }

        string Id2 { get; }

        GlossyString Target2 { get; }

        string Notes { get; }

        string TagList { get; }

        string this[int index] { get; }

        string FlatSource { get; }

        string FlatTarget { get; }

        string FlatTarget2 { get; }

        string SourceLang { get; }

        string TargetLang { get; }

        object AssetIdentity { get; }
    }
}
