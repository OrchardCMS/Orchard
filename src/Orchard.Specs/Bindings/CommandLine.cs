using System.IO;
using System.Linq;
using Orchard.Commands;
using Orchard.Parameters;
using Orchard.Specs.Hosting;
using TechTalk.SpecFlow;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class CommandLine : BindingBase {
        [When(@"I execute >(.*)")]
        public void WhenIExecute(string commandLine) {
            var details = new RequestDetails();
            Binding<WebAppHosting>().Host.Execute(() => {
                var args = new CommandLineParser().Parse(commandLine);
                var parameters = new CommandParametersParser().Parse(args);
                var agent = new CommandHostAgent();
                var input = new StringReader("");
                var output = new StringWriter();
                details.StatusCode = (int)agent.RunSingleCommand(
                    input,
                    output,
                    "Default",
                    parameters.Arguments.ToArray(),
                    parameters.Switches.ToDictionary(kv => kv.Key, kv => kv.Value));
                details.StatusDescription = details.StatusCode.ToString();
                details.ResponseText = output.ToString();
            });

            Binding<WebAppHosting>().Details = details;
        }
    }
}
