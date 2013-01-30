using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Contrib.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Contrib.Taxonomies.Drivers {
    [OrchardFeature("TaxonomyMenuItem")]
    public class TaxonomyMenuItemPartDriver : ContentPartDriver<TaxonomyMenuItemPart> {

        public TaxonomyMenuItemPartDriver() {
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        protected override string Prefix { get { return "TaxonomyMenuItemPart"; } }

        protected override DriverResult Editor(TaxonomyMenuItemPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Taxonomies_TaxonomyMenuItem_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/Taxonomies.TaxonomyMenuItem", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(TaxonomyMenuItemPart menuItemPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(menuItemPart, Prefix, null, null);
            if(menuItemPart.RenderMenuItem) {
                if(String.IsNullOrWhiteSpace(menuItemPart.Name)) {
                    updater.AddModelError("Name", T("Menu item name is required"));
                }

                if (String.IsNullOrWhiteSpace(menuItemPart.Position)) {
                    menuItemPart.Position = "0";
                }
            }
            
            return Editor(menuItemPart, shapeHelper);
        }

        protected override void Exporting(TaxonomyMenuItemPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Name", part.Record.Name);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Position", part.Record.Position);
            context.Element(part.PartDefinition.Name).SetAttributeValue("RenderMenuItem", part.Record.RenderMenuItem);
        }

        protected override void Importing(TaxonomyMenuItemPart part, ImportContentContext context) {
            part.Record.Name = context.Attribute(part.PartDefinition.Name, "Name");
            part.Record.Position = context.Attribute(part.PartDefinition.Name, "Position");
            part.Record.RenderMenuItem = Boolean.Parse(context.Attribute(part.PartDefinition.Name, "RenderMenuItem"));
        }
    }
}