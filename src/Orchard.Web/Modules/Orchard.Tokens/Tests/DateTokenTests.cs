using System;
using System.Globalization;
using Autofac;
using NUnit.Framework;
using Orchard.Core.Shapes.Localization;
using Orchard.Services;
using Orchard.Tokens.Implementation;
using Orchard.Tokens.Providers;

namespace Orchard.Tokens.Tests {
    [TestFixture]
    public class DateTokenTests {
        private IContainer _container;
        private ITokenizer _tokenizer;
        private IClock _clock;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubOrchardServices>().As<IOrchardServices>();
            builder.RegisterType<TokenManager>().As<ITokenManager>();
            builder.RegisterType<Tokenizer>().As<ITokenizer>();
            builder.RegisterType<DateTokens>().As<ITokenProvider>();
            builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterType<DateTimeLocalization>().As<IDateTimeLocalization>();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            _container = builder.Build();
            _tokenizer = _container.Resolve<ITokenizer>();
            _clock = _container.Resolve<IClock>();
        }

        [Test]
        public void TestDateTokens() {
            var dateTimeLocalization = _container.Resolve<IDateTimeLocalization>();
            var culture = CultureInfo.GetCultureInfo(_container.Resolve<IOrchardServices>().WorkContext.CurrentCulture);

            var dateTimeFormat = dateTimeLocalization.ShortDateFormat.Text + " " + dateTimeLocalization.ShortTimeFormat.Text;

            Assert.That(_tokenizer.Replace("{Date}", null), Is.EqualTo(_clock.UtcNow.ToString(dateTimeFormat, culture)));
            Assert.That(_tokenizer.Replace("{Date}", new { Date = new DateTime(1978, 11, 15, 0, 0, 0, DateTimeKind.Utc) }), Is.EqualTo(new DateTime(1978, 11, 15, 0, 0, 0, DateTimeKind.Utc).ToString(dateTimeFormat, culture)));
        }

        [Test]
        public void TestFormat() {
            Assert.That(_tokenizer.Replace("{Date.Format:yyyy}", null), Is.EqualTo(_clock.UtcNow.ToString("yyyy")));
        }

        [Test]
        public void TestSince() {
            var date = _clock.UtcNow.Subtract(TimeSpan.FromHours(25));
            Assert.That(_tokenizer.Replace("{Date.Since}", new { Date = date }), Is.EqualTo("1 day ago"));
        }

    }
}
