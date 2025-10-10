using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.ViewModels;
using Orchard.Services;

namespace Orchard.Layouts.Drivers {
    public class ParagraphElementDriver : ElementDriver<Paragraph> {
        private readonly IHtmlFilterProcessor _htmlFilterProcessor;

        public ParagraphElementDriver(IHtmlFilterProcessor htmlFilterProcessor) {
            _htmlFilterProcessor = htmlFilterProcessor;
        }

        protected override EditorResult OnBuildEditor(Paragraph element, ElementEditorContext context) {
            var viewModel = new ParagraphEditorViewModel {
                Text = element.Content
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Paragraph", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.Content = viewModel.Text;
            }

            return Editor(context, editor);
        }

        protected override void OnDisplaying(Paragraph element, ElementDisplayingContext context) {
            context.ElementShape.ProcessedContent = _htmlFilterProcessor.ProcessFilters(element.Content, new HtmlFilterContext { Flavor = "html", Data = context.GetTokenData() });
        }
    }
}