using IDeliverable.ThemeSettings.Models;
using IDeliverable.ThemeSettings.Services;

namespace IDeliverable.ThemeSettings.SettingProviders
{
    public class ColorThemeSettingProvider : ThemeSettingProviderBase
    {
        public override string TypeName
        {
            get { return "Color"; }
        }

        public override dynamic BuildEditor(dynamic shapeFactory, ThemeSettingDefinition setting)
        {
            return shapeFactory.Textbox(
                Id: setting.Name,
                Name: setting.Name,
                Title: setting.Name,
                Type: "color",
                Value: setting.Default,
                Description: T(setting.Description),
                Classes: new[] { "text medium tokenized" }
            );
        }
    }
}