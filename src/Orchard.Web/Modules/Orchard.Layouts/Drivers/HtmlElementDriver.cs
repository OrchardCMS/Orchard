using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.ViewModels;
using Orchard.Services;

namespace Orchard.Layouts.Drivers
{
    public class HtmlElementDriver : ElementDriver<Html> {
        private readonly IHtmlFilterRunner _runner;

        public HtmlElementDriver(IHtmlFilterRunner runner) {
            _runner = runner;
        }

        protected override EditorResult OnBuildEditor(Html element, ElementEditorContext context) {
            var viewModel = new HtmlEditorViewModel {
                Text = element.Content,
                Part = ((dynamic)context.Content.ContentItem).LayoutPart
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Html", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.Content = viewModel.Text;
            }
            
            return Editor(context, editor);
        }

        protected override void OnDisplaying(Html element, ElementDisplayingContext context) {
            context.ElementShape.ProcessedContent = _runner.RunFilters(element.Content, new HtmlFilterContext { Flavor = "html", Data = context.GetTokenData() });
        }
    }
}