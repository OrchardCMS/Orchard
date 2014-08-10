using System.Web.Routing;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.UI.Admin;

namespace Orchard.Localization.Services {
    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class AdminCultureSelectorFactory : ShapeDisplayEvents {
        private readonly WorkContext _workContext;

        public AdminCultureSelectorFactory(
            IWorkContextAccessor workContextAccessor, 
            IShapeFactory shapeFactory) {
            _workContext = workContextAccessor.GetContext();
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        private bool IsActivable() {
            // activate on front-end only
            if (AdminFilter.IsApplied(new RequestContext(_workContext.HttpContext, new RouteData())))
                return true;

            return false;
        }

        public override void Displaying(ShapeDisplayingContext context) {
            context.ShapeMetadata.OnDisplaying(displayedContext => {
                if (displayedContext.ShapeMetadata.Type == "Layout" && IsActivable()) {
                    _workContext.Layout.Header.Add(Shape.UICultureSelector());
                }
            });
        }
    }
}