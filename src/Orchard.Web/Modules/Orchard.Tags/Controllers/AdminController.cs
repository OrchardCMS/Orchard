using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Settings;
using Orchard.Tags.Models;
using Orchard.Tags.ViewModels;
using Orchard.Tags.Services;
using Orchard.UI.Notify;

namespace Orchard.Tags.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly ITagService _tagService;

        public AdminController(IOrchardServices services, ITagService tagService)
        {
            Services = services;
            _tagService = tagService;
            T = NullLocalizer.Instance;
        }


        public IOrchardServices Services { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        
        public Localizer T { get; set; }

        public ActionResult Index() {
            try {
                IEnumerable<Tag> tags = _tagService.GetTags();
                var entries = tags.Select(tag => CreateTagEntry(tag)).ToList();
                var model = new TagsAdminIndexViewModel { Tags = entries };
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Listing tags failed: " + exception.Message));
                return Index();
            }
        }


        [HttpPost]
        public ActionResult Index(FormCollection input) {
            var viewModel = new TagsAdminIndexViewModel { Tags = new List<TagEntry>(), BulkAction = new TagAdminIndexBulkAction() };
            UpdateModel(viewModel);

            try {
                IEnumerable<TagEntry> checkedEntries = viewModel.Tags.Where(t => t.IsChecked);
                switch (viewModel.BulkAction) {
                    case TagAdminIndexBulkAction.None:
                        break;
                    case TagAdminIndexBulkAction.Delete:
                        if (!Services.Authorizer.Authorize(Permissions.ManageTags, T("Couldn't delete tag")))
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
                Services.Notifier.Error(T("Editing tags failed: " + exception.Message));
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
                UpdateModel(viewModel);
                if (!Services.Authorizer.Authorize(Permissions.CreateTag, T("Couldn't create tag")))
                    return new HttpUnauthorizedResult();
                _tagService.CreateTag(viewModel.TagName);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Creating tag failed: " + exception.Message));
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
                Services.Notifier.Error(T("Retrieving tag information failed: " + exception.Message));
                return Index();
            }
        }

        [HttpPost]
        public ActionResult Edit(FormCollection input) {
            var viewModel = new TagsAdminEditViewModel();
            try {
                UpdateModel(viewModel);
                if (!Services.Authorizer.Authorize(Permissions.ManageTags, T("Couldn't edit tag")))
                    return new HttpUnauthorizedResult();

                _tagService.UpdateTag(viewModel.Id, viewModel.TagName);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing tag failed: " + exception.Message));
                return View(viewModel);
            }
        }

        public ActionResult Search(int id) {
            try {
                Tag tag = _tagService.GetTag(id);
                IEnumerable<IContent> contents = _tagService.GetTaggedContentItems(id).ToList();
                var viewModel = new TagsAdminSearchViewModel {
                    TagName = tag.TagName,
                    Contents = contents,
                };
                return View(viewModel);

            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Retrieving tagged items failed: " + exception.Message));
                return Index();
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
