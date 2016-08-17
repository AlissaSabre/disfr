using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    /// <summary>
    /// Provides an extension method to handle ZIP file stream.
    /// </summary>
    public static class ZipUtils
    {
        /// <summary>
        /// Checks a <see cref="Stream"/> is a ZIP archive.
        /// </summary>
        /// <param name="file">A ZIP archive stream.</param>
        /// <returns>True if it is a ZIP archive.  False otherwise.</returns>
        /// <exception cref="NotSupportedException"><paramref name="file"/> doesn't support <see cref="Stream.Seek(long, SeekOrigin)"/>.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public static bool IsZip(this Stream file)
        {
            var position = file.Position;
            try
            {
                return file.ReadByte() == 0x50
                    && file.ReadByte() == 0x4B
                    && file.ReadByte() == 0x03
                    && file.ReadByte() == 0x04;
            }
            finally
            {
                file.Position = position;
            }
        }
    }
}
