using System.Collections.Generic;

namespace Orchard.Parameters {
    public interface ICommandParametersParser {
        CommandParameters Parse(IEnumerable<string> args);
    }
}