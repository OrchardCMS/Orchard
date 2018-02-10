using System;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Commands;
using Orchard.Environment.Configuration;
using Orchard.MultiTenancy.Services;

namespace Orchard.MultiTenancy.Commands {
    public class TenantCommand : DefaultOrchardCommandHandler {
        private readonly ITenantService _tenantService;
        private readonly string[] _validDataProviderNames = new[] { "SqlCe", "SqlServer", "MySql", "PostgreSql" };

        public TenantCommand(ITenantService tenantService) {
            _tenantService = tenantService;
        }

        [OrchardSwitch]
        public string DataProvider { get; set; }
        [OrchardSwitch]
        public string DataConnectionString { get; set; }
        [OrchardSwitch]
        public string DataTablePrefix { get; set; }
        [OrchardSwitch]
        public string UrlHost { get; set; }
        [OrchardSwitch]
        public string UrlPrefix { get; set; }
        [OrchardSwitch]
        public string Themes { get; set; }
        [OrchardSwitch]
        public string Modules { get; set; }
        [OrchardSwitch]
        public bool DropDatabaseTables { get; set; }
        [OrchardSwitch]
        public bool Force { get; set; }

        [CommandHelp("tenant list\r\n\t" + "Display current tenants of the site.")]
        [CommandName("tenant list")]
        public void List() {
            Context.Output.WriteLine(T("List of tenants"));
            Context.Output.WriteLine(T("---------------------------"));

            var tenants = _tenantService.GetTenants();
            foreach (var tenant in tenants) {
                Context.Output.WriteLine(T("Name: ") + tenant.Name);
                Context.Output.WriteLine(T("State: ") + tenant.State.ToString());
                Context.Output.WriteLine(T("Data provider: ") + tenant.DataProvider);
                Context.Output.WriteLine(T("Connection string: ") + tenant.DataConnectionString);
                Context.Output.WriteLine(T("Data table prefix: ") + tenant.DataTablePrefix);
                Context.Output.WriteLine(T("Request URL host: ") + tenant.RequestUrlHost);
                Context.Output.WriteLine(T("Request URL prefix: ") + tenant.RequestUrlPrefix);
                Context.Output.WriteLine(T("Encryption algorithm: ") + tenant.EncryptionAlgorithm);
                Context.Output.WriteLine(T("Encryption key: ") + tenant.EncryptionKey);
                Context.Output.WriteLine(T("Hash algorithm: ") + tenant.HashAlgorithm);
                Context.Output.WriteLine(T("Hash key: ") + tenant.HashKey);
                Context.Output.WriteLine(T("Themes: ") + String.Join(";", tenant.Themes));
                Context.Output.WriteLine(T("Modules: ") + String.Join(";", tenant.Modules));
                Context.Output.WriteLine(T("---------------------------"));
            }
        }

        [CommandHelp("tenant info <tenantName>\r\n\t" + "Display the current settings for a tenant.")]
        [CommandName("tenant info")]
        public void Info(string tenantName) {
            var tenant = _tenantService.GetTenants().FirstOrDefault(x => x.Name == tenantName);
            if (tenant == null) {
                Context.Output.WriteLine(T("Could not read tenant '{0}'. No tenant with that name exists.", tenantName));
                return;
            }

            Context.Output.WriteLine(T("Tenant settings:"));
            Context.Output.WriteLine(T("---------------------------"));
            Context.Output.WriteLine(T("Name: ") + tenant.Name);
            Context.Output.WriteLine(T("State: ") + tenant.State.ToString());
            Context.Output.WriteLine(T("Data provider: ") + tenant.DataProvider);
            Context.Output.WriteLine(T("Connection string: ") + tenant.DataConnectionString);
            Context.Output.WriteLine(T("Data table prefix: ") + tenant.DataTablePrefix);
            Context.Output.WriteLine(T("Request URL host: ") + tenant.RequestUrlHost);
            Context.Output.WriteLine(T("Request URL prefix: ") + tenant.RequestUrlPrefix);
            Context.Output.WriteLine(T("Encryption algorithm: ") + tenant.EncryptionAlgorithm);
            Context.Output.WriteLine(T("Encryption key: ") + tenant.EncryptionKey);
            Context.Output.WriteLine(T("Hash algorithm: ") + tenant.HashAlgorithm);
            Context.Output.WriteLine(T("Hash key: ") + tenant.HashKey);
            Context.Output.WriteLine(T("Themes: ") + String.Join(";", tenant.Themes));
            Context.Output.WriteLine(T("Modules: ") + String.Join(";", tenant.Modules));
            Context.Output.WriteLine(T("---------------------------"));
        }

