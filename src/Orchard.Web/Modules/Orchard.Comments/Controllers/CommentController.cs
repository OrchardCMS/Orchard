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
        public IOrchardServices Services { get; set; }
        private readonly IAuthorizer _authorizer;
        private readonly ICommentService _commentService;
        private readonly INotifier _notifier;

        public CommentController(IOrchardServices services, ICommentService commentService, INotifier notifier, IAuthorizer authorizer) {
            Services = services;
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
            if (!Services.Authorizer.Authorize(Permissions.AddComment, T("Couldn't add comment")))
                return !String.IsNullOrEmpty(returnUrl)
                    ? Redirect(returnUrl)
                    : Redirect("~/");
            
            var viewModel = new CommentsCreateViewModel();
            try {
                UpdateModel(viewModel);

                var context = new CreateCommentContext {
                                                           Author = viewModel.Name,
                                                           CommentText = viewModel.CommentText,
                                                           Email = viewModel.Email,
                                                           SiteName = viewModel.SiteName,
                                                           CommentedOn = viewModel.CommentedOn
                                                       };

                Comment comment = _commentService.CreateComment(context, CurrentSite.As<CommentSettings>().Record.ModerateComments);

                if (comment.Record.Status == CommentStatus.Pending)
                    Services.Notifier.Information(T("Your comment will appear after the site administrator approves it."));

                return !String.IsNullOrEmpty(returnUrl)
                    ? Redirect(returnUrl)
                    : Redirect("~/");
            }
            catch (Exception exception) {
                _notifier.Error(T("Creating Comment failed: " + exception.Message));
                return View(viewModel);
            }
        }
    }
}