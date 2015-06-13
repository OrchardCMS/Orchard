using IDeliverable.ThemeSettings.Models;
using IDeliverable.ThemeSettings.Services;

namespace IDeliverable.ThemeSettings.SettingProviders
{
    public class UrlThemeSettingProvider : ThemeSettingProviderBase
    {
        public override string TypeName
        {
            get { return "Url"; }
        }

        public override dynamic BuildEditor(dynamic shapeFactory, ThemeSettingDefinition setting)
        {
            return shapeFactory.Textbox(
                Id: setting.Name,
                Name: setting.Name,
                Title: setting.Name,
                Type: "url",
                Value: setting.Default,
                Description: T(setting.Description),
                Classes: new[] { "text large tokenized" }
                );
        }
    }
}