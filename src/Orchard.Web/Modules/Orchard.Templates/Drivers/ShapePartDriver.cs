using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Compilation.Razor;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Templates.Helpers;
using Orchard.Templates.Models;
using Orchard.Templates.Services;
using Orchard.Templates.ViewModels;
using Orchard.Utility.Extensions;

namespace Orchard.Templates.Drivers {
    public class ShapePartDriver : ContentPartDriver<ShapePart> {
        private readonly IEnumerable<ITemplateProcessor> _processors;
        private readonly IRazorTemplateHolder _templateHolder;
        private readonly ITransactionManager _transactions;

        public ShapePartDriver(
            IEnumerable<ITemplateProcessor> processors,
            IRazorTemplateHolder templateHolder, 
            ITransactionManager transactions) {
            _processors = processors;
            _templateHolder = templateHolder;
            _transactions = transactions;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        protected override DriverResult Editor(ShapePart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ShapePart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new ShapePartViewModel {
                Name = part.Name,
                Template = part.Template,
                Language = part.Language,
                AvailableLanguages = _processors.Select(x => x.Type).Distinct().ToArray()
            };

            if (updater != null 
                && updater.TryUpdateModel(viewModel, Prefix, null, new[] { "AvailableLanguages" })
                && ValidateShapeName(viewModel.Name, updater)) {
                    part.Name = viewModel.Name.TrimSafe();
                    part.Language = viewModel.Language;
                    part.Template = viewModel.Template;

                    try {
                        var processor = _processors.FirstOrDefault(x => String.Equals(x.Type, part.Language, StringComparison.OrdinalIgnoreCase));
                        processor.Verify(part.Template);
                        _templateHolder.Set(part.Name, part.Template);
                    }
                    catch (Exception ex) {
                        updater.AddModelError("", T("Template processing error: {0}", ex.Message));
                        _transactions.Cancel();
                    }
            }
            return ContentShape("Parts_Shape_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts.Shape", Model: viewModel, Prefix: Prefix));
        }

        private bool ValidateShapeName(string name, IUpdateModel updater) {
            if (!string.IsNullOrWhiteSpace(name) && 
                name[0].IsLetter() && 
                name.All(c => c.IsLetter() || Char.IsDigit(c) || c == '.' || c == '-' )) {
                return true;
            }

            updater.AddModelError("Name", T("Shape names can only contain alphanumerical, dot (.) or dash (-) characters and have to start with a letter."));
            return false;
        }
    }
}