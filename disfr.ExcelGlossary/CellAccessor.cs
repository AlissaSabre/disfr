using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;
using Excel = NetOffice.ExcelApi.Application;


namespace disfr.ExcelGlossary
{
    /// <summary>
    /// Provides handy aceess to cells on an Excel worksheet.
    /// </summary>
    public class CellAccessor
    {
        private readonly Excel ExcelApp;

        private readonly Range Cells;

        private readonly Range VisibleCells;

        private readonly object[,] CachedCellValues;

        public readonly int Rows;

        public readonly int Columns;

        /// <summary>
        /// Creates a <see cref="CellAccessor"/> instance that represents all effective cells on a <see cref="Worksheet"/>.
        /// </summary>
        /// <param name="worksheet">The worksheet.</param>
        /// <remarks>
        /// <para>
        /// The created instance maintains several child objects of <paramref name="worksheet"/>.
        /// Don't dispose <paramref name="worksheet"/> until you've done with it.
        /// </para>
        /// <para>
        /// This class <i>modifies</i> some formatting of the given worksheet for its own purposes.
        /// It is not a good idea to save the worksheet after using this class.
        /// </para>
        /// </remarks>
        public CellAccessor(Worksheet worksheet)
        {
            ExcelApp = worksheet.Application;
            Cells = worksheet.Cells;
            VisibleCells = Cells.SpecialCells(XlCellType.xlCellTypeVisible);

            // To avoid <see cref="Range.Text"/> to return #### for a narrow column.
            // This is the only modification we make on the worksheet as mentioned in the remarks above.
            Cells.ShrinkToFit = true;

            // Extract the meaningful range of cells on the sheet.
            var bottom_right = Cells
                .SpecialCells(XlCellType.xlCellTypeLastCell)
                .Address(true, true, XlReferenceStyle.xlA1);
            if (bottom_right != "$A$1")
            {
                // The normal case.
                CachedCellValues = Cells.Range("$A$1", bottom_right).Value2 as object[,];
            }
            else if (string.IsNullOrWhiteSpace(Cells.Range("$A$1").Text as string))
            {
                // The worksheet was empty.
                CachedCellValues = Array.CreateInstance(typeof(object), new int[] { 0, 0 }, new int[] { 1, 1 }) as object[,];
            }
            else
            {
                // The worksheet contained only one value at $A$1.
                CachedCellValues = Array.CreateInstance(typeof(object), new int[] { 1, 1 }, new int[] { 1, 1 }) as object[,];
                CachedCellValues[1, 1] = Cells.Range("$A$1").Value2;
            }
            var rows = CachedCellValues.GetLength(0);
            var columns = CachedCellValues.GetLength(1);

            // Trim empty columns at the end if any.
            while (columns > 0
                && Enumerable.Range(1, rows).All(row => string.IsNullOrWhiteSpace(CachedCellValues[row, columns]?.ToString())))
            {
                --columns;
            }

            Rows = rows;
            Columns = columns;
        }

        /// <summary>
        /// Gets the formated text representation of a cell value.
        /// </summary>
        /// <param name="row">Row number of the cell.</param>
        /// <param name="column">Column number of the cell.</param>
        /// <returns>The text representation.  It is never null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="row"/> or <paramref name="column"/> is zero or negative,
        /// <paramref name="row"/> exceededs <see cref="Rows"/>, or
        /// <paramref name="column"/> exceeds <see cref="Columns"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <paramref name="row"/> and <paramref name="column"/> are base 1, i.e.,
        /// the row and column numbers for the cell "A1" is (1, 1) but (0, 0).
        /// </para>
        /// <para>
        /// This indexer returns the <i>formatted</i> text from the cell.
        /// That is, the returned string depends on the applicable cell format on the Excel worksheet
        /// and will be same as what you will see on Excel,
        /// including formula errors (such as #DIV/0!).
        /// An exception is that this indexer never truncates (or turns into ####)
        /// a long result that exceeds the column width.
        /// </para>
        /// </remarks>
        public string this[int row, int column]
        {
            get
            {
                if (row <= 0 || row > Rows) throw new ArgumentOutOfRangeException("row");
                if (column <= 0 || column > Columns) throw new ArgumentOutOfRangeException("column");

                string text;
                
                text = CachedCellValues[row, column] as string;
                if (text != null) return text;

                using (var cell = Cells[row, column])
                {
                    text = cell?.Text as string ?? string.Empty;
                    CachedCellValues[row, column] = text;
                    return text;
                }
            }
        }

        /// <summary>
        /// Checks if an entire row is empty.
        /// </summary>
        /// <param name="row">The row number.</param>
        /// <returns>True if the row is empty.</returns>
        public bool IsRowEmpty(int row)
        {
            return Enumerable.Range(1, Columns).All(column => string.IsNullOrWhiteSpace(CachedCellValues[row, column]?.ToString()));
        }

        /// <summary>
        /// Checks if a row is visible.
        /// </summary>
        /// <param name="row">The row number.</param>
        /// <returns>True if the row is visble.</returns>
        /// <remarks>
        /// Rows set to "hidden" and/or filtered out are considered invisible.
        /// Rows with zero hight are considered visible.
        /// </remarks>
        public bool IsRowVisible(int row)
        {
            using (var cells_in_row = Cells.Rows[row])
            using (var intersection = ExcelApp.Intersect(VisibleCells, cells_in_row))
            {
                return intersection != null;
            }
        }

        /// <summary>
        /// Checks if a particular cell is visible.
        /// </summary>
        /// <param name="row">The row number.</param>
        /// <param name="column">The column number.</param>
        /// <returns>True if the cell is visible</returns>
        /// <remarks>
        /// A cell on a hidden row or column is considered invisible.
        /// A cell on a filtered-out row is considered invisible.
        /// A cell whose width and/or height set to zero is considered visible.
        /// </remarks>
        public bool IsCellVisible(int row, int column)
        {
            using (var cell = Cells[row, column])
            using (var intersection = ExcelApp.Intersect(VisibleCells, cell))
            {
                return intersection != null;
            }
        }

        /// <summary>
        /// Gets an absolute address string for a cell within a worksheet.
        /// </summary>
        /// <param name="row">The row number of the cell.</param>
        /// <param name="column">The column number of the cell.</param>
        /// <returns>A string like "$A$1".</returns>
        public string GetAddressString(int row, int column)
        {
            using (var cell = Cells[row, column])
            {
                return cell.Address(true, true, XlReferenceStyle.xlA1);
            }
        }

        /// <summary>
        /// Gets a column name for a worksheet.
        /// </summary>
        /// <param name="column">The column number.</param>
        /// <returns>A string like "A", "B", ..., "Z", "AA", "AB", ...</returns>
        public string GetColumnName(int column)
        {
            using (var cell = Cells[1, column])
            {
                var address = cell.Address(false, false, XlReferenceStyle.xlA1);
                return address.Substring(0, address.Length - 1);
            }
        }
    }
}
