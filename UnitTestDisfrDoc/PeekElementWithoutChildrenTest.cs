using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class PeekElementWithoutChildrenTest : ReaderTestBase
    {
        [TestMethod]
        public void PeekElementWithoutChildren_1()
        {
            using (var stream = File.OpenRead(Path.Combine(IDIR, "Xliff1.xliff")))
            {
                var root = stream.PeekElementWithoutChildren();
                root.Name.Namespace.Is("");
                root.Name.LocalName.Is("xliff");
                root.Attribute("version").Value.Is("1.2");
                stream.Position.Is(0);
            }
        }

        [TestMethod]
        public void PeekElementWithoutChildren_2()
        {
            using (var stream = File.OpenRead(Path.Combine(IDIR, "Xliff1.xliff")))
            {
                var root = stream.PeekElementWithoutChildren(true);
                root.Name.Namespace.Is("");
                root.Name.LocalName.Is("xliff");
                root.Attribute("version").Value.Is("1.2");
                (stream.Position > 0).IsTrue();
            }
        }

        [TestMethod]
        public void PeekElementWithoutChildren_3()
        {
            using (var stream = File.OpenRead(Path.Combine(IDIR, "Language_Support.doc.sdlxliff")))
            {
                var root = stream.PeekElementWithoutChildren();
                root.Name.Namespace.Is("urn:oasis:names:tc:xliff:document:1.2");
                root.Name.LocalName.Is("xliff");
                root.Attribute("version").Value.Is("1.2");
                stream.Position.Is(0);
            }
        }

        [TestMethod]
        public void PeekElementWithoutChildren_4()
        {
            using (var stream = File.OpenRead(Path.Combine(IDIR, "Language_Support.doc.sdlxliff")))
            {
                var root = stream.PeekElementWithoutChildren(true);
                root.Name.Namespace.Is("urn:oasis:names:tc:xliff:document:1.2");
                root.Name.LocalName.Is("xliff");
                root.Attribute("version").Value.Is("1.2");
                (stream.Position > 0).IsTrue();
            }
        }

        [TestMethod]
        public void PeekElementWithoutChildren_5()
        {
            using (var stream = File.OpenRead(Path.Combine(IDIR, "my_memory.tmx")))
            {
                var root = stream.PeekElementWithoutChildren();
                root.Name.Namespace.Is("");
                root.Name.LocalName.Is("tmx");
                root.Attribute("version").Value.Is("1.1");
                stream.Position.Is(0);
            }
        }

        [TestMethod]
        public void PeekElementWithoutChildren_6()
        {
            using (var stream = File.OpenRead(Path.Combine(IDIR, "my_memory.tmx")))
            {
                var root = stream.PeekElementWithoutChildren(true);
                root.Name.Namespace.Is("");
                root.Name.LocalName.Is("tmx");
                root.Attribute("version").Value.Is("1.1");
                (stream.Position > 0).IsTrue();
            }
        }

        [TestMethod]
        public void PeekElementWithoutChildren_7()
        {
            using (var stream = File.OpenRead(Path.Combine(IDIR, "multilanguage.tmx")))
            {
                var root = stream.PeekElementWithoutChildren();
                root.Name.Namespace.Is("http://www.lisa.org/tmx14");
                root.Name.LocalName.Is("tmx");
                root.Attribute("version").Value.Is("1.4");
                stream.Position.Is(0);
            }
        }

        [TestMethod]
        public void PeekElementWithoutChildren_8()
        {
            using (var stream = File.OpenRead(Path.Combine(IDIR, "multilanguage.tmx")))
            {
                var root = stream.PeekElementWithoutChildren(true);
                root.Name.Namespace.Is("http://www.lisa.org/tmx14");
                root.Name.LocalName.Is("tmx");
                root.Attribute("version").Value.Is("1.4");
                (stream.Position > 0).IsTrue();
            }
        }

        [TestMethod]
        public void PeekElementWithoutChildren_Unseekable_1()
        {
            using (var stream = new StreamWrapper(File.OpenRead(Path.Combine(IDIR, "Xliff1.xliff"))))
            {
                AssertEx.Catch<NotSupportedException>(() => stream.PeekElementWithoutChildren());
            }
        }

        [TestMethod]
        public void PeekElementWithoutChildren_Unseekable_2()
        {
            using (var stream = new StreamWrapper(File.OpenRead(Path.Combine(IDIR, "Xliff1.xliff"))))
            {
                stream.PeekElementWithoutChildren(true);
            }
        }

        private class StreamWrapper : Stream
        {
            public StreamWrapper(Stream stream)
            {
                BaseStream = stream;
            }

            private readonly Stream BaseStream;

            public override bool CanRead => BaseStream.CanRead;

            public override bool CanSeek => false;

            public override bool CanTimeout => BaseStream.CanTimeout;

            public override bool CanWrite => BaseStream.CanWrite;

            public override long Length
            {
                get { throw new NotSupportedException(); }
            }

            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public override int ReadTimeout
            {
                get { return BaseStream.ReadTimeout; }
                set { BaseStream.ReadTimeout = value; }
            }

            public override int WriteTimeout
            {
                get { return BaseStream.WriteTimeout; }
                set { BaseStream.WriteTimeout = value; }
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                => BaseStream.BeginRead(buffer, offset, count, callback, state);

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                => BaseStream.BeginWrite(buffer, offset, count, callback, state);

            public override void Close() => BaseStream.Close();

            public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
                => BaseStream.CopyToAsync(destination, bufferSize, cancellationToken);

            protected override void Dispose(bool disposing)
            {
                if (disposing) BaseStream.Dispose();
            }

            public override int EndRead(IAsyncResult asyncResult) => BaseStream.EndRead(asyncResult);

            public override void EndWrite(IAsyncResult asyncResult) => BaseStream.EndWrite(asyncResult);

            public override void Flush() => BaseStream.Flush();

            public override Task FlushAsync(CancellationToken cancellationToken)
                => BaseStream.FlushAsync(cancellationToken);

            public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => BaseStream.ReadAsync(buffer, offset, count, cancellationToken);

            public override int ReadByte() => BaseStream.ReadByte();

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => BaseStream.WriteAsync(buffer, offset, count, cancellationToken);

            public override void WriteByte(byte value) => BaseStream.WriteByte(value);
        }
    }
}
