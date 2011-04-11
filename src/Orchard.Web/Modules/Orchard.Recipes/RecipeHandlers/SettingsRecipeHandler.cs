using System;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Settings;

namespace Orchard.Recipes.RecipeHandlers {
    public class SettingsRecipeHandler : IRecipeHandler {
        private readonly ISiteService _siteService;

        public SettingsRecipeHandler(ISiteService siteService) {
            _siteService = siteService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

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
            foreach (var element in recipeContext.RecipeStep.Step.Elements()) {
                var partName = XmlConvert.DecodeName(element.Name.LocalName);
                foreach (var contentPart in site.ContentItem.Parts) {
                    if (!String.Equals(contentPart.PartDefinition.Name, partName, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }
                    foreach (var attribute in element.Attributes()) {
                        SetSetting(attribute, contentPart);
                    }
                }
            }

            recipeContext.Executed = true;
        }

        private static void SetSetting(XAttribute attribute, ContentPart contentPart) {
            var attributeName = attribute.Name.LocalName;
            var attributeValue = attribute.Value;
            var property = contentPart.GetType().GetProperty(attributeName);
            if (property == null) {
                throw new InvalidOperationException(string.Format("Could set setting {0} for part {1} because it was not found.", attributeName, contentPart.PartDefinition.Name));
            }
            var propertyType = property.PropertyType;
            if (propertyType == typeof(string)) {
                property.SetValue(contentPart, attributeValue, null);
            }
            else if (propertyType == typeof(bool)) {
                property.SetValue(contentPart, Boolean.Parse(attributeValue), null);
            }
            else if (propertyType == typeof(int)) {
                property.SetValue(contentPart, Int32.Parse(attributeValue), null);
            }
            else {
                throw new InvalidOperationException(string.Format("Could set setting {0} for part {1} because its type is not supported. Settings should be integer,boolean or string.", attributeName, contentPart.PartDefinition.Name));
            }
        }
    }
}
