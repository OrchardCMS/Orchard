using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using IDeliverable.ThemeSettings.Helpers;
using IDeliverable.ThemeSettings.Models;
using IDeliverable.ThemeSettings.Services;
using IDeliverable.ThemeSettings.ViewModels;

namespace IDeliverable.ThemeSettings.Controllers
{
    [Admin]
    public class ProfileController : Controller
    {
        private readonly IThemeSettingsService _themeSettingsService;
        private readonly INotifier _notifier;
        private readonly ISettingsFormBuilder _settingsFormBuilder;

        public ProfileController(IThemeSettingsService themeSettingsService, INotifier notifier, ISettingsFormBuilder settingsFormBuilder)
        {
            _themeSettingsService = themeSettingsService;
            _notifier = notifier;
            _settingsFormBuilder = settingsFormBuilder;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index(string id)
        {
            var viewModel = new ProfilesIndexViewModel
            {
                Theme = _themeSettingsService.GetTheme(id),
                Profiles = _themeSettingsService.GetProfiles(id).ToList()
            };
            return View(viewModel);
        }

        public ActionResult Create(string id)
        {
            var settingsManifest = _themeSettingsService.GetSettingsManifest(id);
            var viewModel = new ProfileViewModel
            {
                Theme = _themeSettingsService.GetTheme(id),
                SettingsForm = _settingsFormBuilder.BuildForm(settingsManifest)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(string id, ProfileViewModel viewModel, FormCollection formCollection)
        {
            viewModel.Theme = _themeSettingsService.GetTheme(id);

            if (!ModelState.IsValid)
            {
                var settingsManifest = _themeSettingsService.GetSettingsManifest(id);
                var form = _settingsFormBuilder.BuildForm(settingsManifest);
                _settingsFormBuilder.BindForm(form, formCollection);
                viewModel.SettingsForm = _settingsFormBuilder.BindForm(form, formCollection);
                return View(viewModel);
            }

            var profile = new ThemeProfile
            {
                Name = viewModel.Name.TrimSafe(),
                Description = viewModel.Description.TrimSafe(),
                Theme = id,
                Settings = ParseThemeSettings(formCollection),
                IsCurrent = viewModel.IsCurrent
            };

            _themeSettingsService.SaveProfile(profile);
            _notifier.Information(T("The profile {0} for the {1} theme was succesfully created.", viewModel.Name, viewModel.Theme.Name));

            return RedirectToAction("Edit", new { id = profile.Id });
        }

        public ActionResult Edit(int id)
        {
            var profile = _themeSettingsService.GetProfile(id);
            var settingsManifest = _themeSettingsService.GetSettingsManifest(profile.Theme);

            var viewModel = new ProfileViewModel
            {
                Name = profile.Name,
                Description = profile.Description,
                Theme = _themeSettingsService.GetTheme(profile.Theme),
                SettingsForm = _settingsFormBuilder.BuildForm(settingsManifest),
                IsCurrent = profile.IsCurrent
            };

            _settingsFormBuilder.BindForm(viewModel.SettingsForm, new ThemeSettingsValueProvider(profile.Settings));
            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int id, ProfileViewModel viewModel, FormCollection formCollection)
        {
            var profile = _themeSettingsService.GetProfile(id);
            var settingsManifest = _themeSettingsService.GetSettingsManifest(profile.Theme);

            viewModel.Theme = _themeSettingsService.GetTheme(profile.Theme);

            if (!ModelState.IsValid)
            {
                var form = _settingsFormBuilder.BuildForm(settingsManifest);
                _settingsFormBuilder.BindForm(form, formCollection);
                viewModel.SettingsForm = _settingsFormBuilder.BindForm(form, formCollection);
                return View(viewModel);
            }

            profile.Settings = ParseThemeSettings(formCollection);
            profile.Name = viewModel.Name.TrimSafe();
            profile.Description = viewModel.Description.TrimSafe();
            profile.IsCurrent = viewModel.IsCurrent;

            _themeSettingsService.SaveProfile(profile);
            _notifier.Information(T("The profile {0} for the {1} theme was succesfully updated.", viewModel.Name, viewModel.Theme.Name));

            return RedirectToAction("Edit", new { id = id });
        }

        [HttpPost]
        public ActionResult MakeCurrent(int id)
        {
            var profile = _themeSettingsService.GetProfile(id);

            profile.IsCurrent = true;
            _themeSettingsService.SaveProfile(profile);
            _notifier.Information(T("The {0} profile is now the current one.", profile.Name));

            return RedirectToAction("Index", new { id = profile.Theme });
        }

        [HttpPost]
        public ActionResult Clone(int id)
        {
            var profile = _themeSettingsService.GetProfile(id);
            var clonedProfile = _themeSettingsService.CloneProfile(profile);

            _themeSettingsService.SaveProfile(clonedProfile);
            _notifier.Information(T("The {0} profile has been cloned.", profile.Name, clonedProfile.Name));

            return RedirectToAction("Index", new { id = profile.Theme });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var profile = _themeSettingsService.GetProfile(id);

            _themeSettingsService.DeleteProfile(profile.Id);
            _notifier.Information(T("The {0} profile has been deleted.", profile.Name));

            return RedirectToAction("Index", new { id = profile.Theme });
        }

        private static IDictionary<string, ThemeSetting> ParseThemeSettings(FormCollection formCollection)
        {
            var blacklist = new[] { "__RequestVerificationToken", "Name", "Description", "IsCurrent" };
            var dictionary = formCollection.AllKeys.Where(x => !blacklist.Contains(x)).ToDictionary(x => x, formCollection.Get);

            return dictionary.ToDictionary(x => x.Key, x => new ThemeSetting { Name = x.Key, Value = x.Value });
        }
    }
}