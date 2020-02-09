using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestDisfrDoc
{
    public class StringStream : MemoryStream
    {
        public StringStream(string text) : base(Encoding.UTF8.GetBytes(text), false) { }
    }
}
