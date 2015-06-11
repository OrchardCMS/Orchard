using System.Collections.Generic;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Comments.Activities {
    [OrchardFeature("Orchard.Comments.Workflows")]
    public class CloseCommentsActivity : Task {

        public CloseCommentsActivity() {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public override string Name {
            get { return "CloseComments"; }
        }

        public override LocalizedString Category {
            get { return T("Comments"); }
        }

        public override LocalizedString Description {
            get { return T("Closes the comments on the currently processed content item.");  }
        }

        public override string Form {
            get { return null; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {T("Done")};
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var content = workflowContext.Content;

            if (content != null) {
                var comments = content.As<CommentsPart>();
                if (comments != null) {
                    comments.CommentsActive = false;
                }
            }

            yield return T("Done");
        }
    }
}