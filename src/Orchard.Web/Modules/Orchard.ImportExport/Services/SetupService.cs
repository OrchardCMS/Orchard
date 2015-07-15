using System;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.State;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Recipes.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Utility.Extensions;

namespace Orchard.ImportExport.Services
{
	public class SetupService : Component, ISetupService {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IProcessingEngine _processingEngine;

	    public SetupService(
            ShellSettings shellSettings,
            IOrchardHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IShellContainerFactory shellContainerFactory,
            ICompositionStrategy compositionStrategy,
            IProcessingEngine processingEngine) {

            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _compositionStrategy = compositionStrategy;
            _processingEngine = processingEngine;
        }
        
        public string Setup(SetupContext context) {
            string executionId;

            Logger.Information("Running setup for tenant '{0}'.", _shellSettings.Name);

            // The vanilla Orchard distibution has the following features enabled.
            string[] hardcoded = {
                // Framework
                "Orchard.Framework",
                // Core
                "Common", "Containers", "Contents", "Dashboard", "Feeds", "Navigation","Scheduling", "Settings", "Shapes", "Title",
                // Modules
                "Orchard.Pages", "Orchard.ContentPicker", "Orchard.Themes", "Orchard.Users", "Orchard.Roles", "Orchard.Modules", 
                "PackagingServices", "Orchard.Packaging", "Gallery", "Orchard.Recipes"
            };

            context.EnabledFeatures = hardcoded.Union(context.EnabledFeatures ?? Enumerable.Empty<string>()).Distinct().ToList();

            var shellSettings = new ShellSettings(_shellSettings);

            if (String.IsNullOrEmpty(shellSettings.DataProvider)) {
                shellSettings.DataProvider = context.DatabaseProvider;
                shellSettings.DataConnectionString = context.DatabaseConnectionString;
                shellSettings.DataTablePrefix = context.DatabaseTablePrefix;
            }
            
            shellSettings.EncryptionAlgorithm = "AES";
            shellSettings.EncryptionKey = SymmetricAlgorithm.Create(shellSettings.EncryptionAlgorithm).Key.ToHexString();
            shellSettings.HashAlgorithm = "HMACSHA256";
            shellSettings.HashKey = HMAC.Create(shellSettings.HashAlgorithm).Key.ToHexString();

            var shellDescriptor = new ShellDescriptor {
                Features = context.EnabledFeatures.Select(name => new ShellFeature { Name = name })
            };

            var shellBlueprint = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            // Initialize database explicitly, and store shell descriptor.
            using (var bootstrapLifetimeScope = _shellContainerFactory.CreateContainer(shellSettings, shellBlueprint)) {

                using (var environment = bootstrapLifetimeScope.CreateWorkContextScope()) {

                    // Check if the database is already created (in case an exception occured in the second phase).
                    var schemaBuilder = new SchemaBuilder(environment.Resolve<IDataMigrationInterpreter>());
                    var installationPresent = true;
                    try {
                        var tablePrefix = String.IsNullOrEmpty(shellSettings.DataTablePrefix) ? "" : shellSettings.DataTablePrefix + "_";
                        schemaBuilder.ExecuteSql("SELECT * FROM " + tablePrefix + "Settings_ShellDescriptorRecord");
                    }
                    catch {
                        installationPresent = false;
                    }

                    if (installationPresent) {
                        if (context.DropExistingTables) {
                            DropTenantDatabaseTables(environment);
                        }
                    }

                    // Make a workaround to avoid the Transaction issue for PostgreSQL.
                    environment.Resolve<ITransactionManager>().RequireNew();

                    schemaBuilder.CreateTable("Orchard_Framework_DataMigrationRecord", table => table
                        .Column<int>("Id", column => column.PrimaryKey().Identity())
                        .Column<string>("DataMigrationClass")
                        .Column<int>("Version"));

                    var dataMigrationManager = environment.Resolve<IDataMigrationManager>();
                    dataMigrationManager.Update("Settings");

                    foreach (var feature in context.EnabledFeatures) {
                        dataMigrationManager.Update(feature);
                    }

                    var descriptorManager = environment.Resolve<IShellDescriptorManager>();
                    descriptorManager.UpdateShellDescriptor(0, shellDescriptor.Features, shellDescriptor.Parameters);
                }
            }

            // In effect "pump messages" see PostMessage circa 1980.
            while ( _processingEngine.AreTasksPending() )
                _processingEngine.ExecuteNextTask();

            // Creating a standalone environment. 
            // In theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect.

            // Must mark state as Running - otherwise standalone environment is created "for setup".
            shellSettings.State = TenantState.Running;
            using (var environment = _orchardHost.CreateStandaloneEnvironment(shellSettings)) {
                try {
                    executionId = CreateTenantData(context, environment);
                }
                catch {
                    environment.Resolve<ITransactionManager>().Cancel();
                    throw;
                }
            }

            _shellSettingsManager.SaveSettings(shellSettings);
 
            return executionId;
        }

        private string CreateTenantData(SetupContext context, IWorkContextScope environment) {
            // Create superuser.
            var membershipService = environment.Resolve<IMembershipService>();
            var user = membershipService.CreateUser(
                new CreateUserParams(
                    context.AdminUsername, 
                    context.AdminPassword, 
                    email: String.Empty, 
                    passwordQuestion: String.Empty, 
                    passwordAnswer: String.Empty, 
                    isApproved: true));

            // Set superuser as current user for request (it will be set as the owner of all content items).
            var authenticationService = environment.Resolve<IAuthenticationService>();
            authenticationService.SetAuthenticatedUserForRequest(user);

            // Set site name and settings.
            var siteService = environment.Resolve<ISiteService>();
            var siteSettings = siteService.GetSiteSettings().As<SiteSettingsPart>();
            siteSettings.SiteSalt = Guid.NewGuid().ToString("N");
            siteSettings.SiteName = context.SiteName;
            siteSettings.SuperUser = context.AdminUsername;
            siteSettings.SiteCulture = "en-US";

            // Add default culture.
            var cultureManager = environment.Resolve<ICultureManager>();
            cultureManager.AddCulture("en-US");

            // Execute recipe
            var recipeParser = environment.Resolve<IRecipeParser>();
            var recipe = recipeParser.ParseRecipe(context.RecipeText);
            var recipeManager = environment.Resolve<IRecipeManager>();
            var executionId = recipeManager.Execute(recipe);

            // Null check: temporary fix for running setup in command line.
            if (HttpContext.Current != null) {
                authenticationService.SignIn(user, true);
            }

            return executionId;
        }

        private void DropTenantDatabaseTables(IWorkContextScope environment) {
            var sessionFactoryHolder = environment.Resolve<ISessionFactoryHolder>();
            var schemaBuilder = new SchemaBuilder(environment.Resolve<IDataMigrationInterpreter>());
            var configuration = sessionFactoryHolder.GetConfiguration();
            foreach (var mapping in configuration.ClassMappings) {
                try {
                    schemaBuilder.DropTable(mapping.Table.Name);
                }
                catch (Exception ex) {
                    Logger.Warning(ex, "Failed to drop table '{0}'.", mapping.Table.Name);
                }
            }
        }
    }
}