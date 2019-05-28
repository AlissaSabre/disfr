using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    public class InlineStringTestBase
    {
        protected static readonly InlineProperty None = InlineProperty.None;
        protected static readonly InlineProperty Ins = InlineProperty.Ins;
        protected static readonly InlineProperty Del = InlineProperty.Del;
        protected static readonly InlineProperty Emp = InlineProperty.Emp;

        protected static readonly InlineProperty[] InlineProperties =
        {
            None, Ins, Del, Emp,
        };

        protected static readonly InlineToString TagCode = InlineToString.TagCode;
        protected static readonly InlineToString TagDebug = InlineToString.TagDebug;
        protected static readonly InlineToString TagLabel = InlineToString.TagLabel;
        protected static readonly InlineToString TagHidden = InlineToString.TagHidden;
        protected static readonly InlineToString TagNumber = InlineToString.TagNumber;
        protected static readonly InlineToString TextDebug = InlineToString.TextDebug;
        protected static readonly InlineToString TextLatest = InlineToString.TextLatest;

        protected static readonly InlineToString[] InlineToStrings =
        {
            TagCode, TagDebug, TagLabel, TagHidden, TagNumber,
            TextDebug, TextLatest,
        };
    }
}
