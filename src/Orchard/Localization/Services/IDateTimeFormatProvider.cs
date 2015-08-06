using System;
using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Localization.Services {

    /// <summary>
    /// Provides a set of properties which control the formatting of dates and times in Orchard.
    /// </summary>
    public interface IDateTimeFormatProvider : IDependency {

        /// <summary>
        /// Gets a list of month names.
        /// </summary>
        string[] MonthNames {
            get;
        }

        /// <summary>
        /// Gets a list of genitive month names (used in contexts when a day is involved).
        /// </summary>
        string[] MonthNamesGenitive {
            get;
        }

        /// <summary>
        /// Gets a list of abbreviated month names.
        /// </summary>
        string[] MonthNamesShort {
            get;
        }

        /// <summary>
        /// Gets a list of abbreviated genivite month names (used in contexts when a day is involved).
        /// </summary>
        string[] MonthNamesShortGenitive {
            get;
        }

        /// <summary>
        /// Gets a list of weekday names.
        /// </summary>
        string[] DayNames {
            get;
        }

        /// <summary>
        /// Gets a list of abbreviated weekday names.
        /// </summary>
        string[] DayNamesShort {
            get;
        }

        /// <summary>
        /// Gets a list of maximally abbreviated weekday names.
        /// </summary>
        string[] DayNamesMin {
            get;
        }

        /// <summary>
        /// Gets a custom DateTime format string used to format dates for short date display.
        /// </summary>
        string ShortDateFormat {
            get;
        }

        /// <summary>
        /// Gets a custom DateTime format string used to format dates for short time display.
        /// </summary>
        string ShortTimeFormat {
            get;
        }

        /// <summary>
        /// Gets a custom DateTime format string used to format dates for general (short) date and time display.
        /// </summary>
        string ShortDateTimeFormat {
            get;
        }

        /// <summary>
        /// Gets a custom DateTime format string used to format dates for long date display.
        /// </summary>
        string LongDateFormat {
            get;
        }

        /// <summary>
        /// Gets a custom DateTime format string used to format dates for long time display.
        /// </summary>
        string LongTimeFormat {
            get;
        }

        /// <summary>
        /// Gets a custom DateTime format string used to format dates for full date and time display.
        /// </summary>
        string LongDateTimeFormat {
            get;
        }

        /// <summary>
        /// Gets the full list of custom DateTime format strings supported to format dates for date display.
        /// </summary>
        IEnumerable<string> AllDateFormats {
            get;
        }

        /// <summary>
        /// Gets the full list of custom DateTime format strings supported to format dates for time display.
        /// </summary>
        IEnumerable<string> AllTimeFormats {
            get;
        }

        /// <summary>
        /// Gets the full list of custom DateTime format strings supported to format dates for date and time display.
        /// </summary>
        IEnumerable<string> AllDateTimeFormats {
            get;
        }

        /// <summary>
        /// Gets an integer representing the first day of the week, where 0 is Sunday, 1 is Monday etc.
        /// </summary>
        int FirstDay {
            get;
        }

        /// <summary>
        /// Gets a boolean indicating whether 24-hour time is used as opposed to 12-hour time.
        /// </summary>
        bool Use24HourTime {
            get;
        }

        /// <summary>
        /// Gets the string that separates the components of date, that is, the year, month and day.
        /// </summary>
        string DateSeparator {
            get;
        }

        /// <summary>
        /// Gets the string that separates the components of time, that is, the hour, minute, and second.
        /// </summary>
        string TimeSeparator {
            get;
        }

        /// <summary>
        /// Gets the string that separates the time from the AM and PM designators.
        /// </summary>
        string AmPmPrefix {
            get;
        }

        /// <summary>
        /// Gets a list of strings used as display text for the AM and PM designators.
        /// </summary>
        string[] AmPmDesignators {
            get;
        }

        /// <summary>
        /// Returns a string containing the name of the specified era.
        /// </summary>
        string GetEraName(int era);

        /// <summary>
        /// Returns a string containing the abbreviated name of the specified era, if an abbreviation exists.
        /// </summary>
        string GetShortEraName(int era);

        /// <summary>
        /// Returns the integer representing the specified era.
        /// </summary>
        int GetEra(string eraName);
    }
}
