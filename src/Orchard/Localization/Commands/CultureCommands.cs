using System.Linq;
using JetBrains.Annotations;
using Orchard.Commands;
using Orchard.Localization.Services;
using Orchard.Settings;

namespace Orchard.Localization.Commands {
    public class CultureCommands : DefaultOrchardCommandHandler {
        private readonly ICultureManager _cultureManager;

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public CultureCommands(ICultureManager cultureManager) {
            _cultureManager = cultureManager;
        }

        [CommandHelp("cultures list \r\n\t" + "List site cultures")]
        [CommandName("cultures list")]
        public void ListCultures() {
            Context.Output.WriteLine(T("Listing Cultures:"));

            string cultures = _cultureManager.ListCultures().Aggregate<string, string>(null, (current, culture) => current + culture + " ");

            Context.Output.WriteLine(cultures);
        }


        [CommandHelp("cultures get site culture \r\n\t" + "Get culture for the site")]
        [CommandName("cultures get site culture")]
        public void GetSiteCulture() {
            Context.Output.WriteLine(T("Site Culture is {0}", CurrentSite.SiteCulture));
        }

        [CommandHelp("cultures set site culture <culture-name> \r\n\t" + "Set culture for the site")]
        [CommandName("cultures set site culture")]
        public void SetSiteCulture(string cultureName) {
            Context.Output.WriteLine(T("Setting site culture to {0}", cultureName));

            if (!_cultureManager.IsValidCulture(cultureName)) {
                Context.Output.WriteLine(T("Supplied culture name {0} is not valid.", cultureName));
                return;
            }
            CurrentSite.SiteCulture = cultureName;

            Context.Output.WriteLine(T("Site culture set to {0} successfully", cultureName));
        }
    }
}

