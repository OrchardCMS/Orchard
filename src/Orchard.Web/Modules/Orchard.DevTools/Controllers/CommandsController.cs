using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.Commands;
using Orchard.DevTools.ViewModels;
using Orchard.Environment.Extensions;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.DevTools.Controllers {
    [Themed, Admin, OrchardFeature("Orchard.DevTools.WebCommandLine")]
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
            var parameters = new CommandParameters {
                Arguments = model.CommandLine.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries),
                Output = writer
            };

            _commandManager.Execute(parameters);
            model.History = (model.History ?? Enumerable.Empty<string>())
                .Concat(new[] { model.CommandLine })
                .Distinct()
                .ToArray();
            model.Results = writer.ToString();
            return View("Execute", model);
        }


    }
}