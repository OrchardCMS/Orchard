using System;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;

namespace Orchard.Comments.Feeds {
    [UsedImplicitly]
    public class CommentedOnContainerFeedQuery : IFeedQueryProvider, IFeedQuery {
        private readonly IContentManager _contentManager;

        public CommentedOnContainerFeedQuery(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public FeedQueryMatch Match(FeedContext context) {
            if (context.ValueProvider.GetValue("commentedoncontainer") != null) {
                return new FeedQueryMatch { Priority = -1, FeedQuery = this };
            }
            return null;
        }

        public void Execute(FeedContext context) {
            var commentedOnContainer = (int)context.ValueProvider.GetValue("commentedoncontainer").ConvertTo(typeof(int));

            var limit = 20;
            var limitValue = context.ValueProvider.GetValue("limit");
            if (limitValue != null) {
                Int32.TryParse(Convert.ToString(limitValue), out limit);
            }

            limit = Math.Min(limit, 100);

            var comments = _contentManager
                .Query<CommentPart, CommentPartRecord>()
                .Where(x => x.CommentedOnContainer == commentedOnContainer && x.Status == CommentStatus.Approved)
                .OrderByDescending(x => x.CommentDateUtc)
                .Slice(0, limit);

            foreach (var comment in comments) {
                context.Builder.AddItem(context, comment);
            }
        }
    }
}
