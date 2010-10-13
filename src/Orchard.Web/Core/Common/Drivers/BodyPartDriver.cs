using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Core.Common.ViewModels;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.Routable.Models;
using Orchard.Services;

namespace Orchard.Core.Common.Drivers {
    [UsedImplicitly]
    public class BodyPartDriver : ContentPartDriver<BodyPart> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        private const string TemplateName = "Parts/Common.Body";
        //todo: change back - or to something better
        private const string DefaultTextEditorTemplate = "TinyMceTextEditor";
        private const string PlainTextEditorTemplate = "PlainTextEditor";

        public BodyPartDriver(IOrchardServices services, IEnumerable<IHtmlFilter> htmlFilters) {
            _htmlFilters = htmlFilters;
            Services = services;
        }

        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "Body"; }
        }

        protected override DriverResult Display(BodyPart part, string displayType, dynamic shapeHelper) {

            return Combined(
                ContentShape("Parts_Common_Body", displayType == "Detail" ? "Content" : null, () => {
                    var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text));
                    return shapeHelper.Parts_Common_Body(ContentPart: part, Html: new HtmlString(bodyText));
                }),
                ContentShape("Parts_Common_Body_Summary", displayType == "Summary" ? "Content" : null, () => {
                    var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text));
                    return shapeHelper.Parts_Common_Body_Summary(ContentPart: part, Html: new HtmlString(bodyText));
                })
            );
        }

        protected override DriverResult Editor(BodyPart part, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part);
            var location = part.GetLocation("Editor");
            return ContentPartTemplate(model, TemplateName, Prefix).Location(location);
        }

        protected override DriverResult Editor(BodyPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part);
            updater.TryUpdateModel(model, Prefix, null, null);

            // only set the format if it has not yet been set to preserve the initial format type - might want to change this later to support changing body formats but...later
            if (string.IsNullOrWhiteSpace(model.Format))
                model.Format = GetFlavor(part);

            var location = part.GetLocation("Editor");
            return ContentPartTemplate(model, TemplateName, Prefix).Location(location);
        }

        private static BodyEditorViewModel BuildEditorViewModel(BodyPart part) {
            return new BodyEditorViewModel {
                BodyPart = part,
                TextEditorTemplate = GetFlavor(part) == "html" ? DefaultTextEditorTemplate : PlainTextEditorTemplate,
                AddMediaPath = new PathBuilder(part).AddContentType().AddContainerSlug().AddSlug().ToString()
            };
        }

        private static string GetFlavor(BodyPart part) {
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
                var common = _content.As<ICommonPart>();
                if (common == null)
                    return this;

                var routable = common.Container.As<RoutePart>();
                if (routable == null)
                    return this;

                Add(routable.Slug);
                return this;
            }

            public PathBuilder AddSlug() {
                var routable = _content.As<RoutePart>();
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
    }
}