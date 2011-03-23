using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Widgets.Drivers {
    public class LayerHintHandler : ContentHandler {
        public LayerHintHandler(IOrchardServices services, RequestContext requestContext) {
            T = NullLocalizer.Instance;

            OnPublished<RoutePart>((context, part) => {
                if (string.IsNullOrWhiteSpace(part.Path)) // only going to help in creating a layer if the content has a path
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