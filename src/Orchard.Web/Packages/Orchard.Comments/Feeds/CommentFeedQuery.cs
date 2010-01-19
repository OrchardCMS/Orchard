using System.Linq;
using Orchard.Comments.Models;
using Orchard.Core.Common.Records;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Data;

namespace Orchard.Comments.Feeds {
    public class CommentScopeFeedQuery : IFeedQueryProvider, IFeedQuery {
        private readonly IRepository<CommonRecord> _commonRepository;
        private readonly IRepository<Comment> _commentRepository;

        public CommentScopeFeedQuery(
            IRepository<CommonRecord> commonRepository,
            IRepository<Comment> commentRepository) {
            _commonRepository = commonRepository;
            _commentRepository = commentRepository;
        }

        public FeedQueryMatch Match(FeedContext context) {
            if (context.ValueProvider.ContainsPrefix("commentscopeid")) {
                return new FeedQueryMatch { Priority = -1, FeedQuery = this };
            }
            return null;
        }

        public void Execute(FeedContext context) {
            var scopeContainerId = (int)context.ValueProvider.GetValue("commentscopeid").ConvertTo(typeof (int));
            _commonRepository.Fetch(x => x.Container.Id == scopeContainerId).Select(x => x.Id);
            var comments = _commentRepository.Fetch(x=>x.)
            context.FeedFormatter.AddItem(context, new Comment());
        }
    }
    
}
