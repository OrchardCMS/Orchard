using System;

namespace Orchard.Dashboards.Services {
    public class DashboardDescriptor {
        public int Priority { get; set; }
        public Func<dynamic, dynamic> DashboardFactory { get; set; }
    }
}