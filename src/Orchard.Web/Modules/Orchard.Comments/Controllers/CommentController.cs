using System.Linq;
using System.Web.Mvc;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.UI.Notify;

namespace Orchard.Comments.Controllers {
    public class CommentController : Controller, IUpdateModel {
        public IOrchardServices Services { get; set; }
        private readonly ICommentService _commentService;

        public Localizer T { get; set; }

        public CommentController(IOrchardServices services, ICommentService commentService) {
            Services = services;
            _commentService = commentService;

            T = NullLocalizer.Instance;
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.AddComment, T("Couldn't add comment")))
                return this.RedirectLocal(returnUrl, "~/");

            var comment = Services.ContentManager.New<CommentPart>("Comment");
            var editorShape = Services.ContentManager.UpdateEditor(comment, this);

            if (!ModelState.IsValidField("Comments.Author")) {
                Services.Notifier.Error(T("Name is mandatory and must have less than 255 chars"));
            }

            if (!ModelState.IsValidField("Comments.Email")) {
                Services.Notifier.Error(T("Email is invalid or is longer than 255 chars"));
            }

            if (!ModelState.IsValidField("Comments.Site")) {
                Services.Notifier.Error(T("Site url is invalid or is longer than 255 chars"));
            }

            if (!ModelState.IsValidField("Comments.CommentText")) {
                Services.Notifier.Error(T("Comment is mandatory"));
            }

            if (ModelState.IsValid) {
                Services.ContentManager.Create(comment);

                var commentPart = comment.As<CommentPart>();

                // ensure the comments are not closed on the container, as the html could have been tampered manually
                if (!_commentService.CanCreateComment(commentPart)) {
                    Services.TransactionManager.Cancel();
                    return this.RedirectLocal(returnUrl, "~/");
                }

                var commentsPart = Services.ContentManager.Get(commentPart.CommentedOn).As<CommentsPart>();
           
                // is it a response to another comment ?
                if(commentPart.RepliedOn.HasValue && commentsPart != null && commentsPart.ThreadedComments) {
                    var replied = Services.ContentManager.Get(commentPart.RepliedOn.Value);
                    if(replied != null) {
                        var repliedPart = replied.As<CommentPart>();
                            
                        // what is the next position after the anwered comment
                        if(repliedPart != null) {
                            // the next comment is the one right after the RepliedOn one, at the same level
                            var nextComment = _commentService.GetCommentsForCommentedContent(commentPart.CommentedOn)
                                .Where(x => x.RepliedOn == repliedPart.RepliedOn && x.CommentDateUtc > repliedPart.CommentDateUtc)
                                .OrderBy(x => x.Position)
                                .Slice(0, 1)
                                .FirstOrDefault();

                            // the previous comment is the last one under the RepliedOn
                            var previousComment = _commentService.GetCommentsForCommentedContent(commentPart.CommentedOn)
                                .Where(x => x.RepliedOn == commentPart.RepliedOn)
                                .OrderByDescending(x => x.Position)
                                .Slice(0, 1)
                                .FirstOrDefault();

                            if(nextComment == null) {
                                commentPart.Position = repliedPart.Position + 1;
                            }
                            else {
                                if (previousComment == null) {
                                    commentPart.Position = (repliedPart.Position + nextComment.Position) / 2;
                                }
                                else {
                                    commentPart.Position = (previousComment.Position + nextComment.Position) / 2;
                                }
                            }
                        }
                    }
                        
                }
                else {
                    // new comment, last in position
                    commentPart.RepliedOn = null;
                    commentPart.Position = comment.Id;
                }

                if (commentPart.Status == CommentStatus.Pending) {
                    // if the user who submitted the comment has the right to moderate, don't make this comment moderated
                    if (Services.Authorizer.Authorize(Permissions.ManageComments)) {
                        commentPart.Status = CommentStatus.Approved;
                        Services.Notifier.Information(T("Your comment has been posted."));
                    }
                    else {
                        Services.Notifier.Information(T("Your comment will appear after the site administrator approves it."));
                    }
                }
                else {
                    Services.Notifier.Information(T("Your comment has been posted."));
                }

                // send email notification
                var siteSettings = Services.WorkContext.CurrentSite.As<CommentSettingsPart>();
                if (siteSettings.NotificationEmail) {
                    _commentService.SendNotificationEmail(commentPart);
                }

            }
            else {
                Services.TransactionManager.Cancel();

                TempData["Comments.InvalidCommentEditorShape"] = editorShape;
                var commentPart = comment.As<CommentPart>(); 
                if(commentPart.RepliedOn.HasValue) {
                    TempData["Comments.RepliedOn"] = commentPart.RepliedOn.Value;
                }
            }

            return this.RedirectLocal(returnUrl, "~/");
        }

        public ActionResult Approve(string nonce) {
            int id;
            if (_commentService.DecryptNonce(nonce, out id)) {
                _commentService.ApproveComment(id);
            }

            Services.Notifier.Information(T("Comment approved successfully"));
            return Redirect("~/");
        }

        public ActionResult Delete(string nonce) {
            int id;
            if (_commentService.DecryptNonce(nonce, out id)) {
                _commentService.DeleteComment(id);
            }

            Services.Notifier.Information(T("Comment deleted successfully"));
            return Redirect("~/");
        }

        public ActionResult Moderate(string nonce) {
            int id;
            if (_commentService.DecryptNonce(nonce, out id)) {
                _commentService.UnapproveComment(id);
            }

            Services.Notifier.Information(T("Comment moderated successfully"));
            return Redirect("~/");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}