using System.Linq;
using System.Web.Mvc;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Licensing;
using IDeliverable.Slides.Models;
using IDeliverable.Slides.Services;
using IDeliverable.Slides.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace IDeliverable.Slides.Controllers
{
    [Admin]
    public class SlideshowProfileController : Controller, IUpdateModel
    {
        public SlideshowProfileController(IOrchardServices services, ISlideshowPlayerEngineManager engineManager)
        {
            _services = services;
            _engineManager = engineManager;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;

        private readonly IOrchardServices _services;
        private readonly ISlideshowPlayerEngineManager _engineManager;


        public ActionResult Index()
        {
            if (!LicenseValidationHelper.GetLicenseIsValid(LicensedProductManifest.ProductId))
                return View("InvalidLicense");

            var viewModel = new SlideshowProfileIndexViewModel
            {
                Profiles = _services.WorkContext.CurrentSite.As<SlideshowSettingsPart>().Profiles.ToList()
            };
            return View(viewModel);
        }

        public ActionResult Create()
        {
            var engines = _engineManager.GetEngines().ToList();
            var viewModel = new SlideshowProfileViewModel
            {
                AvailableEngines = engines,
                EngineSettingsEditors = engines.ToDictionary(x => x.Name, x => x.BuildEditor(_services.New))
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Create(SlideshowProfileViewModel viewModel)
        {
            var engines = _engineManager.GetEngines().ToList();

            viewModel.AvailableEngines = engines;
            viewModel.EngineSettingsEditors = engines.ToDictionary(x => x.Name, x => x.UpdateEditor(_services.New, this));

            if (!ModelState.IsValid)
            {
                _services.TransactionManager.Cancel();
                return View(viewModel);
            }

            var settingsPart = _services.WorkContext.CurrentSite.As<SlideshowSettingsPart>();
            var profile = new SlideshowProfile
            {
                Id = settingsPart.NextId(),
                Name = viewModel.Name.TrimSafe(),
                SelectedEngine = viewModel.SelectedEngine,
                EngineStates = new ElementDataDictionary()
            };

            foreach (var engine in engines)
            {
                profile.EngineStates[engine.Name] = engine.Data.Serialize();
            }

            settingsPart.StoreProfile(profile);
            _services.Notifier.Information(T("That slide show profile has been created."));

            return RedirectToAction("Edit", new { id = profile.Id });
        }

        public ActionResult Edit(int id)
        {
            var settingsPart = _services.WorkContext.CurrentSite.As<SlideshowSettingsPart>();
            var profile = settingsPart.GetProfile(id);
            var engines = _engineManager.GetEngines().ToList();

            foreach (var engine in engines)
            {
                engine.Data = ElementDataHelper.Deserialize(profile.EngineStates[engine.Name]);
            }

            var viewModel = new SlideshowProfileViewModel
            {
                Name = profile.Name,
                AvailableEngines = engines,
                EngineSettingsEditors = engines.ToDictionary(x => x.Name, x => x.BuildEditor(_services.New)),
                SelectedEngine = profile.SelectedEngine
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(int id, SlideshowProfileViewModel viewModel)
        {
            var settingsPart = _services.WorkContext.CurrentSite.As<SlideshowSettingsPart>();
            var profile = settingsPart.GetProfile(id);
            var engines = _engineManager.GetEngines().ToList();

            viewModel.AvailableEngines = engines;
            viewModel.EngineSettingsEditors = engines.ToDictionary(x => x.Name, x => x.UpdateEditor(_services.New, this));

            if (!ModelState.IsValid)
            {
                _services.TransactionManager.Cancel();
                return View(viewModel);
            }

            foreach (var engine in engines)
            {
                profile.EngineStates[engine.Name] = engine.Data.Serialize();
            }

            profile.Name = viewModel.Name?.Trim();
            profile.SelectedEngine = viewModel.SelectedEngine;
            settingsPart.StoreProfile(profile);
            _services.Notifier.Information(T("That slide show profile has been updated."));

            return RedirectToAction("Edit", new { id = id });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var settingsPart = _services.WorkContext.CurrentSite.As<SlideshowSettingsPart>();

            settingsPart.DeleteProfile(id);
            _services.Notifier.Information(T("That slide show profile has been deleted."));

            return RedirectToAction("Index");
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner))
                filterContext.Result = new HttpUnauthorizedResult();
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}