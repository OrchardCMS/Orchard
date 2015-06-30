using Newtonsoft.Json.Linq;
using Orchard.DynamicForms.Elements;
using Orchard.Layouts.Services;

namespace Orchard.DynamicForms.Services {
    public class FieldsetModelMap : LayoutModelMapBase<Fieldset> {
        protected override void ToElement(Fieldset element, JToken node) {
            base.ToElement(element, node);
            element.Legend = (string) node["legend"];
        }

        public override void FromElement(Fieldset element, DescribeElementsContext describeContext, JToken node) {
            base.FromElement(element, describeContext, node);
            node["legend"] = element.Legend;
        }
    }
}