using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.po
{
    /// <summary>Provides several static methods to handle <i>external</i> language/locale/etc. representation.</summary>
    /// <remarks>The current implementation is a quick hack; it won't work in many circumstances.</remarks>
    public static class LangUtils
    {
        /// <summary>Builds an estimated locale label for an XML language indicator (aka BCP 47).</summary>
        /// <param name="lang">XML language indicator.</param>
        /// <returns>Estimated locale label.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lang"/> is null.</exception>
        /// <exception cref="ArgumentException">No appropriate locale label is found for <paramref name="lang"/>.</exception>
        public static string XmlLangToPosixLocaleLabel(string lang)
        {
            if (lang is null) throw new ArgumentNullException(nameof(lang));
            if (lang == string.Empty)
            {
                // In disfr, an empty string represents unknown/unspecified language.
                return string.Empty;
            }
            try
            {
                // Find the CultureInfo corresponding to the given language indicator.
                var culture = CultureInfo.GetCultureInfo(lang);
                if (culture.Equals(CultureInfo.InvariantCulture))
                {
                    // An invariant culture is not a language.
                    throw new ArgumentException("invariant culture is not allowed", nameof(lang));
                }

                // A neutral culture is one without region, i.e.,
                // either a language alone or a language with some non-region subtags 
                // (most likely a script subtag).
                if (culture.IsNeutralCulture)
                {
                    var i = lang.IndexOf('-');
                    return (i >= 0)
                        ? culture.TwoLetterISOLanguageName + '@' + lang.Substring(i + 1)
                        : culture.Name;
                }

                // Otherwise, it is a specific culture, i.e., one with a region.
                // Find the language and region codes, and create a locale label with them.
                var region = new RegionInfo(lang);
                var ll = culture.TwoLetterISOLanguageName;
                var rr = region.TwoLetterISORegionName;
                var locale = ll + '_' + rr;

                // If the pair of the Language and the region can identify the culture info,
                // it means the locale label created above is jsut fine.
                // Otherwise, we need more info.
                if (!CultureInfo.GetCultureInfo(ll + "-" + rr).Equals(culture))
                {
                    // We need information from other subtags.
                    // Just use all the remaining subtags to form a modifier.
                    // Most likely it is a script subtag, and we can't handle other cases anyway.
                    var subtags = lang.Split('-').ToList();
                    subtags.RemoveAt(0);
                    var p = subtags.FindIndex(s => s.Equals(rr, StringComparison.InvariantCultureIgnoreCase));
                    if (p >= 0) subtags.RemoveAt(p);
                    locale += '@' + string.Join("-", subtags);
                }

                return locale;
            }
            catch (Exception e)
            {
                throw new ArgumentException("No appropriate locale label", nameof(lang), e);
            }
        }

        /// <summary>Finds an estimated XML language indicator (aka BCP 47) from a posix locale label.</summary>
        /// <param name="locale">Posix locale label.</param>
        /// <returns>XML language indicator.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is null.</exception>
        /// <exception cref="ArgumentException">No suitable language is found.</exception>
        /// <remarks>
        /// <para>Codeset in the locale label is recognized and ignored.</para>
        /// <para></para>
        /// </remarks>
        public static string PosixLocaleLabelToXmlLang(string locale)
        {
            // Handle some special cases.
            if (locale is null)
            {
                throw new ArgumentNullException(nameof(locale));
            }
            if (locale == string.Empty)
            {
                return "";
            }

            // Parse the locale label into language, territory, codeset, and modifier.
            ParseLocaleLabel(locale,
                out string language,
                out string territory,
                out string _, /* codeset */
                out string modifier);

            // Check some error cases.
            if (language == string.Empty) throw new ArgumentException("Empty language is not allowed.", nameof(locale));
            if (territory == string.Empty) throw new ArgumentException("Empty territory is not allowed.", nameof(locale));
            if (modifier == string.Empty) throw new ArgumentException("EMpty modifier is not allowed.", nameof(locale));
            if (language.Equals("C", StringComparison.InvariantCultureIgnoreCase) ||
                language.Equals("Posix", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("\"C\" and \"Posix\" locales are not a language.", nameof(locale));
            }

            // Try to create a culture info using language, any territory, and any modifier.
            // Note that, in practice, we can only handle modifiers that are scripts,
            // so we presume any modifier of length 4 represents a script.
            string lang = language;
            if (modifier?.Length == 4) lang += '-' + modifier;
            if (!(territory is null)) lang += '-' + territory;
            if (!(modifier is null) && modifier.Length != 4) lang += '-' + modifier;
            try
            {
                return CultureInfo.GetCultureInfo(lang).Name;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"No corresponding language identifier: {locale}", nameof(locale), e);
            }
        }

        private static readonly char[] UnderScoreFullStopOrCommercialAt = { '_', '.', '@' };
        private static readonly char[] FullStopOrCommercialAt = { '.', '@' };

        /// <summary>Parses a posix locale label into components.</summary>
        /// <param name="locale">Posix locale label to parse.</param>
        /// <param name="language">Language component.</param>
        /// <param name="territory">Territory component, or null if no territory is specified.</param>
        /// <param name="codeset">Codeset component, or null if no codeset is specified.</param>
        /// <param name="modifier">Modifier component, or null if no modifier is specified.</param>
        /// <remarks>
        /// This method considers the label's syntax only; it doesn't consider meanings.
        /// </remarks>
        private static void ParseLocaleLabel(string locale,
            out string language, out string territory, out string codeset, out string modifier)
        {
            if (locale is null) throw new ArgumentNullException(nameof(locale));

            int p = locale.IndexOfAny(UnderScoreFullStopOrCommercialAt);
            if (p < 0)
            {
                language = locale;
                territory = null;
                codeset = null;
                modifier = null;
                return;
            }
            if (locale.IndexOf('_', p + 1) >= 0) throw new ArgumentException(nameof(locale));

            int q = locale.IndexOfAny(FullStopOrCommercialAt, p);
            if (q < 0)
            {
                language = locale.Substring(0, p);
                territory = locale.Substring(p + 1);
                codeset = null;
                modifier = null;
                return;
            }
            if (locale.IndexOf('.', q + 1) >= 0) throw new ArgumentException(nameof(locale));

            int r = locale.IndexOf('@', q);
            if (r < 0)
            {
                language = locale.Substring(0, p);
                territory = (p == q) ? null : locale.Substring(p + 1, q - p - 1);
                codeset = locale.Substring(q + 1);
                modifier = null;
                return;
            }
            if (locale.IndexOf('@', r + 1) >= 0) throw new ArgumentException(nameof(locale));

            {
                language = locale.Substring(0, p);
                territory = (p == q) ? null : locale.Substring(p + 1, q - p - 1);
                codeset = (q == r) ? null : locale.Substring(q + 1, r - q - 1);
                modifier = locale.Substring(r + 1);
            }
        }

        private static bool IsAlnum(char c)
        {
            if (c > 'Z')
            {
                return c >= 'a' && c <= 'z';
            }
            else
            {
                return c >= (c > '9' ? 'A' : '0');
            }
        }
    }
}
