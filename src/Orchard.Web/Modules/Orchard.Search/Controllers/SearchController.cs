using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Search.Services;
using Orchard.Search.ViewModels;
using Orchard.Settings;
using Orchard.Search.Models;
using System.Linq;
using System;
using System.Collections.Generic;
using Orchard.Collections;
using Orchard.Themes;

namespace Orchard.Search.Controllers {
    [ValidateInput(false), Themed]
    public class SearchController : Controller {
        private readonly ISearchService _searchService;
        private readonly IContentManager _contentManager;

        public SearchController(ISearchService searchService, IContentManager contentManager) {
            _searchService = searchService;
            _contentManager = contentManager;
        }

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public ActionResult Index(string q, int page = 1, int pageSize = 10) {
            var searchFields = CurrentSite.As<SearchSettingsPart>().SearchedFields;

            var searchHits = _searchService.Query(q, page, pageSize, 
                    CurrentSite.As<SearchSettingsPart>().Record.FilterCulture,
                    searchFields, 
                    searchHit => searchHit);

            var searchResultViewModels = new List<SearchResultViewModel>();

            foreach(var searchHit in searchHits) {
                var contentItem = _contentManager.Get(searchHit.ContentItemId);
                // ignore search results which content item has been removed or unpublished
                if(contentItem == null){
                    searchHits.TotalItemCount--;
                    continue;
                }

                searchResultViewModels.Add(new SearchResultViewModel {
                        Content = _contentManager.BuildDisplayModel(contentItem, "SummaryForSearch"),
                        SearchHit = searchHit
                    });
            }

            var pageOfItems = new PageOfItems<SearchResultViewModel>(searchResultViewModels) {
                PageNumber = page,
                PageSize = searchHits.PageSize,
                TotalItemCount = searchHits.TotalItemCount
            };

            var searchViewModel = new SearchViewModel {
                Query = q,
                DefaultPageSize = 10, // <- yeah, I know :|
                PageOfResults = pageOfItems
            };

            //todo: deal with page requests beyond result count

            return View(searchViewModel);
        }
    }
}