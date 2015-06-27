using System.Collections.Generic;
using System.Linq;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.ViewModels;
using Orchard.Services;

namespace Orchard.Layouts.Drivers {
    public class HeadingElementDriver : ElementDriver<Heading> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        public HeadingElementDriver(IEnumerable<IHtmlFilter> htmlFilters) {
            _htmlFilters = htmlFilters;
        }

        protected override EditorResult OnBuildEditor(Heading element, ElementEditorContext context) {
            var viewModel = new HeadingEditorViewModel {
                Text = element.Content,
                Level = element.Level
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Heading", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.Content = viewModel.Text;
                element.Level = viewModel.Level;
            }
            
            return Editor(context, editor);
        }

        protected override void OnDisplaying(Heading element, ElementDisplayingContext context) {
            var text = element.Content;
            var flavor = "html";
            var processedText = ApplyHtmlFilters(text, flavor);

            context.ElementShape.ProcessedText = processedText;
            context.ElementShape.Level = element.Level;
        }

        private string ApplyHtmlFilters(string content, string flavor) {
            return _htmlFilters.Aggregate(content, (t, filter) => filter.ProcessContent(t, flavor));
        }
    }
}