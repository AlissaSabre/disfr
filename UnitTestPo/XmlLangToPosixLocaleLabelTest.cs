using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static disfr.po.LangUtils;

namespace UnitTestLangUtils
{
    [TestClass]
    public class XmlLangToPosixLocaleLabelTest
    {
        [TestMethod]
        public void Test_Popular_simple_languages_are_converted_well()
        {
            TestXP("en", "en");
            TestXP("fr", "fr");
            TestXP("de", "de");
            TestXP("ja", "ja");
            TestXP("ko", "ko");
            TestXP("ru", "ru");
            TestXP("sr", "sr");
        }

        [TestMethod]
        public void Test_Popular_language_and_region_pairs_are_converted_well()
        {
            TestXP("en-us", "en_us");
            TestXP("en-gb", "en_gb");
            TestXP("fr-fr", "fr_fr");
            TestXP("de-de", "de_de");
            TestXP("ja-jp", "ja_jp");
            TestXP("ko-kr", "ko_kr");
            TestXP("ru-ru", "ru_ru");
            TestXP("zh-cn", "zh_cn");
            TestXP("zh-hk", "zh_hk");
            TestXP("zh-tw", "zh_tw");
            TestXP("pa-pk", "pa_pk");
            TestXP("pa-in", "pa_in");
        }

        [TestMethod]
        public void Test_Popular_languages_with_scripts_are_converted_well()
        {
            TestXP("zh-hans", "zh@hans");
            TestXP("zh-hant", "zh@hant");

            TestXP("sr-latn", "sr@latn"); // Traditionally sr@latin
            TestXP("sr-cyrl", "sr@cyrl"); // Traditionally sr@cyrillic

            TestXP("pa-arab", "pa@arab");
            TestXP("pa-guru", "pa@guru");
        }

        [TestMethod]
        public void Test_Some_popular_languages_with_both_script_and_region_are_converted_reasonablly()
        {
            TestXP("zh-hans-cn", "zh_cn@hans", "zh_cn");
            TestXP("zh-hant-cn", "zh_cn@hant");
            TestXP("zh-hans-hk", "zh_hk@hans");
            TestXP("zh-hant-hk", "zh_hk@hant", "zh_hk");
            TestXP("zh-hans-tw", "zh_tw@hans");
            TestXP("zh-hant-tw", "zh_tw@hant", "zh_tw");

            TestXP("pa-arab-pk", "pa_pk@arab", "pa_pk");
            TestXP("pa-guru-in", "pa_in@guru", "pa_in");
            TestXP("pa-deva-in", "pa_in@deva");
        }

        [TestMethod]
        public void Test_Some_unusual_combinations_convert_straight()
        {
            TestXP("ko-mn", "ko_mn");
            TestXP("sv-au", "sv_au");

            TestXP("de-cyrl", "de@cyrl");
            TestXP("fa-hang", "fa@hang");

            TestXP("fr-hans-gr", "fr_gr@hans");
            TestXP("ar-deva-gb", "ar_gb@deva");
        }

        [TestMethod]
        public void Test_Nonexistent_language_codes_converts_syntactically()
        {
            TestXP("bb", "bb");
            TestXP("qr", "qr");
            TestXP("zz", "zz");

            TestXP("bb-us", "bb_us");
            TestXP("qr-fr", "qr_fr");
            TestXP("zz-jp", "zz_jp");

            TestXP("bb-hant", "bb@hant");
            TestXP("qr-latn", "qr@latn");
            TestXP("zz-arab", "zz@arab");

            TestXP("bb-cyrl-ru", "bb_ru@cyrl");
            TestXP("qr-hans-cn", "qr_cn@hans");
            TestXP("zz-arab-eg", "zz_eg@arab");
        }

        [TestMethod]
        public void Test_Nonexistent_region_codes_converts_syntactically()
        {
            TestXP("en-aa", "en_aa");
            TestXP("ar-ox", "ar_ox");
            TestXP("zh-zz", "zh_zz");

            TestXP("mn-mong-aa", "mn_aa@mong");
            TestXP("pt-latn-ox", "pt_ox@latn");
            TestXP("ru-cyrl-zz", "ru_zz@cyrl");
        }

        [TestMethod]
        public void Test_Nonexistent_script_codes_converts_syntactically()
        {
            TestXP("en-fool", "en@fool");
            TestXP("ar-quiz", "ar@quiz");
            TestXP("zh-xxxx", "zh@xxxx");

            TestXP("es-fool-es", "es_es@fool");
            TestXP("ja-quiz-jp", "ja_jp@quiz");
            TestXP("ko-xxxx-kr", "ko_kr@xxxx");
        }

        [TestMethod]
        public void Test_The_empty_language_corresponds_to_the_empty_locale_label()
        {
            TestXP("", "");
        }

        [TestMethod]
        public void Test_Null_throws_an_exception()
        {
            AssertEx.Catch<ArgumentNullException>(() => XmlLangToPosixLocaleLabel(null));
        }

        /// <summary>Tests <paramref name="lang"/> is converted to <paramref name="locale"/>.</summary>
        /// <param name="lang">XML language.</param>
        /// <param name="locale">Posix locale label.</param>
        /// <remarks>Strings are compared after lowercased.</remarks>
        private static void TestXP(string lang, string locale)
        {
            XmlLangToPosixLocaleLabel(lang).ToLowerInvariant().Is(locale.ToLowerInvariant());
        }

        /// <summary>Tests <paramref name="lang"/> is converted to either of the listed <paramref name="locales"/>.</summary>
        /// <param name="lang">XML language.</param>
        /// <param name="locales">List of Posix locale labels.</param>
        /// <remarks>Strings are compared after lowercased.</remarks>
        private static void TestXP(string lang, params string[] locales)
        {
            var result = XmlLangToPosixLocaleLabel(lang).ToLowerInvariant();
            if (locales.Any(x => x.Equals(result, StringComparison.InvariantCultureIgnoreCase))) return;
            Assert.Fail("Actual:<{0}>", result);
        }

        /// <summary>Describes a test case that meets a condition or throws an exception.</summary>
        /// <typeparam name="T">Base class of allowed exceptions.</typeparam>
        /// <param name="test">An <see cref="Action"/>-wrapped statement usually containing some assertions.</param>
        /// <remarks>
        /// This method is used in a test method typically as follows:
        /// <code>
        /// MayCatch&lt;SomeException&gt;(() => expression.Is(value));
        /// </code>
        /// This construct represents an assertion that
        /// the <c>expression</c> evaluates to the <c>value</c> or throw an exception of type <c>SomeException</c>
        /// (or one of its derived types.)
        /// </remarks>
        private static void AssertsOrCatch<T>(Action test) where T : Exception
        {
            try
            {
                test.Invoke();
                return;
            }
            catch (UnitTestAssertException)
            {
                throw;
            }
            catch (T)
            {
            }
        }

        [TestMethod]
        public void Test_AssertsOrCatch()
        {
            AssertsOrCatch<NullReferenceException>(() => ((string)null).ToLower().Is("abc"));
            AssertsOrCatch<NullReferenceException>(() => "abc".Length.Is(3));

            AssertEx.Catch<UnitTestAssertException>(() =>
                AssertsOrCatch<UnitTestAssertException>(() => "abc".Length.Is(4))
            );
        }
    }
}
