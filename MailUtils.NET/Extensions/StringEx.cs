using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MailUtils.Constants;

namespace MailUtils.Extensions
{
    public static class StringEx
    {
        /// <summary>
        /// Reads the current file to a string - encoding will be automatically detected from the byte order marks).
        /// </summary>
        /// <param name="fileName">File's name</param>
        /// <returns></returns>
        public static string ReadTextFromFile(this string fileName)
        {
            var cssStream = File.OpenRead(fileName);

            using (var reader = new StreamReader(cssStream, detectEncodingFromByteOrderMarks: true))
            {
                var result = reader.ReadToEnd();
                return result;
            }
        }

        /// <summary>
        /// Replaces all non-printable characters with the desired replacement string (by default it will remove non-printables)...
        /// </summary>
        /// <param name="text"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceNonPrintableCharacters(this string text, string replacement = "")
        {
            if (text == null)
                return null;

            var r = new Regex(RegexPatterns.StringNonPrintablePattern, RegexOptions.Compiled);
            return r.Replace(text, replacement);
        }

        /// <summary>
        /// Replaces all non-single spaces whit a single space character.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string SingletonizeSpaces(this string text)
        {
            if (text == null)
                return null;

            var r = new Regex(RegexPatterns.StringMultipleSpacesPattern, RegexOptions.Compiled);
            return r.Replace(text, " ");
        }

        /// <summary>
        /// Removes whitespace characters from the beginning and the end of the given text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string TrimWhitespaces(this string text)
        {
            if (text == null)
                return null;

            var r = new Regex(RegexPatterns.StringWhitespaceTrimmerPattern, RegexOptions.Compiled);
            return r.Replace(text, string.Empty);
        }

        /// <summary>
        /// Checks if the given text is an email - the entire input must be an email, it's not enough if the text contains one!
        /// </summary>
        /// <param name="text"></param>
        /// <param name="emailPattern"></param>
        /// <returns></returns>
        public static bool IsEmail(this string text, string emailPattern = RegexPatterns.EmailPatternRfc2822Simplified)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Trim input - only a full match will be accepted
            text = text.Trim();

            var r = new Regex(emailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var m = r.Match(text);

            return m.Success && m.Length == text.Length;
        }

        /// <summary>
        /// Parses the input string and fetches emails from it (tested with outlook, gmail)...
        /// </summary>
        /// <param name="text"></param>
        /// <param name="emailPattern"></param>
        /// <returns></returns>
        public static List<string> FetchEmailsFromText(this string text, string emailPattern = RegexPatterns.EmailPatternRfc2822Simplified)
        {
            var emails = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
                return emails;

            var r = new Regex(emailPattern, RegexOptions.Compiled);
            var mc = r.Matches(text);

            foreach (Match m in mc)
            {
                if (m.Success)
                    emails.Add(m.Value);
            }

            return emails;
        }

        /// <summary>
        /// Repeats the given string X times, and appends a delimiter between the repeated instances.
        /// </summary>
        /// <param name="text">Text to repeat</param>
        /// <param name="delimiter">Delimiter to use</param>
        /// <param name="no">Number of repeats</param>
        /// <returns></returns>
        public static string Repeat(this string text, string delimiter, int no)
        {
            if (text == null)
                return null;

            var result = new StringBuilder();

            for (int i = 0; i < no; i++)
            {
                if (i > 0) result.Append(delimiter);
                result.Append(text);
            }

            return result.ToString();
        }

        /// <summary>
        /// Adds a suffix to the end of the string, if it does not end so.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string AddSuffixIfMissing(this string text, string suffix)
        {
            if (text == null)
                return null;

            if (text.EndsWith(suffix))
                return text;

            return text + suffix;
        }

        /// <summary>
        /// Truncates the string to the desired length.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string MaxLength(this string text, int size)
        {
            if (text == null)
                return null;

            if (text.Length > size)
                return text.Substring(0, size);

            return text;
        }

        /// <summary>
        /// Splits a text.
        /// </summary>
        /// <param name="text">Text to split. It accepts empty or even NULL value.</param>
        /// <param name="delimiters">Delimiters used to split, if ommitted ; will be used.</param>
        /// <returns>When no text was defined string[0] is the result, otherwise the array with the splitted text is returned.</returns>
        public static string[] SafeSplit(this string text, params string[] delimiters)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new string[0];

