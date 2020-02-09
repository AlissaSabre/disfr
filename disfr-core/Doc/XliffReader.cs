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
        }

        private static readonly string[] _FilterString =
        {
            "Generic XLIFF|*.xlf;*.xliff",
            "Trados Studio|*.sdlxliff;*.sdlppx;*.sdlrpx;*.wsxz",
            "Idiom WorldServer|*.xlz;*.wsxz",
            "memoQ|*.mqxlz;*.mqxliff",
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
            using (var file = File.OpenRead(filename))
            {
                if (file.IsZip())
                {
                    return ReadZip(filename, file, flavour);
                }
                else if (IsXliff(file))
                {
                    return ReadXliff(filename, file, flavour);
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
                foreach (var entry in zip.Entries)
                {
                    // It seems the stream from ZipEntry.Open() doesn't suppoprt Seek.
                    // We need a trick here.
                    using (var f = entry.Open())
                    {
                        if (!IsXliff(f, true)) continue;
                    }
                    using (var f = entry.Open())
                    {
                        var a = ReadXliff(filename, f, flavour, entry);
                        if (a != null) assets.AddRange(a);
                    }
                }
                return assets.Count == 0 ? null : assets;
            }
        }

        private static IEnumerable<IAsset> ReadXliff(string filename, Stream file, Flavour flavour, ZipArchiveEntry entry = null)
        {
            var parser = new XliffParser();
            parser.Filename = filename;
            parser.Flavour = flavour;
            parser.ZipEntry = entry;
            return parser.Read(file);
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
