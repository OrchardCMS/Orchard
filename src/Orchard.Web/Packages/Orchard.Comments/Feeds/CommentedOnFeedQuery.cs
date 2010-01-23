using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Data;

namespace Orchard.Comments.Feeds {
    [UsedImplicitly]
    public class CommentedOnFeedQuery : IFeedQueryProvider, IFeedQuery {
        private readonly IRepository<CommentRecord> _commentRepository;

        public CommentedOnFeedQuery(
            IRepository<CommentRecord> commentRepository) {
            _commentRepository = commentRepository;
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

            var comments = _commentRepository.Fetch(
                x => x.CommentedOn == commentedOn && x.Status == CommentStatus.Approved,
                o => o.Desc(x => x.CommentDateUtc),
                0, limit);

            foreach (var comment in comments) {
                context.Builder.AddItem(context, comment);
            }
        }
    }
}