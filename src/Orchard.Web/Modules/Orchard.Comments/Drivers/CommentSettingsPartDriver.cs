using System;
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

        protected override DriverResult Editor(CommentSettingsPart part, string groupInfoId, dynamic shapeHelper) {
            if (!string.Equals(groupInfoId, "comments", StringComparison.OrdinalIgnoreCase))
                return null;

            return ContentShape("Parts_Comments_SiteSettings",
                               () => shapeHelper.EditorTemplate(TemplateName: "Parts.Comments.SiteSettings", Model: part.Record, Prefix: Prefix));
        }

        protected override DriverResult Editor(CommentSettingsPart part, IUpdateModel updater, string groupInfoId, dynamic shapeHelper) {
            if (!string.Equals(groupInfoId, "comments", StringComparison.OrdinalIgnoreCase))
                return null;

            updater.TryUpdateModel(part.Record, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}