using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Widgets.Handlers {
    [OrchardFeature("Orchard.Widgets.PageLayerHinting")]
    public class LayerHintHandler : ContentHandler {
        public LayerHintHandler(IOrchardServices services, RequestContext requestContext) {
            T = NullLocalizer.Instance;

            OnPublished<RoutePart>((context, part) => {
                // only going to help in creating a layer if the content is a page with no previous version and a path
                if (!(context.ContentType == "Page" && context.PreviousItemVersionRecord == null && !string.IsNullOrWhiteSpace(part.Path)))
                    return;

                var urlHelper = new UrlHelper(requestContext);
                var pathForLayer = "~/" + part.Path;
                services.Notifier.Information(T("Would you like to <a href=\"{0}\">add a widget layer</a> for \"{1}\"?",
                    urlHelper.Action("AddLayer", "Admin", new {
                        area = "Orchard.Widgets",
                        name = part.Title,
                        layerRule = string.Format("url \"{0}\"", pathForLayer),
                        description = T("A widget layer for \"{0}\" at \"{1}\".", part.Title, pathForLayer)
                    }),
                    part.Title));
            });
        }

        public Localizer T { get; set; }
    }
}