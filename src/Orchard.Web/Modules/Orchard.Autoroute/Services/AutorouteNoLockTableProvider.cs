using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data.Providers;

namespace Orchard.Autoroute.Services {
    public class AutorouteNoLockTableProvider : INoLockTableProvider {
        public IEnumerable<string> GetTableNames() {
            return new string[] { "Orchard_Autoroute_AutoroutePartRecord" };
        }
    }
}