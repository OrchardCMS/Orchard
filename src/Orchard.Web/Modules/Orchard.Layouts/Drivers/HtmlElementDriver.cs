using System.Collections.Generic;
using System.Linq;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.ViewModels;
using Orchard.Services;

namespace Orchard.Layouts.Drivers {
    public class HtmlElementDriver : ElementDriver<Html> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        public HtmlElementDriver(IEnumerable<IHtmlFilter> htmlFilters) {
            _htmlFilters = htmlFilters;
        }

        protected override EditorResult OnBuildEditor(Html element, ElementEditorContext context) {
            var viewModel = new HtmlEditorViewModel {
                Text = element.Content
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Html", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.Content = viewModel.Text;
            }
            
            return Editor(context, editor);
        }

        protected override void OnDisplaying(Html element, ElementDisplayContext context) {
            var text = element.Content;
            var flavor = "html";
            var processedText = ApplyHtmlFilters(text, flavor);

            context.ElementShape.ProcessedText = processedText;
        }

        private string ApplyHtmlFilters(string content, string flavor) {
            return _htmlFilters.Aggregate(content, (t, filter) => filter.ProcessContent(t, flavor));
        }
    }
}