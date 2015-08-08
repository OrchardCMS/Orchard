using Orchard.Commands;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MultiTenancy.Services;
using Orchard.MultiTenancy.ViewModels;
using Orchard.Security;
using Orchard.Themes;
using Orchard.Tokens;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.MultiTenancy.Controllers
{
    [Themed, Admin, OrchardFeature("Orchard.MultiTenancy.Commands")]
    public class CommandsController : Controller
    {
        private readonly ITenantService _tenantService;
        private readonly IOrchardHost _orchardHost;
        private readonly ShellSettings _shellSettings;

        public CommandsController(
            IOrchardServices services,
            ITenantService tenantService,
            IOrchardHost orchardHost,
            ShellSettings shellSettings)
        {
            _tenantService = tenantService;
            _orchardHost = orchardHost;
            _shellSettings = shellSettings;

            Services = services;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index()
        {
            return Execute();
        }

        public ActionResult Execute()
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
            {
                return new HttpUnauthorizedResult();
            }

            if (!EnsureDefaultTenant())
                return new HttpUnauthorizedResult();

            return View("Execute", new CommandsExecuteViewModel
            {
                 Tenants= _tenantService.GetTenants()
                 .Where(x => x.State == Environment.Configuration.TenantState.Running)
                 .Select(x => x.Name)
            });
        }

        [HttpPost]
        public ActionResult Execute(string tenant, string commandLine)
        {
            if(!Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
            {
                return new HttpUnauthorizedResult();
            }

            if (!EnsureDefaultTenant())
                return new HttpUnauthorizedResult();

            string output = "";

            var shellContext = _orchardHost.GetShellContext(tenant);

            if(shellContext == null)
            {
                return Json(new
                {
                    error = T("Tenant not found: {0}", tenant).Text,
                });
            }

            if (shellContext.Settings.State != TenantState.Running)
            {
                return Json(new
                {
                    error = T("Tenant not running: {0}", tenant).Text,
                });
            }

            using (var workContext = shellContext.LifetimeScope.CreateWorkContextScope())
            {
                var commandManager = workContext.Resolve<ICommandManager>();
                var transactionManager = workContext.Resolve<ITransactionManager>();
                var tokenizer = workContext.Resolve<ITokenizer>();

                try
                {
                    using (var writer = new StringWriter())
                    {
                        commandLine = tokenizer.Replace(commandLine, new Dictionary<string, object>());

                        var parameters = CommandParser.ParseCommandParameters(commandLine);
                        parameters.Output = writer;
                        commandManager.Execute(parameters);
                        output = writer.ToString();
                    }

                    return Json(new
                    {
                        data = output
                    });
                }
                catch (Exception exception)
                {
                    Logger.Error(T("Error executing command for tenant {0}: {1}", tenant, exception.Message).Text);

                    transactionManager.Cancel();
                    return Json(new
                    {
                        error = exception.Message,
                        data = output
                    });
                }
            }
        }

        private bool EnsureDefaultTenant()
        {
            return _shellSettings.Name == ShellSettings.DefaultName;
        }
    }
}