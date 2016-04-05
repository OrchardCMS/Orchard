using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Core.Title.Models;
using Orchard.Forms.Services;
using Orchard.Mvc;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Projections.ViewModels;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using Orchard.Settings;
using Orchard.UI.Navigation;

namespace Orchard.Projections.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _services;
        private readonly ISiteService _siteService;
        private readonly IQueryService _queryService;
        private readonly IProjectionManager _projectionManager;

        public AdminController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IQueryService queryService,
            IProjectionManager projectionManager) {
            _services = services;
            _siteService = siteService;
            _queryService = queryService;
            _projectionManager = projectionManager;
            Services = services;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(AdminIndexOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to list queries")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new AdminIndexOptions();

            var queries = Services.ContentManager.Query("Query");

            switch (options.Filter) {
                case QueriesFilter.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!String.IsNullOrWhiteSpace(options.Search)) {
                queries = queries.Join<TitlePartRecord>().Where(r => r.Title.Contains(options.Search));
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(queries.Count());

            switch (options.Order) {
                case QueriesOrder.Name:
                    queries = queries.Join<TitlePartRecord>().OrderBy(u => u.Title);
                    break;
            }

            var results = queries
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new AdminIndexViewModel {
                Queries = results.Select(x => new QueryEntry {
                    Query = x.As<QueryPart>().Record, 
                    QueryId = x.Id,
                    Name = x.As<QueryPart>().Name
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
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input) {
            if (!Services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var viewModel = new AdminIndexViewModel { Queries = new List<QueryEntry>(), Options = new AdminIndexOptions() };
            UpdateModel(viewModel);

            var checkedItems = viewModel.Queries.Where(c => c.IsChecked);

            switch (viewModel.Options.BulkAction) {
                case QueriesBulkAction.None:
                    break;
                case QueriesBulkAction.Delete:
                    foreach (var checkedItem in checkedItems) {
                        _queryService.DeleteQuery(checkedItem.QueryId);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to edit queries")))
                return new HttpUnauthorizedResult();

            var query = _queryService.GetQuery(id);
            var viewModel = new AdminEditViewModel {
                Id = query.Id,
                Name = query.Name
            };

            #region Load Filters
            var filterGroupEntries = new List<FilterGroupEntry>();
            var allFilters = _projectionManager.DescribeFilters().SelectMany(x => x.Descriptors).ToList();

            foreach (var group in query.FilterGroups) {
                var filterEntries = new List<FilterEntry>();

                foreach (var filter in group.Filters) {
                    var category = filter.Category;
                    var type = filter.Type;

                    var f = allFilters.FirstOrDefault(x => category == x.Category && type == x.Type);
                    if (f != null) {
                        filterEntries.Add(
                            new FilterEntry {
                                Category = f.Category,
                                Type = f.Type,
                                FilterRecordId = filter.Id,
                                DisplayText = String.IsNullOrWhiteSpace(filter.Description) ? f.Display(new FilterContext {State = FormParametersHelper.ToDynamic(filter.State)}).Text : filter.Description
                            });
                    }
                }

                filterGroupEntries.Add( new FilterGroupEntry { Id = group.Id, Filters = filterEntries } );
            }

            viewModel.FilterGroups = filterGroupEntries;

            #endregion

            #region Load Sort criterias
            var sortCriterionEntries = new List<SortCriterionEntry>();
            var allSortCriteria = _projectionManager.DescribeSortCriteria().SelectMany(x => x.Descriptors).ToList();

            foreach (var sortCriterion in query.SortCriteria.OrderBy(s => s.Position)) {
                var category = sortCriterion.Category;
                var type = sortCriterion.Type;

                var f = allSortCriteria.FirstOrDefault(x => category == x.Category && type == x.Type);
                if (f != null) {
                    sortCriterionEntries.Add(
                        new SortCriterionEntry {
                            Category = f.Category,
                            Type = f.Type,
                            SortCriterionRecordId = sortCriterion.Id,
                            DisplayText = String.IsNullOrWhiteSpace(sortCriterion.Description) ? f.Display(new SortCriterionContext { State = FormParametersHelper.ToDynamic(sortCriterion.State) }).Text : sortCriterion.Description  
                        });
                }
            }

            viewModel.SortCriteria = sortCriterionEntries;

            #endregion

            #region Load Layouts
            var layoutEntries = new List<LayoutEntry>();
            var allLayouts = _projectionManager.DescribeLayouts().SelectMany(x => x.Descriptors).ToList();

            foreach (var layout in query.Layouts) {
                var category = layout.Category;
                var type = layout.Type;

                var f = allLayouts.FirstOrDefault(x => category == x.Category && type == x.Type);
                if (f != null) {
                    layoutEntries.Add(
                        new LayoutEntry {
                            Category = f.Category,
                            Type = f.Type,
                            LayoutRecordId = layout.Id,
                            DisplayText = String.IsNullOrWhiteSpace(layout.Description) ? f.Display(new LayoutContext { State = FormParametersHelper.ToDynamic(layout.State) }).Text : layout.Description
                        });
                }
            }

            viewModel.Layouts = layoutEntries;

            #endregion

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var query = _queryService.GetQuery(id);

            if (query == null) {
                return HttpNotFound();
            }

            Services.ContentManager.Remove(query.ContentItem);
            Services.Notifier.Success(T("Query {0} deleted", query.Name));

            return RedirectToAction("Index");
        }

        public ActionResult Preview(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var contentItems = _projectionManager.GetContentItems(id, 0, 20);
            var contentShapes = contentItems.Select(item => _services.ContentManager.BuildDisplay(item, "Summary"));

            var list = Shape.List();
            list.AddRange(contentShapes);

            return View(list);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
