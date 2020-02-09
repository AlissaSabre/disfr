using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs.Controls;

namespace disfr.UI
{
    /// <summary>
    /// A duck subset of <see cref="Microsoft.Win32.OpenFileDialog"/> with a few additional features,
    /// built on top of Microsoft Windows API Code Pack.
    /// </summary>
    /// <remarks>
    /// For whatever reason, a <see cref="Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog"/> instance may not be reusable.
    /// That is, the second call to <see cref="CommonOpenFileDialog.ShowDialog(Window)"/> malfunctions.
    /// In my case, a <see cref="CommonFileDialogCheckBox"/> added to the dialog stopped returning its
    /// <see cref="CommonFileDialogCheckBox.IsChecked"/> value.
    /// To work around the issue, this class wraps the <see cref="CommonOpenFileDialog"/>
    /// and recreate its instance upon every call to <see cref="DuckOpenFileDialog.ShowDialog(Window)"/>.
    /// </remarks>
    public class DuckOpenFileDialog
    {
        public string[] FileNames { get; protected set; }

        public int FilterIndex { get; protected set; }

        protected string _Filter;

        protected readonly List<CommonFileDialogFilter> ParsedFilter = new List<CommonFileDialogFilter>();

        public string Filter
        {
            get { return _Filter; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    ParsedFilter.Clear();
                }
                else
                {
                    var array = value.Split('|');
                    if (array.Length % 2 != 0) throw new ArgumentException("Count of items separated by \"|\" is odd.");
                    ParsedFilter.Clear();
                    for (int i = 0; i < array.Length; i += 2)
                    {
                        ParsedFilter.Add(new CommonFileDialogFilter(array[i + 0], array[i + 1]));
                    }
                }
                _Filter = value;
            }
        }

        public bool SingleTab { get; protected set; }

        public bool ShowDialog(Window window)
        {
            var dialog = new CommonOpenFileDialog()
            {
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                Multiselect = true,
            };

            var checkbox = new CommonFileDialogCheckBox("SingleTabCheckbox", "Read into a single tab")
            {
                IsProminent = false,
                IsChecked = false,
            };

            dialog.Controls.Add(checkbox);

            var filters = dialog.Filters;
            ParsedFilter.ForEach(f => filters.Add(f));

            var result = dialog.ShowDialog(window) == CommonFileDialogResult.Ok;
            if (result)
            {
                FileNames = dialog.FileNames.ToArray();
                FilterIndex = dialog.SelectedFileTypeIndex;
                SingleTab = checkbox.IsChecked;
            }

            return result;
        }
    }
}
