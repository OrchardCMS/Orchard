using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Orchard.Commands;
using System;
using System.Linq;

namespace Orchard.Tests.Commands {
    [TestFixture]
    public class CommandsTests {
        private ICommandHandler _handler;

        [SetUp]
        public void Init() {
            _handler = new StubCommandHandler();
        }

        private CommandContext CreateCommandContext(string commandName) {
            return CreateCommandContext(commandName, new Dictionary<string, string>(), new string[]{});
        }

        private CommandContext CreateCommandContext(string commandName, IDictionary<string, string> switches) {
            return CreateCommandContext(commandName, switches, new string[]{});
        }

        private CommandContext CreateCommandContext(string commandName, IDictionary<string, string> switches, string[] args) {
            var builder = new CommandHandlerDescriptorBuilder();

            var descriptor = builder.Build(typeof(StubCommandHandler));

            var commandDescriptor = descriptor.Commands.Single(d => string.Equals(d.Name, commandName, StringComparison.OrdinalIgnoreCase));

            return new CommandContext { 
                Command = commandName, 
                Switches = switches, 
                CommandDescriptor = commandDescriptor,
                Arguments = args,
                Input = new StringReader(string.Empty),
                Output = new StringWriter()
            };
        }

        [Test]
        public void TestFooCommand() {
            var commandContext = CreateCommandContext("Foo");
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo("Command Foo Executed"));
        }

        [Test]
        public void TestNotExistingCommand() {
            Assert.Throws<InvalidOperationException>(() => {
                var commandContext = CreateCommandContext("NoSuchCommand");
                _handler.Execute(commandContext);
            });
        }

        [Test]
        public void TestCommandWithCustomAlias() {
            var commandContext = CreateCommandContext("Bar");
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo("Hello World!"));
        }

        [Test]
        public void TestHelpText() {
            var commandContext = CreateCommandContext("Baz");
            Assert.That(commandContext.CommandDescriptor.HelpText, Is.EqualTo("Baz help"));
        }

        [Test]
        public void TestEmptyHelpText() {
            var commandContext = CreateCommandContext("Foo");
            Assert.That(commandContext.CommandDescriptor.HelpText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestCaseInsensitiveForCommand() {
            var commandContext = CreateCommandContext("BAZ", new Dictionary<string, string> { { "VERBOSE", "true" } });
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo("Command Baz Called : This was a test"));
        }


        [Test]
        public void TestBooleanSwitchForCommand() {
            var commandContext = CreateCommandContext("Baz", new Dictionary<string, string> {{"Verbose", "true"}});
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo("Command Baz Called : This was a test"));
        }

        [Test]
        public void TestIntSwitchForCommand() {
            var commandContext = CreateCommandContext("Baz", new Dictionary<string, string> {{"Level", "2"}});
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo("Command Baz Called : Entering Level 2"));
        }

        [Test]
        public void TestStringSwitchForCommand() {
            var commandContext = CreateCommandContext("Baz", new Dictionary<string, string> {{"User", "OrchardUser"}});
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo("Command Baz Called : current user is OrchardUser"));
        }

        [Test]
        public void TestSwitchForCommandWithoutSupportForIt() {
            var switches = new Dictionary<string, string> {{"User", "OrchardUser"}};
            var commandContext = CreateCommandContext("Foo", switches);
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }

        [Test]
        public void TestCommandThatDoesNotReturnAValue() {
            var commandContext = CreateCommandContext("Log");
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo(""));
        }

        [Test]
        public void TestNotExistingSwitch() {
            var switches = new Dictionary<string, string> {{"ThisSwitchDoesNotExist", "Insignificant"}};
            var commandContext = CreateCommandContext("Foo", switches);
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }

        [Test]
        public void TestCommandArgumentsArePassedCorrectly() {
            var commandContext = CreateCommandContext("Concat", new Dictionary<string, string>(), new[] {"left to ", "right"});
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo("left to right"));
        }

        [Test]
        public void TestCommandArgumentsArePassedCorrectlyWithAParamsParameters() {
            var commandContext = CreateCommandContext("ConcatParams", new Dictionary<string, string>(), new[] {"left to ", "right"});
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo("left to right"));
        }

        [Test]
        public void TestCommandArgumentsArePassedCorrectlyWithAParamsParameterAndNoArguments() {
            var commandContext = CreateCommandContext("ConcatParams", new Dictionary<string, string>());
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo(""));
        }

        [Test]
        public void TestCommandArgumentsArePassedCorrectlyWithNormalParametersAndAParamsParameters() {
            var commandContext = CreateCommandContext("ConcatAllParams",
                new Dictionary<string, string>(),
                new[] { "left-", "center-", "right"});
            _handler.Execute(commandContext);
            Assert.That(commandContext.Output.ToString(), Is.EqualTo("left-center-right"));
        }

        [Test]
        public void TestCommandParamsMismatchWithoutParamsNotEnoughArguments() {
            var commandContext = CreateCommandContext("Concat", new Dictionary<string, string>(), new[] { "left to " });
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }

        [Test]
        public void TestCommandParamsMismatchWithoutParamsTooManyArguments() {
            var commandContext = CreateCommandContext("Foo", new Dictionary<string, string>(), new[] { "left to " });
            Assert.Throws<InvalidOperationException>(() => _handler.Execute(commandContext));
        }

        [Test]
        public void TestCommandParamsMismatchWithParamsButNotEnoughArguments() {
            var commandContext = CreateCommandContext("ConcatAllParams", new Dictionary<string, string>());
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

        [CommandName("Bar")]
        public string Hello() {
            return "Hello World!";
        }

        [OrchardSwitches("Verbose, Level, User")]
        [CommandHelp("Baz help")]
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
