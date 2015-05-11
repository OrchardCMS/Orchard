using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Dashboards.Models;
using Orchard.Dashboards.ViewModels;

namespace Orchard.Dashboards.Elements {
    public class DashboardSiteSettingsPartDriver : ContentPartDriver<DashboardSiteSettingsPart> {
        private readonly IContentManager _contentManager;

        public DashboardSiteSettingsPartDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override DriverResult Editor(DashboardSiteSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(DashboardSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_DashboardSettings", () => {
                var viewModel = new DashboardSiteSettingsViewModel {
                    SelectedDashboardId = part.DefaultDashboardId.ToString(),
                };

                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, new[] { "SelectedDashboard" })) {
                        part.DefaultDashboardId = ParseContentId(viewModel.SelectedDashboardId);
                    }
                }

                viewModel.SelectedDashboard = part.DefaultDashboardId != null ? _contentManager.Get(part.DefaultDashboardId.Value) : default(ContentItem);
                return shapeHelper.EditorTemplate(TemplateName: "Parts.DashboardSettings", Model: viewModel, Prefix: Prefix);
            }).OnGroup("Dashboard");
        }

        private int? ParseContentId(string value) {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            var items = value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            if (!items.Any())
                return null;

            return XmlHelper.Parse<int>(items.First());
        }
    }
}