using System.Collections.Generic;
using Orchard.Parameters;

namespace Orchard {
    public class OrchardParametersParser : IOrchardParametersParser {

        public OrchardParameters Parse(CommandParameters parameters) {

            var result = new OrchardParameters {
                Arguments = parameters.Arguments,
                Switches = new Dictionary<string, string>()
            };

            foreach (var sw in parameters.Switches) {
                switch (sw.Key.ToLowerInvariant()) {
                    case "wd":
                    case "workingdirectory":
                        result.WorkingDirectory = sw.Value;
                        break;

                    case "v":
                    case "verbose":
                        result.Verbose = bool.Parse(sw.Value);
                        break;

                    case "vp":
                    case "virtualpath":
                        result.VirtualPath = sw.Value;
                        break;

                    case "t":
                    case "tenant":
                        result.Tenant = sw.Value;
                        break;

                    default:
                        result.Switches.Add(sw.Key, sw.Value);
                        break;
                }
            }

            return result;
        }
    }
}
