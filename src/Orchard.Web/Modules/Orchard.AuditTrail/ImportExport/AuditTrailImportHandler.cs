using System;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Security;

namespace Orchard.AuditTrail.ImportExport {
    [OrchardFeature("Orchard.AuditTrail.ImportExport")]
    public class AuditTrailImportHandler : Component, IRecipeHandler {
        private readonly IRepository<AuditTrailEventRecord> _auditTrailEventRepository;
        private readonly IAuthorizer _authorizer;
        private readonly IWorkContextAccessor _wca;

        public AuditTrailImportHandler(IRepository<AuditTrailEventRecord> auditTrailEventRepository, IAuthorizer authorizer, IWorkContextAccessor wca) {
            _auditTrailEventRepository = auditTrailEventRepository;
            _authorizer = authorizer;
            _wca = wca;
        }

        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "AuditTrail", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            if (!_authorizer.Authorize(Permissions.ImportAuditTrail)) {
                Logger.Warning("Blocked {0} from importing an audit trail because this user does not have the ImportauditTrail permission.", _wca.GetContext().CurrentUser.UserName);
                recipeContext.Executed = false;
                return;
            }

            foreach (var eventElement in recipeContext.RecipeStep.Step.Elements()) {
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

            recipeContext.Executed = true;
        }
    }
}
