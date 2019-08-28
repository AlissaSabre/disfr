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
        /// Checks a <see cref="Stream"/> is a ZIP archive.
        /// </summary>
        /// <param name="file">A ZIP archive stream.</param>
        /// <returns>True if it is a ZIP archive.  False otherwise.</returns>
        /// <exception cref="NotSupportedException"><paramref name="file"/> doesn't support seeking.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <remarks>
        /// This method internally changes the current position of the <paramref name="file"/>,
        /// but it seeks to the original position before returns.
        /// To do so, <paramref name="file"/> needs to support seeking,
        /// or it throws <see cref="NotSupportedException"/>.
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
        /// Returns the first XML element in the <see cref="Stream"/> ignoring any children.
        /// </summary>
        /// <param name="file">An XML stream.</param>
        /// <returns>An <see cref="XElement"/> instance with no children,
        /// or null if the first part of <paramref name="file"/> is not in an XML format.</returns>
        /// <exception cref="NotSupportedException"><paramref name="file"/> doesn't support seeking.</exception>
        /// <exception cref="IOException">An I/O error occured.</exception>
        /// <remarks>
        /// This method internally changes the current position of the <paramref name="file"/>,
        /// but it seeks to the original position before returns.
        /// To do so, <paramref name="file"/> needs to support seeking,
        /// or it throws <see cref="NotSupportedException"/>.
        /// </remarks>
        public static XElement PeekElementWithoutChildren(this Stream file)
        {
            var position = file.Position;
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
                        element.Add(new XAttribute(XName.Get(reader.LocalName, reader.NamespaceURI), reader.Value));
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
                file.Position = position;
            }
        }
    }
}
