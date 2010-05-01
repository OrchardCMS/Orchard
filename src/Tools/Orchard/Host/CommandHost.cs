using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Orchard.Parameters;
using Orchard.ResponseFiles;

namespace Orchard.Host {
    class CommandHost : MarshalByRefObject, IRegisteredObject {
        public CommandHost() {
            HostingEnvironment.RegisterObject(this);
        }

        public override object InitializeLifetimeService() {
            // never expire the license
            return null;
        }

        public void Stop(bool immediate) {
            //TODO
        }

        public int RunCommand(TextReader input, TextWriter output, Logger logger, OrchardParameters args) {
            var agent = Activator.CreateInstance("Orchard.Framework", "Orchard.Commands.CommandHostAgent").Unwrap();
            int result = (int)agent.GetType().GetMethod("RunSingleCommand").Invoke(agent, new object[] { 
                input,
                output,
                args.Tenant,
                args.Arguments.ToArray(),
                args.Switches});

            return result;
        }

        public int RunCommands(TextReader input, TextWriter output, Logger logger, IEnumerable<ResponseLine> responseLines) {
            var agent = Activator.CreateInstance("Orchard.Framework", "Orchard.Commands.CommandHostAgent").Unwrap();

            int result = (int)agent.GetType().GetMethod("StartHost").Invoke(agent, new object[] { input, output });
            if (result != 0)
                return result;

            foreach (var line in responseLines) {
                logger.LogInfo("{0} ({1}): Running command: {2}", line.Filename, line.LineNumber, line.LineText);

                var args = new OrchardParametersParser().Parse(new CommandParametersParser().Parse(line.Args));

                result = (int)agent.GetType().GetMethod("RunCommand").Invoke(agent, new object[] { 
                    input,
                    output,
                    args.Tenant,
                    args.Arguments.ToArray(),
                    args.Switches});

                if (result != 0) {
                    output.WriteLine("{0} ({1}): Command returned error ({2})", line.Filename, line.LineNumber, result);
                    return result;
                }
            }

            result = (int)agent.GetType().GetMethod("StopHost").Invoke(agent, new object[] { input, output });
            return result;
        }
    }
}
