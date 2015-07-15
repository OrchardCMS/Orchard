using Orchard.ContentManagement;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;

namespace Orchard.ImportExport.Providers.ImportActions {
    public class ResetSiteAction : ImportAction {

        public override string Name { get { return "ResetSite"; } }

        public override int Priority {
            get { return 500; }
        }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public bool ResetSite { get; set; }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var viewModel = new ResetSiteViewModel();
            if (updater != null) {
                ResetSite = viewModel.ResetSite;
            }

            return shapeFactory.EditorTemplate(TemplateName: "ImportActions/ResetSite", Model: viewModel, Prefix: Prefix);
        }

        public override void Execute(ImportActionContext context) {
            // Reset site.
        }
    }
}