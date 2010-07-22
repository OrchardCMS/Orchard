using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Core.Common.ViewModels;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.Routable.Models;

namespace Orchard.Core.Common.Drivers {
    [UsedImplicitly]
    public class BodyDriver : ContentPartDriver<BodyAspect> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Parts/Common.Body";
        private const string DefaultTextEditorTemplate = "TinyMceTextEditor";
        private const string PlainTextEditorTemplate = "PlainTextEditor";

        public BodyDriver(IOrchardServices services) {
            Services = services;
        }

        protected override string Prefix {
            get { return "Body"; }
        }

        // \/\/ Hackalicious on many accounts - don't copy what has been done here for the wrapper \/\/
        protected override DriverResult Display(BodyAspect part, string displayType) {
            var model = new BodyDisplayViewModel { BodyAspect = part, Text = BbcodeReplace(part.Text) };
            var location = part.GetLocation(displayType, "primary", "5");

            return Combined(
                Services.Authorizer.Authorize(Permissions.ChangeOwner) ? ContentPartTemplate(model, "Parts/Common.Body.ManageWrapperPre").LongestMatch(displayType, "SummaryAdmin").Location(location) : null,
                Services.Authorizer.Authorize(Permissions.ChangeOwner) ? ContentPartTemplate(model, "Parts/Common.Body.Manage").LongestMatch(displayType, "SummaryAdmin").Location(location) : null,
                ContentPartTemplate(model, TemplateName, Prefix).LongestMatch(displayType, "Summary", "SummaryAdmin").Location(location),
                Services.Authorizer.Authorize(Permissions.ChangeOwner) ? ContentPartTemplate(model, "Parts/Common.Body.ManageWrapperPost").LongestMatch(displayType, "SummaryAdmin").Location(location) : null);
        }
        
        protected override DriverResult Editor(BodyAspect part) {
            var model = BuildEditorViewModel(part);
            var location = part.GetLocation("Editor", "primary", "5");
            return ContentPartTemplate(model, TemplateName, Prefix).Location(location);
        }

        protected override DriverResult Editor(BodyAspect part, IUpdateModel updater) {
            var model = BuildEditorViewModel(part);
            updater.TryUpdateModel(model, Prefix, null, null);

            // only set the format if it has not yet been set to preserve the initial format type - might want to change this later to support changing body formats but...later
            if (string.IsNullOrWhiteSpace(model.Format))
                model.Format = GetFlavor(part);

            var location = part.GetLocation("Editor", "primary", "5");
            return ContentPartTemplate(model, TemplateName, Prefix).Location(location);
        }

        private static BodyEditorViewModel BuildEditorViewModel(BodyAspect part) {
            return new BodyEditorViewModel {
                BodyAspect = part,
                TextEditorTemplate = GetFlavor(part) == "html" ? DefaultTextEditorTemplate : PlainTextEditorTemplate,
                AddMediaPath= new PathBuilder(part).AddContentType().AddContainerSlug().AddSlug().ToString()
            };
        }

        private static string GetFlavor(BodyAspect part) {
            var typePartSettings = part.Settings.GetModel<BodyTypePartSettings>();
            return (typePartSettings != null && !string.IsNullOrWhiteSpace(typePartSettings.Flavor))
                       ? typePartSettings.Flavor
                       : part.PartDefinition.Settings.GetModel<BodyPartSettings>().FlavorDefault;
        }

        class PathBuilder {
            private readonly IContent _content;
            private string _path;

            public PathBuilder(IContent content) {
                _content = content;
                _path = "";
            }

            public override string ToString() {
                return _path;
            }

            public PathBuilder AddContentType() {
                Add(_content.ContentItem.ContentType);
                return this;
            }

            public PathBuilder AddContainerSlug() {
                var common = _content.As<ICommonAspect>();
                if (common == null)
                    return this;

                var routable = common.Container.As<IsRoutable>();
                if (routable == null)
                    return this;

                Add(routable.Slug);
                return this;
            }

            public PathBuilder AddSlug() {
                var routable = _content.As<IsRoutable>();
                if (routable == null)
                    return this;

                Add(routable.Slug);
                return this;
            }

            private void Add(string segment) {
                if (string.IsNullOrEmpty(segment))
                    return;
                if (string.IsNullOrEmpty(_path))
                    _path = segment;
                else
                    _path = _path + "/" + segment;
            }

        }


        // Can be moved somewhere else once we have IoC enabled body text filters.
        private static string BbcodeReplace(string bodyText) {

            if ( string.IsNullOrEmpty(bodyText) )
                return string.Empty;

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