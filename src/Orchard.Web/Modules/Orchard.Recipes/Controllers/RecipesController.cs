using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Controllers {
    public class RecipesController : Controller {
        private readonly IRecipeJournal _recipeJournal;

        public RecipesController(IRecipeJournal recipeJournal) {
            _recipeJournal = recipeJournal;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult RecipeExecutionStatus (string executionId) {
            var recipeStatus = _recipeJournal.GetRecipeStatus(executionId);
            var model = recipeStatus;

            return View(model);
        }
    }
}