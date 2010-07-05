using System.Collections.Generic;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Providers {
    public class SessionFactoryParameters : DataServiceParameters {
        public IEnumerable<RecordBlueprint> RecordDescriptors { get; set; }
        public bool CreateDatabase { get; set; }
    }
}
