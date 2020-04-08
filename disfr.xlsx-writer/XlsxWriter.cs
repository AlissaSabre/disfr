using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;
using Excel = NetOffice.ExcelApi.Application;

using disfr.Plugin;
using disfr.Doc;
using disfr.Writer;

namespace disfr.xlsx_writer
{
    public class XlsxWriterPlugin : IWriterPlugin, IPluginStatus
    {
        public string Name { get { return "XlsxWriter"; } }

        private static bool HasExcel = Type.GetTypeFromProgID("Excel.Application") != null;

        public IWriter CreateWriter()
        {
            return HasExcel ? new XlsxWriter() : null;
        }

        public string Status
        {
            get { return HasExcel ? null : "Excel not found on this computer"; }
        }
    }

    public class XlsxWriter : TableWriterBase, IPairsWriter
    {
        public string Name { get { return "Xlsx Writer"; } }

        private static readonly IList<string> _FilterString = new string[]
        {
            "Microsoft Excel File|*.xlsx",
        };

        public IList<string> FilterString { get { return _FilterString; } }

        public void Write(string filename, int filterindex_UNUSED, IEnumerable<ITransPair> pairs, IColumnDesc[] columns, InlineString.Render render)
        {
            string tmpname = null;
            try
            {
                tmpname = CreateTempFile(Path.GetTempPath(), ".xml");
                new XmlssWriter().Write(tmpname, 0, pairs, columns, render);

                var excel = new Excel() { Visible = false, Interactive = false, DisplayAlerts = false };
                try
                {
                    var book = excel.Workbooks.Open(tmpname);
                    book.SaveAs(filename, XlFileFormat.xlOpenXMLWorkbook);
                    book.Close(false);
                }
                finally
                {
                    excel.Quit();
                    excel.Dispose();
                }
            }
            finally
            {
                if (tmpname != null) File.Delete(tmpname);
            }
        }

        private static string CreateTempFile(string folder, string ext)
        {
            var rng = new byte[9];
            using (var rand = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 100; i++)
                {
                    rand.GetBytes(rng);
                    var name = Path.Combine(folder, "disfr-" + Convert.ToBase64String(rng).Replace('/', '-') + ext);
                    if (File.Exists(name)) continue;
                    try
                    {
                        var s = File.Open(name, FileMode.CreateNew);
                        s.Close();
                        return name;
                    }
                    catch (IOException)
                    {
                        ;
                    }
                }
                throw new Exception("Couldn't find a unique random file name.");
            }
        }
    }
}
