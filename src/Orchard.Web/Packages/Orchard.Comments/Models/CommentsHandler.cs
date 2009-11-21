using System;
using System.Collections.Generic;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.Comments.Models {
    public class HasComments : ContentItemPart {
        public HasComments() {
            Comments = new List<Comment>();
        }

        public IEnumerable<Comment> Comments { get; set; }
    }

    public class HasCommentsHandler : ContentHandler {
        private readonly IRepository<Comment> _commentsRepository;

        public HasCommentsHandler(IRepository<Comment> commentsRepository) {
            _commentsRepository = commentsRepository;
            Filters.Add(new ActivatingFilter<HasComments>("wikipage"));
        }

        protected override void GetDisplays(GetDisplaysContext context) {
            if (context.ContentItem.Has<HasComments>() == false) {
                return;
            }
            context.Displays.Add(new ModelTemplate { Model = context.ContentItem.Get<HasComments>(), Prefix = String.Empty });  
        }

        protected override void Loading(LoadContentContext context) {
            if (context.ContentItem.Has<HasComments>() == false) {
                return;
            }

            HasComments comments = context.ContentItem.Get<HasComments>();
            comments.Comments = _commentsRepository.Fetch(x => x.CommentedOn == context.ContentItem.Id);
        }
    }
}
