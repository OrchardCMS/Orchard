using Orchard.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using Orchard.Taxonomies.Services;
using Orchard.Localization;
using System;

namespace Orchard.Taxonomies.Handlers {
    public class TermPartHandler : ContentHandler {
        private readonly ITaxonomyService _taxonomyService;
        public Localizer T { get; set; }

        public TermPartHandler(IRepository<TermPartRecord> repository, INotifier notifier, ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
            Filters.Add(StorageFilter.For(repository));
            OnInitializing<TermPart>((context, part) => part.Selectable = true);

            OnUpdated<TermPart>(UpdateWeights);
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var term = context.ContentItem.As<TermPart>();

            if (term == null)
                return;

            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Taxonomies"},
                {"Controller", "TermAdmin"},
                {"Action", "Edit"},
                {"Id", term.Id}
            };

        }

        protected override void UpdateEditorShape(UpdateEditorContext context) {
            var part = context.ContentItem.As<TermPart>();
            if (part == null) {
                return;
            }
            base.UpdateEditorShape(context);
            if (context.Updater.TryUpdateModel(part, "Term", null, null)) {
                var existing = _taxonomyService.GetTermByName(part.TaxonomyId, part.Name);
                if (existing != null && existing.Record != part.Record && existing.Container.ContentItem.Record == part.Container.ContentItem.Record) {
                    context.Updater.AddModelError("Name", T("The term {0} already exists at this level", part.Name));
                }
            }
        }

        public void UpdateWeights(UpdateContentContext context, TermPart part) {
            // Given part
            // - Update FullWeight for part
            // - Update FullWeight for part's siblings
            // - If part's FullWeight changed, update FullWeight for all its children
            // - If FullWeight changed for any of part's siblings, update their children
            // We don't have to check for each chil'd siblings, because we are updating
            // all children anyway (as long as we have to update any).

            // Get part and its siblings
            var litter = _taxonomyService.OrderedSiblings(part);
            // For each one, see whether we should update its weight.
            // in that case, update its children as well.
            foreach (var tp in litter) {
                var newWeight = _taxonomyService.ComputeFullWeight(tp);
                if (!newWeight.Equals(tp.FullWeight, StringComparison.InvariantCultureIgnoreCase)) {
                    part.FullWeight = newWeight;
                    UpdateChildrenWeights(part);
                }
            }
        }

        private void UpdateChildrenWeights(TermPart part) {
            foreach (var childTerm in _taxonomyService.GetChildren(part)) {
                var newWeight = _taxonomyService.ComputeFullWeight(childTerm);
                if (!newWeight.Equals(childTerm.FullWeight, StringComparison.InvariantCultureIgnoreCase)) {
                    childTerm.FullWeight = newWeight;
                    UpdateChildrenWeights(childTerm);
                }
            }
        }
    }
}