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

        public ActionResult Index(string q, int page = 1, int pageSize = 10) {
            var searchViewModel = new SearchViewModel {
                Query = q,
                DefaultPageSize = 10, // <- yeah, I know :|
                PageOfResults = _searchService.Query(q, page, pageSize, searchHit => new SearchResultViewModel {
                    Content = _contentManager.BuildDisplayModel(_contentManager.Get(searchHit.ContentItemId), "SummaryForSearch"),
                    SearchHit = searchHit
                })
            };

            //todo: deal with page requests beyond result count

            return View(searchViewModel);
        }
    }
}