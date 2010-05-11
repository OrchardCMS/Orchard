using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using Orchard.Events;
using System;

namespace Orchard.Tests.Events {
    [TestFixture]
    public class EventTests {
        private IContainer _container;
        private IEventBus _eventBus;
        private StubEventBusHandler _eventBusHandler;
        private StubEventHandler _eventHandler;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            _eventBusHandler = new StubEventBusHandler();
            _eventHandler = new StubEventHandler();
            builder.RegisterInstance(_eventBusHandler).As<IEventBusHandler>();
            builder.RegisterInstance(_eventHandler).As<IEventHandler>();
            builder.RegisterType<DefaultOrchardEventBus>().As<IEventBus>();
            _container = builder.Build();
            _eventBus = _container.Resolve<IEventBus>();
        }

        [Test]
        public void EventsAreCorrectlyDispatchedToHandlers() {
            Assert.That(_eventBusHandler.LastMessageName, Is.Null);
            _eventBus.Notify_Obsolete("Notification", new Dictionary<string, string>());
            Assert.That(_eventBusHandler.LastMessageName, Is.EqualTo("Notification"));
        }

        public class StubEventBusHandler : IEventBusHandler {
            public string LastMessageName { get; set; }

            #region Implementation of IEventBusHandler

            public void Process(string messageName, IDictionary<string, string> eventData) {
                LastMessageName = messageName;
            }

            #endregion
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
        public void EventBusThrowsIfMessageNameIsNotCorrectlyFormatted() {
            Assert.Throws<ArgumentException>(() => _eventBus.Notify("StubEventHandlerIncrement", new Dictionary<string, object>()));
        }

        public interface ITestEventHandler : IEventHandler {
            void Increment();
            void Substract(int a, int b);
        }

        public class StubEventHandler : ITestEventHandler {
            public int Count { get; set; }
            public int Result { get; set; }

            public void Increment() {
                Count++;
            }

            public void Substract(int a, int b) {
                Result = a - b;
            }
        }
    }
}
