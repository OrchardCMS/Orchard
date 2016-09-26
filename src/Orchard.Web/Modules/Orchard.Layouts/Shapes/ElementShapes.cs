using System;
using System.IO;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Shapes;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment;
using Orchard.Layouts.Framework.Elements;
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
                var element = (Element)context.Shape.Element;

                // Tokenize common settings
                var content = (ContentItem)context.Shape.ContentItem;
                var htmlId = element.HtmlId;
                var htmlClass = element.HtmlClass;
                var htmlStyle = element.HtmlStyle;

                context.Shape.TokenizeHtmlId = (Func<string>)(() => _tokenizer.Value.Replace(htmlId, new { Content = content }));
                context.Shape.TokenizeHtmlClass = (Func<string>)(() => _tokenizer.Value.Replace(htmlClass, new { Content = content }));
                context.Shape.TokenizeHtmlStyle = (Func<string>)(() => _tokenizer.Value.Replace(htmlStyle, new { Content = content }));
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