using System;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Orchard.Specs.Hosting.Orchard.Web;

namespace Orchard.Specs.Hosting {
    public class WebHostAgent : MarshalByRefObject
    {
        public SerializableDelegate<Action> Execute(SerializableDelegate<Action> shuttle)
        {
            shuttle.Delegate();
            return shuttle;
        }

        public void Shutdown() {
            HostingEnvironment.InitiateShutdown();
        }
    }
}
