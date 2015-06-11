using System.Linq;
using System.Web.Mvc;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Helpers;
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
    public class SlideShowProfileController : Controller, IUpdateModel
    {
        private readonly IOrchardServices _services;
        private readonly IEngineManager _engineManager;
        private readonly ILicenseValidator _licenseValidator;
        private readonly ILicenseAccessor _licenseAccessor;

        public SlideShowProfileController(IOrchardServices services, IEngineManager engineManager, ILicenseValidator licenseValidator, ILicenseAccessor licenseAccessor)
        {
            _services = services;
            _engineManager = engineManager;
            _licenseValidator = licenseValidator;
            _licenseAccessor = licenseAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index()
        {
            if (!_licenseValidator.ValidateLicense(_licenseAccessor.GetSlidesLicense()).IsValid)
                return View("InvalidLicense");

            var viewModel = new SlideShowProfileIndexViewModel
            {
                Profiles = _services.WorkContext.CurrentSite.As<SlideShowSettingsPart>().Profiles.ToList()
            };
            return View(viewModel);
        }

        public ActionResult Create()
        {
            var engines = _engineManager.GetEngines().ToList();
            var viewModel = new SlideShowProfileViewModel
            {
                AvailableEngines = engines,
                EngineSettingsEditors = engines.ToDictionary(x => x.Name, x => x.BuildSettingsEditor(_services.New))
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Create(SlideShowProfileViewModel viewModel)
        {
            var engines = _engineManager.GetEngines().ToList();

            viewModel.AvailableEngines = engines;
            viewModel.EngineSettingsEditors = engines.ToDictionary(x => x.Name, x => x.UpdateSettingsEditor(_services.New, this));

            if (!ModelState.IsValid)
            {
                _services.TransactionManager.Cancel();
                return View(viewModel);
            }

            var settingsPart = _services.WorkContext.CurrentSite.As<SlideShowSettingsPart>();
            var profile = new SlideShowProfile
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
            var settingsPart = _services.WorkContext.CurrentSite.As<SlideShowSettingsPart>();
            var profile = settingsPart.GetProfile(id);
            var engines = _engineManager.GetEngines().ToList();

            foreach (var engine in engines)
            {
                engine.Data = ElementDataHelper.Deserialize(profile.EngineStates[engine.Name]);
            }

            var viewModel = new SlideShowProfileViewModel
            {
                Name = profile.Name,
                AvailableEngines = engines,
                EngineSettingsEditors = engines.ToDictionary(x => x.Name, x => x.BuildSettingsEditor(_services.New)),
                SelectedEngine = profile.SelectedEngine
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(int id, SlideShowProfileViewModel viewModel)
        {
            var settingsPart = _services.WorkContext.CurrentSite.As<SlideShowSettingsPart>();
            var profile = settingsPart.GetProfile(id);
            var engines = _engineManager.GetEngines().ToList();

            viewModel.AvailableEngines = engines;
            viewModel.EngineSettingsEditors = engines.ToDictionary(x => x.Name, x => x.UpdateSettingsEditor(_services.New, this));

            if (!ModelState.IsValid)
            {
                _services.TransactionManager.Cancel();
                return View(viewModel);
            }

            foreach (var engine in engines)
            {
                profile.EngineStates[engine.Name] = engine.Data.Serialize();
            }

            profile.SelectedEngine = viewModel.SelectedEngine;
            settingsPart.StoreProfile(profile);
            _services.Notifier.Information(T("That slide show profile has been updated."));

            return RedirectToAction("Edit", new { id = id });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var settingsPart = _services.WorkContext.CurrentSite.As<SlideShowSettingsPart>();

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