using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Search.Services;
using Orchard.Search.ViewModels;
namespace Orchard.Search.Controllers {
    [ValidateInput(false)]
    public class SearchController : Controller {
        private readonly ISearchService _searchService;
        private readonly IContentManager _contentManager;

        public SearchController(ISearchService searchService, IContentManager contentManager) {
            _searchService = searchService;
            _contentManager = contentManager;
        }

        public ActionResult Index(string q) {
            var searchViewModel = new SearchViewModel {Term = q};

            var results = _searchService.Query(q);
            searchViewModel.Results = results.Select(result => new SearchResultViewModel {
                Content = _contentManager.BuildDisplayModel(_contentManager.Get(result.Id), "SummaryForSearch"),
                SearchHit = result
            }).ToList();

            return View(searchViewModel);
        }
    }
}