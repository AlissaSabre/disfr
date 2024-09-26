using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static disfr.po.LangUtils;

namespace UnitTestLangUtils
{
    [TestClass]
    public class PosixLocaleLabelToXmlLangTest
    {
        [TestMethod]
        public void Test_Popular_simple_languages_are_converted_well()
        {
            TestPX("en", "en");
            TestPX("fr", "fr");
            TestPX("de", "de");
            TestPX("ja", "ja");
            TestPX("ko", "ko");
            TestPX("ru", "ru");
            TestPX("sr", "sr");
        }

        [TestMethod]
        public void Test_Popular_language_and_region_pairs_are_converted_well()
        {
            TestPX("en_us", "en-us");
            TestPX("en_gb", "en-gb");
            TestPX("fr_fr", "fr-fr");
            TestPX("de_de", "de-de");
            TestPX("ja_jp", "ja-jp");
            TestPX("ko_kr", "ko-kr");
            TestPX("ru_ru", "ru-ru");
            TestPX("zh_cn", "zh-cn");
            TestPX("zh_hk", "zh-hk");
            TestPX("zh_tw", "zh-tw");
            TestPX("pa_pk", "pa-pk");
            TestPX("pa_in", "pa-in");
        }

        [TestMethod]
        public void Test_Popular_languages_with_scripts_are_converted_well()
        {
            TestPX("zh@hans", "zh-hans");
            TestPX("zh@hant", "zh-hant");

            TestPX("sr@latn", "sr-latn"); // Traditionally sr@latin
            TestPX("sr@cyrl", "sr-cyrl"); // Traditionally sr@cyrillic

            TestPX("pa@arab", "pa-arab");
            TestPX("pa@guru", "pa-guru");
        }

        [TestMethod]
        public void Test_Some_popular_languages_with_both_script_and_region_are_converted_reasonablly()
        {
            TestPX("zh_cn@hans", "zh-hans-cn", "zh-cn");
            TestPX("zh_cn@hant", "zh-hant-cn");
            TestPX("zh_hk@hans", "zh-hans-hk");
            TestPX("zh_hk@hant", "zh-hant-hk", "zh-hk");
            TestPX("zh_tw@hans", "zh-hans-tw");
            TestPX("zh_tw@hant", "zh-hant-tw", "zh-tw");

            TestPX("pa_pk@arab", "pa-arab-pk", "pa-pk");
            TestPX("pa_in@guru", "pa-guru-in", "pa-in");
            TestPX("pa_in@deva", "pa-deva-in");
        }

        [TestMethod]
        public void Test_Some_unusual_combinations_convert_straight()
        {
            TestPX("ko_mn", "ko-mn");
            TestPX("sv_au", "sv-au");

            TestPX("de@cyrl", "de-cyrl");
            TestPX("fa@hang", "fa-hang");

            TestPX("fr_gr@hans", "fr-hans-gr");
            TestPX("ar_gb@deva", "ar-deva-gb");
        }

        [TestMethod]
        public void Test_Nonexistent_language_codes_converts_syntactically()
        {
            TestPX("bb", "bb");
            TestPX("qr", "qr");
            TestPX("zz", "zz");

            TestPX("bb_us", "bb-us");
            TestPX("qr_fr", "qr-fr");
            TestPX("zz_jp", "zz-jp");

            TestPX("bb@hant", "bb-hant");
            TestPX("qr@latn", "qr-latn");
            TestPX("zz@arab", "zz-arab");

            TestPX("bb_ru@cyrl", "bb-cyrl-ru");
            TestPX("qr_cn@hans", "qr-hans-cn");
            TestPX("zz_eg@arab", "zz-arab-eg");
        }

        [TestMethod]
        public void Test_Nonexistent_region_codes_converts_syntactically()
        {
            TestPX("en_aa", "en-aa");
            TestPX("ar_ox", "ar-ox");
            TestPX("zh_zz", "zh-zz");

            TestPX("mn_aa@mong", "mn-mong-aa");
            TestPX("pt_ox@latn", "pt-latn-ox");
            TestPX("ru_zz@cyrl", "ru-cyrl-zz");
        }

        [TestMethod]
        public void Test_Nonexistent_script_codes_converts_syntactically()
        {
            TestPX("en@fool", "en-fool");
            TestPX("ar@quiz", "ar-quiz");
            TestPX("zh@xxxx", "zh-xxxx");

            TestPX("es_es@fool", "es-fool-es");
            TestPX("ja_jp@quiz", "ja-quiz-jp");
            TestPX("ko_kr@xxxx", "ko-xxxx-kr");
        }

        [TestMethod]
        public void Test_The_empty_language_corresponds_to_the_empty_locale_label()
        {
            TestPX("", "");
        }

        [TestMethod]
        public void Test_Null_throws_an_exception()
        {
            AssertEx.Catch<ArgumentNullException>(() => PosixLocaleLabelToXmlLang(null));
        }

        /// <summary>Tests <paramref name="locale"/> is converted to <paramref name="lang"/>.</summary>
        /// <param name="locale">Posix locale label.</param>
        /// <param name="lang">XML language.</param>
        /// <remarks>Strings are compared after lowercased.</remarks>
        private static void TestPX(string locale, string lang)
        {
            PosixLocaleLabelToXmlLang(locale).ToLowerInvariant().Is(lang.ToLowerInvariant());
        }

        /// <summary>Tests <paramref name="locale"/> is converted to either of the listed <paramref name="langs"/>.</summary>
        /// <param name="locale">Posix locale label.</param>
        /// <param name="langs">List of XML languages.</param>
        /// <remarks>Strings are compared after lowercased.</remarks>
        private static void TestPX(string locale, params string[] langs)
        {
            var result = PosixLocaleLabelToXmlLang(locale).ToLowerInvariant();
            if (langs.Any(x => x.Equals(result, StringComparison.InvariantCultureIgnoreCase))) return;
            Assert.Fail("Actual:<{0}>", result);
        }
    }
}
