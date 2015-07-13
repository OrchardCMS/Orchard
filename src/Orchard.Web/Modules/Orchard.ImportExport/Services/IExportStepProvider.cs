using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.ImportExport.Services {
    public interface IExportStepProvider : IDependency {
        string Name { get; }
        LocalizedString DisplayName { get; }
        dynamic BuildEditor(dynamic shapeFactory);
        dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater);
    }
}