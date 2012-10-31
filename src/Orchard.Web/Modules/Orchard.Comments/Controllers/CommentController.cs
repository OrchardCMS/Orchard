using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.Comments.ViewModels;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.UI.Notify;

namespace Orchard.Comments.Controllers {
    public class CommentController : Controller, IUpdateModel {
        public IOrchardServices Services { get; set; }
        private readonly ICommentService _commentService;
        private readonly INotifier _notifier;

        public Localizer T { get; set; }

        public CommentController(IOrchardServices services, ICommentService commentService, INotifier notifier) {
            Services = services;
            _commentService = commentService;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.AddComment, T("Couldn't add comment")))
                return this.RedirectLocal(returnUrl, "~/");

            var comment = Services.ContentManager.New("Comment");

            Services.ContentManager.Create(comment);
            var editorShape = Services.ContentManager.UpdateEditor(comment, this);

            if (ModelState.IsValid) {
                if (comment.Has<CommentPart>()) {
                    var commentPart = comment.As<CommentPart>();

                    if (commentPart.Status == CommentStatus.Pending) {
                        // if the user who submitted the comment has the right to moderate, don't make this comment moderated
                        if (Services.Authorizer.Authorize(Permissions.ManageComments)) {
                            commentPart.Status = CommentStatus.Approved;
                        }
                        else {
                            Services.Notifier.Information(T("Your comment will appear after the site administrator approves it."));
                        }
                    }
                }
            }
            else {
                Services.TransactionManager.Cancel();

                foreach (var error in ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage)) {
                    _notifier.Error(T(error));
                }

                TempData["Comments.InvalidCommentEditorShape"] = editorShape;
            }

            return this.RedirectLocal(returnUrl, "~/");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}