using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Mvc.Extensions;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Projections.StandardQueries {
    public class QueryFeedQuery : IFeedQueryProvider, IFeedQuery {
        private readonly IContentManager _contentManager;
        private readonly IProjectionManager _projectionManager;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        public QueryFeedQuery(
            IContentManager contentManager,
            IProjectionManager projectionManager,
            IEnumerable<IHtmlFilter> htmlFilters)
        {
            _contentManager = contentManager;
            _projectionManager = projectionManager;
            _htmlFilters = htmlFilters;
        }

        public FeedQueryMatch Match(FeedContext context) {
            var containerIdValue = context.ValueProvider.GetValue("projection");
            if (containerIdValue == null)
                return null;

            return new FeedQueryMatch { FeedQuery = this, Priority = 0 };
        }

        public void Execute(FeedContext context) {
            var projectionId = context.ValueProvider.GetValue("projection");
            if (projectionId == null || String.IsNullOrEmpty(projectionId.AttemptedValue))
                return;

            var limitValue = context.ValueProvider.GetValue("limit");
            var limit = 20;
            if (limitValue != null) {
                Int32.TryParse(Convert.ToString(limitValue), out limit);
            }

            var containerId = (int)projectionId.ConvertTo(typeof(int));
            var container = _contentManager.Get<ProjectionPart>(containerId);

            if (container == null) {
                return;
            }

            var inspector = new ItemInspector(container, _contentManager.GetItemMetadata(container), _htmlFilters);
            if (context.Format == "rss") {
                var link = new XElement("link");
                context.Response.Element.SetElementValue("title", inspector.Title);
                context.Response.Element.Add(link);
                context.Response.Element.SetElementValue("description", inspector.Description);

                context.Response.Contextualize(requestContext => {
                    var urlHelper = new UrlHelper(requestContext);
                    var uriBuilder = new UriBuilder(urlHelper.MakeAbsolute("/")) { Path = urlHelper.RouteUrl(inspector.Link) };
                    link.Add(uriBuilder.Uri.OriginalString);
                });
            }
            else {
                context.Builder.AddProperty(context, null, "title", inspector.Title);
                context.Builder.AddProperty(context, null, "description", inspector.Description);
                context.Response.Contextualize(requestContext => {
                    var urlHelper = new UrlHelper(requestContext);
                    context.Builder.AddProperty(context, null, "link", urlHelper.RouteUrl(inspector.Link));
                });
            }

            var items = _projectionManager.GetContentItems(container.Record.QueryPartRecord.Id, 0, limit).ToList();

            foreach (var item in items) {
                context.Builder.AddItem(context, item);
            }
        }
    }
}