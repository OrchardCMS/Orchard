using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.ViewModels;
using Orchard.Dashboards.Services;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Dashboards.Controllers {
    [Admin]
    public class DashboardController : Controller {
        private readonly IDashboardService _dashboardService;
        private readonly IOrchardServices _services;
        private readonly ICultureFilter _cultureFilter;
        private readonly ICultureManager _cultureManager;

        public DashboardController(IDashboardService dashboardService, IOrchardServices services, ICultureFilter cultureFilter, ICultureManager cultureManager) {
            _dashboardService = dashboardService;
            _services = services;
            _cultureFilter = cultureFilter;
            _cultureManager = cultureManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Display() {
            var shape = _dashboardService.GetDashboardShape();
            return new ShapeResult(this, shape);
        }

        public ActionResult List(ListContentsViewModel model, PagerParameters pagerParameters) {
            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            VersionOptions versionOptions;

            switch (model.Options.ContentsStatus) {
                case ContentsStatus.Published:
                    versionOptions = VersionOptions.Published;
                    break;
                case ContentsStatus.Draft:
                    versionOptions = VersionOptions.Draft;
                    break;
                case ContentsStatus.AllVersions:
                    versionOptions = VersionOptions.AllVersions;
                    break;
                default:
                    versionOptions = VersionOptions.Latest;
                    break;
            }

            var query = _services.ContentManager.Query(versionOptions, "Dashboard");

            switch (model.Options.OrderBy) {
                case ContentsOrder.Modified:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(model.Options.SelectedCulture)) {
                query = _cultureFilter.FilterCulture(query, model.Options.SelectedCulture);
            }

            model.Options.Cultures = _cultureManager.ListCultures();

            var maxPagedCount = _services.WorkContext.CurrentSite.MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
                pager.PageSize = maxPagedCount;
            var pagerShape = _services.New.Pager(pager).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = _services.New.List();
            list.AddRange(pageOfContentItems.Select(ci => _services.ContentManager.BuildDisplay(ci, "SummaryAdmin")));

            var viewModel = _services.New.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                .Options(model.Options)
                .TypeDisplayName(model.TypeDisplayName ?? "");

            return View(viewModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ContentOptions options) {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null) {
                routeValues["Options.SelectedCulture"] = options.SelectedCulture; //todo: don't hard-code the key
                routeValues["Options.OrderBy"] = options.OrderBy; //todo: don't hard-code the key
                routeValues["Options.ContentsStatus"] = options.ContentsStatus; //todo: don't hard-code the key
            }

            return RedirectToAction("List", routeValues);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult ListPOST(ContentOptions options, IEnumerable<int> itemIds, string returnUrl) {
            if (itemIds != null) {
                var checkedContentItems = _services.ContentManager.GetMany<ContentItem>(itemIds, VersionOptions.Latest, QueryHints.Empty);
                switch (options.BulkAction) {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.PublishNow:
                        foreach (var item in checkedContentItems) {
                            if (!_services.Authorizer.Authorize(Core.Contents.Permissions.PublishContent, item, T("Couldn't publish selected content."))) {
                                _services.TransactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _services.ContentManager.Publish(item);
                        }
                        _services.Notifier.Information(T("Content successfully published."));
                        break;
                    case ContentsBulkAction.Unpublish:
                        foreach (var item in checkedContentItems) {
                            if (!_services.Authorizer.Authorize(Core.Contents.Permissions.PublishContent, item, T("Couldn't unpublish selected content."))) {
                                _services.TransactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _services.ContentManager.Unpublish(item);
                        }
                        _services.Notifier.Information(T("Content successfully unpublished."));
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems) {
                            if (!_services.Authorizer.Authorize(Core.Contents.Permissions.DeleteContent, item, T("Couldn't remove selected content."))) {
                                _services.TransactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _services.ContentManager.Remove(item);
                        }
                        _services.Notifier.Information(T("Content successfully removed."));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return RedirectToAction("List");
        }
    }
}