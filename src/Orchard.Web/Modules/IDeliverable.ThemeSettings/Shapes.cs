using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;
using Orchard.Themes.Models;
using IDeliverable.ThemeSettings.Services;

namespace IDeliverable.ThemeSettings
{
    public class Shapes : IShapeTableProvider
    {
        private readonly Work<IThemeSettingsService> _themeSettingsService;

        public Shapes(Work<IThemeSettingsService> themeSettingsService)
        {
            _themeSettingsService = themeSettingsService;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("ThemeEntry_Current").OnDisplaying(context =>
            {
                var theme = (ThemeEntry)context.Shape.Theme;
                context.Shape.HasSettings = _themeSettingsService.Value.HasSettingsManifest(theme.Descriptor.Id);
                context.ShapeMetadata.Alternates.Add("ThemeEntry_Current__Settings");
            });
        }
    }
}