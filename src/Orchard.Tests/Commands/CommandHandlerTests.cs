using System.Collections.Generic;
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
            CommandContext commandContext = new CommandContext { Command = "Baz", Switches = new Dictionary<string, string>() };
            commandContext.Switches.Add("Verbose", "true");
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("Command Baz Called : This was a test"));
        }

        [Test]
        public void TestIntSwitchForCommand() {
            CommandContext commandContext = new CommandContext { Command = "Baz", Switches = new Dictionary<string, string>() };
            commandContext.Switches.Add("Level", "2");
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("Command Baz Called : Entering Level 2"));
        }

        [Test]
        public void TestStringSwitchForCommand() {
            CommandContext commandContext = new CommandContext { Command = "Baz", Switches = new Dictionary<string, string>() };
            commandContext.Switches.Add("User", "OrchardUser");
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("Command Baz Called : current user is OrchardUser"));
        }

        [Test]
        public void TestSwitchForCommandWithoutSupportForIt() {
            CommandContext commandContext = new CommandContext { Command = "Foo", Switches = new Dictionary<string, string>() };
            commandContext.Switches.Add("User", "OrchardUser");
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }

        [Test]
        public void TestCommandThatDoesNotReturnAValue() {
            CommandContext commandContext = new CommandContext { Command = "Log" };
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.Null);
        }

        [Test]
        public void TestNotExistingSwitch() {
            CommandContext commandContext = new CommandContext { Command = "Foo", Switches = new Dictionary<string, string>() };
            commandContext.Switches.Add("ThisSwitchDoesNotExist", "Insignificant");
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }

        [Test]
        public void TestCommandArgumentsArePassedCorrectly() {
            CommandContext commandContext = new CommandContext {
                                                                   Command = "Concat",
                                                                   Switches = new Dictionary<string, string>(),
                                                                   Arguments = new[] {"left to ", "right"}
                                                               };
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("left to right"));
        }

        [Test]
        public void TestCommandArgumentsArePassedCorrectlyWithAParamsParameters() {
            CommandContext commandContext = new CommandContext {
                                                                   Command = "ConcatParams",
                                                                   Switches = new Dictionary<string, string>(),
                                                                   Arguments = new[] {"left to ", "right"}
                                                               };
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("left to right"));
        }

        [Test]
        public void TestCommandArgumentsArePassedCorrectlyWithAParamsParameterAndNoArguments() {
            CommandContext commandContext = new CommandContext {
                Command = "ConcatParams",
                Switches = new Dictionary<string, string>()
            };
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo(""));
        }


        [Test]
        public void TestCommandArgumentsArePassedCorrectlyWithNormalParametersAndAParamsParameters() {
            CommandContext commandContext = new CommandContext {
                Command = "ConcatAllParams",
                Switches = new Dictionary<string, string>(),
                Arguments = new[] { "left-", "center-", "right" }
            };
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output, Is.EqualTo("left-center-right"));
        }

        [Test]
        public void TestCommandParamsMismatchWithoutParamsNotEnoughArguments() {
            CommandContext commandContext = new CommandContext {
                Command = "Concat",
                Switches = new Dictionary<string, string>(),
                Arguments = new[] { "left to "}
            };
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }

        [Test]
        public void TestCommandParamsMismatchWithoutParamsTooManyArguments() {
            CommandContext commandContext = new CommandContext {
                Command = "Foo",
                Switches = new Dictionary<string, string>(),
                Arguments = new[] { "left to " }
            };
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }

        [Test]
        public void TestCommandParamsMismatchWithParamsButNotEnoughArguments() {
            CommandContext commandContext = new CommandContext {
                Command = "ConcatAllParams",
                Switches = new Dictionary<string, string>(),
            };
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

        public string Concat(string left, string right) {
            return left + right;
        }

        public string ConcatParams(params string[] parameters) {
            string concatenated = "";
            foreach (var s in parameters) {
                concatenated += s;
            }
            return concatenated;
        }

        public string ConcatAllParams(string leftmost, params string[] rest) {
            string concatenated = leftmost;
            foreach (var s in rest) {
                concatenated += s;
            }
            return concatenated;
        }

        public void Log() {
            return;
        }
    }
}
