using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Autoroute.Models;
using Orchard.Data;
using Orchard.Autoroute.Services;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Autoroute.Handlers {
    public class AutoroutePartHandler : ContentHandler {

        private readonly Lazy<IAutorouteService> _autorouteService;
        private readonly IOrchardServices _orchardServices;

        public Localizer T { get; set; }

        public AutoroutePartHandler(
            IRepository<AutoroutePartRecord> autoroutePartRepository,
            Lazy<IAutorouteService> autorouteService,
            IOrchardServices orchardServices) {

            Filters.Add(StorageFilter.For(autoroutePartRepository));
            _autorouteService = autorouteService;
            _orchardServices = orchardServices;

            OnUpdated<AutoroutePart>((ctx, part) => CreateAlias(part));

            OnCreated<AutoroutePart>((ctx, part) => {
                // non-draftable items
                if (part.ContentItem.VersionRecord == null) {
                    PublishAlias(part);
                }
            });

            // OnVersioned<AutoroutePart>((ctx, part1, part2) => CreateAlias(part1));

            OnPublished<AutoroutePart>((ctx, part) => PublishAlias(part));

            // Remove alias if removed or unpublished
            OnRemoved<AutoroutePart>((ctx, part) => RemoveAlias(part));
            OnUnpublished<AutoroutePart>((ctx, part) => RemoveAlias(part));

            // Register alias as identity
            OnGetContentItemMetadata<AutoroutePart>((ctx, part) => {
                if (part.DisplayAlias != null)
                    ctx.Metadata.Identity.Add("alias", part.DisplayAlias);
            });
        }

        private void CreateAlias(AutoroutePart part) {
            ProcessAlias(part);
        }

        private void PublishAlias(AutoroutePart part) {
            ProcessAlias(part);

            // should it become the home page ?
            if (part.DisplayAlias == "/") {
                part.DisplayAlias = String.Empty;

                // regenerate the alias for the previous home page
                var currentHomePages = _orchardServices.ContentManager.Query<AutoroutePart, AutoroutePartRecord>().Where(x => x.DisplayAlias == "").List();
                foreach (var current in currentHomePages) {
                    if (current != null) {
                        current.CustomPattern = String.Empty; // force the regeneration
                        current.DisplayAlias = _autorouteService.Value.GenerateAlias(current);
                    }
                    _autorouteService.Value.PublishAlias(current);
                }
            }

            _autorouteService.Value.PublishAlias(part);
        }

        private void ProcessAlias(AutoroutePart part) {
            // generate an alias if one as not already been entered
            if (String.IsNullOrWhiteSpace(part.DisplayAlias)) {
                part.DisplayAlias = _autorouteService.Value.GenerateAlias(part);
            }

            // should it become the home page ?
            if (part.DisplayAlias != "/") {
                var previous = part.Path;
                if (!_autorouteService.Value.ProcessPath(part))
                    _orchardServices.Notifier.Warning(T("Permalinks in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"",
                                                 previous, part.Path, part.ContentItem.ContentType));
            }
        }

        void RemoveAlias(AutoroutePart part) {
            _autorouteService.Value.RemoveAliases(part);
        }
    }
}
