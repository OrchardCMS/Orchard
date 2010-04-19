using System;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Topology.Models {
    public class ShellTopology {
        public IEnumerable<ModuleTopology> Modules { get; set; }
        public IEnumerable<DependencyTopology> Dependencies { get; set; }
        public IEnumerable<ControllerTopology> Controllers { get; set; }
        public IEnumerable<RecordTopology> Records { get; set; }
    }

    public class ShellTopologyItem {
        public Type Type { get; set; }
        public Feature Feature { get; set; }
        public FeatureDescriptor FeatureDescriptor { get; set; }
    }

    public class ModuleTopology : ShellTopologyItem {
    }

    public class DependencyTopology : ShellTopologyItem {
        public IEnumerable<ShellParameter> Parameters { get; set; }
    }

    public class ControllerTopology : ShellTopologyItem {
        public string AreaName { get; set; }
        public string ControllerName { get; set; }
    }

    public class RecordTopology : ShellTopologyItem {
        public string TableName { get; set; }
    }
}
