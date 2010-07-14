using System.Collections.Generic;
using System.Linq;
using Orchard.Commands;
using Orchard.Setup.Services;

namespace Orchard.Setup.Commands {
    public class SetupCommand : DefaultOrchardCommandHandler {
        private readonly ISetupService _setupService;

        public SetupCommand(ISetupService setupService) {
            _setupService = setupService;
        }

        [OrchardSwitch]
        public string SiteName { get; set; }

        [OrchardSwitch]
        public string AdminUsername { get; set; }

        [OrchardSwitch]
        public string AdminPassword { get; set; }

        [OrchardSwitch]
        public string DatabaseProvider { get; set; }

        [OrchardSwitch]
        public string DatabaseConnectionString { get; set; }

        [OrchardSwitch]
        public string DatabaseTablePrefix { get; set; }

        [OrchardSwitch]
        public string EnabledFeatures { get; set; }

        [CommandHelp("setup /SiteName:<siteName> /AdminUserName:<username> /AdminPassword:<password> /DatabaseProvider:<SqlCe|SQLServer> " + 
            "/DatabaseConnectionString:<connection_string> /DatabaseTablePrefix:<table_prefix> /EnabledFeatures:<feature1,feature2,...>" +
            "\r\n\tRun first time setup for the site or for a given tenant")]
        [CommandName("setup")]
        [OrchardSwitches("SiteName,AdminUsername,AdminPassword,DatabaseProvider,DatabaseConnectionString,DatabaseTablePrefix,EnabledFeatures")]
        public void Setup() {
            IEnumerable<string> enabledFeatures = null;
            if (!string.IsNullOrEmpty(this.EnabledFeatures)) {
                enabledFeatures = this.EnabledFeatures
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));
            }

            var setupContext = new SetupContext {
                SiteName = this.SiteName,
                AdminUsername = this.AdminUsername,
                AdminPassword = this.AdminPassword,
                DatabaseProvider = this.DatabaseProvider,
                DatabaseConnectionString = this.DatabaseConnectionString,
                DatabaseTablePrefix = this.DatabaseTablePrefix,
                EnabledFeatures = enabledFeatures
            };

            _setupService.Setup(setupContext);

            Context.Output.WriteLine(T("Site \"{0}\" sucessfully setup to run data provider \"{1}\" (with table prefix \"{2}\").",
                setupContext.SiteName,
                setupContext.DatabaseProvider,
                setupContext.DatabaseTablePrefix));
        }
    }
}
