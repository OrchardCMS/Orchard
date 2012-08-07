using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Comments.ViewModels;
using Orchard.Comments.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Comments.Controllers {
    using Orchard.Settings;

    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly ICommentService _commentService;
        private readonly ISiteService _siteService;

        public AdminController(
            IOrchardServices services, 
            ICommentService commentService, 
            ISiteService siteService,
            IShapeFactory shapeFactory) {
            _commentService = commentService;
            _siteService = siteService;
            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        public ActionResult Index(CommentIndexOptions options, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // Default options
            if (options == null)
                options = new CommentIndexOptions();

            // Filtering
            IContentQuery<CommentPart, CommentPartRecord> commentsQuery;
            switch (options.Filter) {
                case CommentIndexFilter.All:
                    commentsQuery = _commentService.GetComments();
                    break;
                case CommentIndexFilter.Approved:
                    commentsQuery = _commentService.GetComments(CommentStatus.Approved);
                    break;
                case CommentIndexFilter.Pending:
                    commentsQuery = _commentService.GetComments(CommentStatus.Pending);
                    break;
                case CommentIndexFilter.Spam:
                    commentsQuery = _commentService.GetComments(CommentStatus.Spam);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(commentsQuery.Count());
            var entries = commentsQuery
                .OrderByDescending<CommentPartRecord>(cpr => cpr.CommentDateUtc)
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList()
                .Select(comment => CreateCommentEntry(comment.Record));

            var model = new CommentsIndexViewModel {
                Comments = entries.ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input) {
            var viewModel = new CommentsIndexViewModel { Comments = new List<CommentEntry>(), Options = new CommentIndexOptions() };
            UpdateModel(viewModel);

            IEnumerable<CommentEntry> checkedEntries = viewModel.Comments.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction) {
                case CommentIndexBulkAction.None:
                    break;
                case CommentIndexBulkAction.MarkAsSpam:
                    if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                        return new HttpUnauthorizedResult();
                    //TODO: Transaction
                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.MarkCommentAsSpam(entry.Comment.Id);
                    }
                    break;
                case CommentIndexBulkAction.Unapprove:
                    if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                        return new HttpUnauthorizedResult();
                    //TODO: Transaction
                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.UnapproveComment(entry.Comment.Id);
                    }
                    break;
                case CommentIndexBulkAction.Approve:
                    if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                        return new HttpUnauthorizedResult();
                    //TODO: Transaction
                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.ApproveComment(entry.Comment.Id);
                    }
                    break;
                case CommentIndexBulkAction.Delete:
                    if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't delete comment")))
                        return new HttpUnauthorizedResult();

                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.DeleteComment(entry.Comment.Id);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Details(int id, CommentDetailsOptions options) {
            // Default options
            if (options == null)
                options = new CommentDetailsOptions();

            // Filtering
            IContentQuery<CommentPart, CommentPartRecord> comments;
            switch (options.Filter) {
                case CommentDetailsFilter.All:
                    comments = _commentService.GetCommentsForCommentedContent(id);
                    break;
                case CommentDetailsFilter.Approved:
                    comments = _commentService.GetCommentsForCommentedContent(id, CommentStatus.Approved);
                    break;
                case CommentDetailsFilter.Pending:
                    comments = _commentService.GetCommentsForCommentedContent(id, CommentStatus.Pending);
                    break;
                case CommentDetailsFilter.Spam:
                    comments = _commentService.GetCommentsForCommentedContent(id, CommentStatus.Spam);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var entries = comments.List().Select(comment => CreateCommentEntry(comment.Record)).ToList();
            var model = new CommentsDetailsViewModel {
                Comments = entries,
                Options = options,
                DisplayNameForCommentedItem = _commentService.GetDisplayForCommentedContent(id) == null ? "" : _commentService.GetDisplayForCommentedContent(id).DisplayText,
                CommentedItemId = id,
                CommentsClosedOnItem = _commentService.CommentsDisabledForCommentedContent(id),
            };
            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Details(FormCollection input) {
            var viewModel = new CommentsDetailsViewModel { Comments = new List<CommentEntry>(), Options = new CommentDetailsOptions() };
            UpdateModel(viewModel);

            IEnumerable<CommentEntry> checkedEntries = viewModel.Comments.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction) {
                case CommentDetailsBulkAction.None:
                    break;
                case CommentDetailsBulkAction.MarkAsSpam:
                    if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                        return new HttpUnauthorizedResult();
                    //TODO: Transaction
                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.MarkCommentAsSpam(entry.Comment.Id);
                    }
                    break;
                case CommentDetailsBulkAction.Unapprove:
                    if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                        return new HttpUnauthorizedResult();

                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.UnapproveComment(entry.Comment.Id);
                    }
                    break;
                case CommentDetailsBulkAction.Approve:
                    if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                        return new HttpUnauthorizedResult();

                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.ApproveComment(entry.Comment.Id);
                    }
                    break;
                case CommentDetailsBulkAction.Delete:
                    if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't delete comment")))
                        return new HttpUnauthorizedResult();

                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.DeleteComment(entry.Comment.Id);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int commentedItemId, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't disable comments")))
                return new HttpUnauthorizedResult();

            _commentService.DisableCommentsForCommentedContent(commentedItemId);
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        [HttpPost]
        public ActionResult Enable(int commentedItemId, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't enable comments")))
                return new HttpUnauthorizedResult();
                
            _commentService.EnableCommentsForCommentedContent(commentedItemId);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        public ActionResult Edit(int id) {
            CommentPart commentPart = _commentService.GetComment(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            var viewModel = new CommentsEditViewModel {
                CommentText = commentPart.Record.CommentText,
                Email = commentPart.Record.Email,
                Id = commentPart.Record.Id,
                Name = commentPart.Record.Author,
                SiteName = commentPart.Record.SiteName,
                Status = commentPart.Record.Status,
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection input) {
            var viewModel = new CommentsEditViewModel();
            UpdateModel(viewModel);
            if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't edit comment")))
                return new HttpUnauthorizedResult();

            _commentService.UpdateComment(viewModel.Id, viewModel.Name, viewModel.Email, viewModel.SiteName, viewModel.CommentText, viewModel.Status);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Approve(int id, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't approve comment")))
                return new HttpUnauthorizedResult();

            var commentPart = _commentService.GetComment(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            int commentedOn = commentPart.Record.CommentedOn;
            _commentService.ApproveComment(id);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
        }

        [HttpPost]
        public ActionResult Unapprove(int id, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't unapprove comment")))
                return new HttpUnauthorizedResult();

            var commentPart = _commentService.GetComment(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            int commentedOn = commentPart.Record.CommentedOn;
            _commentService.UnapproveComment(id);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
        }

        [HttpPost]
        public ActionResult MarkAsSpam(int id, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't mark comment as spam")))
                return new HttpUnauthorizedResult();

            var commentPart = _commentService.GetComment(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            int commentedOn = commentPart.Record.CommentedOn;
            _commentService.MarkCommentAsSpam(id);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
        }

        [HttpPost]
        public ActionResult Delete(int id, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't delete comment")))
                return new HttpUnauthorizedResult();

            var commentPart = _commentService.GetComment(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            int commentedOn = commentPart.Record.CommentedOn;
            _commentService.DeleteComment(id);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
        }

        private CommentEntry CreateCommentEntry(CommentPartRecord commentPart) {
            return new CommentEntry {
                Comment = commentPart,
                CommentedOn = _commentService.GetCommentedContent(commentPart.CommentedOn),
                IsChecked = false,
            };
        }

        public class FormValueRequiredAttribute : ActionMethodSelectorAttribute {
            private readonly string _submitButtonName;

            public FormValueRequiredAttribute(string submitButtonName) {
                _submitButtonName = submitButtonName;
            }

            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
                var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
                return !string.IsNullOrEmpty(value);
            }
        }
    }
}
