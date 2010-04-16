using System;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Extensions;
using Orchard.Extensions.Models;

namespace Orchard.Environment.Topology.Models {
    public class ShellTopology {
        public ShellSettings ShellSettings { get; set; }
        public IEnumerable<ModuleTopology> Modules { get; set; }
        public IEnumerable<DependencyTopology> Dependencies { get; set; }
        public IEnumerable<ControllerTopology> Controllers { get; set; }
        public IEnumerable<RecordTopology> Records { get; set; }
    }

    public class ShellTopologyItem {
        public Type Type { get; set; }
        public ExtensionDescriptor ExtensionDescriptor { get; set; }
        public FeatureDescriptor FeatureDescriptor { get; set; }
        public ExtensionEntry ExtensionEntry { get; set; }
    }

    public class ModuleTopology : ShellTopologyItem {
    }

    public class DependencyTopology : ShellTopologyItem {
        public IEnumerable<TopologyParameter> Parameters { get; set; }
    }

    public class ControllerTopology : ShellTopologyItem {
        public string AreaName { get; set; }
        public string ControllerName { get; set; }
    }

    public class RecordTopology : ShellTopologyItem {
        public string TableName { get; set; }
    }
}
