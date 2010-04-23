using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;

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

        public int RunCommand(TextReader input, TextWriter output, OrchardParameters args) {
            var agent = Activator.CreateInstance("Orchard.Framework", "Orchard.Commands.CommandHostAgent").Unwrap();
            int result = (int)agent.GetType().GetMethod("RunSingleCommand").Invoke(agent, new object[] { 
                input,
                output,
                args.Tenant,
                args.Arguments.ToArray(),
                args.Switches});

            return result;
        }
    }
}