using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Mvc.Extensions;
using Orchard.Services;
using Orchard.Utility.Extensions;
using Orchard.Core.Feeds;
using Orchard.Tags.Services;
using Orchard.Localization;
using System.Web.Routing;
using Orchard.Environment.Extensions;

namespace Orchard.Tags.Feeds {
    [OrchardFeature("Orchard.Tags.Feeds")]
    public class TagFeedQuery : IFeedQueryProvider, IFeedQuery {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private readonly ITagService _tagService;

        public TagFeedQuery(
            IContentManager contentManager, 
            IEnumerable<IHtmlFilter> htmlFilters,
            ITagService tagService) {
            _contentManager = contentManager;
            _tagService = tagService;
            _htmlFilters = htmlFilters;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public FeedQueryMatch Match(FeedContext context) {
            var tagIdValue = context.ValueProvider.GetValue("tag");
            if (tagIdValue == null)
                return null;

            var tagName = (string)tagIdValue.ConvertTo(typeof(string));
            var tag = _tagService.GetTagByName(tagName);
            
            if (tag == null) {
                return null;
            }
            
            return new FeedQueryMatch { FeedQuery = this, Priority = -5 };
        }

        public void Execute(FeedContext context) {
            var tagIdValue = context.ValueProvider.GetValue("tag");
            if (tagIdValue == null)
                return;

            var limitValue = context.ValueProvider.GetValue("limit");
            var limit = 20;
            if (limitValue != null) { 
                Int32.TryParse(Convert.ToString(limitValue), out limit);
            }
            
            limit = Math.Min(limit, 100);

            var tagName = (string)tagIdValue.ConvertTo(typeof(string));
            var tag = _tagService.GetTagByName(tagName);

            if (tag == null) {
                return;
            }

            var displayRouteValues = new RouteValueDictionary {
                {"area", "Orchard.Tags"},
                {"controller", "Home"},
                {"action", "Search"},
                {"tagName", tag.TagName}
            };

            if (context.Format == "rss") {
                var link = new XElement("link");
                context.Response.Element.SetElementValue("title", tag.TagName);
                context.Response.Element.Add(link);
                context.Response.Element.SetElementValue("description", T("Content tagged with {0}", tag.TagName).ToString());

                context.Response.Contextualize(requestContext => {
                    var urlHelper = new UrlHelper(requestContext);
                    var uriBuilder = new UriBuilder(urlHelper.MakeAbsolute("/")) { Path = urlHelper.RouteUrl(displayRouteValues) };
                    link.Add(uriBuilder.Uri.OriginalString);
                });
            }
            else {
                context.Builder.AddProperty(context, null, "title", tag.TagName);
                context.Builder.AddProperty(context, null, "description", T("Content tagged with {0}", tag.TagName).ToString());
                context.Response.Contextualize(requestContext => {
                    var urlHelper = new UrlHelper(requestContext);
                    context.Builder.AddProperty(context, null, "link", urlHelper.MakeAbsolute(urlHelper.RouteUrl(displayRouteValues)));
                });
            }

            var items = _tagService.GetTaggedContentItems(tag.Id, 0, limit);

            foreach (var item in items) {
                context.Builder.AddItem(context, item.ContentItem);
            }
        }
    }
}