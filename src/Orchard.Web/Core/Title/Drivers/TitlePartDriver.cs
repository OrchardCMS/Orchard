using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Localization;

namespace Orchard.Core.Title.Drivers {
    public class TitlePartDriver : ContentPartDriver<TitlePart> {

        private const string TemplateName = "Parts.Title.TitlePart";

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Title"; }
        }

        protected override DriverResult Display(TitlePart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Title",
                    () => shapeHelper.Parts_Title(Title: part.Title)),
                ContentShape("Parts_Title_Summary",
                    () => shapeHelper.Parts_Title_Summary(Title: part.Title)),
                ContentShape("Parts_Title_SummaryAdmin",
                    () => shapeHelper.Parts_Title_SummaryAdmin(Title: part.Title))
                );
        }

        protected override DriverResult Editor(TitlePart part, dynamic shapeHelper) {

            return ContentShape("Parts_Title_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(TitlePart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }

        protected override void Importing(TitlePart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Title", title =>
                part.Title = title
            );
        }

        protected override void Exporting(TitlePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Title", part.Title);
        }

        protected override void Cloning(TitlePart originalPart, TitlePart clonePart, CloneContentContext context) {
            clonePart.Title = originalPart.Title;
        }
    }
}