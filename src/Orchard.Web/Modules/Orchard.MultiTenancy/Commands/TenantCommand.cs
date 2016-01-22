using System;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Commands;
using Orchard.Environment.Configuration;
using Orchard.MultiTenancy.Services;

namespace Orchard.MultiTenancy.Commands {
    public class TenantCommand : DefaultOrchardCommandHandler {
        private readonly ITenantService _tenantService;

        public TenantCommand(ITenantService tenantService) {
            _tenantService = tenantService;
        }

        [OrchardSwitch]
        public string Host { get; set; }

        [OrchardSwitch]
        public string UrlPrefix { get; set; }

        [CommandHelp("tenant list\r\n\t" + "Display current tenants of a site")]
        [CommandName("tenant list")]
        public void List() {
            Context.Output.WriteLine(T("List of tenants"));
            Context.Output.WriteLine(T("---------------------------"));

            var tenants = _tenantService.GetTenants();
            foreach (var tenant in tenants) {
                Context.Output.WriteLine(T("Name: ") + tenant.Name);
                Context.Output.WriteLine(T("Provider: ") + tenant.DataProvider);
                Context.Output.WriteLine(T("ConnectionString: ") + tenant.DataConnectionString);
                Context.Output.WriteLine(T("Data Table Prefix: ") + tenant.DataTablePrefix);
                Context.Output.WriteLine(T("Request Url Host: ") + tenant.RequestUrlHost);
                Context.Output.WriteLine(T("Request Url Prefix: ") + tenant.RequestUrlPrefix);
                Context.Output.WriteLine(T("State: ") + tenant.State.ToString());
                Context.Output.WriteLine(T("---------------------------"));
            }
        }

        [CommandHelp("tenant add <tenantName> /Host:<hostname> /UrlPrefix:<url prefix>\r\n\t" + 
            "Create new tenant named <tenantName> on the site")]
        [CommandName("tenant add")]
        [OrchardSwitches("Host,UrlPrefix")]
        public void Create(string tenantName) {
            Context.Output.WriteLine(T("Creating tenant"));

            if (string.IsNullOrWhiteSpace(tenantName) || !Regex.IsMatch(tenantName, @"^\w+$")) {
                Context.Output.WriteLine(T("Invalid tenant name. Must contain characters only and no spaces."));
                return;
            }
            if (_tenantService.GetTenants().Any(tenant => string.Equals(tenant.Name, tenantName, StringComparison.OrdinalIgnoreCase))) {
                Context.Output.WriteLine(T("Could not create tenant \"{0}\". A tenant with the same name already exists.", tenantName));
                return;
            }

            _tenantService.CreateTenant(
                    new ShellSettings {
                        Name = tenantName,
                        RequestUrlHost = Host,
                        RequestUrlPrefix = UrlPrefix,
                        State = TenantState.Uninitialized
                    });
        }

        [CommandHelp("tenant info <tenantName>\r\n\t" + "Display settings for a tenant")]
        [CommandName("tenant info")]
        public void Info(string tenantName) {
            ShellSettings tenant = _tenantService.GetTenants().Where(x => x.Name == tenantName).FirstOrDefault();

            if (tenant == null) {
                Context.Output.Write(T("Tenant: ") + tenantName + T(" was not found"));
            }
            else {
                Context.Output.WriteLine(T("Tenant Settings:"));
                Context.Output.WriteLine(T("---------------------------"));
                Context.Output.WriteLine(T("Name: ") + tenant.Name);
                Context.Output.WriteLine(T("Provider: ") + tenant.DataProvider);
                Context.Output.WriteLine(T("ConnectionString: ") + tenant.DataConnectionString);
                Context.Output.WriteLine(T("Data Table Prefix: ") + tenant.DataTablePrefix);
                Context.Output.WriteLine(T("Request Url Host: ") + tenant.RequestUrlHost);
                Context.Output.WriteLine(T("Request Url Prefix: ") + tenant.RequestUrlPrefix);
                Context.Output.WriteLine(T("State: ") + tenant.State.ToString());
                Context.Output.WriteLine(T("---------------------------"));
            }
        }
    }
}
