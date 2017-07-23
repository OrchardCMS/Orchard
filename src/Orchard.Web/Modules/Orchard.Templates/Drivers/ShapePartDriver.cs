using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Templates.Models;
using Orchard.Templates.Services;
using Orchard.Templates.ViewModels;
using Orchard.Utility.Extensions;
using Orchard.Core.Title.Models;

namespace Orchard.Templates.Drivers {
    public class ShapePartDriver : ContentPartDriver<ShapePart> {
        private readonly IEnumerable<ITemplateProcessor> _processors;
        private readonly ITransactionManager _transactions;
        private readonly IContentManager _contentManager;

        public ShapePartDriver(
            IEnumerable<ITemplateProcessor> processors,
            ITransactionManager transactions,
            IContentManager contentManager) {
            _processors = processors;
            _transactions = transactions;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        protected override DriverResult Display(ShapePart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Shape_SummaryAdmin", () => shapeHelper.Parts_Shape_SummaryAdmin());
        }

        protected override DriverResult Editor(ShapePart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ShapePart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new ShapePartViewModel {
                Template = part.Template,
                RenderingMode = part.RenderingMode
            };

            if (updater != null
                && updater.TryUpdateModel(viewModel, Prefix, null, new[] { "AvailableLanguages" })
                && ValidateShapeName(part, updater)) {
                part.Template = viewModel.Template;
                part.RenderingMode = viewModel.RenderingMode;

                try {
                    var processor = _processors.FirstOrDefault(x => String.Equals(x.Type, part.ProcessorName, StringComparison.OrdinalIgnoreCase)) ?? _processors.First();
                    processor.Verify(part.Template);
                }
                catch (Exception ex) {
                    updater.AddModelError("", T("Template processing error: {0}", ex.Message));
                    _transactions.Cancel();
                }

                // We need to query for the content type names because querying for content parts has no effect on the database side.
                var contentTypesWithShapePart = _contentManager
                    .GetContentTypeDefinitions()
                    .Where(typeDefinition => typeDefinition.Parts.Any(partDefinition => partDefinition.PartDefinition.Name == "ShapePart"))
                    .Select(typeDefinition => typeDefinition.Name);

                // If ShapePart is only dynamically added to this content type or even this content item then we won't find
                // a corresponding content type definition, so using the current content type too.
                contentTypesWithShapePart = contentTypesWithShapePart.Union(new[] { part.ContentItem.ContentType });

                var existingShapes = _contentManager
                    .Query(VersionOptions.Latest, contentTypesWithShapePart.ToArray())
                    .Where<TitlePartRecord>(record => record.Title == part.As<TitlePart>().Title && record.ContentItemRecord.Id != part.ContentItem.Id);

                if (existingShapes.List().Any(x => x.As<ShapePart>().RenderingMode == part.RenderingMode)) {
                    updater.AddModelError("ShapeNameAlreadyExists", T("A template with the given name and rendering mode already exists."));
                }
            }
            return ContentShape("Parts_Shape_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts.Shape", Model: viewModel, Prefix: Prefix));
        }

        protected override void Exporting(ShapePart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            element.Add(new XCData(part.Template));
            element.SetAttributeValue("RenderingMode", part.RenderingMode.ToString());
        }

        protected override void Importing(ShapePart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            var shapeElement = context.Data.Element(part.PartDefinition.Name);

            if (shapeElement != null) {
                part.Template = shapeElement.Value;
                context.ImportAttribute(part.PartDefinition.Name, "RenderingMode", value => part.RenderingMode = (RenderingMode)Enum.Parse(typeof(RenderingMode), value));
            }
        }

        private bool ValidateShapeName(ShapePart part, IUpdateModel updater) {
            var titleViewModel = new TitleViewModel();
            if (!updater.TryUpdateModel(titleViewModel, "Title", null, null))
                return false;

            var name = titleViewModel.Title;
            if (!String.IsNullOrWhiteSpace(name) &&
                name[0].IsLetter() &&
                name.All(c => c.IsLetter() || Char.IsDigit(c) || c == '_')) {
                return true;
            }

            updater.AddModelError("Title", T("{0} names can only contain alphanumerical or underscore (_) characters and have to start with a letter.", T(part.ContentItem.TypeDefinition.DisplayName)));
            return false;
        }

        private class TitleViewModel {
            public string Title { get; set; }
        }
    }
}