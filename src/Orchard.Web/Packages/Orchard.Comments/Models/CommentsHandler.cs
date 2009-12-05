using System;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;
using Orchard.Comments.Services;

namespace Orchard.Comments.Models {
    public class HasCommentsProvider : ContentProvider {
        private readonly IRepository<Comment> _commentsRepository;
        private readonly IRepository<ClosedComments> _closedCommentsRepository;
        private readonly ICommentService _commentService;

        public HasCommentsProvider(IRepository<Comment> commentsRepository, IRepository<ClosedComments> closedCommentsRepository, ICommentService commentService) {
            _commentsRepository = commentsRepository;
            _closedCommentsRepository = closedCommentsRepository;
            _commentService = commentService;
            Filters.Add(new ActivatingFilter<HasComments>("sandboxpage"));
            Filters.Add(new ActivatingFilter<HasComments>("blogpost"));
        }

        protected override void GetDisplayViewModel(GetDisplayViewModelContext context) {
            if (context.ContentItem.Has<HasComments>() == false) {
                return;
            }
            context.AddDisplay(new TemplateViewModel(context.ContentItem.Get<HasComments>()) { Position = "999" });
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
            CommentsViewModel viewModel = new CommentsViewModel();
            context.Updater.TryUpdateModel(viewModel, String.Empty, null, null);
            bool closed = viewModel.Closed == null ? false : true;
            bool currentStatus = _commentService.CommentsClosedForCommentedContent(context.ContentItem.Id);
            if (currentStatus != closed) {
                if (closed) {
                    _commentService.CloseCommentsForCommentedContent(context.ContentItem.Id);
                }
                else {
                    _commentService.EnableCommentsForCommentedContent(context.ContentItem.Id);
                }
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

        public class CommentsViewModel {
            public String Closed { get; set; }
        }
    }
}
