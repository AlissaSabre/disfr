using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;
using Excel = NetOffice.ExcelApi.Application;

using disfr.Doc;
using System.Globalization;

namespace disfr.ExcelGlossary
{
    class ExcelGlossaryReader : IAssetReader
    {
        private static readonly string[] _FilterString = { "Glossary on Excel files|*.xlsx;*.xls" };

        public IList<string> FilterString { get { return _FilterString; } }

        public string Name { get { return "ExcelGlossaryReader"; } }

        public int Priority { get { return 7; } }

        public IAssetBundle Read(string filename, int filterindex)
        {
            return LoaderAssetBundle.Create(
                ReaderManager.FriendlyFilename(filename),
                () => ReadAssets(filename, filterindex));
        }

        private IEnumerable<IAsset> ReadAssets(string filename, int fileterindex)
        {
            var list = new List<IAsset>();
            var excel = new Excel() { Visible = false, Interactive = false, DisplayAlerts = false };
            try
            {
                var book = excel.Workbooks.Open(filename);
                var sheets = book.Worksheets;
                for (int i = 1; i <= sheets.Count; i++)
                {
                    var sheet = sheets[i] as Worksheet;
                    var asset = ReadFromSheet(sheet);
                    if (asset != null)
                    {
                        list.Add(asset);
                    }
                    sheet.Dispose();
                }
                return list;
            }
            finally
            {
                excel.Quit();
                excel.Dispose();
            }
        }

        private IAsset ReadFromSheet(Worksheet sheet)
        {
            // Extract the meaningful range of the sheet.
            var bottom_right = sheet.Range("A1")
                .SpecialCells(XlCellType.xlCellTypeLastCell)
                .Address(true, true, XlReferenceStyle.xlA1);
            var cells = sheet.Range("A1", bottom_right).Value2 as object[,];
            var rows = cells.GetLength(0);
            var columns = cells.GetLength(1);

            // Process headers on the first row.
            var header = new string[columns];
            int source_column = -1;
            int target_column = -1;
            string source_language = null;
            string target_language = null;
            for (int i = 0; i < columns; i++)
            {
                var content = cells[1, 1 + i]?.ToString();
                if (string.IsNullOrWhiteSpace(content))
                {
                    var address = sheet.Cells[1, i + 1].Address(false, true, XlReferenceStyle.xlA1);
                    var column_name = address.Substring(address.IndexOf('$'));
                    header[i] = column_name;
                }
                else
                {
                    header[i] = content;
                    var lang = FindLanguageCode(content);
                    if (lang == null) continue;
                    if (source_column < 0)
                    {
                        source_column = i;
                        source_language = lang;
                    }
                    else if (target_column < 0)
                    {
                        target_column = i;
                        target_language = lang;
                    }
                    else
                    {
                        // 3rd or further language codes are handled like a comment column.
                    }
                }
            }
            if (target_column < 0)
            {
                // this sheet doesn't look like a glossary.
                return null;
            }

            var pairs = new List<ITransPair>();
            for (int i = 2; i <= rows; i++)
            {
                var source = cells[i, source_column + 1]?.ToString();
                var target = cells[i, target_column + 1]?.ToString();
                if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(target))
                {
                    pairs.Add(new ExcelGlossaryPair()
                    {
                        Serial = pairs.Count + 1,
                        Id = i.ToString(),
                        Source = new InlineString(source ?? ""),
                        Target = new InlineString(target ?? ""),
                        SourceLang = source_language,
                        TargetLang = target_language,
                    });
                }
            }
            if (pairs.Count == 0) return null;

            return new ExcelGlossaryAsset()
            {
                Package = (sheet.Parent as Workbook)?.FullName,
                Original = sheet.Name,
                SourceLang = source_language,
                TargetLang = target_language,
                TransPairs = pairs,
            };
        }

        private CultureInfo[] CultureInfos = null;

        private string FindLanguageCode(string text)
        {
            var cultures = CultureInfos;
            if (cultures == null)
            {
                CultureInfos = cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            }

            foreach (var c in cultures)
            {
                if (text.Equals(c.IetfLanguageTag, StringComparison.InvariantCultureIgnoreCase))
                {
                    return text;
                }
            }

            foreach (var c in cultures)
            {
                if (text.Equals(c.TwoLetterISOLanguageName, StringComparison.InvariantCultureIgnoreCase) ||
                    text.Equals(c.ThreeLetterISOLanguageName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return c.IetfLanguageTag;
                }
            }

            foreach (var c in cultures)
            {
                if (text.Equals(c.EnglishName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return c.IetfLanguageTag;
                }
            }

            foreach (var c in cultures)
            {
                if (text.Equals(c.DisplayName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return c.IetfLanguageTag;
                }
            }

            return null;
        }
    }

    class ExcelGlossaryAsset : IAsset
    {
        public string Package { get; internal set; }

        public string Original { get; internal set; }

        public string SourceLang { get; internal set; }

        public string TargetLang { get; internal set; }

        public IEnumerable<ITransPair> TransPairs { get; internal set; }

        public IEnumerable<ITransPair> AltPairs { get { return Enumerable.Empty<ITransPair>(); } }

        public IList<PropInfo> Properties { get { return new PropInfo[0]; } }
    }

    class ExcelGlossaryPair : ITransPair
    {
        public int Serial { get; internal set; }

        public string Id { get; internal set; }

        public InlineString Source { get; internal set; }

        public InlineString Target { get; internal set; }

        public string SourceLang { get; internal set; }

        public string TargetLang { get; internal set; }

        public IEnumerable<string> Notes { get { return Enumerable.Empty<string>(); } }

        public string this[int key] { get { return null; } }
    }
}
