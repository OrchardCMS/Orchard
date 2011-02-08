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
            IContentQuery<CommentPart, CommentPartRecord> comments;
            try {
                switch (options.Filter) {
                    case CommentIndexFilter.All:
                        comments = _commentService.GetComments();
                        break;
                    case CommentIndexFilter.Approved:
                        comments = _commentService.GetComments(CommentStatus.Approved);
                        break;
                    case CommentIndexFilter.Pending:
                        comments = _commentService.GetComments(CommentStatus.Pending);
                        break;
                    case CommentIndexFilter.Spam:
                        comments = _commentService.GetComments(CommentStatus.Spam);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var pagerShape = Shape.Pager(pager).TotalItemCount(comments.Count());
                var entries = comments
                    .OrderByDescending<CommentPartRecord, DateTime?>(cpr => cpr.CommentDateUtc)
                    .Slice(pager.GetStartIndex(), pager.PageSize)
                    .ToList()
                    .Select(comment => CreateCommentEntry(comment.Record));

                var model = new CommentsIndexViewModel {
                    Comments = entries.ToList(),
                    Options = options,
                    Pager = pagerShape
                };
                return View(model);
            } catch (Exception exception) {
                this.Error(exception, T("Listing comments failed: {0}", exception.Message), Logger, Services.Notifier);

                return View(new CommentsIndexViewModel());
            }
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input) {
            var viewModel = new CommentsIndexViewModel { Comments = new List<CommentEntry>(), Options = new CommentIndexOptions() };
            UpdateModel(viewModel);

            try {
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
            } catch (Exception exception) {
                this.Error(exception, T("Editing comments failed: {0}", exception.Message), Logger, Services.Notifier);

                return RedirectToAction("Index", "Admin", new { options = viewModel.Options });
            }

            return RedirectToAction("Index");
        }

        public ActionResult Details(int id, CommentDetailsOptions options) {
            // Default options
            if (options == null)
                options = new CommentDetailsOptions();

            // Filtering
            IContentQuery<CommentPart, CommentPartRecord> comments;
            try {
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
            } catch (Exception exception) {
                this.Error(exception, T("Listing comments failed: {0}", exception.Message), Logger, Services.Notifier);

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Details(FormCollection input) {
            var viewModel = new CommentsDetailsViewModel { Comments = new List<CommentEntry>(), Options = new CommentDetailsOptions() };
            UpdateModel(viewModel);

            try {
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
            } catch (Exception exception) {
                this.Error(exception, T("Editing comments failed: {0}", exception.Message), Logger, Services.Notifier);

                return Details(viewModel.CommentedItemId, viewModel.Options);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int commentedItemId, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't disable comments")))
                    return new HttpUnauthorizedResult();

                _commentService.DisableCommentsForCommentedContent(commentedItemId);
            } catch (Exception exception) {
                this.Error(exception, T("Disabling Comments failed: {0}", exception.Message), Logger, Services.Notifier);
            }
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        [HttpPost]
        public ActionResult Enable(int commentedItemId, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't enable comments")))
                    return new HttpUnauthorizedResult();
                
                _commentService.EnableCommentsForCommentedContent(commentedItemId);
            } catch (Exception exception) {
                this.Error(exception, T("Enabling Comments failed: {0}", exception.Message), Logger, Services.Notifier);
            }
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        public ActionResult Edit(int id) {
            try {
                CommentPart commentPart = _commentService.GetComment(id);
                var viewModel = new CommentsEditViewModel {
                    CommentText = commentPart.Record.CommentText,
                    Email = commentPart.Record.Email,
                    Id = commentPart.Record.Id,
                    Name = commentPart.Record.Author,
                    SiteName = commentPart.Record.SiteName,
                    Status = commentPart.Record.Status,
                };
                return View(viewModel);

            } catch (Exception exception) {
                this.Error(exception, T("Editing comment failed: {0}", exception.Message), Logger, Services.Notifier);

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Edit(FormCollection input) {
            var viewModel = new CommentsEditViewModel();
            try {
                UpdateModel(viewModel);
                if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't edit comment")))
                    return new HttpUnauthorizedResult();

                _commentService.UpdateComment(viewModel.Id, viewModel.Name, viewModel.Email, viewModel.SiteName, viewModel.CommentText, viewModel.Status);
                return RedirectToAction("Index");
            } catch (Exception exception) {
                this.Error(exception, T("Editing comment failed: {0}", exception.Message), Logger, Services.Notifier);

                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Approve(int id, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't approve comment")))
                    return new HttpUnauthorizedResult();

                int commentedOn = _commentService.GetComment(id).Record.CommentedOn;
                _commentService.ApproveComment(id);

                return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
            } catch (Exception exception) {
                this.Error(exception, T("Approving comment failed: {0}", exception.Message), Logger, Services.Notifier);

                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
        }

        [HttpPost]
        public ActionResult Unapprove(int id, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't unapprove comment")))
                    return new HttpUnauthorizedResult();

                int commentedOn = _commentService.GetComment(id).Record.CommentedOn;
                _commentService.UnapproveComment(id);

                return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
            } catch (Exception exception) {
                this.Error(exception, T("Unapproving comment failed: {0}", exception.Message), Logger, Services.Notifier);

                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
        }

        [HttpPost]
        public ActionResult MarkAsSpam(int id, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't mark comment as spam")))
                    return new HttpUnauthorizedResult();

                int commentedOn = _commentService.GetComment(id).Record.CommentedOn;
                _commentService.MarkCommentAsSpam(id);

                return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
            } catch (Exception exception) {
                this.Error(exception, T("Marking comment as spam failed: {0}", exception.Message), Logger, Services.Notifier);

                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
        }

        [HttpPost]
        public ActionResult Delete(int id, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't delete comment")))
                    return new HttpUnauthorizedResult();

                int commentedOn = _commentService.GetComment(id).Record.CommentedOn;
                _commentService.DeleteComment(id);

                return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
            } catch (Exception exception) {
                this.Error(exception, T("Deleting comment failed: {0}", exception.Message), Logger, Services.Notifier);

                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
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
