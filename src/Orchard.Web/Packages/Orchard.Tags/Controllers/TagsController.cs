using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Settings;
using Orchard.Tags.Models;
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

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(FormCollection input, string returnUrl) {
            try {
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Editing tags failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }

        public ActionResult Create() {
            return View(new TagsCreateViewModel());
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(FormCollection input) {
            var viewModel = new TagsCreateViewModel();
            try {
                UpdateModel(viewModel, input.ToValueProvider());
                if (!_authorizer.Authorize(Permissions.CreateTag, T("Couldn't create tag")))
                    return new HttpUnauthorizedResult();
                Tag tag = new Tag { TagName = viewModel.TagName };
                _tagService.CreateTag(tag);
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
