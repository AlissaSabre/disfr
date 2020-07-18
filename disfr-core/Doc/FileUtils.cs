using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace disfr.Doc
{
    /// <summary>
    /// Provides extension methods useful for file format detection.
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Quickly checks a <see cref="Stream"/> looks like a ZIP archive.
        /// </summary>
        /// <param name="file">A ZIP archive stream.</param>
        /// <returns>True if it is a ZIP archive.  False otherwise.</returns>
        /// <exception cref="NotSupportedException"><paramref name="file"/> doesn't support seeking.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <remarks>
        /// This method reads from the <paramref name="file"/>,
        /// but it seeks to its original position before returning.
        /// So, <paramref name="file"/> needs to support seeking,
        /// or this method throws <see cref="NotSupportedException"/>.
        /// </remarks>
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

        /// <summary>
        /// Quickly checks and returns the first XML element in a <see cref="Stream"/>,
        /// ignoring any child nodes.
        /// </summary>
        /// <param name="file">An XML stream.</param>
        /// <returns>An <see cref="XElement"/> instance with no children,
        /// or null if the first part of <paramref name="file"/> does not form an XML document fragment.</returns>
        /// <exception cref="NotSupportedException"><paramref name="file"/> doesn't support seeking.</exception>
        /// <exception cref="IOException">An I/O error occured.</exception>
        /// <remarks>
        /// This method reads from the <paramref name="file"/>,
        /// but it tries to seek to its original position before returning.
        /// So, <paramref name="file"/> needs to support seeking,
        /// or this method throws <see cref="NotSupportedException"/> unless <paramref name="read"/> is true.
        /// If you are working on a <see cref="Stream"/> whose <see cref="Stream.CanSeek"/> is false,
        /// you can call this method with <paramref name="read"/> set to true
        /// to get an <see cref="XElement"/> without seeking.
        /// If you do so, you need to somehow rewind the stream afterwards.
        /// </remarks>
        public static XElement PeekElementWithoutChildren(this Stream file, bool read = false)
        {
            var position = read ? -1 : file.Position;
            try
            {
                // I experienced that some import filter (used with some CAT software) 
                // produces some illegal entity references, e.g., "&#x1F;".
                // Although it is NOT a wellformed XML in theory, we need to take care of them.
                // Another issue is that some XML file includes DOCTYPE declaration
                // with a system identifier,
                // that XmlReader tries to access to to get a DTD by default, 
                // so we need to instruct explicitly not to do so.
                var settings = new XmlReaderSettings()
                {
                    CheckCharacters = false,
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null,
                    CloseInput = false,
                };
                using (var reader = XmlReader.Create(file, settings))
                {
                    reader.MoveToContent();
                    var element = new XElement(XName.Get(reader.LocalName, reader.NamespaceURI));
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToAttribute(i);
                        if (reader.Name == "xmlns" || reader.Name == "xml")
                        {
                            // Linq to XML goes mad on an attribute of this name.  Just ignore it.
                        }
                        else
                        {
                            element.Add(new XAttribute(XName.Get(reader.LocalName, reader.NamespaceURI), reader.Value));
                        }
                    }
                    return element;
                }
            }
            catch (XmlException)
            {
                return null;
            }
            finally
            {
                if (position >= 0)
                {
                    file.Position = position;
                }
            }
        }

        /// <summary>
        /// Opens an existing file for reading with a tweak.
        /// </summary>
        /// <param name="filename">The file to be opened for reading.</param>
        /// <returns>A read-only FileStream object.</returns>
        /// <exception cref="IOException">The method is unable to open <paramref name="filename"/>.</exception>
        /// <remarks>
        /// This method provides a similar functionality to <see cref="File.OpenRead(string)"/>,
        /// but it has a tweak to live with Microsoft Office (and other similar) products.
        /// This method first try to open the file in a same way as <see cref="File.OpenRead(string)"/>,
        /// but if it failed, it then automatically tries to use another sharing mode
        /// that is compatible with Microsoft Office to reduce the chance to get a sharing violation error.
        /// It is useful in disfr and its plugins, because,
        /// when a user opens an Excel file with difr using auto-detection,
        /// various <see cref="IAssetReader"/>s try to open the file before <see cref="ReaderManager"/> gets to the
        /// correct plugin.
        /// Throwing an IOException causes a bad experience if the user is viewing the file with Excel.
        /// </remarks>
        public static FileStream OpenRead(string filename)
        {
            try
            {
                return new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (IOException)
            {
                return new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
        }
    }
}
