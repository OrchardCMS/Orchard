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

        protected override DriverResult Editor(CommentSettingsPart part) {
            return ContentPartTemplate(part.Record, "Parts/Comments.SiteSettings");
        }

        protected override DriverResult Editor(CommentSettingsPart part, IUpdateModel updater) {
            updater.TryUpdateModel(part.Record, Prefix, null, null);
            return Editor(part);
        }
    }
}