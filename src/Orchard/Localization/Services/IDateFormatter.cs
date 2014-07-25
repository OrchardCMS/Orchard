using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.Framework.Localization.Models;

namespace Orchard.Framework.Localization.Services {
    public interface IDateFormatter : IDependency {
        DateTimeParts ParseDateTime(string dateTimeString, CultureInfo culture);
        DateParts ParseDate(string dateString, CultureInfo culture);
        TimeParts ParseTime(string timeString, CultureInfo culture);
        string FormatDateTime(DateTimeParts parts, CultureInfo culture);
        string FormatDate(DateParts parts, CultureInfo culture);
        string FormatTime(TimeParts parts, CultureInfo culture);
    }
}
