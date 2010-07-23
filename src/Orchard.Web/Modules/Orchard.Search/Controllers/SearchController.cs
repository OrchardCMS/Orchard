using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Search.Services;
using Orchard.Search.ViewModels;
using Orchard.Settings;
using Orchard.Search.Models;
using System;

namespace Orchard.Search.Controllers {
    [ValidateInput(false)]
    public class SearchController : Controller {
        private readonly ISearchService _searchService;
        private readonly IContentManager _contentManager;

        public SearchController(ISearchService searchService, IContentManager contentManager) {
            _searchService = searchService;
            _contentManager = contentManager;
        }

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public ActionResult Index(string q, int page = 1, int pageSize = 10) {
            var searchFields = CurrentSite.As<SearchSettingsPart>().Record.SearchedFields.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);

            var searchViewModel = new SearchViewModel {
                Query = q,
                DefaultPageSize = 10, // <- yeah, I know :|
                PageOfResults = _searchService.Query(q, page, pageSize, 
                    CurrentSite.As<SearchSettingsPart>().Record.FilterCulture,
                    searchFields, 
                    searchHit => new SearchResultViewModel {
                        Content = _contentManager.BuildDisplayModel(_contentManager.Get(searchHit.ContentItemId), "SummaryForSearch"),
                        SearchHit = searchHit
                    })
            };

            //todo: deal with page requests beyond result count

            return View(searchViewModel);
        }
    }
}