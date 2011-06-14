using System;
using System.Web;
using Autofac;
using NUnit.Framework;
using Orchard.Time;

namespace Orchard.Tests.Time {
    [TestFixture]
    public class TimeZoneProviderTests {
        private IContainer _container;
        private IWorkContextStateProvider _workContextStateProvider;
        private TestTimeZoneSelector _timeZoneSelector;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_timeZoneSelector = new TestTimeZoneSelector()).As<ITimeZoneSelector>();
            builder.RegisterType<CurrentTimeZoneWorkContext>().As<IWorkContextStateProvider>();
            _container = builder.Build();
            _workContextStateProvider = _container.Resolve<IWorkContextStateProvider>();
        }

        [Test]
        public void ShouldProvideCurrentTimeZoneOnly() {
            _timeZoneSelector.TimeZone = null;
            var timeZone = _workContextStateProvider.Get<TimeZoneInfo>("Foo");

            Assert.That(timeZone, Is.Null);
        }

        [Test]
        public void DefaultTimeZoneIsUtc() {
            _timeZoneSelector.TimeZone = null;
            var timeZone = _workContextStateProvider.Get<TimeZoneInfo>("CurrentTimeZone");

            Assert.That(timeZone(new StubWorkContext()), Is.EqualTo(TimeZoneInfo.Utc));
        }

        [Test]
        public void TimeZoneProviderReturnsTimeZoneFromSelector() {
            _timeZoneSelector.TimeZone = TimeZoneInfo.Local;
            var timeZone = _workContextStateProvider.Get<TimeZoneInfo>("CurrentTimeZone");

            Assert.That(timeZone(new StubWorkContext()), Is.EqualTo(TimeZoneInfo.Local));
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

    public class StubWorkContext : WorkContext {
        public override T Resolve<T>() {
            throw new NotImplementedException();
        }

        public override bool TryResolve<T>(out T service) {
            throw new NotImplementedException();
        }

        public override T GetState<T>(string name) {
            return default(T);
        }

        public override void SetState<T>(string name, T value) {
            
        }
    }
}

