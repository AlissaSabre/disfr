using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.po
{
    public abstract class CollectorSinkBase : ISink
    {
        /// <summary>Called back when a single message definition has been collected.</summary>
        /// <remarks>Derived classes of <see cref="CollectorSinkBase"/> are expected to override this method.</remarks>
        protected abstract void Finish();

        protected static readonly char[] AsciiWhiteSpaces = { ' ', '\t', '\n', '\v', '\f', '\r' };

        protected string Domain;

        protected bool IsObsolete;

        protected readonly List<string> TranslatorComments = new List<string>();

        protected readonly List<string> ExtractedComments = new List<string>();

        protected readonly List<string> References = new List<string>();

        protected readonly List<string> Flags = new List<string>();

        protected string PreviousMessageContext;

        protected string PreviousMessageId;

        protected string PreviousMessageIdPlural;

        protected string MessageContext;

        protected string MessageId;

        protected string MessageIdPlural;

        protected string MessageStr0;

        protected readonly List<string> MessageStrPlural = new List<string>();

        public void SetDomain(string domain)
        {
            Domain = domain;
        }

        public void Reset()
        {
            TranslatorComments.Clear();
            ExtractedComments.Clear();
            References.Clear();
            Flags.Clear();
            PreviousMessageContext = null;
            PreviousMessageId = null;
            PreviousMessageIdPlural = null;
            MessageStrPlural.Clear();
        }

        public void AddTranslatorComment(string comment)
        {
            TranslatorComments.Add(comment);
        }

        public void AddExtractedComment(string comment)
        {
            ExtractedComments.Add(comment);
        }

        public void AddReferences(string references)
        {
            References.Add(references);
        }

        public void AddFlags(string flags)
        {
            Flags.AddRange(flags.Split(AsciiWhiteSpaces, StringSplitOptions.RemoveEmptyEntries));
        }

        public void FinishMessage(bool is_obsolete)
        {
            IsObsolete = is_obsolete;
            Finish();
        }

        public void MakePrevious()
        {
            PreviousMessageContext = MessageContext;
            PreviousMessageId = MessageId;
            PreviousMessageIdPlural = MessageIdPlural;
        }

        public void SetMsgCtxt(string context)
        {
            MessageContext = context;
        }

        public void SetMsgId(string source)
        {
            MessageId = source;
        }

        public void SetMsgIdPlural(string source)
        {
            MessageIdPlural = source;
        }

        public void SetMsgStr(string target)
        {
            MessageStr0 = target;
        }

        public void SetMsgStrPlural(string ordinal, string target)
        {
            // What we should do with an overflow?  FIXME.
            var n = int.Parse(ordinal, NumberStyles.None, CultureInfo.InvariantCulture);
            if (n == 0)
            {
                MessageStr0 = target;
            }
            else
            {
                var p = MessageStrPlural;
                while (p.Count < n) p.Add(null);
                if (p.Count == n)
                {
                    p.Add(target);
                }
                else
                {
                    p[n] = target;
                }
            }
        }
    }
}
