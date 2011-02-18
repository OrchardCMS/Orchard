using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Setup.Services;
using Orchard.Setup.ViewModels;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Setup.Controllers {
    [ValidateInput(false), Themed]
    public class SetupController : Controller {
        private readonly IViewsBackgroundCompilation _viewsBackgroundCompilation;
        private readonly INotifier _notifier;
        private readonly ISetupService _setupService;

        public SetupController(
            INotifier notifier, 
            ISetupService setupService, 
            IViewsBackgroundCompilation viewsBackgroundCompilation) {
            _viewsBackgroundCompilation = viewsBackgroundCompilation;
            _notifier = notifier;
            _setupService = setupService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private ActionResult IndexViewResult(SetupViewModel model) {
            return View(model);
        }

        public ActionResult Index() {
            var initialSettings = _setupService.Prime();
            var recipes = (List<Recipe>)_setupService.Recipes();
            
            // On the first time installation of Orchard, the user gets to the setup screen, which
            // will take a while to finish (user inputting data and the setup process itself).
            // We use this opportunity to start a background task to "pre-compile" all the known
            // views in the app folder, so that the application is more reponsive when the user
            // hits the homepage and admin screens for the first time.
            if (StringComparer.OrdinalIgnoreCase.Equals(initialSettings.Name, ShellSettings.DefaultName)) {
                _viewsBackgroundCompilation.Start();
            }

            //

            return IndexViewResult(new SetupViewModel {
                AdminUsername = "admin",
                DatabaseIsPreconfigured = !string.IsNullOrEmpty(initialSettings.DataProvider),
                HasRecipes = recipes.Count > 0,
                Recipes = recipes
            });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model) {
            //TODO: Couldn't get a custom ValidationAttribute to validate two properties
            if (!model.DatabaseOptions && string.IsNullOrEmpty(model.DatabaseConnectionString))
                ModelState.AddModelError("DatabaseConnectionString", T("A SQL connection string is required").Text);

            if (!String.IsNullOrWhiteSpace(model.ConfirmPassword) && model.AdminPassword != model.ConfirmPassword ) {
                ModelState.AddModelError("ConfirmPassword", T("Password confirmation must match").Text);
            }

            if(!model.DatabaseOptions && !String.IsNullOrWhiteSpace(model.DatabaseTablePrefix)) {
                model.DatabaseTablePrefix = model.DatabaseTablePrefix.Trim();
                if(!Char.IsLetter(model.DatabaseTablePrefix[0])) {
                    ModelState.AddModelError("DatabaseTablePrefix", T("The table prefix must begin with a letter").Text);
                }
            }

            if (!ModelState.IsValid) {
                var recipes = (List<Recipe>)_setupService.Recipes();
                model.HasRecipes = recipes.Count > 0;
                model.Recipes = recipes;
                if (!String.IsNullOrEmpty(model.Recipe)) {
                    foreach (var recipe in recipes.Where(recipe => recipe.Name == model.Recipe)) {
                        model.RecipeDescription = recipe.Description;
                    }
                }
                model.DatabaseIsPreconfigured = !string.IsNullOrEmpty(_setupService.Prime().DataProvider);
                
                return IndexViewResult(model);
            }

            try {
                var setupContext = new SetupContext {
                    SiteName = model.SiteName,
                    AdminUsername = model.AdminUsername,
                    AdminPassword = model.AdminPassword,
                    DatabaseProvider = model.DatabaseOptions ? "SqlCe" : "SqlServer",
                    DatabaseConnectionString = model.DatabaseConnectionString,
                    DatabaseTablePrefix = model.DatabaseTablePrefix,
                    EnabledFeatures = null, // default list
                    Recipe = model.Recipe
                };

                string executionId = _setupService.Setup(setupContext);

                // First time installation if finally done. Tell the background views compilation
                // process to stop, so that it doesn't interfere with the user (asp.net compilation
                // uses a "single lock" mechanism for compiling views).
                _viewsBackgroundCompilation.Stop();

                // redirect to the welcome page.
                return Redirect("~/");
            } catch (Exception exception) {
                this.Error(exception, T("Setup failed:"), Logger, _notifier);

                return IndexViewResult(model);
            }
        }
    }
}