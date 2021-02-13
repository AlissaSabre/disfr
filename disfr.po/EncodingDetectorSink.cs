using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.po
{
    class EncodingDetectorSink : ISink
    {
        public class DetectionTerminatedException : Exception { }

        public string Charset { get; private set; }

        public void AddExtractedComment(string comment) { }
        public void AddFlags(string flags) { }
        public void AddReferences(string references) { }
        public void AddTranslatorComment(string comment) { }
        public void MakePrevious() { }
        public void Reset() { }
        public void SetDomain(string domain) { }

        public void FinishMessage(bool is_obsolete)
        {
            NotDetected();
        }

        public void SetMsgCtxt(string context)
        {
            if (!string.IsNullOrEmpty(context)) NotDetected();
        }

        public void SetMsgId(string source)
        {
            if (!string.IsNullOrEmpty(source)) NotDetected();
        }

        public void SetMsgIdPlural(string source)
        {
            NotDetected();
        }

        private const string ContentTypeHeader = "Content-Type:";
        private static readonly string[] ContentTypeFrills =
        {
            "text",
            "/",
            "plain",
            ";",
            "charset",
            "=",
        };

        public void SetMsgStr(string target)
        {
            foreach (var line in target.Split('\n'))
            {
                if (line.StartsWith(ContentTypeHeader, StringComparison.InvariantCultureIgnoreCase))
                {
                    var s = line.Substring(ContentTypeHeader.Length).Trim();
                    foreach (var frill in ContentTypeFrills)
                    {
                        if (!s.StartsWith(frill, StringComparison.InvariantCultureIgnoreCase)) NotDetected();
                        s = s.Substring(frill.Length).TrimStart();
                    }
                    if (s.StartsWith("\""))
                    {
                        var p = s.IndexOf('"', 1);
                        if (p >= 0)
                        {
                            s = s.Substring(1, p - 1);
                        }
                    }
                    else
                    {
                        var p = s.IndexOfAny(new[] { ' ', '\t', '(' });
                        if (p >= 0)
                        {
                            s = s.Substring(0, p);
                        }
                    }
                    Charset = s;
                    throw new DetectionTerminatedException();
                }
            }
            NotDetected();
        }

        public void SetMsgStrPlural(string target, string ordinal)
        {
            NotDetected();
        }

        private void NotDetected()
        {
            Charset = null;
            throw new DetectionTerminatedException();
        }
    }
}
