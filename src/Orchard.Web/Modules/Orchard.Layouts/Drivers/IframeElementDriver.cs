using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.ViewModels;
using Orchard.Tokens;
using System.Collections.Generic;

namespace Orchard.Layouts.Drivers {
    public class IframeElementDriver : ElementDriver<Iframe> {
        private readonly ITokenizer _tokenizer;

        public IframeElementDriver(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(Iframe element, ElementEditorContext context) {
            var viewModel = new IframeEditorViewModel {
                Src = element.Src,
                Width = element.Width,
                Height = element.Height,
                AllowFullscreen = element.AllowFullscreen
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Iframe", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.Src = viewModel.Src;
                element.Width = viewModel.Width;
                element.Height = viewModel.Height;
                element.AllowFullscreen = viewModel.AllowFullscreen;
            }

            return Editor(context, editor);
        }

        protected override void OnDisplaying(Iframe element, ElementDisplayContext context) {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes.Add("src", _tokenizer.Replace(element.Src, new Dictionary<string, object> { { "Content", context.Content.ContentItem } }));
            if (element.Width.HasValue) {
                attributes.Add("width", element.Width.Value.ToString());
            }
            if (element.Height.HasValue) {
                attributes.Add("height", element.Height.Value.ToString());
            }
            if (element.AllowFullscreen) {
                attributes.Add("allowfullscreen", "");
            }
            context.ElementShape.IframeAttributes = attributes;
        }
    }
}