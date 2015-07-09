using System;
using System.Collections.Generic;
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

            Logger.Information("Executing recipe step '{0}'; ExecutionId={1}", recipeContext.RecipeStep.Name, recipeContext.ExecutionId);

            var siteContentItem = _siteService.GetSiteSettings().ContentItem;

            var importContentSession = new ImportContentSession(_contentManager);

            var context = new ImportContentContext(siteContentItem, recipeContext.RecipeStep.Step, importContentSession);
            foreach (var contentHandler in Handlers) {
                contentHandler.Importing(context);
            }

            foreach (var contentPart in siteContentItem.Parts) {
                var partElement = context.Data.Element(contentPart.PartDefinition.Name);
                if (partElement == null) {
                    continue;
                }

                Logger.Information("Importing settings part '{0}'.", contentPart.PartDefinition.Name);
                try {
                    ImportSettingPart(contentPart, partElement);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while importing settings part '{0}'.", contentPart.PartDefinition.Name);
                    throw;
                }
            }

            foreach (var contentHandler in Handlers) {
                contentHandler.Imported(context);
            }

            recipeContext.Executed = true;
            Logger.Information("Finished executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
        }

        private void ImportSettingPart(ContentPart sitePart, XElement element) {

            foreach (var attribute in element.Attributes()) {
                var attributeName = attribute.Name.LocalName;
                var attributeValue = attribute.Value;

                var property = sitePart.GetType().GetProperty(attributeName);
                if (property == null) {
                    continue;
                }

                var propertyType = property.PropertyType;
                if (propertyType == typeof(string)) {
                    property.SetValue(sitePart, attributeValue, null);
                }
                else if (propertyType == typeof(bool)) {
                    property.SetValue(sitePart, Boolean.Parse(attributeValue), null);
                }
                else if (propertyType == typeof(int)) {
                    property.SetValue(sitePart, Int32.Parse(attributeValue), null);
                }
            }
        }
    }
}
