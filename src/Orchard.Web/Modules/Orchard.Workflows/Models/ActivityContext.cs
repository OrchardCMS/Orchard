using System;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Models {
    public class ActivityContext {
        
        public IActivity Activity { get; set; }
        public ActivityRecord Record { get; set; }
        public Lazy<dynamic> State { private get; set; }
        
        public T GetState<T>(string key) {
            if (State == null || State.Value == null) {
                return default(T);
            }

            return State.Value[key];
        }
    }
}