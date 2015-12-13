using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Providers {
    public class SessionFactoryParameters : DataServiceParameters {
        public SessionFactoryParameters() {
            Configurers = Enumerable.Empty<ISessionConfigurationEvents>();
        }
        public IEnumerable<ISessionConfigurationEvents> Configurers { get; set; } 
        public IEnumerable<RecordBlueprint> RecordDescriptors { get; set; }
        public bool CreateDatabase { get; set; }
    }
}
