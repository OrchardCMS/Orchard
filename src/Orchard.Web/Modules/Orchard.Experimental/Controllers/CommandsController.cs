using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.Commands;
using Orchard.Experimental.ViewModels;
using Orchard.Environment.Extensions;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.Experimental.Controllers {
    [Themed, Admin, OrchardFeature("Orchard.Experimental.WebCommandLine")]
    public class CommandsController : Controller {
        private readonly ICommandManager _commandManager;

        public CommandsController(ICommandManager commandManager) {
            _commandManager = commandManager;
        }

        public ActionResult Index() {
            return Execute();
        }

        public ActionResult Execute() {
            return View("Execute", new CommandsExecuteViewModel());
        }

        [HttpPost]
        public ActionResult Execute(CommandsExecuteViewModel model) {
            var writer = new StringWriter();
            var commandLine = model.CommandLine.Trim();
            CommandParameters parameters = GetCommandParameters(commandLine, writer);

            _commandManager.Execute(parameters);
            model.History = (model.History ?? Enumerable.Empty<string>())
                .Concat(new[] { model.CommandLine })
                .Distinct()
                .ToArray();
            model.Results = writer.ToString();
            return View(model);
        }

        private static CommandParameters GetCommandParameters(string commandLine, StringWriter writer) {
            var arguments = new List<string>();
            var switches = new Dictionary<string,string>();
            var current = 0;

            while (current < commandLine.Length) {
                var nextSpace = commandLine.IndexOf(' ', current);
                if (nextSpace == -1) nextSpace = commandLine.Length;
                var arg = commandLine.Substring(current, nextSpace - current).Trim();
                if (arg.Length == 0) {
                    current = nextSpace + 1;
                    continue;
                }
                if (arg[0] == '/') {
                    var colonIndex = arg.IndexOf(':');
                    if (colonIndex != -1) {
                        var switchName = arg.Substring(1, colonIndex - 1);
                        if (arg.Length > colonIndex + 1) {
                            if (arg[colonIndex + 1] == '"') {
                                var beginningOfSwitchValue = commandLine.IndexOf('"', current) + 1;
                                if (beginningOfSwitchValue != 0) {
                                    var endOfSwitchValue = commandLine.IndexOf('"', beginningOfSwitchValue);
                                    if (endOfSwitchValue != -1) {
                                        switches.Add(switchName,
                                                     commandLine.Substring(beginningOfSwitchValue,
                                                                           endOfSwitchValue - beginningOfSwitchValue));
                                        current = endOfSwitchValue + 1;
                                        continue;
                                    }
                                }
                            }
                            else {
                                switches.Add(switchName, arg.Substring(colonIndex + 1));
                                current = nextSpace + 1;
                                continue;
                            }
                        }
                    }
                }
                else if (arg[0] == '"') {
                    var argumentStart = commandLine.IndexOf('"', current) + 1;
                    var argumentEnd = commandLine.IndexOf('"', argumentStart);
                    if (argumentEnd != -1) {
                        arguments.Add(commandLine.Substring(argumentStart, argumentEnd - argumentStart));
                        current = argumentEnd + 1;
                        continue;
                    }
                }
                arguments.Add(arg);
                current = nextSpace + 1;
            }

            return new CommandParameters {
                Arguments = arguments,
                Switches = switches,
                Output = writer
            };
        }
    }
}