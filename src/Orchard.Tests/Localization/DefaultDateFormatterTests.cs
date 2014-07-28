using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Framework.Localization.Models;
using Orchard.Framework.Localization.Services;
using Orchard.Localization.Services;

namespace Orchard.Framework.Tests.Localization {

    [TestFixture]
    public class DefaultDateFormatterTests {

        [Test]
        [Description("Date and time parsing works correctly for all combinations of months, hours, format strings and cultures.")]
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
                var container = InitializeContainer(culture.Name, "GregorianCalendar");
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();
                foreach (var dateTimeFormat in formats.AllDateTimeFormats) { // All date and time formats supported by the culture.
                    var caseKey = String.Format("{0}:{1}", culture.Name, dateTimeFormat);
                    allCases.Add(caseKey);
                    Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));
                    try {
                        for (var month = 1; month <= 12; month++) { // All months in the year.
                            for (var hour = 0; hour <= 23; hour++) { // All hours in the day.
                                DateTime dateTime = new DateTime(1998, month, 1, hour, 30, 30);
                                // Print string using Gregorian calendar to avoid calendar conversion.
                                var cultureGregorian = (CultureInfo)culture.Clone();
                                cultureGregorian.DateTimeFormat.Calendar = cultureGregorian.OptionalCalendars.OfType<GregorianCalendar>().First();
                                var dateTimeString = dateTime.ToString(dateTimeFormat, cultureGregorian);
                                var result = target.ParseDateTime(dateTimeString, dateTimeFormat);
                                var reference = DateTime.ParseExact(dateTimeString, dateTimeFormat, culture);
                                var expected = new DateTimeParts(reference.Year, reference.Month, reference.Day, reference.Hour, reference.Minute, reference.Second, reference.Millisecond);
                                Assert.AreEqual(expected, result);
                            }
                        }
                    }
                    catch (Exception ex) {
                        failedCases.TryAdd(caseKey, ex);
                    }
                }
            });

            if (failedCases.Count > maxFailedCases) {
                throw new AggregateException(String.Format("Parse tests failed for {0} of {1} cases. Expected {2} failed cases or less.", failedCases.Count, allCases.Count, maxFailedCases), failedCases.Values);
            }
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
                var container = InitializeContainer(culture.Name, "GregorianCalendar");
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
                var container = InitializeContainer(culture.Name, null);
                var formats = container.Resolve<IDateTimeFormatProvider>();
                var target = container.Resolve<IDateFormatter>();
                foreach (var timeFormat in formats.AllTimeFormats) { // All time formats supported by the culture.
                    for (var hour = 0; hour <= 23; hour++) { // All hours in the day.
                        DateTime time = new DateTime(1998, 1, 1, hour, 30, 30);
                        var timeString = time.ToString(timeFormat, culture);
                        var caseKey = String.Format("{0}___{1}___{2}", culture.Name, timeFormat, timeString);
                        allCases.Add(caseKey);
                        //Debug.WriteLine(String.Format("{0} cases tested so far. Testing case {1}...", allCases.Count, caseKey));
                        try {
                            var result = target.ParseTime(timeString, timeFormat);
                            var expected = GetExpectedTimeParts(time, timeFormat);
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

        private DateParts GetExpectedDateParts(DateTime date, string format) {
            return new DateParts(
                format.Contains('y') ? date.Year : 0,
                format.Contains('M') ? date.Month : 0,
                format.Contains('d') ? date.Day : 0
            );
        }

        private TimeParts GetExpectedTimeParts(DateTime time, string format) {
            return new TimeParts(
                format.Contains('H') || format.Contains('h') ? time.Hour : 0,
                format.Contains('m') ? time.Minute : 0,
                format.Contains('s') ? time.Second : 0,
                format.Contains('f') ? time.Millisecond : 0
            );
        }

        private IContainer InitializeContainer(string cultureName, string calendarName) {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<WorkContext>(new StubWorkContext(cultureName, calendarName));
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<CultureDateTimeFormatProvider>().As<IDateTimeFormatProvider>();
            builder.RegisterType<DefaultDateFormatter>().As<IDateFormatter>();
            builder.RegisterInstance(new Mock<ICalendarSelector>().Object);
            builder.RegisterType<DefaultCalendarManager>().As<ICalendarManager>();
            return builder.Build();
        }

        private class StubWorkContext : WorkContext {

            private string _cultureName;
            private string _calendarName;

            public StubWorkContext(string cultureName, string calendarName) {
                _cultureName = cultureName;
                _calendarName = calendarName;
            }

            public override T Resolve<T>() {
                throw new NotImplementedException();
            }

            public override bool TryResolve<T>(out T service) {
                throw new NotImplementedException();
            }

            public override T GetState<T>(string name) {
                if (name == "CurrentCulture") return (T)((object)_cultureName);
                if (name == "CurrentCalendar") return (T)((object)_calendarName);
                throw new NotImplementedException(String.Format("Property '{0}' is not implemented.", name));
            }

            public override void SetState<T>(string name, T value) {
                throw new NotImplementedException();
            }
        }

        private class StubWorkContextAccessor : IWorkContextAccessor {

            private WorkContext _workContext;

            public StubWorkContextAccessor(WorkContext workContext) {
                _workContext = workContext;
            }

            public WorkContext GetContext(System.Web.HttpContextBase httpContext) {
                throw new NotImplementedException();
            }

            public IWorkContextScope CreateWorkContextScope(System.Web.HttpContextBase httpContext) {
                throw new NotImplementedException();
            }

            public WorkContext GetContext() {
                return _workContext;
            }

            public IWorkContextScope CreateWorkContextScope() {
                throw new NotImplementedException();
            }
        }
    }
}