        [CommandHelp("tenant add <tenantName> /DataProvider:<provider> /DataConnectionString:<connectionString> /DataTablePrefix:<prefix> /UrlHost:<hostname> /UrlPrefix:<prefix> /Themes:<themes> /Modules:<modules>\r\n\t" + "Create a new tenant named <tenantName> on the site.\r\n" + "The <themes> and <modules> parameters should be semicolon-separated lists of module names.")]
        [CommandName("tenant add")]
        [OrchardSwitches("DataProvider,DataConnectionString,DataTablePrefix,UrlHost,UrlPrefix,Themes,Modules")]
        public void Create(string tenantName) {
            Context.Output.WriteLine(T("Creating tenant '{0}'...", tenantName));

            if (String.IsNullOrWhiteSpace(tenantName) || !Regex.IsMatch(tenantName, @"^[a-zA-Z]\w*$")) {
                Context.Output.WriteLine(T("Invalid tenant name. Must contain characters only and no spaces."));
                return;
            }
            if (_tenantService.GetTenants().Any(tenant => String.Equals(tenant.Name, tenantName, StringComparison.OrdinalIgnoreCase))) {
                Context.Output.WriteLine(T("Could not create tenant '{0}'. A tenant with the same name already exists.", tenantName));
                return;
            }

            if (DataProvider != null && !_validDataProviderNames.Contains(DataProvider)) {
                Context.Output.WriteLine(T("Invalid value '{0}' for parameter DataProvider. Expect one of the following: {1}", DataProvider, String.Join(", ", _validDataProviderNames)));
                return;
            }

            _tenantService.CreateTenant(
                new ShellSettings {
                    Name = tenantName,
                    State = TenantState.Uninitialized,
                    DataProvider = DataProvider,
                    DataConnectionString = DataConnectionString,
                    DataTablePrefix = DataTablePrefix,
                    RequestUrlHost = UrlHost,
                    RequestUrlPrefix = UrlPrefix,
                    Themes = Themes != null ? Themes.Split(';') : new string[] { },
                    Modules = Modules != null ? Modules.Split(';') : new string[] { }
                });
        }

        [CommandHelp("tenant update <tenantName> /DataProvider:<SqlCe|SqlServer|MySql|PostgreSql> /DataConnectionString:<connectionString> /DataTablePrefix:<prefix> /UrlHost:<hostname> /UrlPrefix:<prefix> /Themes:<themes> /Modules:<modules>\r\n\t" + "Update the settings of the existing tenant <tenantName>.\r\n" + "The <themes> and <modules> parameters should be semicolon-separated lists of module names.")]
        [CommandName("tenant update")]
        [OrchardSwitches("DataProvider,DataConnectionString,DataTablePrefix,UrlHost,UrlPrefix,Themes,Modules")]
        public void Edit(string tenantName) {
            Context.Output.WriteLine(T("Updating tenant '{0}'...", tenantName));

            var tenant = _tenantService.GetTenants().FirstOrDefault(t => String.Equals(t.Name, tenantName, StringComparison.OrdinalIgnoreCase));
            if (tenant == null) {
                Context.Output.WriteLine(T("Could not update tenant '{0}'. No tenant with that name exists.", tenantName));
                return;
            }

            if (DataProvider != null && !_validDataProviderNames.Contains(DataProvider)) {
                Context.Output.WriteLine(T("Invalid value '{0}' for parameter DataProvider. Expect one of the following: {1}", DataProvider, String.Join(", ", _validDataProviderNames)));
                return;
            }

            _tenantService.UpdateTenant(
                new ShellSettings {
                    Name = tenant.Name,
                    State = tenant.State,
                    DataProvider = DataProvider ?? tenant.DataProvider,
                    DataConnectionString = DataConnectionString ?? tenant.DataConnectionString,
                    DataTablePrefix = DataTablePrefix ?? tenant.DataTablePrefix,
                    RequestUrlHost = UrlHost ?? tenant.RequestUrlHost,
                    RequestUrlPrefix = UrlPrefix ?? tenant.RequestUrlPrefix,
                    Themes = Themes != null ? Themes.Split(';') : tenant.Themes,
                    Modules = Modules != null ? Modules.Split(';') : tenant.Modules
                });
        }

        [CommandHelp("tenant disable <tenantName>\r\n\t" + "Disable the tenant <tenantName>.")]
        [CommandName("tenant disable")]
        public void Disable(string tenantName) {
            Context.Output.WriteLine(T("Disabling tenant '{0}'...", tenantName));

            var tenant = _tenantService.GetTenants().FirstOrDefault(t => String.Equals(t.Name, tenantName, StringComparison.OrdinalIgnoreCase));
            if (tenant == null) {
                Context.Output.WriteLine(T("Could not disable tenant '{0}'. No tenant with that name exists.", tenantName));
                return;
            }

            tenant.State = TenantState.Disabled;
            _tenantService.UpdateTenant(tenant);
        }

        [CommandHelp("tenant enable <tenantName>\r\n\t" + "Enable the tenant <tenantName>.")]
        [CommandName("tenant enable")]
        public void Enable(string tenantName) {
            Context.Output.WriteLine(T("Enabling tenant '{0}'...", tenantName));

            var tenant = _tenantService.GetTenants().FirstOrDefault(t => String.Equals(t.Name, tenantName, StringComparison.OrdinalIgnoreCase));
            if (tenant == null) {
                Context.Output.WriteLine(T("Could not enable tenant '{0}'. No tenant with that name exists.", tenantName));
                return;
            }

            tenant.State = TenantState.Running;
            _tenantService.UpdateTenant(tenant);
        }

        [CommandHelp("tenant reset <tenantName> /DropDatabaseTables:true|false /Force:true|false\r\n\t" + "Reset the tenant <tenantName> to its uninitialized, optionally dropping its tables from the database.")]
        [CommandName("tenant reset")]
        [OrchardSwitches("DropDatabaseTables,Force")]
        public void Reset(string tenantName) {
            Context.Output.WriteLine(T("Resetting tenant '{0}'...", tenantName));

            var tenant = _tenantService.GetTenants().FirstOrDefault(t => String.Equals(t.Name, tenantName, StringComparison.OrdinalIgnoreCase));
            if (tenant == null) {
                Context.Output.WriteLine(T("Could not reset tenant '{0}'. No tenant with that name exists.", tenantName));
                return;
            }

            _tenantService.ResetTenant(tenant, DropDatabaseTables, Force);
        }
    }
}
