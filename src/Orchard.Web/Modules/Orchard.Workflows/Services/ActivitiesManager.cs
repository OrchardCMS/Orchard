using System.Collections.Generic;
using System.Linq;
using Orchard.Utility.Extensions;

namespace Orchard.Workflows.Services {
    public class ActivitiesManager : IActivitiesManager {
        private readonly IEnumerable<IActivity> _activities;

        public ActivitiesManager(IEnumerable<IActivity> activities) {
            _activities = activities;
        }

        public IEnumerable<IActivity> GetActivities() {
            return _activities.OrderBy(x => x.Name).ToReadOnlyCollection();
        }

        public IActivity GetActivityByName(string name) {
            return _activities.FirstOrDefault(x => x.Name == name);
        }
    }
}