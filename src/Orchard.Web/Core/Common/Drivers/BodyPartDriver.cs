using System.Web;
using Orchard.Mvc.Html;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;
using Orchard.Services;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.Drivers {
    public class BodyPartDriver : ContentPartDriver<BodyPart> {
        private readonly IHtmlFilterProcessor _htmlFilterProcessor;
        private readonly RequestContext _requestContext;

        private const string TemplateName = "Parts.Common.Body";

        public BodyPartDriver(IOrchardServices services, IHtmlFilterProcessor htmlFilterProcessor, RequestContext requestContext) {
            _htmlFilterProcessor = htmlFilterProcessor;
            Services = services;
            _requestContext = requestContext;
        }

        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "Body"; }
        }

        protected override DriverResult Display(BodyPart part, string displayType, dynamic shapeHelper) {
            string GetProcessedBodyText() => _htmlFilterProcessor.ProcessFilters(part.Text, part.GetFlavor(), part);

            return Combined(
                ContentShape("Parts_Common_Body", () =>
                    shapeHelper.Parts_Common_Body(Html: new HtmlString(GetProcessedBodyText()))),
                ContentShape("Parts_Common_Body_Summary", () =>
                    shapeHelper.Parts_Common_Body_Summary(Html: new HtmlString(GetProcessedBodyText()))));
        }

        protected override DriverResult Editor(BodyPart part, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part,_requestContext);
            return ContentShape("Parts_Common_Body_Edit", () =>
                shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(BodyPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part, _requestContext);
            updater.TryUpdateModel(model, Prefix, null, null);

            return ContentShape("Parts_Common_Body_Edit", () =>
                shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override void Importing(BodyPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Text", importedText =>
                part.Text = importedText
            );
        }

        protected override void Exporting(BodyPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Text", part.Text);
        }

        protected override void Cloning(BodyPart originalPart, BodyPart clonePart, CloneContentContext context) {
            clonePart.Text = originalPart.Text;
        }

        private static BodyEditorViewModel BuildEditorViewModel(BodyPart part,RequestContext requestContext) {
            return new BodyEditorViewModel {
                BodyPart = part,
                EditorFlavor = part.GetFlavor(),
                AddMediaPath = new PathBuilder(part,requestContext).AddContentType().AddContainerSlug().ToString()
            };
        }

        class PathBuilder {
            private readonly IContent _content;
            private string _path;
            private readonly RequestContext _requestContext;

            public PathBuilder(IContent content,RequestContext requestContext) {
                _content = content;
                _path = "";
                _requestContext = requestContext;
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
                if (common == null || common.Container==null)
                    return this;
                var helper = new UrlHelper(_requestContext);
                Add(helper.ItemDisplayUrl(common.Container));
                return this;
            }

            private void Add(string segment) {
                if (string.IsNullOrEmpty(segment))
                    return;

                if (string.IsNullOrEmpty(_path)) {
                    _path = segment;
                }
                else if (segment.StartsWith("/")) {
                    _path = _path + segment;
                }
                else {
                    _path = _path + "/" + segment;
                }
            }
        }
    }
}