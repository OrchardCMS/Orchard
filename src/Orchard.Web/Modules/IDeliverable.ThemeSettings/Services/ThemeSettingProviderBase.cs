using Orchard;
using IDeliverable.ThemeSettings.Models;

namespace IDeliverable.ThemeSettings.Services
{
    public abstract class ThemeSettingProviderBase : Component, IThemeSettingProvider
    {
        public abstract string TypeName { get; }
        public abstract dynamic BuildEditor(dynamic shapeFactory, ThemeSettingDefinition setting);
    }
}