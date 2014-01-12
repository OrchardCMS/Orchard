using System;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Settings;

namespace Orchard.Comments.Drivers {
    public class CommentSettingsPartDriver : ContentPartDriver<CommentSettingsPart> {
        public CommentSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "CommentSettings"; } }

        protected override DriverResult Editor(CommentSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CommentSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            return ContentShape("Parts_Comments_SiteSettings", () => {
                    if (updater != null) {
                        updater.TryUpdateModel(part.Record, Prefix, null, null);
                    }
                    return shapeHelper.EditorTemplate(TemplateName: "Parts.Comments.SiteSettings", Model: part.Record, Prefix: Prefix); 
                })
                .OnGroup("comments");
        }

        protected override void Exporting(CommentSettingsPart part, ExportContentContext context) {
            DefaultSettingsPartImportExport.ExportSettingsPart(part, context);
        }

        protected override void Importing(CommentSettingsPart part, ImportContentContext context) {
            DefaultSettingsPartImportExport.ImportSettingPart(part, context.Data.Element(part.PartDefinition.Name));
        }
    }
}