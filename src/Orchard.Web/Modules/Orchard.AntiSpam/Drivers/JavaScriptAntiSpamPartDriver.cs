using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.AntiSpam.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Orchard.AntiSpam.Drivers {
    public class JavaScriptAntiSpamPartDriver : ContentPartDriver<JavaScriptAntiSpamPart> {
        public Localizer T { get; set; }

        public JavaScriptAntiSpamPartDriver() {
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(JavaScriptAntiSpamPart part, dynamic shapeHelper) {
            return ContentShape("Parts_JavaScriptAntiSpam_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Antispam.JavaScriptAntiSpam",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(JavaScriptAntiSpamPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);

            if (!part.IAmHuman) {
                updater.AddModelError("NotHuman", T("You are either a bot (we only serve humans, sorry) or have JavaScript turned off. If the latter, please turn on JavaScript to post this form."));
            }

            return Editor(part, shapeHelper);
        }
    }
}