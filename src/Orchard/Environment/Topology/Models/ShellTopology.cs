using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Topology.Models {

    /// <summary>
    /// Contains the information necessary to initialize an IoC container
    /// for a particular tenant. This model is created by the ICompositionStrategy
    /// and is passed into the IShellContainerFactory.
    /// </summary>
    public class ShellTopology {
        public IEnumerable<ModuleTopology> Modules { get; set; }
        public IEnumerable<DependencyTopology> Dependencies { get; set; }
        public IEnumerable<ControllerTopology> Controllers { get; set; }
        public IEnumerable<RecordTopology> Records { get; set; }
    }

    public class ShellTopologyItem {
        public Type Type { get; set; }
        public Feature Feature { get; set; }
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
