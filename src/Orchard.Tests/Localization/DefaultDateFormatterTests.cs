using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Orchard.Localization.Models;
using Orchard.Localization.Services;

namespace Orchard.Tests.Localization {

    [TestFixture()]
	[Category("longrunning")]
    public class DefaultDateFormatterTests {

        [SetUp]
        public void Init() {
            Regex.CacheSize = 1024;
        }

        [Test]
        [Description("Date/time parsing works correctly for all combinations of months, format strings and cultures.")]
        public void ParseDateTimeTest01() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                var container = TestHelpers.InitializeContainer(culture.Name, "GregorianCalendar", TimeZoneInfo.Utc);
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();

                foreach (var dateTimeFormat in formats.AllDateTimeFormats) { // All date and time formats supported by the culture.
                    for (var month = 1; month <= 12; month++) { // All months in the year.

                        DateTime dateTime = new DateTime(1998, month, 1, 10, 30, 30, 678, DateTimeKind.Utc);

                        // Print string using Gregorian calendar to avoid calendar conversion.
                        var cultureGregorian = (CultureInfo)culture.Clone();
                        cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();
                        var dateTimeString = dateTime.ToString(dateTimeFormat, cultureGregorian);

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, dateTimeFormat, dateTimeString);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.ParseDateTime(dateTimeString, dateTimeFormat);
                            var expected = GetExpectedDateTimeParts(dateTime, dateTimeFormat, TimeZoneInfo.Utc);
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Parse tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Date/time parsing works correctly for all combinations of hours, format strings and cultures.")]
        public void ParseDateTimeTest02() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                var container = TestHelpers.InitializeContainer(culture.Name, "GregorianCalendar", TimeZoneInfo.Utc);
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();
                var hoursToTest = new[] { 0, 6, 12, 18 };

                // Fix for some cultures on Windows 10 where both designators for some reason
                // are empty strings. A 24-hour time cannot possibly be round-tripped without any
                // way to distinguish AM from PM, so for these cases test only 12-hour time.
                if (culture.DateTimeFormat.AMDesignator == culture.DateTimeFormat.PMDesignator)
                    hoursToTest = new[] { 1, 6, 9, 12 };

                foreach (var dateTimeFormat in formats.AllDateTimeFormats) { // All date and time formats supported by the culture.
                    foreach (var hour in hoursToTest) { // Enough hours to cover all code paths (AM/PM, 12<->00, etc).

                        DateTime dateTime = new DateTime(1998, 1, 1, hour, 30, 30, DateTimeKind.Utc);

                        // Print string using Gregorian calendar to avoid calendar conversion.
                        var cultureGregorian = (CultureInfo)culture.Clone();
                        cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();
                        var dateTimeString = dateTime.ToString(dateTimeFormat, cultureGregorian);

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, dateTimeFormat, dateTimeString);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.ParseDateTime(dateTimeString, dateTimeFormat);
                            var expected = GetExpectedDateTimeParts(dateTime, dateTimeFormat, TimeZoneInfo.Utc);
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Parse tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Date/time parsing works correctly for all combinations of kinds, time zones, format strings and cultures..")]
        public void ParseDateTimeTest03() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                foreach (var timeZone in new[] { TimeZoneInfo.Utc, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"), TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time") }) { // Enough time zones to get good coverage: UTC, local, one negative offset and one positive offset.
                    var container = TestHelpers.InitializeContainer(culture.Name, "GregorianCalendar", timeZone);
                    var formats = container.Resolve<IDateTimeFormatProvider>();
                    var target = container.Resolve<IDateFormatter>();

                    foreach (var dateTimeFormat in formats.AllDateTimeFormats) { // All date and time formats supported by the culture.

                        var kind = DateTimeKind.Unspecified;
                        var offset = timeZone.BaseUtcOffset;
                        if (timeZone == TimeZoneInfo.Utc) {
                            kind = DateTimeKind.Utc;
                        }
                        else if (timeZone == TimeZoneInfo.Local) {
                            kind = DateTimeKind.Local;
                        }

                        DateTime dateTime = new DateTime(1998, 1, 1, 10, 30, 30, kind);
                        var dateTimeOffset = new DateTimeOffset(dateTime, timeZone.BaseUtcOffset);

                        // Print string using Gregorian calendar to avoid calendar conversion.
                        var cultureGregorian = (CultureInfo)culture.Clone();
                        cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();
                        var dateTimeString = dateTimeOffset.ToString(dateTimeFormat, cultureGregorian);

                        // The .NET DateTimeOffset class is buggy. Firstly, it does not preserve the DateTimeKind value of the
                        // DateTime from which it is created, causing it to never format "K" to "Z" for the UTC time zone. Secondly it
                        // does not properly format "K" to an empty string for DateTimeKind.Unspecified. Our implementation
                        // does not contain these bugs. Therefore for these two scenarios we use the DateTime formatting as a 
                        // reference instead.
                        if (kind == DateTimeKind.Utc || (kind == DateTimeKind.Unspecified && dateTimeFormat.Contains('K'))) {
                            dateTimeString = dateTime.ToString(dateTimeFormat, cultureGregorian);
                        }

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, dateTimeFormat, dateTimeString);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.ParseDateTime(dateTimeString, dateTimeFormat);
                            var expected = GetExpectedDateTimeParts(dateTime, dateTimeFormat, timeZone);
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Parse tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Date/time parsing works correctly for all combinations of milliseconds, format strings and cultures..")]
        public void ParseDateTimeTest04() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                foreach (var millisecond in new[] { 0, 10, 500, 990, 999 }) { // Enough values to cover all fraction rounding cases.
                    var container = TestHelpers.InitializeContainer(culture.Name, "GregorianCalendar", TimeZoneInfo.Utc);
                    var formats = container.Resolve<IDateTimeFormatProvider>();
                    var target = container.Resolve<IDateFormatter>();

                    foreach (var dateTimeFormat in formats.AllDateTimeFormats) { // All date and time formats supported by the culture.

                        DateTime dateTime = new DateTime(1998, 1, 1, 10, 30, 30, millisecond, DateTimeKind.Utc);

                        // Print string using Gregorian calendar to avoid calendar conversion.
                        var cultureGregorian = (CultureInfo)culture.Clone();
                        cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();
                        var dateTimeString = dateTime.ToString(dateTimeFormat, cultureGregorian);

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, dateTimeFormat, dateTimeString);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.ParseDateTime(dateTimeString, dateTimeFormat);
                            var expected = GetExpectedDateTimeParts(dateTime, dateTimeFormat, TimeZoneInfo.Utc);
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Parse tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Date/time parsing throws a FormatException for unparsable date/time strings.")]
        [ExpectedException(typeof(FormatException))]
        public void ParseDateTimeTest05() {
            var container = TestHelpers.InitializeContainer("en-US", null, TimeZoneInfo.Utc);
            var target = container.Resolve<IDateFormatter>();
            target.ParseDateTime("BlaBlaBla");
        }

        [Test]
        [Description("Date parsing works correctly for all combinations of months, format strings and cultures.")]
        public void ParseDateTest01() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                var container = TestHelpers.InitializeContainer(culture.Name, "GregorianCalendar", TimeZoneInfo.Utc);
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();

                foreach (var dateFormat in formats.AllDateFormats) { // All date formats supported by the culture.
                    for (var month = 1; month <= 12; month++) { // All months in the year.

                        DateTime date = new DateTime(1998, month, 1);

                        // Print string using Gregorian calendar to avoid calendar conversion.
                        var cultureGregorian = (CultureInfo)culture.Clone();
                        cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();
                        var dateString = date.ToString(dateFormat, cultureGregorian);

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, dateFormat, dateString);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.ParseDate(dateString, dateFormat);
                            var expected = GetExpectedDateParts(date, dateFormat);
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Parse tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Date parsing throws a FormatException for unparsable date strings.")]
        [ExpectedException(typeof(FormatException))]
        public void ParseDateTest02() {
            var container = TestHelpers.InitializeContainer("en-US", null, TimeZoneInfo.Utc);
            var target = container.Resolve<IDateFormatter>();
            target.ParseDate("BlaBlaBla");
        }

        [Test]
        [Description("Time parsing works correctly for all combinations of hours, format strings and cultures.")]
        public void ParseTimeTest01() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                var container = TestHelpers.InitializeContainer(culture.Name, null, TimeZoneInfo.Utc);
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();
                var hoursToTest = Enumerable.Range(0, 23);

                // Fix for some cultures on Windows 10 where both designators for some reason
                // are empty strings. A 24-hour time cannot possibly be round-tripped without any
                // way to distinguish AM from PM, so for these cases test only 12-hour time.
                if (culture.DateTimeFormat.AMDesignator == culture.DateTimeFormat.PMDesignator)
                    hoursToTest = Enumerable.Range(1, 12);

                foreach (var timeFormat in formats.AllTimeFormats) { // All time formats supported by the culture.
                    foreach (var hour in hoursToTest) { // All hours in the day.

                        DateTime time = new DateTime(1998, 1, 1, hour, 30, 30);
                        var timeString = time.ToString(timeFormat, culture);

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, timeFormat, timeString);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.ParseTime(timeString, timeFormat);
                            var expected = GetExpectedTimeParts(time, timeFormat, TimeZoneInfo.Utc);
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Parse tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Time parsing throws a FormatException for unparsable time strings.")]
        [ExpectedException(typeof(FormatException))]
        public void ParseTimeTest02() {
            var container = TestHelpers.InitializeContainer("en-US", null, TimeZoneInfo.Utc);
            var target = container.Resolve<IDateFormatter>();
            target.ParseTime("BlaBlaBla");
        }

        [Test]
        [Description("Date/time formatting works correctly for all combinations of months, format strings and cultures.")]
        public void FormatDateTimeTest01() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                var container = TestHelpers.InitializeContainer(culture.Name, "GregorianCalendar", TimeZoneInfo.Utc);
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();

                foreach (var dateTimeFormat in formats.AllDateTimeFormats) { // All date/time formats supported by the culture.
                    for (var month = 1; month <= 12; month++) { // All months in the year.

                        DateTime dateTime = new DateTime(1998, month, 1, 10, 30, 30, 678);
                        DateTimeParts dateTimeParts = new DateTimeParts(1998, month, 1, 10, 30, 30, 678, DateTimeKind.Unspecified, offset: TimeSpan.Zero);

                        // Print reference string using Gregorian calendar to avoid calendar conversion.
                        var cultureGregorian = (CultureInfo)culture.Clone();
                        cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, dateTimeFormat, dateTimeParts);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.FormatDateTime(dateTimeParts, dateTimeFormat);
                            var expected = dateTime.ToString(dateTimeFormat, cultureGregorian);
                            if (result != expected) {
                                // The .NET date formatting logic contains a bug that causes it to recognize 'd' and 'dd'
                                // as numerical day specifiers even when they are embedded in literals. Our implementation
                                // does not contain this bug. If we encounter an unexpected result and the .NET reference
                                // result contains the genitive month name, replace it with the non-genitive month name
                                // before asserting.
                                var numericalDayPattern = @"(\b|[^d])d{1,2}(\b|[^d])";
                                var containsNumericalDay = Regex.IsMatch(dateTimeFormat, numericalDayPattern);
                                if (containsNumericalDay) {
                                    var monthName = formats.MonthNames[month - 1];
                                    var monthNameGenitive = formats.MonthNamesGenitive[month - 1];
                                    expected = expected.Replace(monthNameGenitive, monthName);
                                }
                            }
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Format tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Date/time formatting works correctly for all combinations of hours, format strings and cultures.")]
        public void FormatDateTimeTest02() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                var container = TestHelpers.InitializeContainer(culture.Name, "GregorianCalendar", TimeZoneInfo.Utc);
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();

                foreach (var dateTimeFormat in formats.AllDateTimeFormats) { // All date/time formats supported by the culture.
                    foreach (var hour in new[] { 0, 6, 12, 18 }) { // Enough hours to cover all code paths (AM/PM, 12<->00, 1/2 digits, etc).

                        DateTime dateTime = new DateTime(1998, 1, 1, hour, 30, 30, 678);
                        DateTimeParts dateTimeParts = new DateTimeParts(1998, 1, 1, hour, 30, 30, 678, DateTimeKind.Unspecified, offset: TimeSpan.Zero);

                        // Print reference string using Gregorian calendar to avoid calendar conversion.
                        var cultureGregorian = (CultureInfo)culture.Clone();
                        cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, dateTimeFormat, dateTimeParts);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.FormatDateTime(dateTimeParts, dateTimeFormat);
                            var expected = dateTime.ToString(dateTimeFormat, cultureGregorian);
                            if (result != expected) {
                                // The .NET date formatting logic contains a bug that causes it to recognize 'd' and 'dd'
                                // as numerical day specifiers even when they are embedded in literals. Our implementation
                                // does not contain this bug. If we encounter an unexpected result and the .NET reference
                                // result contains the genitive month name, replace it with the non-genitive month name
                                // before asserting.
                                var numericalDayPattern = @"(\b|[^d])d{1,2}(\b|[^d])";
                                var containsNumericalDay = Regex.IsMatch(dateTimeFormat, numericalDayPattern);
                                if (containsNumericalDay) {
                                    var monthName = formats.MonthNames[0];
                                    var monthNameGenitive = formats.MonthNamesGenitive[0];
                                    expected = expected.Replace(monthNameGenitive, monthName);
                                }
                            }
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Format tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Date/time formatting works correctly for all combinations of kinds, time zones, format strings and cultures.")]
        public void FormatDateTimeTest03() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                foreach (var timeZone in new[] { TimeZoneInfo.Utc, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"), TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time") }) { // Enough time zones to get good coverage: UTC, local, one negative offset and one positive offset.
                    var container = TestHelpers.InitializeContainer(culture.Name, "GregorianCalendar", timeZone);
                    var formats = container.Resolve<IDateTimeFormatProvider>();
                    var target = container.Resolve<IDateFormatter>();

                    foreach (var dateTimeFormat in formats.AllDateTimeFormats) { // All date/time formats supported by the culture.

                        // Unfortunately because the System.Globalization classes are tightly coupled to the 
                        // configured culture, calendar and time zone of the local machine, it's not possible
                        // to test all scenarios and combinations while still relying on the .NET Framework
                        // date/time formatting logic for reference. Therefore the logic of this test code takes
                        // into account the configured local time zone of the machine when determining values for
                        // DateTimeKind and offset. Less than ideal, but there really is no way around it.

                        var kind = DateTimeKind.Unspecified;
                        var offset = timeZone.BaseUtcOffset;
                        if (timeZone == TimeZoneInfo.Utc) {
                            kind = DateTimeKind.Utc;
                        }
                        else if (timeZone == TimeZoneInfo.Local) {
                            kind = DateTimeKind.Local;
                        }

                        var dateTime = new DateTime(1998, 1, 1, 10, 30, 30, 678, kind);
                        var dateTimeOffset = new DateTimeOffset(dateTime, timeZone.BaseUtcOffset);
                        var dateTimeParts = DateTimeParts.FromDateTime(dateTime, offset);

                        // Print reference string using Gregorian calendar to avoid calendar conversion.
                        var cultureGregorian = (CultureInfo)culture.Clone();
                        cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, dateTimeFormat, dateTimeParts);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.FormatDateTime(dateTimeParts, dateTimeFormat);
                            var expected = dateTimeOffset.ToString(dateTimeFormat, cultureGregorian);

                            // The .NET DateTimeOffset class is buggy. Firstly, it does not preserve the DateTimeKind value of the
                            // DateTime from which it is created, causing it to never format "K" to "Z" for the UTC time zone. Secondly it
                            // does not properly format "K" to an empty string for DateTimeKind.Unspecified. Our implementation
                            // does not contain these bugs. Therefore for these two scenarios we use the DateTime formatting as a 
                            // reference instead.
                            if (kind == DateTimeKind.Utc || (kind == DateTimeKind.Unspecified && dateTimeFormat.Contains('K'))) {
                                expected = dateTime.ToString(dateTimeFormat, cultureGregorian);
                            }

                            if (result != expected) {
                                // The .NET date formatting logic contains a bug that causes it to recognize 'd' and 'dd'
                                // as numerical day specifiers even when they are embedded in literals. Our implementation
                                // does not contain this bug. If we encounter an unexpected result and the .NET reference
                                // result contains the genitive month name, replace it with the non-genitive month name
                                // before asserting.
                                var numericalDayPattern = @"(\b|[^d])d{1,2}(\b|[^d])";
                                var containsNumericalDay = Regex.IsMatch(dateTimeFormat, numericalDayPattern);
                                if (containsNumericalDay) {
                                    var monthName = formats.MonthNames[0];
                                    var monthNameGenitive = formats.MonthNamesGenitive[0];
                                    expected = expected.Replace(monthNameGenitive, monthName);
                                }
                            }

                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Format tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Date/time formatting works correctly for all combinations of milliseconds, format strings and cultures.")]
        public void FormatDateTimeTest04() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                var container = TestHelpers.InitializeContainer(culture.Name, "GregorianCalendar", TimeZoneInfo.Utc);
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();

                foreach (var dateTimeFormat in formats.AllDateTimeFormats) { // All date/time formats supported by the culture.
                    foreach (var millisecond in new[] { 0, 10, 500, 990, 999 }) { // Enough values to cover all fraction rounding cases.

                        DateTime dateTime = new DateTime(1998, 1, 1, 10, 30, 30, millisecond);
                        DateTimeParts dateTimeParts = new DateTimeParts(1998, 1, 1, 10, 30, 30, millisecond, DateTimeKind.Unspecified, offset: TimeSpan.Zero);

                        // Print reference string using Gregorian calendar to avoid calendar conversion.
                        var cultureGregorian = (CultureInfo)culture.Clone();
                        cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, dateTimeFormat, dateTimeParts);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.FormatDateTime(dateTimeParts, dateTimeFormat);
                            var expected = dateTime.ToString(dateTimeFormat, cultureGregorian);
                            if (result != expected) {
                                // The .NET date formatting logic contains a bug that causes it to recognize 'd' and 'dd'
                                // as numerical day specifiers even when they are embedded in literals. Our implementation
                                // does not contain this bug. If we encounter an unexpected result and the .NET reference
                                // result contains the genitive month name, replace it with the non-genitive month name
                                // before asserting.
                                var numericalDayPattern = @"(\b|[^d])d{1,2}(\b|[^d])";
                                var containsNumericalDay = Regex.IsMatch(dateTimeFormat, numericalDayPattern);
                                if (containsNumericalDay) {
                                    var monthName = formats.MonthNames[0];
                                    var monthNameGenitive = formats.MonthNamesGenitive[0];
                                    expected = expected.Replace(monthNameGenitive, monthName);
                                }
                            }
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Format tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Date formatting works correctly for all combinations of months, format strings and cultures.")]
        public void FormatDateTest01() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                var container = TestHelpers.InitializeContainer(culture.Name, "GregorianCalendar", TimeZoneInfo.Utc);
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();

                foreach (var dateFormat in formats.AllDateFormats) { // All date formats supported by the culture.
                    for (var month = 1; month <= 12; month++) { // All months in the year.

                        DateTime date = new DateTime(1998, month, 1);
                        DateParts dateParts = new DateParts(1998, month, 1);

                        // Print reference string using Gregorian calendar to avoid calendar conversion.
                        var cultureGregorian = (CultureInfo)culture.Clone();
                        cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, dateFormat, dateParts);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.FormatDate(dateParts, dateFormat);
                            var expected = date.ToString(dateFormat, cultureGregorian);
                            if (result != expected) {
                                // The .NET date formatting logic contains a bug that causes it to recognize 'd' and 'dd'
                                // as numerical day specifiers even when they are embedded in literals. Our implementation
                                // does not contain this bug. If we encounter an unexpected result and the .NET reference
                                // result contains the genitive month name, replace it with the non-genitive month name
                                // before asserting.
                                var numericalDayPattern = @"(\b|[^d])d{1,2}(\b|[^d])";
                                var containsNumericalDay = Regex.IsMatch(dateFormat, numericalDayPattern);
                                if (containsNumericalDay) {
                                    var monthName = formats.MonthNames[month - 1];
                                    var monthNameGenitive = formats.MonthNamesGenitive[month - 1];
                                    expected = expected.Replace(monthNameGenitive, monthName);
                                }
                            }
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Format tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        [Test]
        [Description("Time formatting works correctly for all combinations of hours, format strings and cultures.")]
        public void FormatTimeTest01() {
            var allCases = new ConcurrentBag<string>();
            var failedCases = new ConcurrentDictionary<string, Exception>();
            var maxFailedCases = 0;

            var options = new ParallelOptions();
            if (Debugger.IsAttached) {
                options.MaxDegreeOfParallelism = 1;
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Parallel.ForEach(allCultures, options, culture => { // All cultures on the machine.
                var container = TestHelpers.InitializeContainer(culture.Name, null, TimeZoneInfo.Utc);
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();

                foreach (var timeFormat in formats.AllTimeFormats) { // All time formats supported by the culture.
                    for (var hour = 0; hour <= 23; hour++) { // All hours in the day.

                        DateTime date = new DateTime(1998, 1, 1, hour, 30, 30, 678);
                        TimeParts timeParts = new TimeParts(hour, 30, 30, 678, DateTimeKind.Unspecified, offset: TimeSpan.Zero);

                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, timeFormat, timeParts);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));

                        try {
                            var result = target.FormatTime(timeParts, timeFormat);
                            var expected = date.ToString(timeFormat, culture);
                            Assert.AreEqual(expected, result);
                        }
                        catch (Exception ex) {
                            failedCases.TryAdd(caseKey, ex);
                        }
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Format tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
        }

        private DateTimeParts GetExpectedDateTimeParts(DateTime dateTime, string format, TimeZoneInfo timeZone) {
            return new DateTimeParts(
                GetExpectedDateParts(dateTime, format),
                GetExpectedTimeParts(dateTime, format, timeZone)
            );
        }

        private DateParts GetExpectedDateParts(DateTime date, string format) {
            var formatWithoutLiterals = Regex.Replace(format, @"(?<!\\)'(.*?)(?<!\\)'|(?<!\\)""(.*?)(?<!\\)""", "");
            return new DateParts(
                formatWithoutLiterals.Contains('y') ? date.Year : 0,
                formatWithoutLiterals.Contains('M') ? date.Month : 0,
                formatWithoutLiterals.Contains('d') ? date.Day : 0
            );
        }

        private TimeParts GetExpectedTimeParts(DateTime time, string format, TimeZoneInfo timeZone) {
            var formatWithoutLiterals = Regex.Replace(format, @"(?<!\\)'(.*?)(?<!\\)'|(?<!\\)""(.*?)(?<!\\)""", "");
            var expectedKind = DateTimeKind.Unspecified;
            if (formatWithoutLiterals.Contains('K') || formatWithoutLiterals.Contains('z')) {
                if (timeZone == TimeZoneInfo.Utc) {
                    expectedKind = DateTimeKind.Utc;
                }
                else if (timeZone == TimeZoneInfo.Local) {
                    expectedKind = DateTimeKind.Local;
                }
            }

            var expectedOffset = TimeSpan.Zero;
            if (formatWithoutLiterals.Contains('K') && expectedKind != DateTimeKind.Unspecified) {
                expectedOffset = timeZone.BaseUtcOffset;
            }
            else if (formatWithoutLiterals.Contains("zzz")) {
                expectedOffset = timeZone.BaseUtcOffset;
            }
            else if (formatWithoutLiterals.Contains('z')) {
                expectedOffset = TimeSpan.FromHours(timeZone.BaseUtcOffset.Hours);
            }

            return new TimeParts(
                formatWithoutLiterals.Contains('H') || format.Contains('h') ? time.Hour : 0,
                formatWithoutLiterals.Contains('m') ? time.Minute : 0,
                formatWithoutLiterals.Contains('s') ? time.Second : 0,
                formatWithoutLiterals.Contains('f') ? time.Millisecond : 0,
                expectedKind,
                expectedOffset
            );
        }
    }
}
