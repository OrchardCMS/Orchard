using IDeliverable.ThemeSettings.Models;
using IDeliverable.ThemeSettings.Services;

namespace IDeliverable.ThemeSettings.SettingProviders
{
    public class TextThemeSettingProvider : ThemeSettingProviderBase
    {
        public override string TypeName
        {
            get { return "Text"; }
        }

        public override dynamic BuildEditor(dynamic shapeFactory, ThemeSettingDefinition setting)
        {
            return shapeFactory.Textbox(
                Id: setting.Name,
                Name: setting.Name,
                Title: setting.Name,
                Type: "text",
                Value: setting.Default,
                Description: T(setting.Description),
                Classes: new[] { "text large tokenized" }
            );
        }
    }
}