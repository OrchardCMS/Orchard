using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.UI.Admin;

namespace Orchard.Localization.Services {
    [OrchardFeature("Orchard.Localization.CultureSelector")]
    public class AdminDirectionalityFactory : ShapeDisplayEvents {
        private readonly IWorkContextAccessor _workContextAccessor;

        public AdminDirectionalityFactory(
            IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }


        private bool IsActivable() {
            var workContext = _workContextAccessor.GetContext();

            // activate on admin screen only
            if (AdminFilter.IsApplied(new RequestContext(workContext.HttpContext, new RouteData())))
                return true;

            return false;
        }

        public override void Displaying(ShapeDisplayingContext context) {
            context.ShapeMetadata.OnDisplaying(displayedContext => {
                if (!IsActivable()) {
                    return;
                }

                if (context.ShapeMetadata.Type != "EditorTemplate" &&
                    context.ShapeMetadata.Type != "Zone")
                    return;

                ContentItem contentItem = context.Shape.ContentItem;

                // if not, check for ContentPart 
                if (contentItem == null) {
                    ContentPart contentPart = context.Shape.ContentPart;
                    if (contentPart != null) {
                        contentItem = contentPart.ContentItem;
                    }
                }

                var workContext = _workContextAccessor.GetContext();
                var className = "content-" + workContext.GetTextDirection(contentItem);

                if (!_workContext.Layout.Content.Classes.Contains(className))
                    _workContext.Layout.Content.Classes.Add(className);
            });
        }


    }
}
