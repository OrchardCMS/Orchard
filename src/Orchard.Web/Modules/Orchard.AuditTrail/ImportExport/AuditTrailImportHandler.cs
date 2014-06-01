using System;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.AuditTrail.ImportExport {
    public class AuditTrailImportHandler : Component, IRecipeHandler {
        private readonly IRepository<AuditTrailEventRecord> _auditTrailEventRepository;

        public AuditTrailImportHandler(IRepository<AuditTrailEventRecord> auditTrailEventRepository) {
            _auditTrailEventRepository = auditTrailEventRepository;
        }

        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "AuditTrail", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            foreach (var eventElement in recipeContext.RecipeStep.Step.Elements()) {
                var record = new AuditTrailEventRecord {
                    Event = eventElement.Attr<string>("Name"),
                    Category = eventElement.Attr<string>("Category"),
                    UserName = eventElement.Attr<string>("User"),
                    CreatedUtc = eventElement.Attr<DateTime>("CreatedUtc"),
                    EventFilterKey = eventElement.Attr<string>("EventFilterKey"),
                    EventFilterData = eventElement.Attr<string>("EventFilterData"),
                    Comment = eventElement.El("Comment"),
                    EventData = eventElement.El("EventData"),
                };
                
                _auditTrailEventRepository.Create(record);
            }

            recipeContext.Executed = true;
        }
    }
}
