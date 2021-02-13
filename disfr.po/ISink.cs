using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.po
{
    public interface ISink
    {
        void SetDomain(string domain);

        void Reset();

        void AddTranslatorComment(string comment);

        void AddExtractedComment(string comment);

        void AddReferences(string references);

        void AddFlags(string flags);

        void FinishMessage(bool is_obsolete);

        void MakePrevious();

        void SetMsgCtxt(string context);

        void SetMsgId(string source);

        void SetMsgIdPlural(string source);

        void SetMsgStr(string target);

        void SetMsgStrPlural(string target, string ordinal);
    }
}
