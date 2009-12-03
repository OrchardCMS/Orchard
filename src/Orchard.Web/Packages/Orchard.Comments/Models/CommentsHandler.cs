using System.Collections.Generic;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.Comments.Models {
    public class HasCommentsProvider : ContentProvider {
        private readonly IRepository<Comment> _commentsRepository;
        private readonly IRepository<ClosedComments> _closedCommentsRepository;

        public HasCommentsProvider(IRepository<Comment> commentsRepository, IRepository<ClosedComments> closedCommentsRepository) {
            _commentsRepository = commentsRepository;
            _closedCommentsRepository = closedCommentsRepository;
            Filters.Add(new ActivatingFilter<HasComments>("sandboxpage"));
            Filters.Add(new ActivatingFilter<HasComments>("blogpost"));
        }

        protected override void GetDisplayViewModel(GetDisplayViewModelContext context) {
            if (context.ContentItem.Has<HasComments>() == false) {
                return;
            }
            context.AddDisplay(new TemplateViewModel(context.ContentItem.Get<HasComments>()));
        }

        protected override void GetEditorViewModel(GetEditorViewModelContext context) {
            if (context.ContentItem.Has<HasComments>() == false) {
                return;
            }
            context.AddEditor(new TemplateViewModel(context.ContentItem.Get<HasComments>()));
        }

        protected override void UpdateEditorViewModel(UpdateEditorViewModelContext context) {
            if (context.ContentItem.Has<HasComments>() == false) {
                return;
            }
            context.AddEditor(new TemplateViewModel(context.ContentItem.Get<HasComments>()));
        }

        protected override void Loading(LoadContentContext context) {
            if (context.ContentItem.Has<HasComments>() == false) {
                return;
            }

            HasComments comments = context.ContentItem.Get<HasComments>();
            comments.Comments = _commentsRepository.Fetch(x => x.CommentedOn == context.ContentItem.Id && x.Status == CommentStatus.Approved);
            if (_closedCommentsRepository.Get(x => x.ContentItemId == context.ContentItem.Id) != null) {
                comments.Closed = true;
            }
        }
    }
}
