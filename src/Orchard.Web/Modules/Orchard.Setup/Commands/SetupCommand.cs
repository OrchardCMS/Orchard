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

        [CommandHelp("setup /SiteName:<siteName> /AdminUserName:<username> /AdminPassword:<password> /DatabaseProvider:<SQLite|SQLServer> " + 
            "/DatabaseConnectionString:<connection_string> /DatabaseTablePrefix:<table_prefix> /EnabledFeatures:<feature1,feature2,...>" +
            "\r\n\tRun first time setup for the site or for a given tenant")]
        [CommandName("setup")]
        [OrchardSwitches("SiteName,AdminUsername,AdminPassword,DatabaseProvider,DatabaseConnectionString,DatabaseTablePrefix,EnabledFeatures")]
        public void Setup() {
            var setupContext = new SetupContext {
                SiteName = this.SiteName,
                AdminUsername = this.AdminUsername,
                AdminPassword = this.AdminPassword,
                DatabaseProvider = this.DatabaseProvider,
                DatabaseConnectionString = this.DatabaseConnectionString,
                DatabaseTablePrefix = this.DatabaseTablePrefix,
                EnabledFeatures = this.EnabledFeatures.Split(',').Select(s => s.Trim())
            };

            _setupService.Setup(setupContext);

            Context.Output.WriteLine("Site \"{0}\" setup to run data provider \"{1}\" (with table prefix \"{2}\") with the following features enabled:",
                setupContext.SiteName,
                setupContext.DatabaseProvider,
                setupContext.DatabaseTablePrefix);

            foreach (var feature in setupContext.EnabledFeatures) {
                this.Context.Output.WriteLine("{0}", feature);
            }
        }
    }
}
