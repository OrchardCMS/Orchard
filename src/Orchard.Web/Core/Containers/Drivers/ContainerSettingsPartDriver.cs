using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Containers.Models;
using Orchard.Data;
using Orchard.Localization;

namespace Orchard.Core.Containers.Drivers {
    public class ContainerSettingsPartDriver : ContentPartDriver<ContainerSettingsPart> {
        public ContainerSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "ContainerSettings"; } }

        protected override DriverResult Editor(ContainerSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Container_SiteSettings",
                               () => shapeHelper.EditorTemplate(TemplateName: "Container.SiteSettings", Model: part.Record, Prefix: Prefix));
        }

        protected override DriverResult Editor(ContainerSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part.Record, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }

    public class ContainerSettingsPartHandler : ContentHandler {
        public ContainerSettingsPartHandler(IRepository<ContainerSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<ContainerSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}