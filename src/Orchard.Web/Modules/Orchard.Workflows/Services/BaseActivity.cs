using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Workflows.Models.Descriptors;

namespace Orchard.Workflows.Services {
    public abstract class Task : IActivity {

        public abstract string Name { get; }
        public abstract LocalizedString Category { get; }
        public abstract LocalizedString Description { get; }

        public virtual bool IsEvent {
            get { return false; }
        }

        public bool CanStartWorkflow { get { return false; } }

        public virtual string Form {
            get { return null; }
        }

        public abstract IEnumerable<LocalizedString> GetPossibleOutcomes(ActivityContext context);

        public virtual bool CanExecute(ActivityContext context) {
            return true;
        }

        public abstract IEnumerable<LocalizedString> Execute(ActivityContext context);

        public virtual void Touch(dynamic state) {

        }

    }
}