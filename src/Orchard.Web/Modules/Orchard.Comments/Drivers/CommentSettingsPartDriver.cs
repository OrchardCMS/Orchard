using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Orchard.Comments.Drivers {
    public class CommentSettingsPartDriver : ContentPartDriver<CommentSettingsPart> {
        public CommentSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "CommentSettings"; } }

        protected override DriverResult Editor(CommentSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Comments_SiteSettings",
                               () => shapeHelper.EditorTemplate(TemplateName: "Parts.Comments.SiteSettings", Model: part.Record, Prefix: Prefix));
        }

        protected override DriverResult Editor(CommentSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part.Record, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}