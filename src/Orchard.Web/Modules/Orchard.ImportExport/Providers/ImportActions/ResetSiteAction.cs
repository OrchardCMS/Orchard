using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.MultiTenancy.Services;

namespace Orchard.ImportExport.Providers.ImportActions {
    [OrchardFeature("Orchard.ImportExport.ResetSite")]
    public class ResetSiteAction : ImportAction {
        private readonly ITenantService _tenantService;
        private readonly ShellSettings _shellSettings;

        public ResetSiteAction(ITenantService tenantService, ShellSettings shellSettings) {
            _tenantService = tenantService;
            _shellSettings = shellSettings;
        }

        public override string Name { get { return "ResetSite"; } }

        public override int Priority {
            get { return 500; }
        }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public bool ResetSite { get; set; }
        public bool DropTables { get; set; }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var viewModel = new ResetSiteViewModel();
            if (updater != null) {
                ResetSite = viewModel.ResetSite;
                DropTables = viewModel.DropTables;
            }

            return shapeFactory.EditorTemplate(TemplateName: "ImportActions/ResetSite", Model: viewModel, Prefix: Prefix);
        }

        public override void Execute(ImportActionContext context) {
            _shellSettings.State = TenantState.Disabled;
            _tenantService.ResetTenant(_shellSettings, DropTables);
            _shellSettings.DataProvider = null;
            _tenantService.UpdateTenant(_shellSettings);
            context.ActionResult = new RedirectResult("~/");
        }
    }
}