using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions.Models;
using Orchard.UI;
using Orchard.UI.Zones;

namespace Orchard.Core.Shapes {
    public class CoreShapes : IShapeDescriptorBindingStrategy {
        public Feature Feature { get; set; }

        public void Discover(ShapeTableBuilder builder) {            
            // the root page shape named 'Layout' is wrapped with 'Document'
            // and has an automatic zone creating behavior
            builder.Describe.Named("Layout").From(Feature.Descriptor)
                .OnCreating(creating => creating.Behaviors.Add(new ZoneHoldingBehavior(creating.ShapeFactory)))
                .Configure(descriptor => descriptor.Wrappers.Add("Document"));

            // 'Zone' shapes are built on the Zone base class
            builder.Describe.Named("Zone").From(Feature.Descriptor)
                .OnCreating(creating => creating.BaseType = typeof(Zone));
        }

        static object DetermineModel(HtmlHelper Html, object Model) {
            bool isNull = ((dynamic)Model) == null;
            return isNull ? Html.ViewData.Model : Model;
        }

        [Shape]
        public IHtmlString Partial(HtmlHelper Html, string TemplateName, object Model) {
            return Html.Partial(TemplateName, DetermineModel(Html, Model));
        }

        [Shape]
        public IHtmlString DisplayTemplate(HtmlHelper Html, string TemplateName, object Model, string Prefix) {
            return Html.Partial(TemplateName, DetermineModel(Html, Model));
        }

        [Shape]
        public IHtmlString EditorTemplate(HtmlHelper Html, string TemplateName, object Model, string Prefix) {
            return Html.Partial(TemplateName, DetermineModel(Html, Model));
        }
    }
}