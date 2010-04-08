using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Arguments;

namespace Orchard {
    public interface IOrchardArgumentsParser {
        OrchardArguments Parse(ParserResult arguments);
    }

    public class OrchardArgumentsParser  : IOrchardArgumentsParser {

        public OrchardArguments Parse(ParserResult arguments) {
            var result = new OrchardArguments();

            foreach (var sw in arguments.Switches) {
                switch (sw.Name.ToLowerInvariant()) {
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
                }
            }

            return result;
        }
    }
}
