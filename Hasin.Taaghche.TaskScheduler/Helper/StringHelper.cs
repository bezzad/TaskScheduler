using System;
using System.Text.RegularExpressions;

namespace Hasin.Taaghche.TaskScheduler.Helper
{
    public static class StringHelper
    {
        private static readonly Regex EnglishPattern = new Regex("[a-zA-Z]+", RegexOptions.Compiled);
        private static readonly Regex LowerCaseConvention = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

        public static string ToLowerCaseNamingConvention(this string s, bool toLowerCase = false)
        {
            return toLowerCase
                ? LowerCaseConvention.Replace(s, " ").ToLower()
                : LowerCaseConvention.Replace(s, " ");
        }
        public static bool HasEnglishChar(this string s)
        {
            return !string.IsNullOrEmpty(s) && EnglishPattern.IsMatch(s);
        }
        public static string EncodeToBase64(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string DecodeFromBase64(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string[] SplitUp(this string text)
        {
            return text.Split(new[] {",", ";", " "}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string CleanText(this string text)
        {
            return text.Replace("`", "").Replace(">", "").Replace("_", "").Replace("*", " ");
        }
    }
}