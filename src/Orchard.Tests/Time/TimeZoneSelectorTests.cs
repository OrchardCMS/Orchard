using System;
using System.Web;
using Autofac;
using NUnit.Framework;
using Orchard.Tests.Stubs;
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
            builder.RegisterType<FallbackTimeZoneSelector>().As<ITimeZoneSelector>();
            builder.RegisterType<SiteTimeZoneSelector>().As<ITimeZoneSelector>();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            
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
        public void DefaultTimeZoneIsLocal() {
            _timeZoneSelector.Priority = -200;
            var timeZone = _workContextStateProvider.Get<TimeZoneInfo>("CurrentTimeZone");

            Assert.That(timeZone(new StubWorkContext()), Is.EqualTo(TimeZoneInfo.Local));
        }

        [Test]
        public void TimeZoneProviderReturnsTimeZoneFromSelector() {
            _timeZoneSelector.Priority = 999;
            _timeZoneSelector.TimeZone = TimeZoneInfo.Utc;
            var timeZone = _workContextStateProvider.Get<TimeZoneInfo>("CurrentTimeZone");

            Assert.That(timeZone(new StubWorkContext()), Is.EqualTo(TimeZoneInfo.Utc));
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

