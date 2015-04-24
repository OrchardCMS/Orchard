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
                "فررودین",
                "اردیبهشت",
                "خرداد",
                "تیر",
                "مرداد",
                "شهریور",
                "مهر",
                "آبان",
                "آذر",
                "دی",
                "بهمن",
                "اسفند",
                "" // 13 months names always necessary...
            };

            persianFormats.MonthNames =
                persianFormats.AbbreviatedMonthNames =
                persianFormats.MonthGenitiveNames =
                persianFormats.AbbreviatedMonthGenitiveNames =
                persianCalendarMonthNames;

            var persianDayNames = new[] {
                "یکشنبه", // Changes the Arabic "ي" and "ك" to the Farsi "ی" and "ک" respectively (incorrect in .NET Framework).
                "دوشنبه",
                "سه شنبه",
                "چهارشنبه",
                "پنجشنبه",
                "جمعه",
                "شنبه"
            };

            persianFormats.DayNames =
                persianFormats.AbbreviatedDayNames =
                persianDayNames;

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
