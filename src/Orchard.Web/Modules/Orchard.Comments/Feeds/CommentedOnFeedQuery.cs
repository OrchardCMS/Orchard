using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;

namespace Orchard.Comments.Feeds {
    [UsedImplicitly]
    public class CommentedOnFeedQuery : IFeedQueryProvider, IFeedQuery {
        private readonly IContentManager _contentManager;

        public CommentedOnFeedQuery(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public FeedQueryMatch Match(FeedContext context) {
            if (context.ValueProvider.GetValue("commentedon") != null) {
                return new FeedQueryMatch { Priority = -1, FeedQuery = this };
            }
            return null;
        }

        public void Execute(FeedContext context) {
            var commentedOn = (int)context.ValueProvider.GetValue("commentedon").ConvertTo(typeof(int));

            var limit = 20;
            var limitValue = context.ValueProvider.GetValue("limit");
            if (limitValue != null)
                limit = (int)limitValue.ConvertTo(typeof(int));

            var comments = _contentManager
                .Query<CommentPart, CommentPartRecord>()
                .Where(x => x.CommentedOn == commentedOn && x.Status == CommentStatus.Approved)
                .OrderByDescending(x => x.CommentDateUtc)
                .Slice(0, limit);

            foreach (var comment in comments) {
                context.Builder.AddItem(context, comment);
            }
        }
    }
}