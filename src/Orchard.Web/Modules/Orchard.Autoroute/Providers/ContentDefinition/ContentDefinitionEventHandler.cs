using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.Autoroute.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.Events;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Autoroute.Providers.ContentDefinition {
    public class ContentDefinitionEventHandler : IContentDefinitionEventHandler {
        private readonly ICultureManager _cultureManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly Lazy<IAutorouteService> _autorouteService;
        private readonly IContentManager _contentManager;

        public ContentDefinitionEventHandler(
            IContentManager contentManager,
            Lazy<IAutorouteService> autorouteService,
            IOrchardServices orchardServices,
            IContentDefinitionManager contentDefinitionManager,
            ICultureManager cultureManager) {
            _cultureManager = cultureManager;
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _autorouteService = autorouteService;
            _contentManager = contentManager;
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context) {
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context) {
        }

        public void ContentTypeImporting(ContentTypeImportingContext context) {
        }

        public void ContentTypeImported(ContentTypeImportedContext context) {
        }

        public void ContentPartCreated(ContentPartCreatedContext context) {
        }

        public void ContentPartRemoved(ContentPartRemovedContext context) {
        }

        public void ContentPartAttached(ContentPartAttachedContext context) {
            if (context.ContentPartName == "AutoroutePart") {
                // Create pattern and default pattern for each culture installed and for the neutral culture

                // Get cultures
                var SiteCultures = _cultureManager.ListCultures().ToList();

                // Adding a null culture for the culture neutral pattern
                List<string> cultures = new List<string>();
                cultures.Add(null);
                cultures.AddRange(SiteCultures);

                // Create Patterns and DefaultPatterns
                var settings = new AutorouteSettings {
                    Patterns = new List<RoutePattern>()
                };

                List<RoutePattern> newPatterns = new List<RoutePattern>();
                List<DefaultPattern> newDefaultPatterns = new List<DefaultPattern>();
                foreach (string culture in cultures) {
                    newPatterns.Add(new RoutePattern {
                        Name = "Title",
                        Description = "my-title",
                        Pattern = "{Content.Slug}",
                        Culture = culture
                    });
                    newDefaultPatterns.Add(new DefaultPattern {
                        Culture = culture,
                        PatternIndex = "0"
                    });
                }

                settings.Patterns = newPatterns;
                settings.DefaultPatterns = newDefaultPatterns;

                //Update Settings
                _contentDefinitionManager.AlterTypeDefinition(context.ContentTypeName, builder => builder.WithPart("AutoroutePart", settings.Build));

                //TODO Generate URL's for existing content items
                //We should provide a global setting to enable/disable this feature

            }
        }
        
        public void ContentPartDetached(ContentPartDetachedContext context) {
        }

        public void ContentPartImporting(ContentPartImportingContext context) {
        }

        public void ContentPartImported(ContentPartImportedContext context) {
        }

        public void ContentFieldAttached(ContentFieldAttachedContext context) {
        }

        public void ContentFieldDetached(ContentFieldDetachedContext context) {
        }

    }
}