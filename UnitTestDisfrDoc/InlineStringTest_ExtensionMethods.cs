using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    public static class InlineStringTest_ExtensionMethods
    {
        public static void Is(this InlineTag tag, Tag type, string id, string rid, string name, string ctype, string display, string code)
        {
            tag.Is(new InlineTag(type, id, rid, name, ctype, display, code));
        }

        public static void Is(this InlineString inline, params Rwp[] items)
        {
            inline.RunsWithProperties.Is(items.Select(Rwp.Unbox));
        }

        public static void Is(this InlineBuilder builder, params Rwp[] items)
        {
            builder.AsEnumerable().Is(items.Select(Rwp.Unbox));
        }

        /// <summary>
        /// Wraps an <see cref="InlineRunWithProperty"/> value, providing several operator overrides.
        /// </summary>
        public class Rwp
        {
            private InlineRunWithProperty Value;

            private Rwp(InlineRunWithProperty rwp)
            {
                Value = rwp;
            }

            public static Rwp Create(InlineProperty property, string text)
            {
                return new Rwp(new InlineRunWithProperty(property, new InlineText(text)));
            }

            public static Rwp Create(InlineProperty property, InlineRun run)
            {
                return new Rwp(new InlineRunWithProperty(property, run));
            }

            public static InlineRunWithProperty Unbox(Rwp rwp)
            {
                return rwp.Value;
            }

            public static implicit operator Rwp(string text)
            {
                return new Rwp(new InlineRunWithProperty(text));
            }

            public static implicit operator Rwp(InlineRun run)
            {
                return new Rwp(new InlineRunWithProperty(run));
            }
        }
    }


}
