using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Mvc.Extensions;
using Orchard.Services;

namespace Orchard.Blogs.Feeds {
    public class BlogPartFeedItemBuilder : IFeedItemBuilder {
        private IContentManager _contentManager;
        public BlogPartFeedItemBuilder(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void Populate(FeedContext context) {
            var containerIdValue = context.ValueProvider.GetValue("containerid");
            if (containerIdValue == null)
                return;

            var containerId = (int)containerIdValue.ConvertTo(typeof(int));
            var container = _contentManager.Get(containerId);

            if (container == null) {
                return;
            }

            if (container.ContentType != "Blog") {
                return;
            }

            var blog = container.As<BlogPart>();

            if (context.Format == "rss") {
                context.Response.Element.SetElementValue("description", blog.Description);
            }
            else {
                context.Builder.AddProperty(context, null, "description", blog.Description);
            }
        }
    }
}