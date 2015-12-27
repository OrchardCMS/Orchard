using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Autofac;
using Orchard.Caching.Services;
using Orchard.ContentManagement;
using Orchard.Mvc.Filters;

namespace Orchard.Glimpse.Tests
{
    public class TestFilter : FilterProvider, IActionFilter
    {
        private readonly ICacheService _cacheService;

        public TestFilter(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _cacheService.Put("Cache Test", 0);
            _cacheService.Put("Timed Test", new MySuperCachedItem(), new TimeSpan(5, 4, 3, 2, 1));
            _cacheService.GetObject<MySuperCachedItem>("Timed Test");
            _cacheService.GetObject<int>("Cache Miss");
            _cacheService.Remove("Cache Test");
            _cacheService.GetObject<int>("Cache Test");
            _cacheService.Clear();
            _cacheService.GetObject<MySuperCachedItem>("Timed Test");
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) { }
    }

    public class MySuperCachedItem
    {
        public MySuperCachedItem() {
            Name = "Chris Payne";
            DateOfBirth = new DateTime(1985, 10, 26);
            NumberOfOrchardHarvestTalksGiven = 1;
        }

        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int NumberOfOrchardHarvestTalksGiven { get; set; }
    }
}