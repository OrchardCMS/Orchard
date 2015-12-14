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
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Comments.ViewModels;
using Orchard.Comments.Services;

namespace Orchard.Comments.Controllers {
    using Orchard.Settings;

    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly ICommentService _commentService;
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;

        public AdminController(
            IOrchardServices orchardServices,
            ICommentService commentService,
            ISiteService siteService,
            IShapeFactory shapeFactory) {
            _orchardServices = orchardServices;
            _commentService = commentService;
            _siteService = siteService;
            _contentManager = _orchardServices.ContentManager;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(commentsQuery.Count());
            var entries = commentsQuery
                .OrderByDescending<CommentPartRecord>(cpr => cpr.CommentDateUtc)
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList()
                .Select(CreateCommentEntry);

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
                case CommentIndexBulkAction.Unapprove:
                    if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                        return new HttpUnauthorizedResult();
                    //TODO: Transaction
                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.UnapproveComment(entry.Comment.Id);
                    }
                    break;
                case CommentIndexBulkAction.Approve:
                    if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                        return new HttpUnauthorizedResult();
                    //TODO: Transaction
                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.ApproveComment(entry.Comment.Id);
                    }
                    break;
                case CommentIndexBulkAction.Delete:
                    if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't delete comment")))
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var entries = comments.List().Select(comment => CreateCommentEntry(comment)).ToList();
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
                case CommentDetailsBulkAction.Unapprove:
                    if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                        return new HttpUnauthorizedResult();

                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.UnapproveComment(entry.Comment.Id);
                    }
                    break;
                case CommentDetailsBulkAction.Approve:
                    if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment")))
                        return new HttpUnauthorizedResult();

                    foreach (CommentEntry entry in checkedEntries) {
                        _commentService.ApproveComment(entry.Comment.Id);
                    }
                    break;
                case CommentDetailsBulkAction.Delete:
                    if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't delete comment")))
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
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't disable comments")))
                return new HttpUnauthorizedResult();

            _commentService.DisableCommentsForCommentedContent(commentedItemId);
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        [HttpPost]
        public ActionResult Enable(int commentedItemId, string returnUrl) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't enable comments")))
                return new HttpUnauthorizedResult();

            _commentService.EnableCommentsForCommentedContent(commentedItemId);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        public ActionResult Edit(int id) {
            var commentPart = _contentManager.Get<CommentPart>(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            dynamic editorShape = _contentManager.BuildEditor(commentPart);
            return View(editorShape);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection input) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't edit comment")))
                return new HttpUnauthorizedResult();

            var commentPart = _contentManager.Get<CommentPart>(id);

            var editorShape = _contentManager.UpdateEditor(commentPart, this);

            if (!ModelState.IsValid) {
                foreach (var error in ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage)) {
                    _orchardServices.Notifier.Error(T(error));
                }

                TempData["Comments.InvalidCommentEditorShape"] = editorShape;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Approve(int id, string returnUrl) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't approve comment")))
                return new HttpUnauthorizedResult();

            var commentPart = _contentManager.Get<CommentPart>(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            int commentedOn = commentPart.Record.CommentedOn;
            _commentService.ApproveComment(id);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
        }

        [HttpPost]
        public ActionResult Unapprove(int id, string returnUrl) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't unapprove comment")))
                return new HttpUnauthorizedResult();

            var commentPart = _contentManager.Get<CommentPart>(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            int commentedOn = commentPart.Record.CommentedOn;
            _commentService.UnapproveComment(id);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
        }

        [HttpPost]
        public ActionResult Delete(int id, string returnUrl) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't delete comment")))
                return new HttpUnauthorizedResult();

            var commentPart = _contentManager.Get<CommentPart>(id);
            if (commentPart == null)
                return new HttpNotFoundResult();

            int commentedOn = commentPart.Record.CommentedOn;
            _commentService.DeleteComment(id);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Details", new { id = commentedOn }));
        }

        private CommentEntry CreateCommentEntry(CommentPart item) {
            return new CommentEntry {
                Comment = item.Record,
                CommentedOn = _commentService.GetCommentedContent(item.CommentedOn),
                IsChecked = false,
            };
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
