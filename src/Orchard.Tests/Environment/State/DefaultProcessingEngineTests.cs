using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.State;
using Orchard.Environment.Descriptor.Models;
using Orchard.Events;
using Orchard.Mvc;
using Orchard.Tests.Utility;

namespace Orchard.Tests.Environment.State {
    [TestFixture]
    public class DefaultProcessingEngineTests {
        private IContainer _container;
        private ShellContext _shellContext;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultProcessingEngine>().As<IProcessingEngine>();
            builder.RegisterModule(new WorkContextModule());
            builder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterAutoMocking();
            _container = builder.Build();

            _shellContext = new ShellContext {
                Descriptor = new ShellDescriptor(),
                Settings = new ShellSettings(),
                LifetimeScope = _container.BeginLifetimeScope(),
            };

            _container.Mock<IShellContextFactory>()
                .Setup(x => x.CreateDescribedContext(_shellContext.Settings, _shellContext.Descriptor))
                .Returns(_shellContext);
            _container.Mock<IHttpContextAccessor>()
                .Setup(x=>x.Current())
                .Returns(default(HttpContextBase));

        }

        [Test]
        public void NoTasksPendingByDefault() {
            var engine = _container.Resolve<IProcessingEngine>();
            var pending = engine.AreTasksPending();
            Assert.That(pending, Is.False);
        }

        [Test]
        public void ExecuteTaskIsSafeToCallWhenItDoesNothing() {
            var engine = _container.Resolve<IProcessingEngine>();
            var pending1 = engine.AreTasksPending();
            engine.ExecuteNextTask();
            var pending2 = engine.AreTasksPending();
            Assert.That(pending1, Is.False);
            Assert.That(pending2, Is.False);
        }

        [Test]
        public void CallingAddTaskReturnsResultIdentifierAndCausesPendingToBeTrue() {
            var engine = _container.Resolve<IProcessingEngine>();
            var pending1 = engine.AreTasksPending();
            var resultId = engine.AddTask(new ShellSettings { Name = ShellSettings.DefaultName }, null, null, null);
            var pending2 = engine.AreTasksPending();
            Assert.That(pending1, Is.False);
            Assert.That(resultId, Is.Not.Null);
            Assert.That(resultId, Is.Not.Empty);
            Assert.That(pending2, Is.True);
        }

        [Test]
        public void CallingExecuteCausesEventToFireAndPendingFlagToBeCleared() {
            _container.Mock<IEventBus>()
                .Setup(x => x.Notify(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(Enumerable.Empty<object>());

            var engine = _container.Resolve<IProcessingEngine>();
            var pending1 = engine.AreTasksPending();
            engine.AddTask(_shellContext.Settings, _shellContext.Descriptor, "foo", null);
            var pending2 = engine.AreTasksPending();
            engine.ExecuteNextTask();
            var pending3 = engine.AreTasksPending();
            Assert.That(pending1, Is.False);
            Assert.That(pending2, Is.True);
            Assert.That(pending3, Is.False);

            _container.Mock<IEventBus>()
                .Verify(x => x.Notify("foo", null));
        }


    }
}
