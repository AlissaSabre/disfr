using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using ISink = disfr.po.ISink;

namespace UnitTestPoParser
{
    class PoParser
    {
        private const string AssemblyName = "disfr.po";

        private const string ClassName = "disfr.po.PoParser";

        private readonly object Parser;

        private readonly MethodInfo ParseMethod;

        public PoParser()
        {
            var assembly = Assembly.Load(AssemblyName);
            var type = assembly.GetType(ClassName);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var ctor = type.GetConstructor(flags, null, Type.EmptyTypes, null);
            Parser = ctor.Invoke(null);

            ParseMethod = type.GetMethod("Parse", new[] { typeof(string), typeof(ISink) });
        }

        public void Parse(string filename, ISink sink)
        {
            ParseMethod.Invoke(Parser, new object[] { filename, sink });
        }
    }
}
