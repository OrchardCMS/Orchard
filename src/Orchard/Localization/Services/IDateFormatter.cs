using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.Framework.Localization.Models;

namespace Orchard.Framework.Localization.Services {
    public interface IDateFormatter : IDependency {
        DateTimeParts ParseDateTime(string dateTimeString);
        DateTimeParts ParseDateTime(string dateTimeString, string format);
        DateParts ParseDate(string dateString);
        DateParts ParseDate(string dateString, string format);
        TimeParts ParseTime(string timeString);
        TimeParts ParseTime(string timeString, string format);
        string FormatDateTime(DateTimeParts parts);
        string FormatDateTime(DateTimeParts parts, string format);
        string FormatDate(DateParts parts);
        string FormatDate(DateParts parts, string format);
        string FormatTime(TimeParts parts);
        string FormatTime(TimeParts parts, string format);
    }
}
