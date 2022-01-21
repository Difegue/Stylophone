using MpcNET.Types;
using SkiaSharp;
using System;
using System.IO;

namespace Stylophone.Common.Helpers
{
    public static class Miscellaneous
    {
        /// <summary>
        ///     Formats a timespan into a human
        ///     readable form.
        /// </summary>
        /// <param name="inputMilliseconds">Time to format</param>
        /// <returns>Formatted time string</returns>
        public static string FormatTimeString(double inputMilliseconds)
        {
            // Convert the milliseconds into a usable timespan
            var timeSpan = TimeSpan.FromMilliseconds(inputMilliseconds);

            // Check if the length is less than one minute
            if (timeSpan.TotalMinutes < 1.0)
                return string.Format("{0:D2}:{1:D2}", 0, timeSpan.Seconds);

            return timeSpan.TotalHours < 1.0
                ? string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds)
                : string.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }

        public static bool Contains(this string str, string value, StringComparison comparison)
        {
            return str.IndexOf(value, comparison) >= 0;
        }

        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        /// <summary>
        /// Get a unique identifier for a given MPD file.
        /// </summary>
        /// <param name="f">The MPDFile</param>
        /// <returns></returns>
        public static string GetFileIdentifier(IMpdFile f)
        {
            // This allows to cache per album, avoiding saving the same album art a ton of times.
            // Doesn't work if files in an album have different albumarts, but that happens so rarely it's fine to ignore it.
            var uniqueIdentifier = f.HasAlbum ? f.Album : f.HasTitle ? f.Title : f.Path;

            return EscapeFilename(uniqueIdentifier);
        }

        /// <summary>
        /// Escapes an object name so that it is a valid filename.
        /// </summary>
        /// <param name="fileName">Original object name.</param>
        /// <returns>Escaped name.</returns>
        /// <remarks>
        /// All characters that are not valid for a filename, plus "%" and ".", are converted into "uuuu", where uuuu is the hexadecimal
        /// unicode representation of the character.
        /// </remarks>
        public static string EscapeFilename(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // Replace "%", then replace all other characters, then replace "."

            fileName = fileName.Replace("%", "0025");
            foreach (char invalidChar in invalidChars)
            {
                fileName = fileName.Replace(invalidChar.ToString(), string.Format("{0,4:X}", Convert.ToInt16(invalidChar)).Replace(' ', '0'));
            }
            return fileName.Replace(".", "002E");
        }
    }
}
