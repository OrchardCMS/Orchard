using Orchard.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using Orchard.Taxonomies.Services;
using Orchard.Localization;

namespace Orchard.Taxonomies.Handlers {
    public class TermPartHandler : ContentHandler {
        private readonly INotifier _notifier;
        private readonly ITaxonomyService _taxonomyService;
        public Localizer T { get; set; }

        public TermPartHandler(IRepository<TermPartRecord> repository, INotifier notifier, ITaxonomyService taxonomyService) {
            _notifier = notifier;
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
            Filters.Add(StorageFilter.For(repository));
            OnInitializing<TermPart>((context, part) => part.Selectable = true);
        }

        protected override void UpdateEditorShape(UpdateEditorContext context) {
            base.UpdateEditorShape(context);
            TermPart termPart = context.ContentItem.As<TermPart>();
            var existing = _taxonomyService.GetTermByName(termPart.TaxonomyId, termPart.Name);
            if (existing != null && existing.Record != termPart.Record && existing.Container.ContentItem.Record == termPart.Container.ContentItem.Record) {
                context.Updater.AddModelError("Name", T("The term {0} already exists at this level", termPart.Name));
            }
        }
    }
}