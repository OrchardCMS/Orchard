using Orchard.Parameters;

namespace Orchard {
    public interface IOrchardParametersParser {
        OrchardParameters Parse(CommandParameters parameters);
    }
}