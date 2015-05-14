using System;
using Newtonsoft.Json.Linq;
using Orchard.DynamicForms.Elements;
using Orchard.Layouts.Services;
using Orchard.Utility.Extensions;

namespace Orchard.DynamicForms.Services {
    public class FormModelMap : LayoutModelMapBase<Form> {
        protected override void ToElement(Form element, JToken node) {
            base.ToElement(element, node);
            element.Name = (string)node["name"];
            element.FormBindingContentType = (string)node["formBindingContentType"];
        }

        public override void FromElement(Form element, DescribeElementsContext describeContext, JToken node) {
            base.FromElement(element, describeContext, node);
            node["name"] = element.Name;
            node["formBindingContentType"] = element.FormBindingContentType;
            node["hasEditor"] = element.HasEditor;
            node["contentType"] = element.Descriptor.TypeName;
            node["contentTypeLabel"] = element.Descriptor.DisplayText.Text;
            node["contentTypeClass"] = String.Format(element.DisplayText.Text.HtmlClassify());
        }
    }
}