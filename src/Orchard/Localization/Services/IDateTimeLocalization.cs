using System;
using Orchard.Localization;

namespace Orchard.Localization.Services {

    /// <summary>
    /// Provides a set of localizable strings which in turn control localization of dates and
    /// times in Orchard. These strings can be changed for other cultures using the normal
    /// string localization process.
    /// </summary>
    public interface IDateTimeLocalization : IDependency {

        /// <summary>
        /// Returns a comma-separated list of month names.
        /// </summary>
        LocalizedString MonthNames {
            get;
        }

        /// <summary>
        /// Returns a comma-separated list of abbreviated month names.
        /// </summary>
        LocalizedString MonthNamesShort {
            get;
        }

        /// <summary>
        /// Returns a comma-separated list of weekday names.
        /// </summary>
        LocalizedString DayNames {
            get;
        }

        /// <summary>
        /// Returns a comma-separated list of abbreviated weekday names.
        /// </summary>
        LocalizedString DayNamesShort {
            get;
        }

        /// <summary>
        /// Returns a comma-separated list of maximally abbreviated weekday names.
        /// </summary>
        LocalizedString DayNamesMin {
            get;
        }

        /// <summary>
        /// Returns a standard or custom DateTime format string used to format dates for short date display.
        /// </summary>
        LocalizedString ShortDateFormat {
            get;
        }

        /// <summary>
        /// Returns a standard or custom DateTime format string used to format dates for short time display.
        /// </summary>
        LocalizedString ShortTimeFormat {
            get;
        }

        /// <summary>
        /// Returns a standard or custom DateTime format string used to format dates for general (short) date and time display.
        /// </summary>
        LocalizedString ShortDateTimeFormat {
            get;
        }

        /// <summary>
        /// Returns a standard or custom DateTime format string used to format dates for full date and time display.
        /// </summary>
        LocalizedString LongDateTimeFormat {
            get;
        }

        /// <summary>
        /// Returns an integer representing the first day of the week, where 0 is Sunday, 1 is Monday etc.
        /// </summary>
        int FirstDay {
            get;
        }

        /// <summary>
        /// Returns true if the year select precedes month, false for month then year.
        /// </summary>
        bool ShowMonthAfterYear {
            get;
        }

        /// <summary>
        /// Returns an additional text to append to the year in the month headers.
        /// </summary>
        string YearSuffix {
            get;
        }
    }
}
