using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Alissa.Test.Utils
{
    static class TestHelper
    {
        public static object CreateInstance(string assembly_name, string type_name, params object[] parameters)
        {
            var parameter_types = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) parameter_types[i] = parameters[i].GetType();
            var assembly = Assembly.Load(assembly_name);
            var type = assembly.GetType(type_name);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var ctor = type.GetConstructor(flags, null, parameter_types, null);
            return ctor.Invoke(parameters);
        }
    }
}
