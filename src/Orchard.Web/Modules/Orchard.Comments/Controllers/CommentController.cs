using System;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.Comments.ViewModels;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Orchard.Comments.Controllers {
    public class CommentController : Controller {
        private readonly IAuthorizer _authorizer;
        private readonly ICommentService _commentService;
        private readonly INotifier _notifier;

        public CommentController(ICommentService commentService, INotifier notifier, IAuthorizer authorizer) {
            _commentService = commentService;
            _notifier = notifier;
            _authorizer = authorizer;
            T = NullLocalizer.Instance;
        }

        protected virtual IUser CurrentUser { get; [UsedImplicitly]
        private set; }

        protected virtual ISite CurrentSite { get; [UsedImplicitly]
        private set; }

        public Localizer T { get; set; }

        [HttpPost]
        public ActionResult Create(string returnUrl) {
            var viewModel = new CommentsCreateViewModel();
            try {
                UpdateModel(viewModel);
                if (CurrentSite.As<CommentSettings>().Record.RequireLoginToAddComment) {
                    if (!_authorizer.Authorize(Permissions.AddComment, T("Couldn't add comment"))) {
                        return new HttpUnauthorizedResult();
                    }
                }

                var context = new CreateCommentContext {
                                                           Author = viewModel.Name,
                                                           CommentText = viewModel.CommentText,
                                                           Email = viewModel.Email,
                                                           SiteName = viewModel.SiteName,
                                                           CommentedOn = viewModel.CommentedOn
                                                       };

                Comment comment = _commentService.CreateComment(context, CurrentSite.As<CommentSettings>().Record.ModerateComments);

                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Creating Comment failed: " + exception.Message));
                return View(viewModel);
            }
        }
    }
}