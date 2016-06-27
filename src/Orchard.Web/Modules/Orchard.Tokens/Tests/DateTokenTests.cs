using System;
using System.Globalization;
using Autofac;
using NUnit.Framework;
using Orchard.Localization.Services;
using Orchard.Services;
using Orchard.Tokens.Implementation;
using Orchard.Tokens.Providers;
using Orchard.Localization.Models;

namespace Orchard.Tokens.Tests {
    [TestFixture]
    public class DateTokenTests {
        private IContainer _container;
        private ITokenizer _tokenizer;
        private IClock _clock;
        private IDateTimeFormatProvider _dateTimeFormats;
        private IDateLocalizationServices _dateLocalizationServices;
        private IDateFormatter _dateFormatter;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubOrchardServices>().As<IOrchardServices>();
            builder.RegisterType<TokenManager>().As<ITokenManager>();
            builder.RegisterType<Tokenizer>().As<ITokenizer>();
            builder.RegisterType<DateTokens>().As<ITokenProvider>();
            builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<SiteCalendarSelector>().As<ICalendarSelector>();
            builder.RegisterType<DefaultCalendarManager>().As<ICalendarManager>();
            builder.RegisterType<CultureDateTimeFormatProvider>().As<IDateTimeFormatProvider>();
            builder.RegisterType<DefaultDateFormatter>().As<IDateFormatter>();
            builder.RegisterType<DefaultDateLocalizationServices>().As<IDateLocalizationServices>();
 
            _container = builder.Build();
            _tokenizer = _container.Resolve<ITokenizer>();
            _clock = _container.Resolve<IClock>();
            _dateTimeFormats = _container.Resolve<IDateTimeFormatProvider>();
            _dateLocalizationServices = _container.Resolve<IDateLocalizationServices>();
            _dateFormatter = _container.Resolve<IDateFormatter>();
        }

        [Test]
        public void TestDate() {
            Assert.That(_tokenizer.Replace("{Date}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, new DateLocalizationOptions() { EnableTimeZoneConversion = false })));
            Assert.That(_tokenizer.Replace("{Date}", new { Date = new DateTime(1978, 11, 15, 0, 0, 0, DateTimeKind.Utc) }), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(new DateTime(1978, 11, 15, 0, 0, 0, DateTimeKind.Utc), new DateLocalizationOptions() { EnableTimeZoneConversion = false })));
        }

        [Test]
        public void TestDateSince() {
            var date = _clock.UtcNow.AddHours(-25);
            Assert.That(_tokenizer.Replace("{Date.Since}", new { Date = date }), Is.EqualTo("1 day ago"));
        }

        [Test]
        public void TestDateShort() {
            Assert.That(_tokenizer.Replace("{Date.Short}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.ShortDateTimeFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false })));
        }

        [Test]
        public void TestDateShortDate() {
            Assert.That(_tokenizer.Replace("{Date.ShortDate}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.ShortDateFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false })));
        }

        [Test]
        public void TestDateShortTime() {
            Assert.That(_tokenizer.Replace("{Date.ShortTime}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.ShortTimeFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false })));
        }

        [Test]
        public void TestDateLong() {
            Assert.That(_tokenizer.Replace("{Date.Long}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.LongDateTimeFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false })));
        }

        [Test]
        public void TestDateLongDate() {
            Assert.That(_tokenizer.Replace("{Date.LongDate}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.LongDateFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false })));
        }

        [Test]
        public void TestDateLongTime() {
            Assert.That(_tokenizer.Replace("{Date.LongTime}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.LongTimeFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false })));
        }

        [Test]
        public void TestDateFormat() {
            Assert.That(_tokenizer.Replace("{Date.Format:yyyyMMdd}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, "yyyyMMdd", new DateLocalizationOptions() { EnableTimeZoneConversion = false })));
        }

        [Test]
        public void TestDateLocal() {
            Assert.That(_tokenizer.Replace("{Date.Local}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow)));
            Assert.That(_tokenizer.Replace("{Date.Local}", new { Date = new DateTime(1978, 11, 15, 0, 0, 0, DateTimeKind.Utc) }), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(new DateTime(1978, 11, 15, 0, 0, 0, DateTimeKind.Utc))));
        }

        [Test]
        public void TestDateLocalShort() {
            Assert.That(_tokenizer.Replace("{Date.Local.Short}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.ShortDateTimeFormat)));
        }

        [Test]
        public void TestDateLocalShortDate() {
            Assert.That(_tokenizer.Replace("{Date.Local.ShortDate}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.ShortDateFormat)));
        }

        [Test]
        public void TestDateLocalShortTime() {
            Assert.That(_tokenizer.Replace("{Date.Local.ShortTime}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.ShortTimeFormat)));
        }

        [Test]
        public void TestDateLocalLong() {
            Assert.That(_tokenizer.Replace("{Date.Local.Long}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.LongDateTimeFormat)));
        }

        [Test]
        public void TestDateLocalLongDate() {
            Assert.That(_tokenizer.Replace("{Date.Local.LongDate}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.LongDateFormat)));
        }

        [Test]
        public void TestDateLocalLongTime() {
            Assert.That(_tokenizer.Replace("{Date.Local.LongTime}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, _dateTimeFormats.LongTimeFormat)));
        }

        [Test]
        public void TestDateLocalFormat() {
            Assert.That(_tokenizer.Replace("{Date.Local.Format:yyyyMMdd}", null), Is.EqualTo(_dateLocalizationServices.ConvertToLocalizedString(_clock.UtcNow, "yyyyMMdd")));
        }
    }
}
