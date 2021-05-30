using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace disfr.Doc
{
    public class XliffReader : IAssetReader
    {
        public enum Flavour
        {
            Auto = -1,
            Generic = 0,
            Trados = 1,
            Idiom = 2,
            MemoQ = 3,
            MemoQHistory = 4,
            Wordfast = 5,
        }

        private static readonly string[] _FilterString =
        {
            "Generic XLIFF|*.xlf;*.xliff",
            "Trados Studio|*.sdlxliff;*.sdlppx;*.sdlrpx;*.wsxz",
            "Idiom WorldServer|*.xlz;*.wsxz",
            "memoQ|*.mqxlz;*.mqxliff",
            "memoQ major version histories|*.mqxlz",
            "Wordfast|*.txlf",
        };

        public IList<string> FilterString { get { return _FilterString; } }

        public string Name { get { return "XliffReader"; } }

        public int Priority { get { return 10; } }

        public IAssetBundle Read(string filename, int filterindex)
        {
            return Read(filename, (Flavour)filterindex);
        }

        public IAssetBundle Read(string filename, Flavour flavour = Flavour.Auto)
        {
            return LoaderAssetBundle.Create(
                ReaderManager.FriendlyFilename(filename),
                () => ReadAssets(filename, flavour));
        }

        private IEnumerable<IAsset> ReadAssets(string filename, Flavour flavour)
        {
            using (var file = FileUtils.OpenRead(filename))
            {
                if (file.IsZip())
                {
                    return ReadZip(filename, file, flavour);
                }
                else if (IsXliff(file) && flavour != Flavour.MemoQHistory)
                {
                    var local_flavour = flavour;
                    return ReadXliff(filename, file, ref flavour, null);
                }
                else
                {
                    return null;
                }
            }
        }

        private static IEnumerable<IAsset> ReadZip(string filename, Stream file, Flavour flavour)
        {
            // If this is an MS-DOS compatible zip file, 
            // the filenames are encoded with the created machine's OEM codepage, 
            // which we don't know what it is.
            // Windows assumes it is same as this machine's ANSI.
            // It works fine in most cases, and on SBCS world, 
            // the worst-case symptom is getting files with broken filename.
            // On MBCS world, however, when receiving a zip file from other region,
            // some particular combinations of bytes in a filename 
            // could be considered illegal per the receiving system's MBCS ANSI, 
            // and opening a file may fail.
            // It is unacceptable, especially given that a name of a zip entry is
            // not important for the purpose of this program.  
            // That's why we explicitly say it is 850, 
            // even though it is very unlikely today.
            // Please be reminded that the specified encoding only affects the decoding of
            // zip entry names and has nothing to do with the contents.

            using (var zip = new ZipArchive(file, ZipArchiveMode.Read, true, Encoding.GetEncoding(850)))
            {
                var assets = new List<IAsset>();
                bool is_memoq = false;

                // First collect entries in the _root_ folder in the zip
                // to handle mqxlz files containing multiple versions of an xliff.
                foreach (var entry in zip.Entries)
                {
                    if (entry.Name == entry.FullName)
                    {
                        var local_flavour = flavour;
                        if (local_flavour == Flavour.MemoQHistory) local_flavour = Flavour.Auto;
                        var a = ReadZipEntry(filename, entry, ref local_flavour);
                        if (a != null) assets.AddRange(a);
                        is_memoq |= local_flavour == Flavour.MemoQ;
                    }
                }

                // If memoQ histries are requested, but this is NOT a memoQ archive,
                // We consider it is an error.
                if (flavour == Flavour.MemoQHistory && !is_memoq)
                {
                    throw new FormatException("Not a memoQ mqxlz file.");
                }

                // If memoQ histries are requested, and this *IS* a memoQ archive,
                // Read the histries, marking them appropriately.
                else if (flavour == Flavour.MemoQHistory && is_memoq)
                {
                    foreach (var entry in zip.Entries)
                    {
                        if (entry.Name != entry.FullName)
                        {
                            var local_flavour = Flavour.MemoQ;
                            var local_filename = string.Format("{0} ({1})",
                                filename,
                                Path.GetFileName(Path.GetDirectoryName(entry.FullName)));
                            var a = ReadZipEntry(local_filename, entry, ref local_flavour);
                            if (a != null) assets.AddRange(a);
                        }
                    }
                }

                // If memoQ histries are *NOT* requested, and this *IS* a memoQ archive,
                // stop here to avoid reading the histories.
                else if (flavour != Flavour.MemoQHistory && is_memoq)
                {
                    // Do nothing on other zip entries.
                }

                // If memoQ histries are *NOT* requested, and this is *NOT* a memoQ archive,
                // collect other zip entries as in previous versions.
                else
                {
                    foreach (var entry in zip.Entries)
                    {
                        if (entry.Name != entry.FullName)
                        {
                            var local_flavour = flavour;
                            var a = ReadZipEntry(filename, entry, ref local_flavour);
                            if (a != null) assets.AddRange(a);
                        }
                    }
                }

                if (assets.Count > 0)
                {
                    return assets;
                }
                else if (flavour == Flavour.Auto)
                {
                    return null;
                }
                else
                {
                    throw new FormatException(string.Format("{0}: Not in a requested format.", filename));
                }
            }
        }

        private static IEnumerable<IAsset> ReadZipEntry(string filename, ZipArchiveEntry entry, ref Flavour flavour)
        {
            // It seems the stream from ZipEntry.Open() doesn't suppoprt Seek.
            // We need a trick here.
            using (var f = entry.Open())
            {
                if (!IsXliff(f, true)) return null;
            }
            using (var f = entry.Open())
            {
                return ReadXliff(filename, f, ref flavour, entry);
            }
        }

        private static IEnumerable<IAsset> ReadXliff(string filename, Stream file, ref Flavour flavour, ZipArchiveEntry entry)
        {
            var parser = new XliffParser();
            parser.Filename = filename;
            parser.Flavour = flavour;
            parser.ZipEntry = entry;
            var assets = parser.Read(file);
            flavour = parser.Flavour;
            return assets;
        }

        /// <summary>Parses an xliff (root) element into an asset bundle.</summary>
        /// <param name="xliff">An XML element that is supposed to be an xliff element.</param>
        /// <param name="filename">The (full path) name of the file <paramref name="xliff"/> was taken from.</param>
        /// <param name="flavour">The flavour of XLIFF to assume.</param>
        /// <returns>An asset bundle from <see cref="xliff"/>.</returns>
        /// <remarks>The returned bundle's <see cref="IAssetBundle.CanRefresh"/> will be false.</remarks>
        public IAssetBundle Parse(XElement xliff, string filename, Flavour flavour = Flavour.Auto)
        {
            if (xliff == null) throw new ArgumentNullException("xliff");
            if (filename == null) throw new ArgumentNullException("filename");
            var parser = new XliffParser();
            parser.Filename = filename;
            parser.Flavour = flavour;
            var assets = parser.Parse(xliff);
            if (assets == null) throw new ArgumentException("Not a valid <xliff> element", "xliff");
            return new SimpleAssetBundle(assets, ReaderManager.FriendlyFilename(filename));
        }

        private static bool IsXliff(Stream file, bool read = false)
        {
            var root = file.PeekElementWithoutChildren(read);
            return root != null
                && root.Name.LocalName == "xliff"
                && (root.Name.Namespace == XliffAsset.XLIFF || root.Name.Namespace == XNamespace.None);
        }
    }
}
