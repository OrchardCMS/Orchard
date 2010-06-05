using System;
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

        public ActionResult Index(string q, int page = 0, int pageSize = 0) {
            var take = pageSize > 0 ? pageSize : 10;
            var skip = (page > 0 ? page - 1 : 0) * take;
            var searchViewModel = new SearchViewModel {
                Query = q,
                Page = page > 0 ? page : 1,
                PageSize = take
            };

            var results = _searchService.Query(q, skip, take);

            if (results == null)
                return View(searchViewModel);

            searchViewModel.Count = results.TotalCount;
            searchViewModel.TotalPageCount = (int)Math.Ceiling((decimal)searchViewModel.Count/searchViewModel.PageSize);
            //todo: deal with page requests beyond result count
            searchViewModel.ResultsPage = results.Page
                .Select(result => new SearchResultViewModel {
                    Content = _contentManager.BuildDisplayModel(_contentManager.Get(result.Id), "SummaryForSearch"),
                    SearchHit = result
                })
                .ToList();

            return View(searchViewModel);
        }
    }
}