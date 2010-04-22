using System.Linq;
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

        [OrchardSwitch]
        public string ConnectionString { get; set; } 

        [CommandHelp("tenant list: Display current tenants of a site")]
        [CommandName("tenant list")]
        public void List() {
            Context.Output.WriteLine(T("List of tenants"));
            Context.Output.WriteLine(T("---------------------------"));

            var tenants = _tenantService.GetTenants();
            foreach (var tenant in tenants) {
                Context.Output.WriteLine(T("Name: ") + tenant.Name);
                Context.Output.WriteLine(T("Provider: ") + tenant.DataProvider);
                Context.Output.WriteLine(T("ConnectionString: ") + tenant.DataConnectionString);
                Context.Output.WriteLine(T("Prefix: ") + tenant.DataPrefix);
                Context.Output.WriteLine(T("---------------------------"));
            }
        }

        [CommandHelp("tenant add <tenantName> <providerName> <dataPrefix> /ConnectionString:<SQL connection string> /Host:<hostname> /UrlPrefix:<url prefix>" + 
            ": create new tenant named <tenantName> on the site")]
        [CommandName("tenant add")]
        public void Create(string tenantName, string providerName, string prefix) {
            Context.Output.WriteLine(T("Creating tenant"));
            _tenantService.CreateTenant(
                    new ShellSettings {
                        Name = tenantName,
                        DataProvider = providerName,
                        DataConnectionString = ConnectionString,
                        DataPrefix = prefix
                    });
        }

        [CommandHelp("tenant info <tenantName>: Display settings for a tenant")]
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
                Context.Output.WriteLine(T("Prefix: ") + tenant.DataPrefix);
                Context.Output.WriteLine(T("---------------------------"));
            }
        }
    }
}
