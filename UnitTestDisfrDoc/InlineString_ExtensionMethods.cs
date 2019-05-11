using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    public static class InlineString_ExtensionMethods
    {
        public static void Is(this InlineTag tag, Tag type, string id, string rid, string name, string ctype, string display, string code)
        {
            tag.Is(new InlineTag(type, id, rid, name, ctype, display, code));
        }

        public static void Is(this InlineString inline, params InlineElement[] elements)
        {
            inline.Elements.Is(elements);
        }

        public static List<InlineElement> Contents(this InlineBuilder inline)
        {
            return inline.AsDynamic()._Contents;
        }
    }
}
