using System.Collections.Specialized;
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
            CommandContext commandContext = new CommandContext { Command = "Bar" };
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("Hello World!"));
        }

        [Test]
        public void TestBooleanSwitchForCommand() {
            CommandContext commandContext = new CommandContext { Command = "Baz", Switches = new NameValueCollection() };
            commandContext.Switches.Add("Verbose", "true");
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("Command Baz Called : This was a test"));
        }

        [Test]
        public void TestIntSwitchForCommand() {
            CommandContext commandContext = new CommandContext { Command = "Baz", Switches = new NameValueCollection() };
            commandContext.Switches.Add("Level", "2");
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("Command Baz Called : Entering Level 2"));
        }

        [Test]
        public void TestStringSwitchForCommand() {
            CommandContext commandContext = new CommandContext { Command = "Baz", Switches = new NameValueCollection() };
            commandContext.Switches.Add("User", "OrchardUser");
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("Command Baz Called : current user is OrchardUser"));
        }

        [Test]
        public void TestSwitchForCommandWithoutSupportForIt() {
            CommandContext commandContext = new CommandContext { Command = "Foo", Switches = new NameValueCollection() };
            commandContext.Switches.Add("User", "OrchardUser");
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }

        [Test]
        public void TestNotExistingSwitch() {
            CommandContext commandContext = new CommandContext { Command = "Foo", Switches = new NameValueCollection() };
            commandContext.Switches.Add("ThisSwitchDoesNotExist", "Insignificant");
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }
    }

    public class StubCommandHandler : DefaultOrchardCommandHandler {
        [OrchardSwitch]
        public bool Verbose { get; set; }

        [OrchardSwitch]
        public int Level { get; set; }

        [OrchardSwitch]
        public string User { get; set; }

        public string Foo() {
            return "Command Foo Executed";
        }

        [OrchardCommand("Bar")]
        public string Hello() {
            return "Hello World!";
        }

        [OrchardSwitches("Verbose, Level, User")]
        public string Baz() {
            string trace = "Command Baz Called";

            if (Verbose) {
                trace += " : This was a test";
            }

            if (Level == 2) {
                trace += " : Entering Level 2";
            }

            if (!String.IsNullOrEmpty(User)) {
                trace += " : current user is " + User;
            }

            return trace;
        }
    }
}
