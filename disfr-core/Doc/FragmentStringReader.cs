﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    /// <summary>
    /// A <see cref="TextReader"/> that reads from strings using deferred execution technique.
    /// </summary>
    /// <remarks>
    /// This class may be useful if you need to parse a big XLIFF or another XML document
    /// that is generated by a program on-the-fly.
    /// </remarks>
    public class FragmentStringReader : TextReader
    {
        private IEnumerator<object> FragmentEnumerator;

        private string CurrentFragment;

        private int ReadIndex;

        private readonly bool ForceBlocking;

        private bool Disposed;

        /// <summary>Creates an instance.</summary>
        /// <param name="fragments">An iteration of objects whose <see cref="Object.ToString()"/> values are read.</param>
        public FragmentStringReader(IEnumerable<object> fragments, bool force_blocking = false)
        {
            FragmentEnumerator = fragments.GetEnumerator();
            ForceBlocking = force_blocking;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && !Disposed)
            {
                FragmentEnumerator?.Dispose();
            }
            FragmentEnumerator = null;
            CurrentFragment = null;
            Disposed = true;
        }

        public override int Peek()
        {
            if (Disposed) new ObjectDisposedException(ToString());
            if (!Fill()) return -1;
            return CurrentFragment[ReadIndex];
        }

        public override int Read()
        {
            if (Disposed) new ObjectDisposedException(ToString());
            if (!Fill()) return -1;
            return CurrentFragment[ReadIndex++];
        }

        public override int Read(char[] buffer, int index, int count)
        {
            return ReadImpl(buffer, index, count, ForceBlocking);
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            return ReadImpl(buffer, index, count, true);
        }

        /// <summary>
        /// Performs an equivalent to 
        /// <see cref="TextReader.Read(char[], int, int)"/> or <see cref="TextReader.ReadBlock(char[], int, int)"/> 
        /// depending on the flag.
        /// </summary>
        /// <param name="blocking">Set to true to perform a blocking operation.  False otherwise.</param>
        private int ReadImpl(char[] buffer, int index, int count, bool blocking)
        {
            if (Disposed) new ObjectDisposedException(ToString());

            if (buffer == null) throw new ArgumentNullException("buffer");
            if (index < 0) throw new ArgumentOutOfRangeException("index", index, "must be zero or positive");
            if (count < 0) throw new ArgumentOutOfRangeException("count", count, "must be zero or positive");
            if (index + count > buffer.Length) throw new ArgumentOutOfRangeException("count", count, "exceeded buffer size");

            int read_into = index;
            int remaining = count;
            while (remaining > 0 && Fill())
            {
                int chunk_size = Math.Min(remaining, CurrentFragment.Length - ReadIndex);
                CurrentFragment.CopyTo(ReadIndex, buffer, read_into, chunk_size);
                ReadIndex += chunk_size;
                read_into += chunk_size;
                remaining -= chunk_size;
                if (!blocking) break;
            }
            return count - remaining;
        }

        /// <summary>
        /// Makes the next character ready.
        /// </summary>
        /// <returns>
        /// True if a character is ready for reading.
        /// False if the iteration has ended and no more characters are available.
        /// </returns>
        private bool Fill()
        {
            for (; ; )
            {
                if (ReadIndex < CurrentFragment?.Length) return true;
                if (!FragmentEnumerator.MoveNext()) return false;
                CurrentFragment = FragmentEnumerator.Current.ToString();
                ReadIndex = 0;
            }
        }
    }
}
