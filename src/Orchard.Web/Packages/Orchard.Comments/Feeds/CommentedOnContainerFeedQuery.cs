using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Data;

namespace Orchard.Comments.Feeds {
    [UsedImplicitly]
    public class CommentedOnContainerFeedQuery : IFeedQueryProvider, IFeedQuery {
        private readonly IRepository<Comment> _commentRepository;

        public CommentedOnContainerFeedQuery(
            IRepository<Comment> commentRepository) {
            _commentRepository = commentRepository;
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
            if (limitValue != null)
                limit = (int)limitValue.ConvertTo(typeof(int));

            var comments = _commentRepository.Fetch(
                x => x.CommentedOnContainer == commentedOnContainer && x.Status == CommentStatus.Approved,
                o => o.Desc(x => x.CommentDate),
                0, limit);

            foreach (var comment in comments) {
                context.Builder.AddItem(context, comment);
            }
        }
    }
}
