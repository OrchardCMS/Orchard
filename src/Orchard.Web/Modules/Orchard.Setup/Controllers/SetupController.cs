using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Setup.Services;
using Orchard.Setup.ViewModels;
using Orchard.Localization;
using Orchard.Recipes.Services;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Orchard.Setup.Controllers {
    [ValidateInput(false), Themed]
    public class SetupController : Controller {
        private readonly IViewsBackgroundCompilation _viewsBackgroundCompilation;
        private readonly ShellSettings _shellSettings;
        private readonly INotifier _notifier;
        private readonly ISetupService _setupService;
        private const string DefaultRecipe = "Default";

        public SetupController(
            INotifier notifier,
            ISetupService setupService,
            IViewsBackgroundCompilation viewsBackgroundCompilation,
            ShellSettings shellSettings) {

            _viewsBackgroundCompilation = viewsBackgroundCompilation;
            _shellSettings = shellSettings;
            _notifier = notifier;
            _setupService = setupService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            RecipeExecutionTimeout = 600;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public int RecipeExecutionTimeout { get; set; }

        private ActionResult IndexViewResult(SetupViewModel model) {
            return View(model);
        }

        public ActionResult Index() {
            var initialSettings = _setupService.Prime();
            var recipes = _setupService.Recipes().OrderBy(c => c.Name).ToList();
            string recipeDescription = null;

            if (recipes.Any()) {
                recipeDescription = recipes.First().Description;
            }

            // On the first time installation of Orchard, the user gets to the setup screen, which
            // will take a while to finish (user inputting data and the setup process itself).
            // We use this opportunity to start a background task to "pre-compile" all the known
            // views in the app folder, so that the application is more reponsive when the user
            // hits the homepage and admin screens for the first time).
            if (StringComparer.OrdinalIgnoreCase.Equals(initialSettings.Name, ShellSettings.DefaultName)) {
                _viewsBackgroundCompilation.Start();
            }

            return IndexViewResult(new SetupViewModel {
                AdminUsername = "admin",
                DatabaseIsPreconfigured = !String.IsNullOrEmpty(initialSettings.DataProvider),
                Recipes = recipes,
                RecipeDescription = recipeDescription
            });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model) {
            // Sets the setup request timeout to a configurable amount of seconds to give enough time to execute custom recipes.
            HttpContext.Server.ScriptTimeout = RecipeExecutionTimeout;

            var recipes = _setupService.Recipes().ToList();

            // If no builtin provider, a connection string is mandatory.
            if (model.DatabaseProvider != SetupDatabaseType.Builtin && string.IsNullOrEmpty(model.DatabaseConnectionString))
                ModelState.AddModelError("DatabaseConnectionString", T("A connection string is required.").Text);

            if (!String.IsNullOrWhiteSpace(model.ConfirmPassword) && model.AdminPassword != model.ConfirmPassword) {
                ModelState.AddModelError("ConfirmPassword", T("Password confirmation must match.").Text);
            }

            if (model.DatabaseProvider != SetupDatabaseType.Builtin && !string.IsNullOrWhiteSpace(model.DatabaseTablePrefix)) {
                model.DatabaseTablePrefix = model.DatabaseTablePrefix.Trim();
                if (!Char.IsLetter(model.DatabaseTablePrefix[0])) {
                    ModelState.AddModelError("DatabaseTablePrefix", T("The table prefix must begin with a letter.").Text);
                }

                if (model.DatabaseTablePrefix.Any(x => !Char.IsLetterOrDigit(x))) {
                    ModelState.AddModelError("DatabaseTablePrefix", T("The table prefix must contain letters or digits.").Text);
                }
            }
            if (model.Recipe == null) {
                if (!(recipes.Select(r => r.Name).Contains(DefaultRecipe))) {
                    ModelState.AddModelError("Recipe", T("No recipes were found.").Text);
                }
                else {
                    model.Recipe = DefaultRecipe;
                }
            }
            if (!ModelState.IsValid) {
                model.Recipes = recipes;
                foreach (var recipe in recipes.Where(recipe => recipe.Name == model.Recipe)) {
                    model.RecipeDescription = recipe.Description;
                }
                model.DatabaseIsPreconfigured = !String.IsNullOrEmpty(_setupService.Prime().DataProvider);

                return IndexViewResult(model);
            }

            try {
                string providerName = null;

                switch (model.DatabaseProvider) {
                    case SetupDatabaseType.Builtin:
                        providerName = "SqlCe";
                        break;

                    case SetupDatabaseType.SqlServer:
                        providerName = "SqlServer";
                        break;

                    case SetupDatabaseType.MySql:
                        providerName = "MySql";
                        break;

                    case SetupDatabaseType.PostgreSql:
                        providerName = "PostgreSql";
                        break;

                    default:
                        throw new ApplicationException("Unknown database type: " + model.DatabaseProvider);
                }

                var recipe = recipes.GetRecipeByName(model.Recipe);
                var setupContext = new SetupContext {
                    SiteName = model.SiteName,
                    AdminUsername = model.AdminUsername,
                    AdminPassword = model.AdminPassword,
                    DatabaseProvider = providerName,
                    DatabaseConnectionString = model.DatabaseConnectionString,
                    DatabaseTablePrefix = model.DatabaseTablePrefix,
                    EnabledFeatures = null, // Default list
                    Recipe = recipe
                };

                var executionId = _setupService.Setup(setupContext);

                // First time installation if finally done. Tell the background views compilation
                // process to stop, so that it doesn't interfere with the user (asp.net compilation
                // uses a "single lock" mechanism for compiling views).
                _viewsBackgroundCompilation.Stop();

                // Redirect to the welcome page.
                return Redirect("~/" + _shellSettings.RequestUrlPrefix);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Setup failed");
                _notifier.Error(T("Setup failed: {0}", ex.Message));

                model.Recipes = recipes;
                foreach (var recipe in recipes.Where(recipe => recipe.Name == model.Recipe)) {
                    model.RecipeDescription = recipe.Description;
                }
                model.DatabaseIsPreconfigured = !String.IsNullOrEmpty(_setupService.Prime().DataProvider);

                return IndexViewResult(model);
            }
        }
    }
}