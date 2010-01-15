using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Records;
using Orchard.Core.Feeds.Models;

namespace Orchard.Core.Feeds.StandardQueries {
    [UsedImplicitly]
    public class ContainerFeedQuery : IFeedQueryProvider, IFeedQuery {
        private readonly IContentManager _contentManager;

        public ContainerFeedQuery(IContentManager contentManager) {
            _contentManager = contentManager;
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

            var containerRoutable = container.As<RoutableAspect>();
            var containerBody = container.As<BodyAspect>();
            if (containerRoutable != null) {
                context.FeedFormatter.AddProperty(context, null, "title", containerRoutable.Title);
                context.FeedFormatter.AddProperty(context, null, "link", "/" + containerRoutable.Slug);
            }
            if (containerBody != null) {
                context.FeedFormatter.AddProperty(context, null, "description", containerBody.Text);
            }
            else if (containerRoutable != null) {
                context.FeedFormatter.AddProperty(context, null, "description", containerRoutable.Title);
            }

            var items = _contentManager.Query()
                .Where<CommonRecord>(x => x.Container == container.Record)
                .OrderByDescending(x => x.PublishedUtc)
                .Slice(0, limit);

            foreach (var item in items) {
                context.FeedFormatter.AddItem(context, item);
            }
        }
    }
}