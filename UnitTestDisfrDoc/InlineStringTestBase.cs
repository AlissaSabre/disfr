using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    public abstract class InlineStringTestBase
    {
        protected static readonly InlineProperty None = InlineProperty.None;
        protected static readonly InlineProperty Ins = InlineProperty.Ins;
        protected static readonly InlineProperty Del = InlineProperty.Del;
        protected static readonly InlineProperty Emp = InlineProperty.Emp;

        protected static readonly InlineProperty[] InlineProperties =
        {
            None, Ins, Del, Emp,
        };

        protected InlineStringTest_ExtensionMethods.Rwp Ins_(string text) => InlineStringTest_ExtensionMethods.Rwp.Create(Ins, text);
        protected InlineStringTest_ExtensionMethods.Rwp Del_(string text) => InlineStringTest_ExtensionMethods.Rwp.Create(Del, text);
        protected InlineStringTest_ExtensionMethods.Rwp Emp_(string text) => InlineStringTest_ExtensionMethods.Rwp.Create(Emp, text);
        protected InlineStringTest_ExtensionMethods.Rwp Ins_(InlineRun run) => InlineStringTest_ExtensionMethods.Rwp.Create(Ins, run);
        protected InlineStringTest_ExtensionMethods.Rwp Del_(InlineRun run) => InlineStringTest_ExtensionMethods.Rwp.Create(Del, run);
        protected InlineStringTest_ExtensionMethods.Rwp Emp_(InlineRun run) => InlineStringTest_ExtensionMethods.Rwp.Create(Emp, run);
    }
}
