using Orchard;
using IDeliverable.ThemeSettings.Models;

namespace IDeliverable.ThemeSettings.Services
{
    public interface IThemeSettingProvider : IDependency
    {
        string TypeName { get; }
        dynamic BuildEditor(dynamic shapeFactory, ThemeSettingDefinition setting);
    }
}