using System;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    public class LoggingActivity : Task {

        public LoggingActivity() {
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public override string Name {
            get { return "Logging"; }
        }

        public override LocalizedString Category {
            get { return T("Diagnostics"); }
        }

        public override LocalizedString Description {
            get { return T("Creates an entry in the application's logs.");  }
        }

        public override string Form {
            get { return "ActivityLog"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var levelString = activityContext.GetState<string>("Level");
            var message = activityContext.GetState<string>("Message");

            LogLevel level;
            Enum.TryParse(levelString, true, out level);
            Logger.Log(level, null, message);

            yield return T("Done");
        }
    }
}