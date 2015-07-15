using Orchard.ContentManagement;

namespace Orchard.ImportExport.Services {
    public abstract class ImportAction : Component, IImportAction {
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

        public abstract void Execute(ImportActionContext context);
    }
}