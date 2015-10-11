using Orchard.ContentManagement;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public abstract class ExportAction : Component, IExportAction {
        public virtual int Priority { get { return 0; } }
        public abstract string Name { get; }

        protected virtual string Prefix {
            get { return GetType().Name; }
        }

        public virtual dynamic BuildEditor(dynamic shapeFactory) {
            return null;
        }

        public virtual dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            return null;
        }

        public virtual void Configure(ExportActionConfigurationContext context) {
        }

        public virtual void ConfigureDefault() {
        }

        public abstract void Execute(ExportActionContext context);
    }
}