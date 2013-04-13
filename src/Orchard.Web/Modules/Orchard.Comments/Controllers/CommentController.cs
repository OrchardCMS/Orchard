using System;
using System.Web.Mvc;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.Comments.ViewModels;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.UI.Notify;

namespace Orchard.Comments.Controllers {
    public class CommentController : Controller {
        public IOrchardServices Services { get; set; }
        private readonly ICommentService _commentService;

        public CommentController(IOrchardServices services, ICommentService commentService) {
            Services = services;
            _commentService = commentService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.AddComment, T("Couldn't add comment")))
                return this.RedirectLocal(returnUrl, "~/");
            
            var viewModel = new CommentsCreateViewModel();

            TryUpdateModel(viewModel);

            if (!ModelState.IsValidField("Name")) {
                Services.Notifier.Error(T("Name is mandatory and must have less than 255 chars"));
            }

            if (!ModelState.IsValidField("Email")) {
                Services.Notifier.Error(T("Email is invalid or is longer than 255 chars"));
            }

            if (!ModelState.IsValidField("Site")) {
                Services.Notifier.Error(T("Site url is invalid or is longer than 255 chars"));
            }

            if (!ModelState.IsValidField("CommentText")) {
                Services.Notifier.Error(T("Comment is mandatory"));
            }
            
            var context = new CreateCommentContext {
                Author = viewModel.Name,
                CommentText = viewModel.CommentText,
                Email = viewModel.Email,
                SiteName = viewModel.SiteName,
                CommentedOn = viewModel.CommentedOn
            };

            if (ModelState.IsValid) {
                if (!String.IsNullOrEmpty(context.SiteName) && !context.SiteName.StartsWith("http://") && !context.SiteName.StartsWith("https://")) {
                    context.SiteName = "http://" + context.SiteName;
                }

                CommentPart commentPart = _commentService.CreateComment(context, Services.WorkContext.CurrentSite.As<CommentSettingsPart>().Record.ModerateComments);

                if (commentPart.Record.Status == CommentStatus.Pending) {
                    // if the user who submitted the comment has the right to moderate, don't make this comment moderated
                    if (Services.Authorizer.Authorize(Permissions.ManageComments)) {
                        commentPart.Record.Status = CommentStatus.Approved;
                    }
                    else {
                        Services.Notifier.Information(T("Your comment will appear after the site administrator approves it."));
                    }
                }
            }
            else {
                TempData["CreateCommentContext.Name"] = context.Author;
                TempData["CreateCommentContext.CommentText"] = context.CommentText;
                TempData["CreateCommentContext.Email"] = context.Email;
                TempData["CreateCommentContext.SiteName"] = context.SiteName;
            }

            return this.RedirectLocal(returnUrl, "~/");
        }
    }
}