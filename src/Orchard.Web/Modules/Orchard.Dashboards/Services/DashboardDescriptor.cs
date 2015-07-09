using System;
using Orchard.ContentManagement;

namespace Orchard.Dashboards.Services {
    public class DashboardDescriptor {
        public int Priority { get; set; }
        public Func<dynamic, dynamic> Display { get; set; }
        public Func<dynamic, dynamic> Editor { get; set; }
        public Func<dynamic, IUpdateModel, dynamic> UpdateEditor { get; set; }
    }
}