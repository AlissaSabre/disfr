using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void Is(this InlineString inline, params object[] items)
        {
            inline.RunsWithProperties.Is(Rwps(items));
        }

        public static void Is(this InlineBuilder builder, params object[] items)
        {
            builder.AsEnumerable().Is(Rwps(items));
        }

        private static IEnumerable<InlineRunWithProperty> Rwps(object[] items)
        {
            var prop = default(InlineProperty);
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item is string)
                {
                    yield return new InlineRunWithProperty(prop, new InlineText((string)item));
                }
                else if (item is InlineRun)
                {
                    yield return new InlineRunWithProperty(prop, (InlineRun)item);
                }
                else if (item is InlineProperty)
                {
                    prop = (InlineProperty)item;
                }
            }
        }
    }
}
