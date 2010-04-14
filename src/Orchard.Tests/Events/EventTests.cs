using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using Orchard.Events;

namespace Orchard.Tests.Events {
    [TestFixture]
    public class EventTests {
        private IContainer _container;
        private IEventBus _eventBus;
        private StubEventBusHandler _eventBusHandler;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            _eventBusHandler = new StubEventBusHandler();
            builder.RegisterInstance(_eventBusHandler).As<IEventBusHandler>();
            builder.RegisterType<DefaultOrchardEventBus>().As<IEventBus>();
            _container = builder.Build();
            _eventBus = _container.Resolve<IEventBus>();
        }

        [Test]
        public void EventsAreCorrectlyDispatchedToHandlers() {
            Assert.That(_eventBusHandler.LastMessageName, Is.Null);
            _eventBus.Notify("Notification", new Dictionary<string, string>());
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
    }
}
