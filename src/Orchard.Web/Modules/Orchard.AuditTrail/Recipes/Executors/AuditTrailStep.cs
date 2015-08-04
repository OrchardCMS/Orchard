using System;
using System.Linq;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Security;

namespace Orchard.AuditTrail.Recipes.Executors {
    [OrchardFeature("Orchard.AuditTrail.ImportExport")]
    public class AuditTrailStep : RecipeExecutionStep {
        private readonly IRepository<AuditTrailEventRecord> _auditTrailEventRepository;
        private readonly IAuthorizer _authorizer;
        private readonly IWorkContextAccessor _wca;

        public AuditTrailStep(
            IRepository<AuditTrailEventRecord> auditTrailEventRepository,
            IAuthorizer authorizer,
            IWorkContextAccessor wca,
            RecipeExecutionLogger logger) : base(logger) {

            _auditTrailEventRepository = auditTrailEventRepository;
            _authorizer = authorizer;
            _wca = wca;
        }

        public override string Name {
            get { return "AuditTrail"; }
        }

        public override void Execute(RecipeExecutionContext context) {
            if (!_authorizer.Authorize(Permissions.ImportAuditTrail)) {
                Logger.Warning("Blocked {0} from importing an audit trail because this user does not have the ImportauditTrail permission.", _wca.GetContext().CurrentUser.UserName);
                return;
            }

            var elements = context.RecipeStep.Step.Elements().ToArray();
            for (var i = 0; i < elements.Length; i ++) {
                var eventElement = elements[i];
                Logger.Information("Importing audit trail event {0}/{1}.", i + 1, elements.Length);

                try {
                    var record = new AuditTrailEventRecord {
                        EventName = eventElement.Attr<string>("Name"),
                        FullEventName = eventElement.Attr<string>("FullName"),
                        Category = eventElement.Attr<string>("Category"),
                        UserName = eventElement.Attr<string>("User"),
                        CreatedUtc = eventElement.Attr<DateTime>("CreatedUtc"),
                        EventFilterKey = eventElement.Attr<string>("EventFilterKey"),
                        EventFilterData = eventElement.Attr<string>("EventFilterData"),
                        Comment = eventElement.El("Comment"),
                        EventData = eventElement.Element("EventData").ToString(),
                    };

                    _auditTrailEventRepository.Create(record);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while importing audit trail event {0}/{1}.", i + 1, elements.Length);
                    throw;
                }
            }
        }
    }
}
