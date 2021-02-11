using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace disfr.po
{
    internal partial class PoParser
    {
        public PoParser() : base(null) { }

        public void Parse(string filename, ISink sink)
        {
            using (var stream = File.OpenRead(filename))
            {
                Scanner = new PoScanner(stream, "UTF-8") { SourceFileName = filename };
                Sink = sink;
                Parse();
            }
        }

        private ISink Sink;
    }
}
