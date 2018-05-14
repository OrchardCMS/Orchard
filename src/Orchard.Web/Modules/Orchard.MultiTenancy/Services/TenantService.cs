using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.ShellBuilders;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Data.Providers;
using Orchard.Data;
using Orchard.Logging;

namespace Orchard.MultiTenancy.Services {
    public class TenantService : ITenantService {
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IShellContainerFactory _shellContainerFactory;

        public TenantService(
            IShellSettingsManager shellSettingsManager,
            IExtensionManager extensionManager,
            IShellContextFactory shellContextFactory,
            IShellContainerFactory shellContainerFactory) {
            _shellSettingsManager = shellSettingsManager;
            _extensionManager = extensionManager;
            _shellContextFactory = shellContextFactory;
            _shellContainerFactory = shellContainerFactory;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<ShellSettings> GetTenants() {
            return _shellSettingsManager.LoadSettings();
        }

        public void CreateTenant(ShellSettings settings) {
            _shellSettingsManager.SaveSettings(settings);
        }

        public void UpdateTenant(ShellSettings settings) {
            _shellSettingsManager.SaveSettings(settings);
        }

        public void ResetTenant(ShellSettings settings, bool dropDatabaseTables, bool force) {
            if (settings.State == TenantState.Uninitialized)
                return;
            if (settings.State == TenantState.Invalid)
                throw new InvalidOperationException(String.Format("Tenant reset action cannot be performed when tenant state is '{0}'.", settings.State));
            if (!force && settings.State != TenantState.Disabled)
                throw new InvalidOperationException(String.Format("Tenant state is '{0}'; must be '{1}' to perform reset action. The 'force' option can be used to override this.", settings.State, TenantState.Disabled));

            ExecuteOnTenantScope(settings, environment => {
                ExecuteResetEventHandlers(environment);
                if (dropDatabaseTables)
                    DropTenantDatabaseTables(environment);
            });

            settings.State = TenantState.Uninitialized;
            _shellSettingsManager.SaveSettings(settings);
        }

        public IEnumerable<string> GetTenantDatabaseTableNames(ShellSettings settings) {
            IEnumerable<string> result = null;
            ExecuteOnTenantScope(settings, environment => {
                result = GetTenantDatabaseTableNames(environment);
            });
            return result;
        }

        public IEnumerable<ExtensionDescriptor> GetInstalledThemes() {
            return GetThemes(_extensionManager.AvailableExtensions());
        }

        public IEnumerable<ExtensionDescriptor> GetInstalledModules() {
            return _extensionManager.AvailableExtensions().Where(descriptor => DefaultExtensionTypes.IsModule(descriptor.ExtensionType));
        }

        private IEnumerable<ExtensionDescriptor> GetThemes(IEnumerable<ExtensionDescriptor> extensions) {
            var themes = new List<ExtensionDescriptor>();
            foreach (var descriptor in extensions) {

                if (!DefaultExtensionTypes.IsTheme(descriptor.ExtensionType)) {
                    continue;
                }

                ExtensionDescriptor theme = descriptor;

                if (theme.Tags == null || !theme.Tags.Contains("hidden")) {
                    themes.Add(theme);
                }
            }
            return themes;
        }

        private void ExecuteOnTenantScope(ShellSettings settings, Action<IWorkContextScope> action) {
            var shellContext = _shellContextFactory.CreateShellContext(settings);
            using (var container = _shellContainerFactory.CreateContainer(shellContext.Settings, shellContext.Blueprint)) {
                using (var environment = container.CreateWorkContextScope()) {
                    action(environment);
                }
            }
        }

        private IEnumerable<string> GetTenantDatabaseTableNames(IWorkContextScope environment) {
            var shellSettings = environment.Resolve<ShellSettings>();
            var sqlStatementProviders = environment.Resolve<IEnumerable<ISqlStatementProvider>>();
            var transactionManager = environment.Resolve<ITransactionManager>();
            var session = transactionManager.GetSession();
            var tenants = GetTenants().Where(x => x.Name != shellSettings.Name);

            string command = null;
            IEnumerable<string> result = null;

            foreach (var sqlStatementProvider in sqlStatementProviders) {
                if (!String.Equals(sqlStatementProvider.DataProvider, shellSettings.DataProvider)) {
                    continue;
                }

                command = sqlStatementProvider.GetStatement("table_names") ?? command;
            }

            if (command != null) {
                var tableNames = session.CreateSQLQuery(command).List<string>();

                if (string.IsNullOrWhiteSpace(shellSettings.DataTablePrefix)) {
                    // If current tenant doesn't has table prefix, then exclude all tables which have prefixes for other tenants
                    result = tableNames.Where(table => !tenants.Any(tenant => table.StartsWith(tenant.DataTablePrefix + "_")));
                }
                else {
                    // If current tenant has table prefix, then filter tables which have the right prefix
                    result = tableNames.Where(table => table.StartsWith(shellSettings.DataTablePrefix + "_"));
                }
            }

            return (result ?? Enumerable.Empty<string>()).OrderBy(x => x).ToList();
        }

        private void DropTenantDatabaseTables(IWorkContextScope environment) {
            var tableNames = GetTenantDatabaseTableNames(environment);
            var schemaBuilder = new SchemaBuilder(environment.Resolve<IDataMigrationInterpreter>());

            foreach (var tableName in tableNames) {
                try {
                    schemaBuilder.DropTable(schemaBuilder.RemoveDataTablePrefix(tableName));
                }
                catch (Exception ex) {
                    Logger.Warning(ex, "Failed to drop table '{0}'.", tableName);
                }
            }
        }

        private void ExecuteResetEventHandlers(IWorkContextScope environment) {
            var handler = environment.Resolve<ITenantResetEventHandler>();
            handler.Resetting();
        }
    }
}