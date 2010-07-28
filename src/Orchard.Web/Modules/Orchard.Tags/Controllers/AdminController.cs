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

namespace Orchard.Tags.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly ITagService _tagService;

        public AdminController(IOrchardServices services, ITagService tagService) {
            Services = services;
            _tagService = tagService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        
        public Localizer T { get; set; }

        public ActionResult Index() {
            IEnumerable<Tag> tags = _tagService.GetTags();
            var entries = tags.Select(CreateTagEntry).ToList();
            var model = new TagsAdminIndexViewModel { Tags = entries };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(FormCollection input) {
            var viewModel = new TagsAdminIndexViewModel {Tags = new List<TagEntry>(), BulkAction = new TagAdminIndexBulkAction()};
            
            if ( !TryUpdateModel(viewModel) ) {
                return View(viewModel);
            }

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

            return RedirectToAction("Index");
        }

        public ActionResult Create() {
            return View(new TagsAdminCreateViewModel());
        }

        [HttpPost]
        public ActionResult Create(FormCollection input) {
            var viewModel = new TagsAdminCreateViewModel();

            if (!TryUpdateModel(viewModel)) {
                return View(viewModel);
            }

            if (!Services.Authorizer.Authorize(Permissions.CreateTag, T("Couldn't create tag")))
                return new HttpUnauthorizedResult();
            
            _tagService.CreateTag(viewModel.TagName);
            
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id) {
            Tag tag = _tagService.GetTag(id);

            if(tag == null) {
                return RedirectToAction("Index");
            }

            var viewModel = new TagsAdminEditViewModel {
                Id = tag.Id,
                TagName = tag.TagName,
            };
            
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection input) {
            var viewModel = new TagsAdminEditViewModel();

            if ( !TryUpdateModel(viewModel) ) {
                return View(viewModel);
            }
            
            if (!Services.Authorizer.Authorize(Permissions.ManageTags, T("Couldn't edit tag")))
                return new HttpUnauthorizedResult();

            _tagService.UpdateTag(viewModel.Id, viewModel.TagName);
            return RedirectToAction("Index");
        }

        public ActionResult Search(int id) {
            Tag tag = _tagService.GetTag(id);

            if (tag == null) {
                return RedirectToAction("Index");
            }

            IEnumerable<IContent> contents = _tagService.GetTaggedContentItems(id).ToList();
            var viewModel = new TagsAdminSearchViewModel {
                TagName = tag.TagName,
                Contents = contents,
            };
            return View(viewModel);
        }

        private static TagEntry CreateTagEntry(Tag tag) {
            return new TagEntry {
                Tag = tag,
                IsChecked = false,
            };
        }
    }
}
