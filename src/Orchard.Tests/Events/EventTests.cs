using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Events;
using System;
using Orchard.Exceptions;

namespace Orchard.Tests.Events {
    [TestFixture]
    public class EventTests {
        private IContainer _container;
        private IEventBus _eventBus;
        private StubEventHandler _eventHandler;

        [SetUp]
        public void Init() {
            _eventHandler = new StubEventHandler();

            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultOrchardEventBus>().As<IEventBus>();
            builder.RegisterType<StubExceptionPolicy>().As<IExceptionPolicy>();
            builder.RegisterType<StubEventHandler2>().As<IEventHandler>();
            builder.RegisterInstance(_eventHandler).As<IEventHandler>();

            _container = builder.Build();
            _eventBus = _container.Resolve<IEventBus>();
        }

        [Test]
        public void EventsAreCorrectlyDispatchedToEventHandlers() {
            Assert.That(_eventHandler.Count, Is.EqualTo(0));
            _eventBus.Notify("ITestEventHandler.Increment", new Dictionary<string, object>());
            Assert.That(_eventHandler.Count, Is.EqualTo(1));
        }

        [Test]
        public void EventParametersAreCorrectlyPassedToEventHandlers() {
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 5200;
            arguments["b"] = 2600;
            _eventBus.Notify("ITestEventHandler.Substract", arguments);
            Assert.That(_eventHandler.Result, Is.EqualTo(2600));
        }

        [Test]
        public void EventParametersArePassedInCorrectOrderToEventHandlers() {
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 2600;
            arguments["b"] = 5200;
            _eventBus.Notify("ITestEventHandler.Substract", arguments);
            Assert.That(_eventHandler.Result, Is.EqualTo(-2600));
        }

        [Test]
        public void EventParametersAreCorrectlyPassedToMatchingMethod() {
            Assert.That(_eventHandler.Summary, Is.Null);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = "a";
            arguments["b"] = "b";
            arguments["c"] = "c";
            _eventBus.Notify("ITestEventHandler.Concat", arguments);
            Assert.That(_eventHandler.Summary, Is.EqualTo("abc"));
        }

        [Test]
        public void EventParametersAreCorrectlyPassedToExactlyMatchingMethod() {
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 1000;
            arguments["b"] = 100;
            arguments["c"] = 10;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.That(_eventHandler.Result, Is.EqualTo(1110));
        }

        [Test]
        public void EventParametersAreCorrectlyPassedToBestMatchingMethodAndExtraParametersAreIgnored() {
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 1000;
            arguments["b"] = 100;
            arguments["c"] = 10;
            arguments["e"] = 1;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.That(_eventHandler.Result, Is.EqualTo(1110));
        }

        [Test]
        public void EventParametersAreCorrectlyPassedToBestMatchingMethodAndExtraParametersAreIgnored2() {
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 1000;
            arguments["e"] = 1;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.That(_eventHandler.Result, Is.EqualTo(3000));
        }

        [Test]
        public void EventParametersAreCorrectlyPassedToExactlyMatchingMethodWhenThereIsOne() {
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 1000;
            arguments["b"] = 100;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.That(_eventHandler.Result, Is.EqualTo(2200));
        }

        [Test]
        public void EventParametersAreCorrectlyPassedToExactlyMatchingMethodWhenThereIsOne2() {
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 1000;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.That(_eventHandler.Result, Is.EqualTo(3000));
        }

        [Test]
        public void EventHandlerWontBeCalledWhenNoParameterMatchExists() {
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["e"] = 1;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
        }

        [Test]
        public void EventHandlerWontBeCalledWhenNoParameterMatchExists2() {
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.That(_eventHandler.Result, Is.EqualTo(0));
        }

        [Test]
        public void EventHandlerWontThrowIfMethodDoesNotExists() {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            Assert.DoesNotThrow(() => _eventBus.Notify("ITestEventHandler.NotExisting", arguments));
        }

        [Test]
        public void EventBusThrowsIfMessageNameIsNotCorrectlyFormatted() {
            Assert.Throws<ArgumentException>(() => _eventBus.Notify("StubEventHandlerIncrement", new Dictionary<string, object>()));
        }

        [Test]
        public void InterceptorCanCoerceResultingCollection() {
            var data = new object[]{"5","18","2"};
            var adjusted = EventsInterceptor.Adjust(data, typeof(IEnumerable<string>));
            Assert.That(data, Is.InstanceOf<IEnumerable<object>>());
            Assert.That(data, Is.Not.InstanceOf<IEnumerable<string>>());
            Assert.That(adjusted, Is.InstanceOf<IEnumerable<string>>());
        }

        [Test]
        public void EnumerableResultsAreTreatedLikeSelectMany() {
            var results = _eventBus.Notify("ITestEventHandler.Gather", new Dictionary<string, object> { { "a", 42 }, { "b", "alpha" } }).Cast<string>();
            Assert.That(results.Count(), Is.EqualTo(3));
            Assert.That(results, Has.Some.EqualTo("42"));
            Assert.That(results, Has.Some.EqualTo("alpha"));
            Assert.That(results, Has.Some.EqualTo("[42,alpha]"));
        }


        public interface ITestEventHandler : IEventHandler {
            void Increment();
            void Sum(int a);
            void Sum(int a, int b);
            void Sum(int a, int b, int c);
            void Substract(int a, int b);
            void Concat(string a, string b, string c);
            IEnumerable<string> Gather(int a, string b);
        }

        public class StubEventHandler : ITestEventHandler {
            public int Count { get; set; }
            public int Result { get; set; }
            public string Summary { get; set; }

            public void Increment() {
                Count++;
            }

            public void Sum(int a) {
                Result = 3 * a;
            }

            public void Sum(int a, int b) {
                Result = 2 * (a + b);
            }

            public void Sum(int a, int b, int c) {
                Result = a + b + c;
            }

            public void Substract(int a, int b) {
                Result = a - b;
            }

            public void Concat(string a, string b, string c) {
                Summary = a + b + c;
            }

            public IEnumerable<string> Gather(int a, string b) {
                yield return String.Format("[{0},{1}]", a, b);
            }
        }
        public class StubEventHandler2 : ITestEventHandler {
            public void Increment() {
            }

            public void Sum(int a) {
            }

            public void Sum(int a, int b) {
            }

            public void Sum(int a, int b, int c) {
            }

            public void Substract(int a, int b) {
            }

            public void Concat(string a, string b, string c) {
            }

            public IEnumerable<string> Gather(int a, string b) {
                return new[] { a.ToString(), b };
            }
        }
    }

    class StubExceptionPolicy : IExceptionPolicy {
        public bool HandleException(object sender, Exception exception) {
            return true;
        }
    }
}
