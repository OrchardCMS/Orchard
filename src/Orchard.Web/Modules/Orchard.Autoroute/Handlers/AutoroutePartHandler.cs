using System;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Autoroute.Handlers {
    public class AutoroutePartHandler : ContentHandler {

        private readonly Lazy<IAutorouteService> _autorouteService;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IHomeAliasService _homeAliasService;

        public Localizer T { get; set; }

        public AutoroutePartHandler(
            IRepository<AutoroutePartRecord> autoroutePartRepository,
            Lazy<IAutorouteService> autorouteService,
            IContentManager contentManager,
            IOrchardServices orchardServices, 
            IHomeAliasService homeAliasService) {

            Filters.Add(StorageFilter.For(autoroutePartRepository));
            _autorouteService = autorouteService;
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _homeAliasService = homeAliasService;

            OnUpdated<AutoroutePart>((ctx, part) => CreateAlias(part));

            OnCreated<AutoroutePart>((ctx, part) => {
                // non-draftable items
                if (part.ContentItem.VersionRecord == null) {
                    PublishAlias(part);
                }
            });

            OnPublished<AutoroutePart>((ctx, part) => PublishAlias(part));

            // Remove alias if destroyed, removed or unpublished
            OnRemoving<AutoroutePart>((ctx, part) => RemoveAlias(part));
            OnDestroying<AutoroutePart>((ctx, part) => RemoveAlias(part));
            OnUnpublishing<AutoroutePart>((ctx, part) => RemoveAlias(part));

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

            // Should it become the home page?
            if (part.PromoteToHomePage) {
                // Get the current homepage an unmark it as the homepage.
                var currentHomePage = _homeAliasService.GetHomePage(VersionOptions.Latest);
                if(currentHomePage != null && currentHomePage.Id != part.Id) {
                    var autoroutePart = currentHomePage.As<AutoroutePart>();

                    if (autoroutePart != null) {
                        autoroutePart.PromoteToHomePage = false;
                        if(autoroutePart.IsPublished())
                            _orchardServices.ContentManager.Publish(autoroutePart.ContentItem);
                    }
                }

                // Update the home alias to point to this item being published.
                _homeAliasService.PublishHomeAlias(part);
            }
            
            _autorouteService.Value.PublishAlias(part);
        }

        private void ProcessAlias(AutoroutePart part) {
            // Generate an alias if one as not already been entered.
            if (String.IsNullOrWhiteSpace(part.DisplayAlias)) {
                part.DisplayAlias = _autorouteService.Value.GenerateAlias(part);
            }

            // If the generated alias is empty, compute a new one.
            if (String.IsNullOrWhiteSpace(part.DisplayAlias)) {
                _autorouteService.Value.ProcessPath(part);
                _orchardServices.Notifier.Warning(T("The permalink could not be generated, a new slug has been defined: \"{0}\"", part.Path));
                return;
            }

            // Check for permalink conflict, unless we are trying to set the home page.
            var previous = part.Path;
            if (!_autorouteService.Value.ProcessPath(part))
                _orchardServices.Notifier.Warning(
                    T("Permalinks in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"", 
                                                previous, part.Path, part.ContentItem.ContentType));
        }

        void RemoveAlias(AutoroutePart part) {
            var homePageId = _homeAliasService.GetHomePageId(VersionOptions.Latest);

            // Is this the current home page?
            if (part.ContentItem.Id == homePageId) {
                _orchardServices.Notifier.Warning(T("You removed the content item that served as the site's home page. \nMost possibly this means that instead of the home page a \"404 Not Found\" page will be displayed. \n\nTo prevent this you can e.g. publish a content item that has the \"Set as home page\" checkbox ticked."));
            }

            var publishedPart = _contentManager.Get<AutoroutePart>(part.ContentItem.Id, VersionOptions.Published);
            _autorouteService.Value.RemoveAliases(publishedPart);
        }
    }
}
