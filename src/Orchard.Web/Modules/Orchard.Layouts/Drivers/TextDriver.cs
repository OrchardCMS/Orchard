using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Settings;
using Orchard.Layouts.ViewModels;
using Orchard.Services;

namespace Orchard.Layouts.Drivers {
    public class TextDriver : ElementDriver<Text> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        public TextDriver(IEnumerable<IHtmlFilter> htmlFilters) {
            _htmlFilters = htmlFilters;
        }

        protected override EditorResult OnBuildEditor(Text element, ElementEditorContext context) {
            var content = context.Content;
            var layoutPart = content.As<LayoutPart>();
            var flavor = layoutPart != null ? layoutPart.GetFlavor() : LayoutPartSettings.FlavorDefaultDefault; // TODO: make this configurable.

            var viewModel = new TextEditorViewModel {
                Flavor = flavor,
                Text = element.Content
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Text", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.Content = viewModel.Text;
            }
            
            return Editor(context, editor);
        }

        protected override void OnDisplaying(Text element, ElementDisplayContext context) {
            var text = element.Content;
            var layoutPart = context.Content.As<LayoutPart>();
            var flavor = layoutPart != null ? layoutPart.GetFlavor() : LayoutPartSettings.FlavorDefaultDefault; // TODO: provide a different way to configure flavor, so that an ElementWrapperWidget can control this setting as well.
            var processedText = _htmlFilters.Aggregate(text, (t, filter) => filter.ProcessContent(t, flavor));

            context.ElementShape.ProcessedText = processedText;
        }
    }
}