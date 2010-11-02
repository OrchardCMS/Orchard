using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Security;
using Orchard.Comments.ViewModels;
using Orchard.Comments.Services;

namespace Orchard.Comments.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly ICommentService _commentService;

        public AdminController(IOrchardServices services, ICommentService commentService, IShapeFactory shapeFactory) {
            _commentService = commentService;
            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        protected virtual IUser CurrentUser { get; [UsedImplicitly] private set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        public ActionResult Index(CommentIndexOptions options, Pager pager) {
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

                var entries = comments.Slice(pager.GetStartIndex(), pager.PageSize).ToList().Select(comment => CreateCommentEntry(comment.Record));

                var pagerShape = Shape.Pager(pager).TotalItemCount(comments.Count());

                var model = new CommentsIndexViewModel {
                    Comments = entries.ToList(),
                    Options = options,
                    Pager = pagerShape
                };
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Listing comments failed: " + exception.Message));
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
                    case CommentIndexBulkAction.Pend:
                        if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                            return new HttpUnauthorizedResult();
                        //TODO: Transaction
                        foreach (CommentEntry entry in checkedEntries) {
                            _commentService.PendComment(entry.Comment.Id);
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
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing comments failed: " + exception.Message));
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
                    CommentsClosedOnItem = _commentService.CommentsClosedForCommentedContent(id),
                };
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Listing comments failed: " + exception.Message));
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
                    case CommentDetailsBulkAction.Pend:
                        if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                            return new HttpUnauthorizedResult();

                        foreach (CommentEntry entry in checkedEntries) {
                            _commentService.PendComment(entry.Comment.Id);
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
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing comments failed: " + exception.Message));
                return Details(viewModel.CommentedItemId, viewModel.Options);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Close(int commentedItemId, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.CloseComment, T("Couldn't close comments")))
                    return new HttpUnauthorizedResult();
                _commentService.CloseCommentsForCommentedContent(commentedItemId);
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Closing Comments failed: " + exception.Message));
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Enable(int commentedItemId, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.EnableComment, T("Couldn't enable comments")))
                    return new HttpUnauthorizedResult();
                _commentService.EnableCommentsForCommentedContent(commentedItemId);
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Enabling Comments failed: " + exception.Message));
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
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

            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing comment failed: " + exception.Message));
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
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing Comment failed: " + exception.Message));
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't delete comment")))
                    return new HttpUnauthorizedResult();

                int commentedOn = _commentService.GetComment(id).Record.CommentedOn;
                _commentService.DeleteComment(id);

                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Details", new { id = commentedOn });
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Deleting comment failed: " + exception.Message));
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
        }

        private CommentEntry CreateCommentEntry(CommentPartRecord commentPart) {
            return new CommentEntry {
                Comment = commentPart,
                CommentedOn = _commentService.GetDisplayForCommentedContent(commentPart.CommentedOn).DisplayText,
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
