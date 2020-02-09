using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace disfr.Writer
{
    /// <summary>
    /// A version of <see cref="XmlTextWriter"/> that takes care of space characters and likes beyond <see cref="NewLineHandling.Entitize"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="SpaceSensitiveXmlTextWriter"/> provide the following three features over <see cref="XmlTextWriter"/>:
    /// </para>
    /// <para>
    /// Has a constructor that accepts <see cref="XmlWriterSettings"/>.
    /// </para>
    /// <para>
    /// Always escapes &amp;#9;, &amp;#10;, &amp;#13; and space characters in the contents (as <c>&amp;#9;</c>, <c>&amp;#10;</c>, <c>&amp;#13;</c> and <c>&amp;#32;</c>, respectively.)
    /// This is primarily to fake Microsoft Excel's unique behaviour upon reading an XML spreadsheet file,
    /// but it also makes the XML document more legible on an ordinary text editor when indented.
    /// </para>
    /// <para>
    /// Accepts so-called C0/C1 controls that are forbidden by the XML standard in text contents, turning it into a well-formed contents.
    /// Each of such characters is written as a text string of form &amp;#<i>nnn</i>; where <i>nnn</i> is the code value.
    /// (The "&amp;" will be escaped, as usual, so the actual sequence on an XML file will be <c>&amp;amp;#<i>nnn</i>;</c>.) 
    /// </para>
    /// </remarks>
    public class SpaceSensitiveXmlTextWriter : XmlWriter
    {
        protected readonly XmlWriter Impl;

        /// <summary>
        /// Creates a new <see cref="SpaceSensitiveXmlTextWriter"/> that serializes to a stream.
        /// </summary>
        /// <param name="stream">The stream to write an XML document into.</param>
        /// <param name="settings">Settings passed from an XML processor.</param>
        /// <remarks>
        /// Only the following members of <paramref name="settings"/> are handled:
        /// 
        /// Specifically, <see cref="XmlWriterSettings.Indent"/> is ignored and assumed being <c>false</c>.
        /// </remarks>
        public SpaceSensitiveXmlTextWriter(Stream stream, XmlWriterSettings settings)
        {
            Impl = XmlWriter.Create(stream, settings);
            //if (!settings.OmitXmlDeclaration)
            //{
            //    Impl.WriteStartDocument();
            //}
        }

        /// <summary>
        /// Writes text from an array of characters.
        /// </summary>
        /// <param name="buffer">The array to write characters from.</param>
        /// <param name="index">The starting index of characters to write.</param>
        /// <param name="count">The number of characters to write.</param>
        /// <remarks>
        /// Use of <see cref="WriteString"/> is preferred over this method.
        /// </remarks>
        /// <seealso cref="XmlWriter.WriteChars(char[], int, int)"/>
        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.WriteString(new String(buffer, index, count));
        }

        /// <summary>
        /// Writes text from an array of characters async.
        /// </summary>
        /// <param name="buffer">The array to write characters from.</param>
        /// <param name="index">The starting index of characters to write.</param>
        /// <param name="count">The number of characters to write.</param>
        /// <returns>The async task.</returns>
        /// <seealso cref="XmlWriter.WriteCharsAsync(char[], int, int)"/>
        public override Task WriteCharsAsync(char[] buffer, int index, int count)
        {
            return this.WriteStringAsync(new String(buffer, index, count));
        }

        /// <summary>
        /// Writes text from a string.
        /// </summary>
        /// <param name="text">The string to write.</param>
        /// <seealso cref="XmlWriter.WriteString(string)"/>
        public override void WriteString(string text)
        {
            if (WriteState == WriteState.Attribute)
            {
                Impl.WriteRaw(ATTR_ESCAPE.Replace(text, Escape));
            }
            else
            {
                Impl.WriteRaw(CONTENT_ESCAPE.Replace(text, Escape));
            }
        }

        /// <summary>
        /// Writes text from a string async. 
        /// </summary>
        /// <param name="text">The string to write.</param>
        /// <returns>The async task.</returns>
        /// <seealso cref="XmlWriter.WriteStringAsync(string)"/>
        public override Task WriteStringAsync(string text)
        {
            if (WriteState == WriteState.Attribute)
            {
                return Impl.WriteRawAsync(ATTR_ESCAPE.Replace(text, Escape));
            }
            else
            {
                return Impl.WriteRawAsync(CONTENT_ESCAPE.Replace(text, Escape));
            }
        }

        private static readonly Regex CONTENT_ESCAPE = new Regex("[\x00-\x20\x7F&<>]");
        private static readonly Regex ATTR_ESCAPE = new Regex("['\"\x00-\x20\x7F&<>]");

        /// <summary>
        /// Find a character entity reference suitable for a character 
        /// in a text that a <see cref="Regex"/> matched.
        /// </summary>
        /// <param name="m">A result from a <see cref="Regex"/> match for a character.</param>
        /// <returns>Character entity reference.</returns>
        private static string Escape(Match m)
        {
            var c = m.Value[0];
            switch (c)
            {
                case '&': return "&amp;";
                case '<': return "&lt;";
                case '>': return "&gt;";
                case '"': return "&#34;";   // some application doesn't support "&quot;"
                case '\'': return "&#39;";  // some application doesn't support "&apos;"
                case ' ': return "&#32;";
                case '\r':
                case '\n':
                case '\t':
                    // These are controlls explicitly allowed in XML.
                    return "&#" + (int)c + ";";
                default:
                    // Everything else that matches the RE are invalid characters.
                    return "&amp;#" + (int)c + ";";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Impl.Dispose();
            base.Dispose(disposing);
        }

        #region Delegated properties

        public override XmlWriterSettings Settings
        {
            get { return Impl.Settings; }
        }

        public override WriteState WriteState
        {
            get { return Impl.WriteState; }
        }

        public override string XmlLang
        {
            get { return Impl.XmlLang; }
        }

        public override XmlSpace XmlSpace
        {
            get { return Impl.XmlSpace; }
        }

        #endregion

        #region Delegated (non Async) method

        public override void Close()
        {
            Impl.Close();
        }

        public override void Flush()
        {
            Impl.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return Impl.LookupPrefix(ns);
        }

        public override void WriteAttributes(XmlReader reader, bool defattr)
        {
            Impl.WriteAttributes(reader, defattr);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            Impl.WriteBase64(buffer, index, count);
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            Impl.WriteBinHex(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            Impl.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            Impl.WriteCharEntity(ch);
        }

        public override void WriteComment(string text)
        {
            Impl.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            Impl.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            Impl.WriteEndAttribute();
        }

        public override void WriteEndElement()
        {
            Impl.WriteEndElement();
        }

        public override void WriteEndDocument()
        {
            Impl.WriteEndDocument();
        }

        public override void WriteEntityRef(string name)
        {
            Impl.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            Impl.WriteFullEndElement();
        }

        public override void WriteName(string name)
        {
            Impl.WriteName(name);
        }

        public override void WriteNmToken(string name)
        {
            Impl.WriteNmToken(name);
        }

        public override void WriteNode(System.Xml.XPath.XPathNavigator navigator, bool defattr)
        {
            Impl.WriteNode(navigator, defattr);
        }

        public override void WriteNode(XmlReader reader, bool defattr)
        {
            Impl.WriteNode(reader, defattr);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            Impl.WriteProcessingInstruction(name, text);
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            Impl.WriteQualifiedName(localName, ns);
        }

        public override void WriteRaw(string data)
        {
            Impl.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            Impl.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            Impl.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument(bool standalone)
        {
            Impl.WriteStartDocument(standalone);
        }

        public override void WriteStartDocument()
        {
            Impl.WriteStartDocument();
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            Impl.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            Impl.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            Impl.WriteWhitespace(ws);
        }

        #endregion

        #region Delegated Async methods

        public override Task FlushAsync()
        {
            return Impl.FlushAsync();
        }

        public override Task WriteAttributesAsync(XmlReader reader, bool defattr)
        {
            return Impl.WriteAttributesAsync(reader, defattr);
        }

        public override Task WriteBase64Async(byte[] buffer, int index, int count)
        {
            return Impl.WriteBase64Async(buffer, index, count);
        }

        public override Task WriteBinHexAsync(byte[] buffer, int index, int count)
        {
            return Impl.WriteBinHexAsync(buffer, index, count);
        }

        public override Task WriteCDataAsync(string text)
        {
            return Impl.WriteCDataAsync(text);
        }

        public override Task WriteCharEntityAsync(char ch)
        {
            return Impl.WriteCharEntityAsync(ch);
        }

        public override Task WriteCommentAsync(string text)
        {
            return Impl.WriteCommentAsync(text);
        }

        public override Task WriteDocTypeAsync(string name, string pubid, string sysid, string subset)
        {
            return Impl.WriteDocTypeAsync(name, pubid, sysid, subset);
        }

        public override Task WriteEndDocumentAsync()
        {
            return Impl.WriteEndDocumentAsync();
        }

        public override Task WriteEndElementAsync()
        {
            return Impl.WriteEndElementAsync();
        }

        public override Task WriteEntityRefAsync(string name)
        {
            return Impl.WriteEntityRefAsync(name);
        }

        public override Task WriteFullEndElementAsync()
        {
            return Impl.WriteFullEndElementAsync();
        }

        public override Task WriteNameAsync(string name)
        {
            return Impl.WriteNameAsync(name);
        }

        public override Task WriteNmTokenAsync(string name)
        {
            return Impl.WriteNmTokenAsync(name);
        }

        public override Task WriteNodeAsync(XPathNavigator navigator, bool defattr)
        {
            return Impl.WriteNodeAsync(navigator, defattr);
        }

        public override Task WriteNodeAsync(XmlReader reader, bool defattr)
        {
            return Impl.WriteNodeAsync(reader, defattr);
        }

        public override Task WriteProcessingInstructionAsync(string name, string text)
        {
            return Impl.WriteProcessingInstructionAsync(name, text);
        }

        public override Task WriteQualifiedNameAsync(string localName, string ns)
        {
            return Impl.WriteQualifiedNameAsync(localName, ns);
        }

        public override Task WriteRawAsync(char[] buffer, int index, int count)
        {
            return Impl.WriteRawAsync(buffer, index, count);
        }

        public override Task WriteRawAsync(string data)
        {
            return Impl.WriteRawAsync(data);
        }

        public override Task WriteStartDocumentAsync()
        {
            return Impl.WriteStartDocumentAsync();
        }

        public override Task WriteStartDocumentAsync(bool standalone)
        {
            return Impl.WriteStartDocumentAsync(standalone);
        }

        public override Task WriteStartElementAsync(string prefix, string localName, string ns)
        {
            return Impl.WriteStartElementAsync(prefix, localName, ns);
        }

        public override Task WriteSurrogateCharEntityAsync(char lowChar, char highChar)
        {
            return Impl.WriteSurrogateCharEntityAsync(lowChar, highChar);
        }

        public override Task WriteWhitespaceAsync(string ws)
        {
            return Impl.WriteWhitespaceAsync(ws);
        }

        #endregion

        #region Impossible Async methods

        protected override Task WriteEndAttributeAsync()
        {
            throw new NotSupportedException();
        }

        protected override Task WriteStartAttributeAsync(string prefix, string localName, string ns)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
