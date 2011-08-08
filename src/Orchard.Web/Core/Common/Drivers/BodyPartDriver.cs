using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Parts;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Core.Common.ViewModels;
using Orchard.Core.Routable.Models;
using Orchard.Services;

namespace Orchard.Core.Common.Drivers {
    public class BodyPartDriver : ContentPartDriver<BodyPart> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        private const string TemplateName = "Parts.Common.Body";

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
                ContentShape("Parts_Common_Body",
                             () => {
                                 var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text));
                                 return shapeHelper.Parts_Common_Body(ContentPart: part, Html: new HtmlString(bodyText));
                             }),
                ContentShape("Parts_Common_Body_Summary",
                             () => {
                                 var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text));
                                 return shapeHelper.Parts_Common_Body_Summary(ContentPart: part, Html: new HtmlString(bodyText));
                             })
                );
        }

        protected override DriverResult Editor(BodyPart part, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part);
            return ContentShape("Parts_Common_Body_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(BodyPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part);
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

        private static BodyEditorViewModel BuildEditorViewModel(BodyPart part) {
            return new BodyEditorViewModel {
                BodyPart = part,
                EditorFlavor = GetFlavor(part)
            };
        }

        private static string GetFlavor(BodyPart part) {
            var typePartSettings = part.Settings.GetModel<BodyTypePartSettings>();
            return (typePartSettings != null && !string.IsNullOrWhiteSpace(typePartSettings.Flavor))
                       ? typePartSettings.Flavor
                       : part.PartDefinition.Settings.GetModel<BodyPartSettings>().FlavorDefault;
        }
    }
}