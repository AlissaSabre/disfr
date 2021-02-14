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
                Scanner = new PoScanner(stream, "ISO-8859-1");
                var detector = new EncodingDetectorSink();
                Sink = detector;
                try
                {
                    Parse();
                }
                catch (EncodingDetectorSink.DetectionTerminatedException)
                {
                    // do nothing.
                }
                var charset = detector.Charset ?? "UTF-8";

                stream.Seek(0, SeekOrigin.Begin);
                Scanner = new PoScanner(stream, charset) { SourceFileName = filename };
                Sink = sink;
                Parse();
            }
        }

        private ISink Sink;
    }
}
