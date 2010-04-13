using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Parameters {
    public class CommandParametersParser : ICommandParametersParser {
        public CommandParameters Parse(IEnumerable<string> args) {
            var result = new CommandParameters {
                Arguments = new List<string>(),
                Switches = new Dictionary<string, string>()
            };

            foreach (var arg in args) {
                if (arg[0] == '/') {
                    var switchKeyValue = arg.Substring(1).Split(':');
                    result.Switches.Add(switchKeyValue[0], switchKeyValue.Length >= 2 ? switchKeyValue[1] : string.Empty);
                }
                else {
                    result.Arguments.Add(arg);
                }
            }

            return result;
        }
    }
}