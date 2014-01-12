using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Settings;

namespace Orchard.Recipes.RecipeHandlers {
    public class SettingsRecipeHandler : IRecipeHandler {
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;

        public SettingsRecipeHandler(ISiteService siteService, IContentManager contentManager, Lazy<IEnumerable<IContentHandler>> handlers) {
            _siteService = siteService;
            _contentManager = contentManager;
            _handlers = handlers;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        private IEnumerable<IContentHandler> Handlers { get { return _handlers.Value; } }

        /*  
         <Settings>
          <SiteSettingsPart PageSize="30" />
          <CommentSettingsPart ModerateComments="true" />
         </Settings>
        */
        // Set site and part settings.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Settings", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var site = _siteService.GetSiteSettings();

            var importContentSession = new ImportContentSession(_contentManager);

            var context = new ImportContentContext(site.ContentItem, recipeContext.RecipeStep.Step, importContentSession);
            foreach (var contentHandler in Handlers) {
                contentHandler.Importing(context);
            }

            foreach (var contentHandler in Handlers) {
                contentHandler.Imported(context);
            }

            recipeContext.Executed = true;
        }
    }
}
