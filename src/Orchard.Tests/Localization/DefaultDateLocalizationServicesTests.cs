using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    [TestFixture]
    public class DefaultDateLocalizationServicesTests {

        [SetUp]
        public void Init() {
            //Regex.CacheSize = 1024;
        }

        [Test]
        [Description("Date component is decremented by one day when converting to time zone with negative offset greater than time component.")]
        public void ConvertToSiteTimeZoneTest01() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            Assert.That(timeZone.BaseUtcOffset, Is.LessThan(TimeSpan.FromHours(-3)));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeUtc = new DateTime(1998, 1, 15, 3, 0, 0, DateTimeKind.Utc);
            var result = target.ConvertToSiteTimeZone(dateTimeUtc);
            Assert.AreEqual(14, result.Day);
        }

        [Test]
        [Description("Date component is incremented by one day when converting to time zone with positive offset greater than 24 hours minus time component.")]
        public void ConvertToSiteTimeZoneTest02() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            Assert.That(timeZone.BaseUtcOffset, Is.GreaterThan(TimeSpan.FromHours(3)));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeUtc = new DateTime(1998, 1, 15, 21, 0, 0, DateTimeKind.Utc);
            var result = target.ConvertToSiteTimeZone(dateTimeUtc);
            Assert.AreEqual(16, result.Day);
        }

        [Test]
        [Description("DateTime which is DateTimeKind.Utc is converted to DateTimeKind.Local with offset when target time zone is not UTC.")]
        public void ConvertToSiteTimeZoneTest03() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            Assert.That(timeZone.BaseUtcOffset, Is.Not.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeUtc = new DateTime(1998, 1, 15, 3, 0, 0, DateTimeKind.Utc);
            var result = target.ConvertToSiteTimeZone(dateTimeUtc);
            Assert.AreEqual(DateTimeKind.Local, result.Kind);
            Assert.AreEqual(dateTimeUtc.Hour + timeZone.BaseUtcOffset.Hours, result.Hour);
        }

        [Test]
        [Description("DateTime which is DateTimeKind.Utc is not converted when target time zone is UTC.")]
        public void ConvertToSiteTimeZoneTest04() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
            Assert.That(timeZone.BaseUtcOffset, Is.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeUtc = new DateTime(1998, 1, 15, 3, 0, 0, DateTimeKind.Utc);
            var result = target.ConvertToSiteTimeZone(dateTimeUtc);
            Assert.AreEqual(DateTimeKind.Utc, result.Kind);
            Assert.AreEqual(dateTimeUtc, result);
        }

        [Test]
        [Description("DateTime which is DateTimeKind.Unspecified is converted to DateTimeKind.Local with offset when target time zone is not UTC.")]
        public void ConvertToSiteTimeZoneTest05() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            Assert.That(timeZone.BaseUtcOffset, Is.Not.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeUtc = new DateTime(1998, 1, 15, 3, 0, 0, DateTimeKind.Unspecified);
            var result = target.ConvertToSiteTimeZone(dateTimeUtc);
            Assert.AreEqual(DateTimeKind.Local, result.Kind);
            Assert.AreEqual(dateTimeUtc.Hour + timeZone.BaseUtcOffset.Hours, result.Hour);
        }

        [Test]
        [Description("DateTime which is DateTimeKind.Unspecified is converted to DateTimeKind.Utc with no offset when target time zone is UTC.")]
        public void ConvertToSiteTimeZoneTest06() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
            Assert.That(timeZone.BaseUtcOffset, Is.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeUtc = new DateTime(1998, 1, 15, 3, 0, 0, DateTimeKind.Unspecified);
            var result = target.ConvertToSiteTimeZone(dateTimeUtc);
            Assert.AreEqual(DateTimeKind.Utc, result.Kind);
            Assert.AreEqual(dateTimeUtc, result);
        }

        [Test]
        [Description("DateTime which is already DateTimeKind.Local is never converted.")]
        public void ConvertToSiteTimeZoneTest07() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            Assert.That(timeZone.BaseUtcOffset, Is.Not.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeUtc = new DateTime(1998, 1, 15, 3, 0, 0, DateTimeKind.Local);
            var result = target.ConvertToSiteTimeZone(dateTimeUtc);
            Assert.AreEqual(DateTimeKind.Local, result.Kind);
            Assert.AreEqual(dateTimeUtc, result);
        }

        [Test]
        [Description("Resulting DateTime is DateTimeKind.Local even when target time zone is not configured time zone of local computer.")]
        public void ConvertToSiteTimeZoneTest08() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            if (timeZone == TimeZoneInfo.Local) {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }
            Assert.That(timeZone.BaseUtcOffset, Is.Not.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeUtc = new DateTime(1998, 1, 15, 3, 0, 0, DateTimeKind.Utc);
            var result = target.ConvertToSiteTimeZone(dateTimeUtc);
            Assert.AreEqual(DateTimeKind.Local, result.Kind);
        }

        [Test]
        [Description("Date component is incremented by one day when converting from time zone with negative offset greater than 24 hours minus time component.")]
        public void ConvertFromSiteTimeZoneTest01() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            Assert.That(timeZone.BaseUtcOffset, Is.LessThan(TimeSpan.FromHours(-3)));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeLocal = new DateTime(1998, 1, 15, 21, 0, 0, DateTimeKind.Local);
            var result = target.ConvertFromSiteTimeZone(dateTimeLocal);
            Assert.AreEqual(16, result.Day);
        }

        [Test]
        [Description("Date component is decremented by one day when converting from time zone with positive offset greater than time component.")]
        public void ConvertFromSiteTimeZoneTest02() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            Assert.That(timeZone.BaseUtcOffset, Is.GreaterThan(TimeSpan.FromHours(3)));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeLocal = new DateTime(1998, 1, 15, 3, 0, 0, DateTimeKind.Local);
            var result = target.ConvertFromSiteTimeZone(dateTimeLocal);
            Assert.AreEqual(14, result.Day);
        }

        [Test]
        [Description("DateTime which is DateTimeKind.Local is converted to DateTimeKind.Utc with offset when target time zone is not UTC.")]
        public void ConvertFromSiteTimeZoneTest03() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            Assert.That(timeZone.BaseUtcOffset, Is.Not.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeLocal = new DateTime(1998, 1, 15, 21, 0, 0, DateTimeKind.Local);
            var result = target.ConvertFromSiteTimeZone(dateTimeLocal);
            Assert.AreEqual(DateTimeKind.Utc, result.Kind);
            Assert.AreEqual(dateTimeLocal.Hour - timeZone.BaseUtcOffset.Hours, result.Hour);
        }

        [Test]
        [Description("DateTime which is DateTimeKind.Local is converted to DateTimeKind.Utc with no offset when target time zone is UTC.")]
        public void ConvertFromSiteTimeZoneTest04() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
            Assert.That(timeZone.BaseUtcOffset, Is.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeLocal = new DateTime(1998, 1, 15, 21, 0, 0, DateTimeKind.Local);
            var result = target.ConvertFromSiteTimeZone(dateTimeLocal);
            Assert.AreEqual(DateTimeKind.Utc, result.Kind);
            Assert.AreEqual(dateTimeLocal.Hour, result.Hour);
            Assert.AreEqual(dateTimeLocal.Minute, result.Minute);
        }

        [Test]
        [Description("DateTime which is DateTimeKind.Unspecified is converted to DateTimeKind.Utc with offset when target time zone is not UTC.")]
        public void ConvertFromSiteTimeZoneTest05() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            Assert.That(timeZone.BaseUtcOffset, Is.Not.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeLocal = new DateTime(1998, 1, 15, 21, 0, 0, DateTimeKind.Unspecified);
            var result = target.ConvertFromSiteTimeZone(dateTimeLocal);
            Assert.AreEqual(DateTimeKind.Utc, result.Kind);
            Assert.AreEqual(dateTimeLocal.Hour - timeZone.BaseUtcOffset.Hours, result.Hour);
        }

        [Test]
        [Description("DateTime which is DateTimeKind.Unspecified is converted to DateTimeKind.Utc with no offset when target time zone is UTC.")]
        public void ConvertFromSiteTimeZoneTest06() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
            Assert.That(timeZone.BaseUtcOffset, Is.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeLocal = new DateTime(1998, 1, 15, 21, 0, 0, DateTimeKind.Unspecified);
            var result = target.ConvertFromSiteTimeZone(dateTimeLocal);
            Assert.AreEqual(DateTimeKind.Utc, result.Kind);
            Assert.AreEqual(dateTimeLocal.Hour, result.Hour);
            Assert.AreEqual(dateTimeLocal.Minute, result.Minute);
        }

        [Test]
        [Description("DateTime which is already DateTimeKind.Utc is never converted.")]
        public void ConvertFromSiteTimeZoneTest07() {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            Assert.That(timeZone.BaseUtcOffset, Is.Not.EqualTo(TimeSpan.Zero));
            var container = TestHelpers.InitializeContainer(null, null, timeZone);
            var target = container.Resolve<IDateLocalizationServices>();
            var dateTimeLocal = new DateTime(1998, 1, 15, 21, 0, 0, DateTimeKind.Utc);
            var result = target.ConvertFromSiteTimeZone(dateTimeLocal);
            Assert.AreEqual(DateTimeKind.Utc, result.Kind);
            Assert.AreEqual(dateTimeLocal, result);
        }
    }
}
