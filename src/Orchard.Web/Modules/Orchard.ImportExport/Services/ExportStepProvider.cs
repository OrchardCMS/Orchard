using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.ImportExport.Services {
    public abstract class ExportStepProvider : Component, IExportStepProvider {
        public abstract string Name { get; }
        public abstract LocalizedString DisplayName { get; }

        protected virtual string Prefix {
            get { return GetType().Name; }
        }

        public virtual dynamic BuildEditor(dynamic shapeFactory) {
            return null;
        }

        public virtual dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            return null;
        }
    }
}