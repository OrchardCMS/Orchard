using Orchard.Environment.Extensions;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.ViewModels;
using MarkdownElement = Orchard.Layouts.Elements.Markdown;

namespace Orchard.Layouts.Drivers {
    [OrchardFeature("Orchard.Layouts.Markdown")]
    public class MarkdownDriver : ElementDriver<MarkdownElement> {

        protected override EditorResult OnBuildEditor(MarkdownElement element, ElementEditorContext context) {
            var viewModel = new MarkdownEditorViewModel {
                Text = element.Content
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Markdown", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.Content = viewModel.Text;
            }
            
            return Editor(context, editor);
        }

        protected override void OnDisplaying(MarkdownElement element, ElementDisplayContext context) {
            context.ElementShape.ProcessedContent = new MarkdownSharp.Markdown().Transform(element.Content);
        }
    }
}