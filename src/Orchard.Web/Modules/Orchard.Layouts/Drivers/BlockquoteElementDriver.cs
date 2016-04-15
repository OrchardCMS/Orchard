using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Layouts.ViewModels;

namespace Orchard.Layouts.Drivers {
    public class BlockquoteElementDriver : ElementDriver<Blockquote> {
        private readonly IElementFilterProcessor _processor;

        public BlockquoteElementDriver(IElementFilterProcessor processor) {
            _processor = processor;
        }

        protected override EditorResult OnBuildEditor(Blockquote element, ElementEditorContext context) {
            var viewModel = new BlockquoteEditorViewModel {
                Text = element.Content,
                Citation = element.Citation
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Blockquote", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.Content = viewModel.Text;
                element.Citation = viewModel.Citation;
            }
            
            return Editor(context, editor);
        }

        protected override void OnDisplaying(Blockquote element, ElementDisplayingContext context) {
            context.ElementShape.ProcessedContent = _processor.ProcessContent(element.Content, "html", context.GetTokenData());
        }
    }
}