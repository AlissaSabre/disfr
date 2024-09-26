using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.po;

namespace UnitTestLangUtils
{
    /// <summary>Tests disfr.po.LangUtils.ParseLocaleLabel static method.</summary>
    [TestClass]
    public class ParseLocaleLabelTest
    {
        [TestMethod]
        public void Test_Parses_valid_labels()
        {
            Test("en",               "en", null, null,    null);
            Test("en_us",            "en", "us", null,    null);
            Test("en.utf-8",         "en", null, "utf-8", null);
            Test("en_us.utf-8",      "en", "us", "utf-8", null);
            Test("en@latn",          "en", null, null,    "latn");
            Test("en_us@latn",       "en", "us", null,    "latn");
            Test("en.utf-8@latn",    "en", null, "utf-8", "latn");
            Test("en_us.utf-8@latn", "en", "us", "utf-8", "latn");
        }

        [TestMethod]
        public void Test_Handles_empty_components()
        {
            // language

            Test("",                  "",   null, null,    null);

            // language_territory

            Test("_us",               "",   "us", null,    null);
            Test("en_",               "en", "",   null,    null);

            Test("_",                 "",   "",   null,    null);

            // language.codeset

            Test(".utf-8",            "",   null, "utf-8", null);
            Test("en.",               "en", null, "",      null);

            Test(".",                 "",   null, "",      null);

            // language_territory.codeset

            Test("_us.utf-8",         "",   "us", "utf-8", null);
            Test("en_.utf-8",         "en", "",   "utf-8", null);
            Test("en_us.",            "en", "us", "",      null);

            Test("_.utf-8",           "",   "",   "utf-8", null);
            Test("_us.",              "",   "us", "",      null);
            Test("en_.",              "en", "",   "",      null);

            Test("_.",                "",   "",   "",      null);

            // language@modifier
            Test("@latn",             "",   null, null,    "latn");
            Test("en@",               "en", null, null,    ""    );

            Test("@",                 "",   null, null,    ""    );

            // language_territory@modifier

            Test("_us@latn",          "",   "us", null,    "latn");
            Test("en_@latn",          "en", "",   null,    "latn");
            Test("en_us@",            "en", "us", null,    ""    );

            Test("_@latn",            "",   "",   null,    "latn");
            Test("_us@",              "",   "us", null,    ""    );
            Test("en_@",              "en", "",   null,    ""    );

            Test("_@",                "",   "",   null,    ""    );

            // language.codeset@modifier

            Test(".utf-8@latn",       "",   null, "utf-8", "latn");
            Test("en.@latn",          "en", null, "",      "latn");
            Test("en.utf-8@",         "en", null, "utf-8", ""    );

            Test(".@latn",            "",   null, "",      "latn");
            Test(".utf-8@",           "",   null, "utf-8", ""    );
            Test("en.@",              "en", null, "",      ""    );

            Test(".@",                "",   null, "",      ""    );
            
            // language_territory.codeset@modifier

            Test("_us.utf-8@latn",   "",   "us", "utf-8", "latn");
            Test("en_.utf-8@latn",   "en", "",   "utf-8", "latn");
            Test("en_us.@latn",      "en", "us", "",      "latn");
            Test("en_us.utf-8@",     "en", "us", "utf-8", ""    );

            Test("_.utf-8@latn",     "",   "",   "utf-8", "latn");
            Test("_us.@latn",        "",   "us", "",      "latn");
            Test("_us.utf-8@",       "",   "us", "utf-8", ""    );
            Test("en_.@latn",        "en", "",   "",      "latn");
            Test("en_.utf-8@",       "en", "",   "utf-8", ""    );
            Test("en_us.@",          "en", "us", "",      ""    );

            Test("_.@latn",          "",   "",   "",      "latn");
            Test("_.utf-8@",         "",   "",   "utf-8", ""    );
            Test("_us.@",            "",   "us", "",      ""    );
            Test("en_.@",            "en", "",   "",      ""    );

            Test("_.@",              "",   "",   "",      ""    );
        }

        [TestMethod]
        public void Test_Null_locale_throws_an_exception()
        {
            Test<ArgumentNullException>(null);
        }

        [TestMethod]
        public void Test_A_wrong_order_of_components_throws_an_exception()
        {
            // en_us.utf-8
            Test<ArgumentException>("en.utf-8_us");

            // en_us@latn
            Test<ArgumentException>("en@latn_us");

            // en_us.utf-8@latn
            Test<ArgumentException>("en_us@latn.utf-8");
            Test<ArgumentException>("en.utf-8_us@latn");
            Test<ArgumentException>("en.utf-8@latn_us");
            Test<ArgumentException>("en@latn_us.utf-8");
            Test<ArgumentException>("en@latn.utf-8_us");
        }

        [TestMethod]
        public void Test_Duplicate_components_throw_an_exception()
        {
            // language_territory

            Test<ArgumentException>("en_us_us");

            // language.codeset

            Test<ArgumentException>("en.utf-8.utf-8");

            // language_territory.codeset

            Test<ArgumentException>("en_us_us.utf-8");
            Test<ArgumentException>("en_us.utf-8.utf-8");

            // language@modifier

            Test<ArgumentException>("en@latn@latn");

            // language_territory@modifier

            Test<ArgumentException>("en_us_us@latn");
            Test<ArgumentException>("en_us@latn@latn");

            // language.codeset@modifier

            Test<ArgumentException>("en.utf-8.utf-8@latn");
            Test<ArgumentException>("en.utf-8@latn@latn");

            // language_territory.codeset@modifier

            Test<ArgumentException>("en_us_us.utf-8@latn");
            Test<ArgumentException>("en_us.utf-8.utf-8@latn");
            Test<ArgumentException>("en_us.utf-8@latn@latn");
        }

        private static void Test(string locale, string language, string territory, string codeset, string modifier)
        {
            LangUtilsProxy.ParseLocaleLabel(locale,
                out string out_language,
                out string out_territory,
                out string out_codeset,
                out string out_modifier);
            (out_language?. ToLowerInvariant()).Is(language?. ToLowerInvariant(), nameof(language));
            (out_territory?.ToLowerInvariant()).Is(territory?.ToLowerInvariant(), nameof(territory));
            (out_codeset?.  ToLowerInvariant()).Is(codeset?.  ToLowerInvariant(), nameof(codeset));
            (out_modifier?. ToLowerInvariant()).Is(modifier?. ToLowerInvariant(), nameof(modifier));
        }

        private static void Test<T>(string locale) where T: Exception
        {
            AssertEx.Catch<T>(() => LangUtilsProxy.ParseLocaleLabel(locale,
                out string out_language,
                out string out_territory,
                out string out_codeset,
                out string out_modifier));
        }
    }
}
