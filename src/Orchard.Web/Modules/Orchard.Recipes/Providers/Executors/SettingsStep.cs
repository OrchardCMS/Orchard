using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Settings;

namespace Orchard.Recipes.Providers.Executors {
    public class SettingsStep : RecipeExecutionStep {
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;

        public SettingsStep(
            ISiteService siteService,
            IContentManager contentManager,
            Lazy<IEnumerable<IContentHandler>> handlers,
            RecipeExecutionLogger logger) : base(logger) {

            _siteService = siteService;
            _contentManager = contentManager;
            _handlers = handlers;
        }

        public override string Name { get { return "Settings"; } }
        private IEnumerable<IContentHandler> Handlers { get { return _handlers.Value; } }

        /*  
         <Settings>
          <SiteSettingsPart PageSize="30" />
          <CommentSettingsPart ModerateComments="true" />
         </Settings>
        */
        // Set site and part settings.
        public override void Execute(RecipeExecutionContext context) {
            var siteContentItem = _siteService.GetSiteSettings().ContentItem;
            var importContentSession = new ImportContentSession(_contentManager);
            var importContentContext = new ImportContentContext(siteContentItem, context.RecipeStep.Step, importContentSession);

            foreach (var contentHandler in Handlers) {
                contentHandler.Importing(importContentContext);
            }

            foreach (var contentPart in siteContentItem.Parts) {
                var partElement = importContentContext.Data.Element(contentPart.PartDefinition.Name);
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
                contentHandler.Imported(importContentContext);
            }
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
