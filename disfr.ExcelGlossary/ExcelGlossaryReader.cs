using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;
using NetOffice.OfficeApi.Enums;
using Excel = NetOffice.ExcelApi.Application;

using disfr.Doc;

namespace disfr.ExcelGlossary
{
    class ExcelGlossaryReader : IAssetReader
    {
        private static readonly string[] PrimaryExtensions = { ".xlsx", ".xls" };

        private static readonly string[] _FilterString = { "Excel glossary files|*.xlsx;*.xls" };

        public IList<string> FilterString { get { return _FilterString; } }

        public string Name { get { return "ExcelGlossaryReader"; } }

        /// <summary>
        /// The priority of <see cref="ExcelGlossaryReader"/> in auto-detection.
        /// </summary>
        /// <remarks>
        /// Because Excel can read almost anything,
        /// its priority is set to the minimum positive value, i.e., 1,
        /// so that it doesn't consume a file intended for another plugin.
        /// </remarks>
        public int Priority { get { return 1; } }

        public IAssetBundle Read(string filename, int filterindex)
        {
            if (filterindex < 0)
            {
                // Excel can open virtually any text-based file formats,
                // but it could take incredibly long time if the file is not suitable for a table.
                // If a user opened an unknown file with disfr using "All files" file types,
                // assuming it is an XLIFF, but it was actually not,
                // disfr's automatic file detection scheme eventually passes the file to this method
                // (unless another reader accepted it), and the file could occupy Excel for long,
                // causing disfr to appear as though it hung up.
                // I think we should avoid such a behaviour.
                if (!PrimaryExtensions.Any(ext => filename.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return null;
                }
            }

            var full_pathname = Path.GetFullPath(filename);
            return LoaderAssetBundle.Create(
                ReaderManager.FriendlyFilename(filename),
                () => ReadAssets(full_pathname));
        }

        /// <summary>
        /// Reads ExcelGlossary assets from an Excel book.
        /// </summary>
        /// <param name="filename">Full path name of Excel book.</param>
        /// <returns>A series of ExcelGlossary assets.</returns>
        private IEnumerable<IAsset> ReadAssets(string filename)
        {
            var list = new List<IAsset>();
            var excel = new Excel() { Visible = false, Interactive = false, DisplayAlerts = false };
            try
            {
                // We should avoid execution of macros included in an Excel file,
                // because it could contain some malicious operation.
                // Excel enables macros by default when invoked via automation API (read "NetOffice"),
                // so we should disable it by ourselves.
                // Microsoft's documentation on AutomationSecurity says:
                //     this property should be set immediately before and after opening a file programmatically
                //     to avoid malicious subversion
                // I'm not very sure what it means exactly,
                // but it sounds to me we should use a code like following, though it looks silly...
                // Anyway it seems working for me.
                excel.AutomationSecurity = MsoAutomationSecurity.msoAutomationSecurityForceDisable;
                var book = excel.Workbooks.Open(filename, XlUpdateLinks.xlUpdateLinksNever, true);
                excel.AutomationSecurity = MsoAutomationSecurity.msoAutomationSecurityForceDisable;

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
                return list.Count > 0 ? list : null;
            }
            finally
            {
                excel.Quit();
                excel.Dispose();
            }
        }

        private static readonly string[] EMPTY_PROPERTY_VALUES = new string[0];

        private IAsset ReadFromSheet(Worksheet sheet)
        {
            // We need this sheet to be active so that we can access to its window via Application.ActiveWindow.
            sheet.Activate();
            var cells = new CellAccessor(sheet);

            // Find the header and data rows.
            int header_row, data_row;
            {
                AutoFilter auto_filter = null;
                Window active_window = null;
                if ((auto_filter = sheet.AutoFilter) != null)
                {
                    // This worksheet has an auto filter.
                    // Use the filter's header row (i.e., those with filter buttons) as the header,
                    // and the rows below it as the data.
                    header_row = auto_filter.Range.Row;
                    data_row = header_row + 1;
                }
                else if ((active_window = sheet.Application.ActiveWindow).FreezePanes)
                {
                    // This worksheet has freeze panes.
                    // Use the last non-empty row on the frozen pane as the header,
                    // and the non-frozen rows as the data.
                    var r = active_window.ScrollRow;
                    var panes = active_window.Panes;
                    for (int i = 1; i <= panes.Count; i++)
                    {
                        var pane = panes[i];
                        r = Math.Min(r, pane.ScrollRow + pane.VisibleRange.Rows.Count);
                    }
                    data_row = r;
                    while (--r > 1 && cells.IsRowEmpty(r)) ;
                    header_row = r;
                }
                else
                {
                    // Otherwise, use the first row as the header (even if it is totally empty),
                    // and the remainig rows as data.
                    header_row = 1;
                    data_row = 2;
                }
                active_window?.Dispose();
                auto_filter?.Dispose();
            }

            // Fetch column headers on the first row.
            var headers = new string[cells.Columns + 1];
            for (int column = 1; column <= cells.Columns; column++)
            {
                var label = cells[header_row, column];
                headers[column] = string.IsNullOrWhiteSpace(label) ? null : label;
            }

            // Detect source and target languages.
            // We only consider visible cells for source and target language columns.
            string source_label = null, source_language = null;
            string target_label = null, target_language = null;
            for (int column = 1; column <= cells.Columns; column++)
            {
                if (cells.IsCellVisible(header_row, column))
                {
                    var lang = FindLanguageCode(headers[column]);
                    if (lang != null)
                    {
                        if (source_language == null)
                        {
                            // Assume the first language code represents the source language.
                            source_label = headers[column];
                            source_language = lang;
                        }
                        else if (target_language == null && lang != source_language)
                        {
                            // Assume the second language code represents the target language.
                            target_label = headers[column];
                            target_language = lang;
                            break;
                        }
                    }
                }
            }

            // Group columns into source, target, and other properties.
            var source_columns = new List<int>();
            var target_columns = new List<int>();
            var properties_columns = new List<int>();
            for (int column = 1; column <= cells.Columns; column++)
            {
                var label = headers[column];
                if (source_label?.Equals(label, StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    source_columns.Add(column);
                }
                else if (target_label?.Equals(label, StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    target_columns.Add(column);
                }
                else
                {
                    properties_columns.Add(column);
                }
            }

            // Detect duplicate header labels (i.e., property names)
            var duplicates = new HashSet<string>(headers.Where(h => h != null).GroupBy(h => h).Where(g => g.Count() > 1).Select(g => g.Key));

            // Modify header labels so that they are unique.
            // Note that the following modification could produce new ducplicates.
            // We don't go too strict, because this is just a nice-to-have feature.
            foreach (var column in properties_columns)
            {
                var label = headers[column];
                if (label == null)
                {
                    // No label.  Use Excel column name as a label.
                    headers[column] = cells.GetColumnName(column);
                }
                else if (duplicates.Contains(label))
                {
                    // Duplicate label.  Append the column name.
                    headers[column] = string.Format("{0} ({1})", label, cells.GetColumnName(column));
                }
                else
                {
                    // Already unique name.  Leave it alone.
                }
            }

            // Turn data rows into TransPairs.
            var pairs = new List<ITransPair>(cells.Rows); // This capacity is just a rough standard.
            int serial = 0;
            var sources = new List<InlineString>(source_columns.Count);
            var targets = new List<InlineString>(target_columns.Count);
            var properties = new List<string>(properties_columns.Count);
            for (int row = data_row; row <= cells.Rows; row++)
            {
                if (cells.IsRowEmpty(row))
                {
                    // Ignore an empty row.
                }
                else
                {
                    var visible = cells.IsRowVisible(row);
                    var id = row.ToString();

                    // Collect source texts.
                    sources.Clear();
                    sources.AddRange(source_columns.Select(c => cells[row, c]).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => new InlineString(s)));
                    if (sources.Count == 0) sources.Add(InlineString.Empty);

                    // Collect target texts.
                    targets.Clear();
                    targets.AddRange(target_columns.Select(c => cells[row, c]).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => new InlineString(s)));
                    if (targets.Count == 0) targets.Add(InlineString.Empty);

                    // Collect property values.
                    properties.Clear();
                    properties.AddRange(properties_columns.Select(c => cells[row, c]));
                    int last = properties.Count - 1;
                    while (last > 0 && string.IsNullOrWhiteSpace(properties[last])) --last;
                    if (last < properties.Count - 1) properties.RemoveRange(last + 1, properties.Count - last - 1);
                    var properties_array = properties.Count > 0 ? properties.ToArray() : EMPTY_PROPERTY_VALUES;

                    // Create TransPairs.
                    // We allow multiple source and/or target texts in an Excel row,
                    // so we generate all possible source-target combinations.
                    foreach (var source in sources)
                    {
                        foreach (var target in targets)
                        {
                            pairs.Add(new ExcelGlossaryPair()
                            {
                                Serial = visible ? ++serial : -1,
                                Id = id,
                                Source = source,
                                Target = target,
                                SourceLang = source_language,
                                TargetLang = target_language,
                                PropertyValues = properties_array,
                            });
                        }
                    }
                }
            }
            if (pairs.Count == 0) return null;
            pairs.TrimExcess();

            return new ExcelGlossaryAsset()
            {
                Package = (sheet.Parent as Workbook)?.FullName,
                Original = sheet.Name,
                SourceLang = source_language,
                TargetLang = target_language,
                TransPairs = pairs,
                Properties = properties_columns.Select(column => new PropInfo(headers[column], false)).ToArray(),
            };
        }

        private CultureInfo[] CultureInfos = null;

        /// <summary>
        /// Finds a BCP 47 language code for a given user-friendly language designation.
        /// </summary>
        /// <param name="text">User friendly name of a language.</param>
        /// <returns>BCP 47 language code, or null if this method can't find one.</returns>
        private string FindLanguageCode(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

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

        public IList<PropInfo> Properties { get; internal set; }
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

        internal string[] PropertyValues; 

        public string this[int key]
        {
            get { return key >= 0 && key < PropertyValues.Length ? PropertyValues[key] : null; }
        }
    }
}
