using System;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Core.Shapes;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Settings;
using Orchard.Tokens;

namespace Orchard.Layouts.Shapes {
    public class ElementShapes : IShapeTableProvider {
        private readonly ITagBuilderFactory _tagBuilderFactory;
        private readonly Work<IShapeFactory> _shapeFactory;
        private readonly Work<ITokenizer> _tokenizer;

        public ElementShapes(
            ITagBuilderFactory tagBuilderFactory, 
            Work<IShapeFactory> shapeFactory, 
            Work<ITokenizer> tokenizer) {

            _tagBuilderFactory = tagBuilderFactory;
            _shapeFactory = shapeFactory;
            _tokenizer = tokenizer;
        }

        public dynamic New {
            get { return _shapeFactory.Value; }
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Element").OnDisplaying(context => {
                var element = (IElement)context.Shape.Element;
                if (context.ShapeMetadata.DisplayType == "Design") {
                    var descriptor = element.Descriptor;

                    if (!(element is IContainer)) {
                        context.ShapeMetadata.Wrappers.Add("Element_DesignWrapper");
                    }

                    // Inject client side json.
                    context.Shape.ElementJson = JsonConvert.SerializeObject(new {
                        typeName = descriptor.TypeName,
                        displayText = descriptor.DisplayText.Text,
                        state = element.State.Serialize(),
                        index = element.Index,
                        isTemplated = element.IsTemplated
                    });
                }

                // Tokenize common settings
                var content = (ContentItem)context.Shape.ContentItem;
                var settings = element.State ?? new StateDictionary();
                var commonSettings = settings.GetModel<CommonElementSettings>();
                var id = commonSettings.Id;
                var cssClass = commonSettings.CssClass;
                var inlineStyle = commonSettings.InlineStyle;

                context.Shape.TokenizeId = (Func<string>)(() => _tokenizer.Value.Replace(id, new { Content = content }));
                context.Shape.TokenizeInlineStyle = (Func<string>)(() => _tokenizer.Value.Replace(inlineStyle, new { Content = content }));
                context.Shape.TokenizeCssClass = (Func<string>)(() => _tokenizer.Value.Replace(cssClass, new { Content = content }));
            });
        }

        [Shape]
        public void ElementEditor__Forms(dynamic Shape, dynamic Display, TextWriter Output) {
            foreach (var form in Shape.Forms) {
                form.Classes.Add("form-fieldset");
                var tagBuilder = _tagBuilderFactory.Create(form, "fieldset");
                var title = form.Title != null ? form.Title.ToString() : default(string);

                Output.Write(tagBuilder.StartElement);
                if (title != null) {
                    var legendTagBuilder = new TagBuilder("legend") {InnerHtml = title};
                    Output.WriteLine(legendTagBuilder.ToString());
                }
                DisplayChildren(form, Display, Output);
                Output.WriteLine(tagBuilder.EndElement);
            }
        }

        [Shape]
        public void ElementZone(dynamic Display, dynamic Shape, TextWriter Output) {
            foreach (var item in CoreShapes.Order(Shape))
                Output.Write(Display(item));
        }

        private static void DisplayChildren(dynamic shape, dynamic display, TextWriter output) {
            foreach (var child in shape) {
                output.Write(display(child));
            }
        }
    }
}