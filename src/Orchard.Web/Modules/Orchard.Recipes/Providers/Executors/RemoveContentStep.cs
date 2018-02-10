using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Providers.Executors {
    public class RemoveContentStep : RecipeExecutionStep {
        private readonly IContentManager _contentManager;

        public RemoveContentStep(
            RecipeExecutionLogger logger, IContentManager contentManager) : base(logger) {
            _contentManager = contentManager;
        }

        public override string Name {
            get { return "RemoveContent"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Remove Content"); }
        }

        public override LocalizedString Description {
            get { return T("Removes a list of content items."); }
        }

        // <RemoveContent>
        //   <Page Id="{identifier}" />
        //   <BlogPost Id="{identifier}" />
        //   ...
        // </RemoveContent>
        public override void Execute(RecipeExecutionContext context) {
            var identitiesQuery =
                from element in context.RecipeStep.Step.Elements()
                let id = element.Attr("Id")
                where !String.IsNullOrWhiteSpace(id)
                select new ContentIdentity(id);

            foreach (var identity in identitiesQuery) {
                Logger.Information("Removing content item with identity '{0}'...", identity);
                var contentItem = _contentManager.ResolveIdentity(identity);

                if (contentItem == null) {
                    Logger.Warning("No content item with identity '{0}' could be found.", identity);
                    continue;
                }

                _contentManager.Remove(contentItem);
                Logger.Information("Content item with identity '{0}' was found with id '{1}' and has been successfully removed.", identity, contentItem.Id);
            }
        }
    }
}