            // note: set default delimiter
            if (delimiters == null || delimiters.Length == 0)
                delimiters = new[] { ";" };

            return text
                .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();
        }

        /// <summary>
        /// Joins multiple strings, with options to join or ignore empty ones.
        /// </summary>
        /// <param name="textParts"></param>
        /// <param name="delimiter"></param>
        /// <param name="ignoreNullOrWhitespace"></param>
        /// <returns></returns>
        public static string Join(this IEnumerable<string> textParts, string delimiter, bool ignoreNullOrWhitespace = true)
        {
            return string.Join(
                delimiter,
                textParts.Where(s => ignoreNullOrWhitespace == false || !string.IsNullOrWhiteSpace(s))
                );
        }

        public static string Mask(this string unmaskedString, int keepLeft = 3, int keepRight = 0, char mask = '*')
        {
            unmaskedString = unmaskedString ?? "";

            var len = unmaskedString.Length;
            var left = Math.Min(keepLeft, len);
            var middle = Math.Max(len - keepLeft - keepRight, 0);
            var right = len - left - middle;

            var result =
                unmaskedString.Substring(0, left) +
                (middle > 0 ? new string(mask, middle) : "") +
                (right > 0 ? unmaskedString.Substring(left + middle, right) : "");

            return result;
        }

        /// <summary>
        /// Count the occurence of pattern inside the given string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="patternToSearch"></param>
        /// <param name="countIndexesNotFullMatches">
        /// If we search the occurence of "abab" in "ababababab" we may get 2 differrent result.
        /// a) the "abab" is 2 times found entirely inside the given text (countIndexesNotFullMatches is FALSE)
        /// b) or the "abab" is found at 4 multiple indexes in the original text (countIndexesNotFullMatches is TRUE)
        /// </param>
        /// <returns></returns>
        public static int CountOccurrencesOf(this string text, string patternToSearch, bool countIndexesNotFullMatches = false)
        {
            if (text == null)
                return 0;

            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;

            while ((i = text.IndexOf(patternToSearch, i)) != -1)
            {
                if (countIndexesNotFullMatches)
                {
                    i += 1;
                }
                else
                {
                    i += patternToSearch.Length;
                }

                count++;
            }

            return count;
        }

        /// <summary>
        /// Accent stripping.
        /// -----------------
        /// NOTE:
        /// This is the MSDN way.
        /// The implementation on http://www.codeproject.com/KB/cs/UnicodeNormalization.aspx?display=Print may contain errors.
        /// </summary>
        /// <param name="stIn"></param>
        /// <returns></returns>
        public static string StripAccents(this string stIn)
        {
            if (stIn == null)
                return null;

            string stFormD = stIn.Normalize(NormalizationForm.FormKD);

            var sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);

                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        /// <summary>
        /// Converts camel case to human readable format (title case)...
        /// </summary>
        /// <param name="camelCaseString"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string camelCaseString)
        {
            return Regex.Replace(Regex.Replace(camelCaseString, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        /// <summary>Converts a <see cref="T:System.String" /> to lower camel case.</summary>
		/// <returns>The converted name.</returns>
		/// <param name="name">The name to be converted with lower camel case.</param>
		public static string ToCamelCase(this string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            if (!char.IsUpper(name[0]))
                return name;

            var stringBuilder = new StringBuilder();

            // NOTE:
            // Searches for the first non-upper case character (excet at index = 0) and makes every char lower-case to that point.
            // This is the code from System.Web.OData.Buidlder::LowerCamelCase, but similar code exists in Newtonsoft.Json.Utilities.StringUtils::ToCamelCase
            for (int i = 0; i < name.Length; i++)
            {
                if (i != 0 && i + 1 < name.Length && !char.IsUpper(name[i + 1]))
                {
                    stringBuilder.Append(name.Substring(i));
                    break;
                }

                stringBuilder.Append(char.ToLower(name[i], CultureInfo.InvariantCulture));
            }

            return stringBuilder.ToString();
        }
    }
}
