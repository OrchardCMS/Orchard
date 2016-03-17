using System.Collections.Generic;

namespace Orchard.Workflows.Services {
    public interface IActivitiesManager : IDependency {
        IEnumerable<IActivity> GetActivities();
        IActivity GetActivityByName(string name);
    }

}