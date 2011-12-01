using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Feeds.StandardQueries {
    [UsedImplicitly]
    public class ContainerFeedQuery : IFeedQueryProvider, IFeedQuery {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        public ContainerFeedQuery(IContentManager contentManager, IEnumerable<IHtmlFilter> htmlFilters) {
            _contentManager = contentManager;
            _htmlFilters = htmlFilters;
        }

        public FeedQueryMatch Match(FeedContext context) {
            var containerIdValue = context.ValueProvider.GetValue("containerid");
            if (containerIdValue == null)
                return null;

            return new FeedQueryMatch { FeedQuery = this, Priority = -5 };
        }

        public void Execute(FeedContext context) {
            var containerIdValue = context.ValueProvider.GetValue("containerid");
            if (containerIdValue == null)
                return;

            var limitValue = context.ValueProvider.GetValue("limit");
            var limit = 20;
            if (limitValue != null)
                limit = (int)limitValue.ConvertTo(typeof(int));

            var containerId = (int)containerIdValue.ConvertTo(typeof(int));
            var container = _contentManager.Get(containerId);

            var inspector = new ItemInspector(container, _contentManager.GetItemMetadata(container), _htmlFilters);
            if (context.Format == "rss") {
                var link = new XElement("link");
                context.Response.Element.SetElementValue("title", inspector.Title);
                context.Response.Element.Add(link);
                context.Response.Element.SetElementValue("description", inspector.Description);

                context.Response.Contextualize(requestContext => {
                    var urlHelper = new UrlHelper(requestContext);
                    var uriBuilder = new UriBuilder(urlHelper.RequestContext.HttpContext.Request.ToRootUrlString()) { Path = urlHelper.RouteUrl(inspector.Link) };
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

            var items = _contentManager.Query()
                .Where<CommonPartRecord>(x => x.Container == container.Record)
                .OrderByDescending(x => x.CreatedUtc)
                .Slice(0, limit);

            foreach (var item in items) {
                context.Builder.AddItem(context, item);
            }
        }
    }
}