using System.Collections.Generic;
using System.Linq;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.ViewModels;
using Orchard.Services;

namespace Orchard.Layouts.Drivers {
    public class TextElementDriver : ElementDriver<Text> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        public TextElementDriver(IEnumerable<IHtmlFilter> htmlFilters) {
            _htmlFilters = htmlFilters;
        }

        protected override EditorResult OnBuildEditor(Text element, ElementEditorContext context) {
            var viewModel = new TextEditorViewModel {
                Content = element.Content
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Text", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.Content = viewModel.Content;
            }
            
            return Editor(context, editor);
        }

        protected override void OnDisplaying(Text element, ElementDisplayContext context) {
            var text = element.Content;
            var flavor = "textarea";
            var processedText = ApplyHtmlFilters(text, flavor);

            context.ElementShape.ProcessedText = processedText;
        }

        private string ApplyHtmlFilters(string content, string flavor) {
            return _htmlFilters.Aggregate(content, (t, filter) => filter.ProcessContent(t, flavor));
        }
    }
}