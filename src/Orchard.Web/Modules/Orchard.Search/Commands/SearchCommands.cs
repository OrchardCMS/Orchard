using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.Search.Models;

namespace Orchard.Search.Commands {
    public class SearchCommands : DefaultOrchardCommandHandler {
        private readonly IOrchardServices _orchardServices;

        public SearchCommands(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        [CommandName("search use")]
        [CommandHelp("search use <index>\r\n\t" + "Defines the default index to use for search")]
        public void Index(string index) {
            var settings = _orchardServices.WorkContext.CurrentSite.As<SearchSettingsPart>();

            if (string.IsNullOrWhiteSpace(index)) {
                Context.Output.WriteLine(T("Invalid index name."));
                return;
            }

            if (settings != null) {
                settings.SearchIndex = index;
            }
        }
    }
}