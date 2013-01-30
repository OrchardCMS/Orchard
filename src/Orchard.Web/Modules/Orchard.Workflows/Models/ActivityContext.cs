using System;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Models {
    public class ActivityContext {
        
        public IActivity Activity { get; set; }
        public ActivityRecord Record { get; set; }
        public Lazy<dynamic> State { private get; set; }
        
        public T GetState<T>(string key) {
            return State.Value[key];
        }
    }
}