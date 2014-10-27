using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Localization.Services {
    internal static class PersianDateTimeFormatInfo {
        internal static DateTimeFormatInfo Build(DateTimeFormatInfo original) {
            var persianFormats = (DateTimeFormatInfo)original.Clone();

            var persianCalendarMonthNames = new[] {
                "فررودين",
                "ارديبهشت",
                "خرداد",
                "تير",
                "مرداد",
                "شهريور",
                "مهر",
                "آبان",
                "آذر",
                "دي",
                "بهمن",
                "اسفند",
                "" // 13 months names always necessary...
            };

            persianFormats.MonthNames =
                persianFormats.AbbreviatedMonthNames =
                persianFormats.MonthGenitiveNames =
                persianFormats.AbbreviatedMonthGenitiveNames =
                persianCalendarMonthNames;

            persianFormats.SetAllDateTimePatterns(new[] {
                "yyyy/MM/dd",
                "yy/MM/dd",
                "yyyy/M/d",
                "yy/M/d"
            }, 'd');

            persianFormats.SetAllDateTimePatterns(new[] {
                "dddd، d MMMM yyyy",
                "d MMMM yyyy"
            }, 'D');

            persianFormats.SetAllDateTimePatterns(new[] {
                "MMMM yyyy",
                "MMMM yy"
            }, 'y');

            persianFormats.SetAllDateTimePatterns(new[] {
                "HH:mm",
                "H:mm",
                "hh:mm tt",
                "h:mm tt"
            }, 't');

            persianFormats.SetAllDateTimePatterns(new[] {
                "HH:mm:ss",
                "H:mm:ss",
                "hh:mm:ss tt",
                "h:mm:ss tt"
            }, 'T');

            return persianFormats;
        }
    }
}
