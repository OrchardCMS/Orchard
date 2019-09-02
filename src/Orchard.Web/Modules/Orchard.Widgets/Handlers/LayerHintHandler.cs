using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Events;
using Orchard.ContentManagement;
using System;

namespace Orchard.Widgets.Handlers {

    public interface IRouteEvents : IEventHandler {
        void Routed(IContent content, String path);
    }

    [OrchardFeature("Orchard.Widgets.PageLayerHinting")]
    public class LayerHintHandler : IRouteEvents {
        public LayerHintHandler(IOrchardServices services, RequestContext requestContext) {
            T = NullLocalizer.Instance;
            _requestContext = requestContext;
            Services = services;
        }
        private readonly RequestContext _requestContext;
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public void Routed(IContent content, String path) {
            // Only going to help in creating a layer if the content is a page
            // TODO: (PH) Any reason not to enable the hint for *all* routed content?
            // TODO: (PH:Autoroute) Previously this only ran when the item was first published. Now it's running any time item is published. We want to catch
            // that and edit the existing layer rule rather than create a new one.
            if (content.ContentItem.ContentType != "Page")
                return;

            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets))
                return;

            var urlHelper = new UrlHelper(_requestContext);
            var pathForLayer = "~/" + path;
            var title = content.ContentItem.ContentManager.GetItemMetadata(content).DisplayText;

            Services.Notifier.Information(T("Would you like to <a href=\"{0}\">add a widget layer</a> for \"{1}\"?",
                urlHelper.Action("AddLayer", "Admin", new {
                    area = "Orchard.Widgets",
                    name = title,
                    layerRule = string.Format("url \"{0}\"", pathForLayer),
                    description = T("A widget layer for \"{0}\" at \"{1}\".", title, pathForLayer)
                }),
                title));
        }
    }
}