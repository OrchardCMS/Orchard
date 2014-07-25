using System;
using Orchard.Framework.Localization.Models;

namespace Orchard.Localization.Services {

    /// <summary>
    /// Provides conversion and formatting of dates according to the Orchard configured
    /// time zone, culture and calendar (as opposed to the system configured time zone and
    /// culture).
    /// </summary>
    public interface IDateLocalizationServices : IDependency {

        /// <summary>
        /// Converts a non-nullable date from UTC to the Orchard configured time zone.
        /// </summary>
        /// <param name="dateUtc">The non-nullable UTC date to convert. DateTime.MinValue is translated to null.</param>
        /// <returns></returns>
        DateTime? ConvertToSiteTimeZone(DateTime dateUtc);

        /// <summary>
        /// Converts a nullable date from UTC to the Orchard configured time zone.
        /// </summary>
        /// <param name="dateUtc">The nullable UTC date to convert.</param>
        /// <returns></returns>
        DateTime? ConvertToSiteTimeZone(DateTime? dateUtc);

        /// <summary>
        /// Converts a non-nullable date from the Orchard configured time zone to UTC.
        /// </summary>
        /// <param name="dateLocal">The non-nullable local date to convert. DateTime.MinValue is translated to null.</param>
        /// <returns></returns>
        DateTime? ConvertFromSiteTimeZone(DateTime dateLocal);

        /// <summary>
        /// Converts a nullable date from the Orchard configured time zone to UTC.
        /// </summary>
        /// <param name="dateLocal">The nullable local date to convert.</param>
        /// <returns></returns>
        DateTime? ConvertFromSiteTimeZone(DateTime? dateLocal);

        /// <summary>
        /// Converts a date from Gregorian calendar to the Orchard configured calendar.
        /// </summary>
        /// <param name="date">The Gregorian calendar date to convert.</param>
        /// <returns>Null if the supplied date parameter was null. Otherwise a <c>DateTimeParts</c> instance representing the converted date.</returns>
        DateTimeParts? ConvertToSiteCalendar(DateTime? date);

        /// <summary>
        /// Converts a date from the Orchard configured calendar to Gregorian calendar.
        /// </summary>
        /// <param name="parts">A <c>DateTimeParts</c> instance representing the Orchard configured calendar date to convert.</param>
        /// <returns>Null if the supplied parts parameter was null. Otherwise the converted Gregorian calendar date.</returns>
        DateTime? ConvertFromSiteCalendar(DateTimeParts? parts);



        /// <summary>
        /// Converts a non-nullable date from Gregorian calendar UTC to the Orchard configured calendar and time zone and formats it using the default long date and time format string.
        /// </summary>
        /// <param name="date">The non-nullable UTC date to convert. DateTime.MinValue is translated to null.</param>
        /// <param name="nullText">A text to be returned if the supplied UTC date is equal to DateTime.MinValue.</param>
        /// <returns></returns>
        string ConvertToLocalString(DateTime date, string nullText = null);

        /// <summary>
        /// Converts a non-nullable date from Gregorian calendar UTC to the Orchard configured calendar and time zone and formats it using the specified format string using the Orchard configured culture.
        /// </summary>
        /// <param name="date">The non-nullable UTC date to convert. DateTime.MinValue is translated to null.</param>
        /// <param name="format">A standard DateTime format string to use for formating the converted date.</param>
        /// <param name="nullText">A text to be returned if the supplied UTC date is equal to DateTime.MinValue.</param>
        /// <returns></returns>
        string ConvertToLocalString(DateTime date, string format, string nullText = null);

        /// <summary>
        /// Converts a nullable date from Gregorian calendar UTC to the Orchard configured calendar and time zone and formats it using the specified format string using the Orchard configured culture.
        /// </summary>
        /// <param name="date">The nullable UTC date to convert.</param>
        /// <param name="format">A standard DateTime format string to use for formating the converted date.</param>
        /// <param name="nullText">A text to be returned if the supplied UTC date has no value.</param>
        /// <returns></returns>
        string ConvertToLocalString(DateTime? date, string format, string nullText = null);

        /// <summary>
        /// Converts a non-nullable date from Gregorian calendar UTC to the Orchard configured calendar and time zone and formats it as a date-only string using the Orchard configured culture.
        /// </summary>
        /// <param name="date">The non-nullable UTC date to convert. DateTime.MinValue is translated to null.</param>
        /// <param name="nullText">A text to be returned if the supplied UTC date is equal to DateTime.MinValue.</param>
        /// <returns></returns>
        string ConvertToLocalDateString(DateTime date, string nullText = null);

        /// <summary>
        /// Converts a nullable date from Gregorian calendar UTC to the Orchard configured calendar and time zone and formats it as a date-only string using the Orchard configured culture.
        /// </summary>
        /// <param name="date">The nullable UTC date to convert.</param>
        /// <param name="nullText">A text to be returned if the supplied UTC date has no value.</param>
        /// <returns></returns>
        string ConvertToLocalDateString(DateTime? date, string nullText = null);

        /// <summary>
        /// Converts a non-nullable date from Gregorian calendar UTC to the Orchard configured calendar and time zone and formats it as a time-only string using the Orchard configured culture.
        /// </summary>
        /// <param name="date">The non-nullable UTC date to convert. DateTime.MinValue is translated to null.</param>
        /// <param name="nullText">A text to be returned if the supplied UTC date is equal to DateTime.MinValue.</param>
        /// <returns></returns>
        string ConvertToLocalTimeString(DateTime date, string nullText = null);

        /// <summary>
        /// Converts a nullable date from Gregorian calendar UTC to the Orchard configured calendar and time zone and formats it as a time-only string using the Orchard configured culture.
        /// </summary>
        /// <param name="date">The nullable UTC date to convert.</param>
        /// <param name="nullText">A text to be returned if the supplied UTC date has no value.</param>
        /// <returns></returns>
        string ConvertToLocalTimeString(DateTime? date, string nullText = null);

        /// <summary>
        /// Parses a date and time string using the Orchard configured culture and converts it to Gregorian calendar UTC from the Orchard configured calendar and time zone.
        /// </summary>
        /// <param name="dateString">The local date and time string to parse and convert.</param>
        /// <returns></returns>
        DateTime? ConvertFromLocalString(string dateString);

        /// <summary>
        /// Parses separate date and time strings using the Orchard configured culture and converts the resulting combined DateTime to Gregorian calendar UTC from the Orchard configured calendar and time zone.
        /// </summary>
        /// <param name="dateString">The local date string to parse and convert, or null or an empty string to only convert the time component.</param>
        /// <param name="timeString">The local time string to parse and convert, or null or an empty string to only convert the date component.</param>
        /// <returns></returns>
        DateTime? ConvertFromLocalString(string dateString, string timeString);
    }
}
