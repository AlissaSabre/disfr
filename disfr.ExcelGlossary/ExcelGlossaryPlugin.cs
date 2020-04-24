using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Plugin;

namespace disfr.ExcelGlossary
{
    /// <summary>
    /// disfr plugin for reading glossary files based on Excel spreadsheet.
    /// </summary>
    /// <remarks>
    /// This plugin only supports Excel files in the following format:
    /// </remarks>
    public class ExcelGlossaryPlugin : IReaderPlugin, IWriterPlugin, IPluginStatus
    {
        public string Name { get { return "ExcelGlossaryReader"; } }

        private static bool HasExcel = Type.GetTypeFromProgID("Excel.Application") != null;

        public IReader CreateReader() { return HasExcel ? new ExcelGlossaryReader() : null; }

        public IWriter CreateWriter() { return null; }

        public string Status { get { return HasExcel ? null : "Excel not found on this computer"; } }
    }
}
