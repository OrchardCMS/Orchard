using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.Tags.Services;
using Orchard.Tags.ViewModels;
using Orchard.UI.Notify;
using Orchard.Security;

namespace Orchard.Tags.Controllers {
    [ValidateInput(false)]
    public class TagsController : Controller {
        private readonly ITagService _tagService;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;

        public TagsController(ITagService tagService, INotifier notifier, IAuthorizer authorizer) {
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
                var tags = _tagService.GetTags();
                var model = new TagsIndexViewModel { Tags = tags.ToList() };
                return View(model);
            }
            catch (Exception exception) {
                _notifier.Error(T("Listing tags failed: " + exception.Message));
                return Index();
            }
        }

        [HttpPost]
        public ActionResult Edit(FormCollection input, int taggedContentId, string returnUrl, string newTagName) {
            try {
                if (!String.IsNullOrEmpty(HttpContext.Request.Form["submit.Save"])) {
                    if (!_authorizer.Authorize(Permissions.ApplyTag, T("Couldn't apply tag")))
                        return new HttpUnauthorizedResult();
                    List<int> tagsForContentItem = new List<int>();
                    foreach (string key in input.Keys) {
                        if (key.StartsWith("Checkbox.") && input[key] == "true") {
                            int tagId = Convert.ToInt32(key.Substring("Checkbox.".Length));
                            tagsForContentItem.Add(tagId);
                        }
                    }
                    _tagService.UpdateTagsForContentItem(taggedContentId, tagsForContentItem);
                }
                else {
                    if (!_authorizer.Authorize(Permissions.CreateTag, T("Couldn't create tag")))
                        return new HttpUnauthorizedResult();
                    _tagService.CreateTag(newTagName);
                    _tagService.TagContentItem(taggedContentId, newTagName);
                }
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Editing tags failed: " + exception.Message));
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
        }

        public ActionResult Create() {
            return View(new TagsCreateViewModel());
        }

        [HttpPost]
        public ActionResult Create(FormCollection input) {
            var viewModel = new TagsCreateViewModel();
            try {
                UpdateModel(viewModel, input.ToValueProvider());
                if (!_authorizer.Authorize(Permissions.CreateTag, T("Couldn't create tag")))
                    return new HttpUnauthorizedResult();
                _tagService.CreateTag(viewModel.TagName);
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Creating Tag failed: " + exception.Message));
                return View(viewModel);
            }
        }

        public ActionResult TagName(int tagId) {
            return RedirectToAction("Index");
        }
    }
}
