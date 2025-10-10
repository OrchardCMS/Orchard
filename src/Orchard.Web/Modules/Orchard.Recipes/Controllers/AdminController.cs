using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Services;
using Orchard.Recipes.ViewModels;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Recipes.Controllers {
    [Admin]
    public class AdminController : Controller {
        private readonly IExtensionManager _extensionManager;
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IRecipeManager _recipeManager;
        private readonly IRecipeResultAccessor _recipeResultAccessor;
        private readonly ShellSettings _shellSettings;

        public AdminController(
            IOrchardServices services,
            IExtensionManager extensionManager,
            IRecipeHarvester recipeHarvester,
            IRecipeManager recipeManager,
            IRecipeResultAccessor recipeResultAccessor,
            ShellSettings shellSettings,
            IShapeFactory shapeFactory) {
            Services = services;
            _extensionManager = extensionManager;
            _recipeHarvester = recipeHarvester;
            _recipeManager = recipeManager;
            _recipeResultAccessor = recipeResultAccessor;
            _shellSettings = shellSettings;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to execute recipe files.")))
                return new HttpUnauthorizedResult();

            var modules = _extensionManager.AvailableExtensions()
                .Where(extensionDescriptor => ExtensionIsAllowed(extensionDescriptor))
                .OrderBy(extensionDescriptor => extensionDescriptor.Name);

            var viewModel = new RecipesViewModel {
                Modules = modules
                    .Select(x => new ModuleRecipesViewModel {
                        Descriptor = x,
                        Recipes = _recipeHarvester.HarvestRecipes(x.Id).Where(recipe => !recipe.IsSetupRecipe).ToList()
                    })
                    .Where(x => x.Recipes.Any())
                    .ToList()
            };

            return View(viewModel);

        }

        [HttpPost, ActionName("Recipes")]
        public ActionResult RecipesPOST(string moduleId, string name) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to execute recipe files.")))
                return new HttpUnauthorizedResult();

            var module = _extensionManager.AvailableExtensions()
                .Where(extensionDescriptor => extensionDescriptor.Id == moduleId && ExtensionIsAllowed(extensionDescriptor))
                .FirstOrDefault();

            if (module == null) {
                return HttpNotFound();
            }

            var recipe = _recipeHarvester.HarvestRecipes(module.Id).FirstOrDefault(x => !x.IsSetupRecipe && x.Name == name);

            if (recipe == null) {
                return HttpNotFound();
            }

            var executionId = _recipeManager.Execute(recipe);

            if (string.IsNullOrEmpty(executionId)) {
                Logger.Error("Error while executing recipe {0} in {1}.", name, moduleId);

                Services.Notifier.Error(T("Error while executing recipe {0} in {1}.", name, moduleId));

                return RedirectToAction("Index");
            }
            else {
                return RedirectToAction("RecipeResult", new { executionId });
            }

        }

        public ActionResult RecipeResult(string executionId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to view recipe file execution results.")))
                return new HttpUnauthorizedResult();

            var result = _recipeResultAccessor.GetResult(executionId);

            var viewModel = new RecipeResultViewModel() {
                Result = result
            };

            return View(viewModel);
        }


        /// <summary>
        /// Checks whether the given Extension is allowed for the current Tenant.
        /// </summary>
        private bool ExtensionIsAllowed(ExtensionDescriptor extensionDescriptor) {
            return _shellSettings.Modules.Length == 0 || _shellSettings.Modules.Contains(extensionDescriptor.Id);
        }
    }
}