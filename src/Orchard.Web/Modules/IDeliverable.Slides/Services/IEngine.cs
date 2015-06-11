using Orchard;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Elements;
using Orchard.Localization;

namespace IDeliverable.Slides.Services
{
    public interface IEngine : IDependency
    {
        string Name { get; }
        LocalizedString DisplayName { get; }
        ElementDataDictionary Data { get; set; }
        dynamic BuildSettingsEditor(dynamic shapeFactory);
        dynamic UpdateSettingsEditor(dynamic shapeFactory, IUpdateModel updater);
        dynamic BuildDisplay(dynamic shapeFactory);
    }
}