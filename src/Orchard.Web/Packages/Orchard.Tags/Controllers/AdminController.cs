using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Settings;
using Orchard.Tags.Models;
using Orchard.Tags.ViewModels;
using Orchard.UI.Notify;
using Orchard.Security;
using Orchard.Tags.Services;

namespace Orchard.Tags.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly ITagService _tagService;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;

        public AdminController(ITagService tagService, INotifier notifier, IAuthorizer authorizer) {
            _tagService = tagService;
            _authorizer = authorizer;
            _notifier = notifier;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IUser CurrentUser { get; set; }
        public ISite CurrentSite { get; set; }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            try {
                IEnumerable<Tag> tags = _tagService.GetTags();
                var entries = tags.Select(tag => CreateTagEntry(tag)).ToList();
                var model = new TagsAdminIndexViewModel { Tags = entries };
                return View(model);
            }
            catch (Exception exception) {
                _notifier.Error(T("Listing tags failed: " + exception.Message));
                return Index();
            }
        }


        [HttpPost]
        public ActionResult Index(FormCollection input) {
            var viewModel = new TagsAdminIndexViewModel { Tags = new List<TagEntry>(), BulkAction = new TagAdminIndexBulkAction() };
            UpdateModel(viewModel, input.ToValueProvider());

            try {
                IEnumerable<TagEntry> checkedEntries = viewModel.Tags.Where(t => t.IsChecked);
                switch (viewModel.BulkAction) {
                    case TagAdminIndexBulkAction.None:
                        break;
                    case TagAdminIndexBulkAction.Delete:
                        if (!_authorizer.Authorize(Permissions.DeleteTag, T("Couldn't delete tag")))
                            return new HttpUnauthorizedResult();

                        foreach (TagEntry entry in checkedEntries) {
                            _tagService.DeleteTag(entry.Tag.Id);
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception exception) {
                _notifier.Error(T("Editing tags failed: " + exception.Message));
                return Index();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Create() {
            return View(new TagsAdminCreateViewModel());
        }

        [HttpPost]
        public ActionResult Create(FormCollection input) {
            var viewModel = new TagsAdminCreateViewModel();
            try {
                UpdateModel(viewModel, input.ToValueProvider());
                if (!_authorizer.Authorize(Permissions.CreateTag, T("Couldn't create tag")))
                    return new HttpUnauthorizedResult();
                _tagService.CreateTag(viewModel.TagName);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Creating tag failed: " + exception.Message));
                return View(viewModel);
            }
        }

        public ActionResult Edit(int id) {
            try {
                Tag tag = _tagService.GetTag(id);
                var viewModel = new TagsAdminEditViewModel {
                    Id = tag.Id,
                    TagName = tag.TagName,
                };
                return View(viewModel);

            }
            catch (Exception exception) {
                _notifier.Error(T("Retrieving tag information failed: " + exception.Message));
                return Index();
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(FormCollection input) {
            var viewModel = new TagsAdminEditViewModel();
            try {
                UpdateModel(viewModel, input.ToValueProvider());
                if (!_authorizer.Authorize(Permissions.RenameTag, T("Couldn't edit tag")))
                    return new HttpUnauthorizedResult();

                _tagService.UpdateTag(viewModel.Id, viewModel.TagName);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Editing tag failed: " + exception.Message));
                return View(viewModel);
            }
        }

        private static TagEntry CreateTagEntry(Tag tag) {
            return new TagEntry {
                Tag = tag,
                IsChecked = false,
            };
        }
    }
}
