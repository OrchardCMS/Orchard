using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.Mvc.Html;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Core.Common.ViewModels;
using Orchard.Services;
using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.Core.Common.Drivers {
    [UsedImplicitly]
    public class BodyPartDriver : ContentPartDriver<BodyPart> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private readonly RequestContext _requestContext;

        private const string TemplateName = "Parts.Common.Body";

        public BodyPartDriver(IOrchardServices services, IEnumerable<IHtmlFilter> htmlFilters, RequestContext requestContext) {
            _htmlFilters = htmlFilters;
            Services = services;
            _requestContext = requestContext;
        }

        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "Body"; }
        }

        protected override DriverResult Display(BodyPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Common_Body",
                             () => {
                                 var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text, GetFlavor(part)));
                                 return shapeHelper.Parts_Common_Body(Html: new HtmlString(bodyText));
                             }),
                ContentShape("Parts_Common_Body_Summary",
                             () => {
                                 var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text, GetFlavor(part)));
                                 return shapeHelper.Parts_Common_Body_Summary(Html: new HtmlString(bodyText));
                             })
                );
        }

        protected override DriverResult Editor(BodyPart part, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part,_requestContext);
            return ContentShape("Parts_Common_Body_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(BodyPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part, _requestContext);
            updater.TryUpdateModel(model, Prefix, null, null);

            return ContentShape("Parts_Common_Body_Edit", 
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override void Importing(BodyPart part, ContentManagement.Handlers.ImportContentContext context) {
            var importedText = context.Attribute(part.PartDefinition.Name, "Text");
            if (importedText != null) {
                part.Text = importedText;
            }
        }

        protected override void Exporting(BodyPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Text", part.Text);
        }

        private static BodyEditorViewModel BuildEditorViewModel(BodyPart part,RequestContext requestContext) {
            return new BodyEditorViewModel {
                BodyPart = part,
                EditorFlavor = GetFlavor(part),
                AddMediaPath = new PathBuilder(part,requestContext).AddContentType().AddContainerSlug().ToString()
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