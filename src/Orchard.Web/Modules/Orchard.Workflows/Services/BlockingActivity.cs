using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Workflows.Models.Descriptors;

namespace Orchard.Workflows.Services {
    public abstract class BlockingActivity : IActivity {

        public abstract string Name { get; }
        public abstract LocalizedString Category { get; }
        public abstract LocalizedString Description { get; }

        public virtual bool IsBlocking {
            get { return true; }
        }

        public virtual string Form {
            get { return null; }
        }

        public abstract IEnumerable<LocalizedString> GetPossibleOutcomes(ActivityContext context);
        
        public virtual bool CanTransition(ActivityContext context) {
            return true;
        }

        public abstract LocalizedString Transition(ActivityContext context);
    }
}