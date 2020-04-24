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
    public class ExcelGlossaryPlugin : IReaderPlugin, IWriterPlugin
    {
        public string Name { get { return "ExcelGlossary"; } }
        public IReader CreateReader() { return new ExcelGlossaryReader(); }
        public IWriter CreateWriter() { return null; }
    }
}
