using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using disfr.po;

namespace UnitTestLangUtils
{
    /// <summary>A set of proxy methods for some private methods of <see cref="LangUtils"/>.</summary>
    public static class LangUtilsProxy
    {
        /// <summary>Invokes the LangUtils.ParseLocaleLabel method via reflection.</summary>
        public static void ParseLocaleLabel(string locale,
            out string language, out string territory, out string codeset, out string modifier)
        {
            var method = typeof(LangUtils).GetMethod(nameof(ParseLocaleLabel),
                BindingFlags.NonPublic | BindingFlags.Static);

            var parameters = new object[5];
            parameters[0] = locale;
            try
            {
                method.Invoke(null, parameters);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }

            language = (string)parameters[1];
            territory = (string)parameters[2];
            codeset = (string)parameters[3];
            modifier = (string)parameters[4];
        }
    }
}
