using System;
using System.Linq;
using NUnit.Framework;
using Orchard.Logging;

namespace Orchard.Tests {
    [TestFixture]
    public class EventsTests {
        [Test]
        public void AllEventsAreCalled() {
            var events = new ITestEvents[] { new FooSink(), new BarSink() };

            events.Invoke(x => x.Hello("world"), NullLogger.Instance);

            Assert.That(events.OfType<FooSink>().Single().Name, Is.EqualTo("world"));
            Assert.That(events.OfType<BarSink>().Single().Name, Is.EqualTo("world"));
        }

        [Test]
        public void AnExceptionShouldBeLoggedAndOtherEventsWillBeFired() {
            var events = new ITestEvents[] { new FooSink(), new CrashSink(), new BarSink() };

            var logger = new TestLogger();

            events.Invoke(x => x.Hello("world"), logger);

            Assert.That(events.OfType<FooSink>().Single().Name, Is.EqualTo("world"));
            Assert.That(events.OfType<BarSink>().Single().Name, Is.EqualTo("world"));
            Assert.That(logger.LogException, Is.TypeOf<ApplicationException>());
            Assert.That(logger.LogException, Has.Property("Message").EqualTo("Illegal name 'world'"));
        }

        private class TestLogger : ILogger {
            public bool IsEnabled(LogLevel level) {
                return true;
            }

            public void Log(LogLevel level, Exception exception, string format, params object[] args) {
                LogException = exception;
                LogFormat = format;
                LogArgs = args;
            }

            public Exception LogException { get; set; }
            public string LogFormat { get; set; }
            public object[] LogArgs { get; set; }
        }

        private interface ITestEvents : IDependency {
            void Hello(string name);
        }

        private class FooSink : ITestEvents {
            void ITestEvents.Hello(string name) {
                Name = name;
            }

            public string Name { get; set; }
        }

        private class BarSink : ITestEvents {
            void ITestEvents.Hello(string name) {
                Name = name;
            }
            public string Name { get; set; }
        }


        private class CrashSink : ITestEvents {
            void ITestEvents.Hello(string name) {
                throw new ApplicationException("Illegal name '" + name + "'");
            }
        }
    }
}
