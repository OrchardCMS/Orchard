using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Search.Services;
using Orchard.Search.ViewModels;

namespace Orchard.Search.Controllers {
    public class SearchController : Controller {
        private readonly ISearchService _searchService;
        private readonly IContentManager _contentManager;

        public SearchController(ISearchService searchService, IContentManager contentManager) {
            _searchService = searchService;
            _contentManager = contentManager;
        }

        public ActionResult Index(string term) {
            var results = _searchService.Query(term);
            return View(new SearchViewModel {
                Term = term,
                Results = results.Select(result => new SearchResultViewModel {
                    Content = _contentManager.BuildDisplayModel(_contentManager.Get(result.Id), "SummaryForSearch"),
                    SearchHit = result
                }).ToList()
            });
        }
    }
}