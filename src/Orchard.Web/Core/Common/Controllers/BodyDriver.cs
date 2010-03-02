using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;

namespace Orchard.Core.Common.Controllers {
    public class BodyDriver : ContentPartDriver<BodyAspect> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Parts/Common.Body";
        private const string DefaultTextEditorTemplate = "TinyMceTextEditor";

        public BodyDriver(IOrchardServices services) {
            Services = services;
        }

        protected override string Prefix {
            get {return "Body";}
        }

        // \/\/ Haackalicious on many accounts - don't copy what has been done here for the wrapper \/\/
        protected override DriverResult Display(BodyAspect part, string displayType) {
            var model = new BodyDisplayViewModel { BodyAspect = part, Text = BbcodeReplace(part.Text)};
            return Combined(
                Services.Authorizer.Authorize(Permissions.ChangeOwner) ? ContentPartTemplate(model, "Parts/ManageWrapperPre").Location("primary", "5") : null,
                Services.Authorizer.Authorize(Permissions.ChangeOwner) ? ContentPartTemplate(model, "Parts/Manage").Location("primary", "5") : null,
                ContentPartTemplate(model, TemplateName, Prefix).Location("primary", "5"),
                Services.Authorizer.Authorize(Permissions.ChangeOwner) ? ContentPartTemplate(model, "Parts/ManageWrapperPost").Location("primary", "5") : null);
        }

        protected override DriverResult Editor(BodyAspect part) {
            var model = new BodyEditorViewModel { BodyAspect = part, TextEditorTemplate = DefaultTextEditorTemplate };
            return ContentPartTemplate(model, TemplateName, Prefix).Location("primary", "5");
        }

        protected override DriverResult Editor(BodyAspect part, IUpdateModel updater) {
            var model = new BodyEditorViewModel { BodyAspect = part, TextEditorTemplate = DefaultTextEditorTemplate };
            updater.TryUpdateModel(model, Prefix, null, null);
            return ContentPartTemplate(model, TemplateName, Prefix).Location("primary", "5");
        }

        // Can be moved somewhere else once we have IoC enabled body text filters.
        private static string BbcodeReplace(string bodyText) {
            Regex urlRegex = new Regex(@"\[url\]([^\]]+)\[\/url\]");
            Regex urlRegexWithLink = new Regex(@"\[url=([^\]]+)\]([^\]]+)\[\/url\]");
            Regex imgRegex = new Regex(@"\[img\]([^\]]+)\[\/img\]");

            bodyText = urlRegex.Replace(bodyText, "<a href=\"$1\">$1</a>");
            bodyText = urlRegexWithLink.Replace(bodyText, "<a href=\"$1\">$2</a>");
            bodyText = imgRegex.Replace(bodyText, "<img src=\"$1\" />");

            return bodyText;
        }
    }
}
