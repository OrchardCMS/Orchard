using System;
using System.Web;
using Autofac;
using NUnit.Framework;
using Orchard.Time;

namespace Orchard.Tests.Time {
    [TestFixture]
    public class TimeZoneProviderTests {
        private IContainer _container;
        private ITimeZoneProvider _timeZoneProvider;
        private TestTimeZoneSelector _timeZoneSelector;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_timeZoneSelector = new TestTimeZoneSelector()).As<ITimeZoneSelector>();
            builder.RegisterType<DefaultTimeZoneProvider>().As<ITimeZoneProvider>();
            _container = builder.Build();
            _timeZoneProvider = _container.Resolve<ITimeZoneProvider>();
        }

        [Test]
        public void DefaultTimeZoneIsUtc() {
            _timeZoneSelector.TimeZone = null;
            var timeZone = _timeZoneProvider.GetTimeZone(null);

            Assert.That(timeZone, Is.EqualTo(TimeZoneInfo.Utc));
        }

        [Test]
        public void TimeZoneProviderReturnsTimeZoneFromSelector() {
            _timeZoneSelector.TimeZone = TimeZoneInfo.Local;
            var timeZone = _timeZoneProvider.GetTimeZone(null);

            Assert.That(timeZone, Is.EqualTo(TimeZoneInfo.Local));
        }

    }

    public class TestTimeZoneSelector : ITimeZoneSelector {
        public TimeZoneInfo TimeZone { get; set; }
        public int Priority { get; set; }

        public TimeZoneSelectorResult GetTimeZone(HttpContextBase context) {
            return new TimeZoneSelectorResult {
                Priority = Priority, 
                TimeZone= TimeZone
            };
        }
    }
}

