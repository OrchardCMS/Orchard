using Orchard.ContentManagement;
using Orchard.ImportExport.Services;
using Orchard.Localization;

namespace Orchard.ImportExport.Providers {
    public class DataExportStep : ExportStepProvider {
        public override string Name {
            get { return "Data"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Data"); }
        }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            if (updater != null) {
                
            }

            return shapeFactory.EditorTemplate(TemplateName: "ExportSteps/Data");
        }
    }
}