using NUnit.Framework;
using Orchard.Commands;
using System;

namespace Orchard.Tests.Commands {
    [TestFixture]
    public class CommandsTests {
        private ICommandHandler _handler;

        [SetUp]
        public void Init() {
            _handler = new StubCommandHandler();
        }

        [Test]
        public void TestFooCommand() {
            CommandContext commandContext = new CommandContext { Command = "Foo" };
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("Command Foo Executed"));
        }

        [Test]
        public void TestNotExistingCommand() {
            CommandContext commandContext = new CommandContext { Command = "NoSuchCommand" };
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }

        [Test]
        public void TestCommandWithCustomAlias() {
            CommandContext commandContext = new CommandContext { Command = "bar" };
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("Hello World!"));
        }
    }

    public class StubCommandHandler : DefaultOrchardCommandHandler {
        public string Foo() {
            return "Command Foo Executed";
        }

        [OrchardCommand("bar")]
        public string Hello() {
            return "Hello World!";
        }
    }
}
