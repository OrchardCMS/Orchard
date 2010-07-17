using System;
using System.Web.Hosting;

namespace Orchard.Specs.Hosting {
    public class WebHostAgent : MarshalByRefObject
    {
        public SerializableDelegate<Action> Execute(SerializableDelegate<Action> shuttle)
        {
            shuttle.Delegate();
            return shuttle;
        }

        public void Shutdown() {
            //TODO: this line is required to properly shutdown the ASP.NET
            //      host and release memory when running multiple SpecFlow tests.
            //      However, nuint complains about an unhandled AppdomainUnloadedException
            //      When we figure out a way around this, we will re-enable this line.
            //HostingEnvironment.InitiateShutdown();
        }
    }
}
