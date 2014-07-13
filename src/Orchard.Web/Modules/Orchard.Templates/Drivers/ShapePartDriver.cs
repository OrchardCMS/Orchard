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

namespace Orchard.Templates.Drivers {
    public class ShapePartDriver : ContentPartDriver<ShapePart> {
        private readonly IEnumerable<ITemplateProcessor> _processors;
        private readonly ITransactionManager _transactions;

        public ShapePartDriver(
            IEnumerable<ITemplateProcessor> processors,
            ITransactionManager transactions) {
            _processors = processors;
            _transactions = transactions;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        protected override DriverResult Editor(ShapePart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ShapePart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new ShapePartViewModel {
                Template = part.Template
            };

            if (updater != null
                && updater.TryUpdateModel(viewModel, Prefix, null, new[] { "AvailableLanguages" })
                && ValidateShapeName(part, updater)) {
                part.Template = viewModel.Template;

                try {
                    var processor = _processors.FirstOrDefault(x => String.Equals(x.Type, part.ProcessorName, StringComparison.OrdinalIgnoreCase)) ?? _processors.First();
                    processor.Verify(part.Template);
                }
                catch (Exception ex) {
                    updater.AddModelError("", T("Template processing error: {0}", ex.Message));
                    _transactions.Cancel();
                }
            }
            return ContentShape("Parts_Shape_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts.Shape", Model: viewModel, Prefix: Prefix));
        }

        protected override void Exporting(ShapePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).Add(new XCData(part.Template));
        }

        protected override void Importing(ShapePart part, ImportContentContext context) {
            var shapeElement = context.Data.Element(part.PartDefinition.Name);

            if (shapeElement != null)
                part.Template = shapeElement.Value;
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

            updater.AddModelError("Title", T("{0} names can only contain alphanumerical or underscore (_) characters and have to start with a letter.", part.ContentItem.TypeDefinition.DisplayName));
            return false;
        }

        private class TitleViewModel {
            public string Title { get; set; }
        }
    }
}