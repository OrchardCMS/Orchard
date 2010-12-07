using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.DisplayManagement.Shapes {
    public interface ITagBuilderFactory : IDependency {
        OrchardTagBuilder Create(dynamic shape, string tagName);
    }

    public class OrchardTagBuilder : TagBuilder {
        public OrchardTagBuilder(string tagName) : base(tagName) { }

        public IHtmlString StartElement { get { return new HtmlString(ToString(TagRenderMode.StartTag)); } }
        public IHtmlString EndElement { get { return new HtmlString(ToString(TagRenderMode.EndTag)); } }
    }

    public class TagBuilderFactory : ITagBuilderFactory {
        public OrchardTagBuilder Create(dynamic shape, string tagName) {
            var tagBuilder = new OrchardTagBuilder(tagName);
            tagBuilder.MergeAttributes(shape.Attributes, false);
            foreach (var cssClass in shape.Classes ?? Enumerable.Empty<string>())
                tagBuilder.AddCssClass(cssClass);
            if (!string.IsNullOrEmpty(shape.Id))
                tagBuilder.GenerateId(shape.Id);
            return tagBuilder;
        }
    }
}
