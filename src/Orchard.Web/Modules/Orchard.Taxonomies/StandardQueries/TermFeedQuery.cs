using System;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Mvc.Extensions;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Utility.Extensions;

namespace Orchard.Taxonomies.StandardQueries {
    [UsedImplicitly]
    public class TermFeedQuery : IFeedQueryProvider, IFeedQuery {
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;

        public TermFeedQuery(IContentManager contentManager, ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
        }

        public FeedQueryMatch Match(FeedContext context) {
            var containerIdValue = context.ValueProvider.GetValue("term");
            if (containerIdValue == null)
                return null;

            return new FeedQueryMatch { FeedQuery = this, Priority = -5 };
        }

        public void Execute(FeedContext context) {
            var termParthId = context.ValueProvider.GetValue("term");
            if (termParthId == null)
                return;

            var limitValue = context.ValueProvider.GetValue("limit");
            var limit = 20;
            if (limitValue != null)
                limit = (int)limitValue.ConvertTo(typeof(int));

            var containerId = (int)termParthId.ConvertTo(typeof(int));
            var container = _contentManager.Get<TermPart>(containerId);

            if(container == null){
                return;
            }

            var inspector = new ItemInspector(container, _contentManager.GetItemMetadata(container));
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

            var items = _taxonomyService.GetContentItems(container, 0, limit);

            foreach (var item in items) {
                // call item.ContentItem to force a cast to ContentItem, and 
                // thus use CorePartsFeedItemBuilder
                context.Builder.AddItem(context, item.ContentItem);
            }
        }
    }
}