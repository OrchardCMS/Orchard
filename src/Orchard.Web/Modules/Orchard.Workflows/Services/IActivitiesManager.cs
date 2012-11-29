using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Workflows.Models.Descriptors;

namespace Orchard.Workflows.Services {
    public interface IActivitiesManager : IDependency {
        IEnumerable<TypeDescriptor<ActivityDescriptor>> DescribeActivities();
    }

}