using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Services;
using Orchard.AntiSpam.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Navigation;

namespace Orchard.AntiSpam.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly ISiteService _siteService;
        private readonly ISpamService _spamService;

        public AdminController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            ISpamService spamService) {
            _siteService = siteService;
            _spamService = spamService;
            Services = services;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(SpamIndexOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(Permissions.ManageAntiSpam, T("Not authorized to manage spam")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new SpamIndexOptions();

            var query = Services.ContentManager.Query().ForPart<SpamFilterPart>();
            
            switch(options.Filter) {
                case SpamFilter.Spam:
                    query = query.Where<SpamFilterPartRecord>(x => x.Status == SpamStatus.Spam);
                    break;
                case SpamFilter.Ham:
                    query = query.Where<SpamFilterPartRecord>(x => x.Status == SpamStatus.Ham);
                    break;
                case SpamFilter.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());

            switch (options.Order) {
                case SpamOrder.Creation:
                    query = query.Join<CommonPartRecord>().OrderByDescending(u => u.CreatedUtc);
                    break;
            }

            var results = query
                .Slice(pager.GetStartIndex(), pager.PageSize);

            var model = new SpamIndexViewModel {
                Spams = results.Select(x => new SpamEntry {
                    Spam = x.As<SpamFilterPart>(),
                    Shape = Services.ContentManager.BuildDisplay(x, "SummaryAdmin")
                }).ToList(),
                
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [Core.Contents.Controllers.FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(SpamIndexOptions options, IEnumerable<int> itemIds) {
            if (!Services.Authorizer.Authorize(Permissions.ManageAntiSpam, T("Not authorized to manage spam")))
                return new HttpUnauthorizedResult();

            switch (options.BulkAction) {
                case SpamBulkAction.None:
                    break;
                case SpamBulkAction.Spam:
                    foreach (var checkedId in itemIds) {
                        var spam = Services.ContentManager.Get(checkedId);
                        if(spam != null) {
                            spam.As<SpamFilterPart>().Status = SpamStatus.Spam;
                            _spamService.ReportSpam(spam.As<SpamFilterPart>());
                        }
                    }
                    break;
                case SpamBulkAction.Ham:
                    foreach (var checkedId in itemIds) {
                        var ham = Services.ContentManager.Get(checkedId);
                        if (ham != null) {
                            ham.As<SpamFilterPart>().Status = SpamStatus.Ham;
                            _spamService.ReportHam(ham.As<SpamFilterPart>());
                        }
                    }
                    break;
                case SpamBulkAction.Delete:
                    foreach (var checkedId in itemIds) {
                        Services.ContentManager.Remove(Services.ContentManager.Get(checkedId));
                    }
                    break;
            }

            Services.ContentManager.Flush();

            return Index(options, new PagerParameters());
        }
    }
}
