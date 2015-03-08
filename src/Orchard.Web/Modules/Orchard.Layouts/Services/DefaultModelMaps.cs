using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Orchard.DisplayManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Services {
    public abstract class LayoutModelMapBase<T> : ILayoutModelMap where T:Element {
        public virtual string LayoutElementType { get { return typeof(T).Name; } }

        public virtual bool CanMap(Element element) {
            return element is T;
        }

        public Element ToElement(IElementManager elementManager, DescribeElementsContext describeContext, JToken node) {
            var descriptor = elementManager.GetElementDescriptorByType<T>(describeContext);
            var element = elementManager.ActivateElement<T>(descriptor);
            ToElement(element, node);
            return element;
        }

        void ILayoutModelMap.FromElement(Element element, DescribeElementsContext describeContext, JToken node) {
            FromElement((T)element, describeContext, node);
        }

        public virtual void FromElement(T element, DescribeElementsContext describeContext, JToken node) {
            node["data"] = element.Data.Serialize();
            node["htmlId"] = element.HtmlId;
            node["htmlClass"] = element.HtmlClass;
            node["htmlStyle"] = element.HtmlStyle;
            node["isTemplated"] = element.IsTemplated;
        }

        protected virtual void ToElement(T element, JToken node) {
            element.Data = ElementDataHelper.Deserialize((string)node["data"]);
            element.HtmlId = (string)node["htmlId"];
            element.HtmlClass = (string)node["htmlClass"];
            element.HtmlStyle = (string)node["htmlStyle"];
            element.IsTemplated = (bool)(node["isTemplated"] ?? false);
        }
    }

    public class CanvasModelMap : LayoutModelMapBase<Canvas> {}
    public class GridModelMap : LayoutModelMapBase<Grid> {}
    public class RowModelMap : LayoutModelMapBase<Row> {}

    public class ColumnModelMap : LayoutModelMapBase<Column> {
        protected override void ToElement(Column element, JToken node) {
            base.ToElement(element, node);
            element.Width = (int?)node["width"];
            element.Offset = (int?)node["offset"];
        }

        public override void FromElement(Column element, DescribeElementsContext describeContext, JToken node) {
            base.FromElement(element, describeContext, node);
            node["width"] = element.Width;
            node["offset"] = element.Offset;
        }
    }

    public class ContentModelMap : ILayoutModelMap {
        private readonly IShapeDisplay _shapeDisplay;
        private readonly IElementDisplay _elementDisplay;
        private readonly UrlHelper _urlHelper;

        public ContentModelMap(IShapeDisplay shapeDisplay, IElementDisplay elementDisplay, UrlHelper urlHelper) {
            _shapeDisplay = shapeDisplay;
            _elementDisplay = elementDisplay;
            _urlHelper = urlHelper;
        }

        public string LayoutElementType { get { return "Content"; } }
        public bool CanMap(Element element) {
            return !(element is Container);
        }

        public Element ToElement(IElementManager elementManager, DescribeElementsContext describeContext, JToken node) {
            var elementTypeName = (string)node["contentType"];
            var descriptor = elementManager.GetElementDescriptorByTypeName(describeContext, elementTypeName);
            var element = elementManager.ActivateElement(descriptor);

            element.Data = ElementDataHelper.Deserialize((string)node["data"]);
            element.HtmlId = (string)node["htmlId"];
            element.HtmlClass = (string)node["htmlClass"];
            element.HtmlStyle = (string)node["htmlStyle"];
            element.IsTemplated = (bool)(node["isTemplated"] ?? false);

            // To support inline editing, we need to tell the element to update its content.
            var contentElement = element as IContentElement;
            if (contentElement != null) {
                var html = (string)node["html"];
                contentElement.Content = html;
            }

            return element;
        }

        public void FromElement(Element element, DescribeElementsContext describeContext, JToken node) {
            node["data"] = element.Data.Serialize();
            node["htmlId"] = element.HtmlId;
            node["htmlClass"] = element.HtmlClass;
            node["htmlStyle"] = element.HtmlStyle;
            node["isTemplated"] = element.IsTemplated;
            node["hasEditor"] = element.HasEditor;
            node["contentType"] = element.Descriptor.TypeName;
            node["contentTypeLabel"] = element.Descriptor.DisplayText.Text;
            node["contentTypeClass"] = element.DisplayText.Text.HtmlClassify();
            node["contentTypeDescription"] = element.Descriptor.Description.Text;
            node["html"] = _urlHelper.Encode(_shapeDisplay.Display(_elementDisplay.DisplayElement(element, content: describeContext.Content, displayType: "Design")));
        }
    }
}