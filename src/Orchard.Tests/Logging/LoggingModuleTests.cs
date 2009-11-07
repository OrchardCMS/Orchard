using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autofac.Builder;
using NUnit.Framework;
using Orchard.Logging;

namespace Orchard.Tests.Logging {
    [TestFixture]
    public class LoggingModuleTests {
        [Test]
        public void LoggingModuleWillSetLoggerProperty() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.Register<Thing>();
            var container = builder.Build();
            var thing = container.Resolve<Thing>();
            Assert.That(thing.Logger, Is.Not.Null);
        }

        [Test]
        public void LoggerFactoryIsPassedTheTypeOfTheContainingInstance() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.Register<Thing>();
            var stubFactory = new StubFactory();
            builder.Register(stubFactory).As<ILoggerFactory>();

            var container = builder.Build();
            var thing = container.Resolve<Thing>();
            Assert.That(thing.Logger, Is.Not.Null);
            Assert.That(stubFactory.CalledType, Is.EqualTo(typeof(Thing)));
        }

        public class StubFactory : ILoggerFactory {
            public ILogger CreateLogger(Type type) {
                CalledType = type;
                return NullLogger.Instance;
            }

            public Type CalledType { get; set; }
        }

        [Test]
        public void DefaultLoggerConfigurationUsesCastleLoggerFactoryOverTraceSource() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.Register<Thing>();
            var container = builder.Build();
            var thing = container.Resolve<Thing>();
            Assert.That(thing.Logger, Is.Not.Null);

            InMemoryCapture.Messages.Clear();
            thing.Logger.Error("-boom{0}-", 42);
            Assert.That(InMemoryCapture.Messages, Has.Some.StringContaining("-boom42-"));

            InMemoryCapture.Messages.Clear();
            thing.Logger.Warning(new ApplicationException("problem"), "crash");
            Assert.That(InMemoryCapture.Messages, Has.Some.StringContaining("problem"));
            Assert.That(InMemoryCapture.Messages, Has.Some.StringContaining("crash"));
            Assert.That(InMemoryCapture.Messages, Has.Some.StringContaining("ApplicationException"));
        }
    }

    public class Thing {
        public ILogger Logger { get; set; }
    }

    public class InMemoryCapture : TraceListener {
        static InMemoryCapture() {
            Messages = new List<string>();
        }

        public static List<string> Messages { get; set; }

        public override void Write(string message) {
            lock (Messages) Messages.Add(message);
        }

        public override void WriteLine(string message) {
            lock (Messages) Messages.Add(message + System.Environment.NewLine);
        }
    }
}
