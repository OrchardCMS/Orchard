using System;
using System.Collections.Generic;
using System.Globalization;

namespace Orchard.Localization.Services {

    /// <summary>
    /// Provides an implementation of IDateTimeFormatProvider which delegates
    /// properties to the already localized strings made available by the CultureInfo
    /// object for the currently configured culture. This removes the need to have
    /// these strings translated.
    /// </summary>
    public class CultureDateTimeFormatProvider : IDateTimeFormatProvider {

        private readonly IOrchardServices _orchardServices;

        public CultureDateTimeFormatProvider(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public IEnumerable<string> MonthNames {
            get {
                return CurrentCulture.DateTimeFormat.MonthNames;
            }
        }

        public IEnumerable<string> MonthNamesShort {
            get {
                return CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
            }
        }

        public IEnumerable<string> DayNames {
            get {
                return CurrentCulture.DateTimeFormat.DayNames;
            }
        }

        public IEnumerable<string> DayNamesShort {
            get {
                return CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
            }
        }

        public IEnumerable<string> DayNamesMin {
            get {
                return CurrentCulture.DateTimeFormat.ShortestDayNames;
            }
        }

        public string ShortDateFormat {
            get {
                return CurrentCulture.DateTimeFormat.ShortDatePattern;
            }
        }

        public string ShortTimeFormat {
            get {
                return CurrentCulture.DateTimeFormat.ShortTimePattern;
            }
        }

        public string ShortDateTimeFormat {
            get {
                return String.Format("{0} {1}", ShortDateFormat, ShortTimeFormat);
            }
        }

        public string LongDateFormat {
            get {
                return CurrentCulture.DateTimeFormat.LongDatePattern;
            }
        }

        public string LongTimeFormat {
            get {
                return CurrentCulture.DateTimeFormat.LongTimePattern;
            }
        }

        public string LongDateTimeFormat {
            get {
                return String.Format("{0} {1}", LongDateFormat, LongTimeFormat);
            }
        }

        public int FirstDay {
            get {
                return Convert.ToInt32(CurrentCulture.DateTimeFormat.FirstDayOfWeek);
            }
        }

        public bool Use24HourTime {
            get {
                if (ShortTimeFormat.Contains("H")) // Capital H is the format specifier for the hour using a 24-hour clock.
                    return true;
                return false;
            }
        }

        public string TimeSeparator {
            get {
                return CurrentCulture.DateTimeFormat.TimeSeparator;
            }
        }

        public string AmPmPrefix {
            get {
                return " "; // No way to get this from CultureInfo unfortunately, so assume a single space.
            }
        }

        public IEnumerable<string> AmPmDesignators {
            get {
                return new string[] { CurrentCulture.DateTimeFormat.AMDesignator, CurrentCulture.DateTimeFormat.PMDesignator };
            }
        }

        private CultureInfo CurrentCulture {
            get {
                return CultureInfo.GetCultureInfo(_orchardServices.WorkContext.CurrentCulture);
            }
        }
    }
}