using Orchard.Commands;
using Orchard.Autoroute.Services;

namespace Orchard.Autoroute.Commands {
    public class AutorouteCommands : DefaultOrchardCommandHandler {
        private readonly IAutorouteService _autorouteService;
        
        public AutorouteCommands(IAutorouteService autorouteService) {
            _autorouteService = autorouteService;
        }

        [CommandHelp("autoroute create <content-type> <name> <pattern> <description> <isDefault>\r\n\t" + "Adds a new autoroute pattern to a specific content type")]
        [CommandName("autoroute create")]
        public void CreatePattern(string contentType, string name, string pattern, string description, bool isDefault) {
            _autorouteService.CreatePattern(contentType, name, pattern, description, isDefault);
        }
    }
}
