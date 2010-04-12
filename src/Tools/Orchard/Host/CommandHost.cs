using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public void RunCommand(OrchardParameters args) {
            var agent = Activator.CreateInstance("Orchard.Framework", "Orchard.Commands.CommandHostAgent").Unwrap();
            agent.GetType().GetMethod("RunSingleCommand").Invoke(agent, new object[] { 
                Console.In,
                Console.Out,
                args.Tenant,
                args.Arguments.ToArray(),
                args.Switches});
        }
    }
}