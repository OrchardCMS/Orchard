using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Core.Title.Models;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using System;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using Orchard.Workflows.ViewModels;

namespace Orchard.Workflows.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _services;
        private readonly ISiteService _siteService;
        private readonly IRepository<WorkflowDefinitionRecord> _workflowDefinitionRecords;
        private readonly IEnumerable<IActivity> _activities;

        public AdminController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IRepository<WorkflowDefinitionRecord> workflowDefinitionRecords,
            IEnumerable<IActivity> activities
            ) {
            _services = services;
            _siteService = siteService;
            _workflowDefinitionRecords = workflowDefinitionRecords;
            _activities = activities;
            Services = services;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(AdminIndexOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list workflows")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new AdminIndexOptions();

            var queries = _workflowDefinitionRecords.Table;

            switch (options.Filter) {
                case WorkflowDefinitionFilter.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!String.IsNullOrWhiteSpace(options.Search)) {
                queries = queries.Where(w => w.Name.Contains(options.Search));
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(queries.Count());

            switch (options.Order) {
                case WorkflowDefinitionOrder.Name:
                    queries = queries.OrderBy(u => u.Name);
                    break;
            }

            var results = queries
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            var model = new AdminIndexViewModel {
                WorkflowDefinitions = results.Select(x => new WorkflowDefinitionEntry {
                    WorkflowDefinitionRecord = x, 
                    WokflowDefinitionId = x.Id,
                    Name = x.Name
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

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to create workflows")))
                return new HttpUnauthorizedResult();

            return View();
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(string name) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to create workflows")))
                return new HttpUnauthorizedResult();

            var workflowDefinitionRecord = new WorkflowDefinitionRecord {
                Name = name
            };

            _workflowDefinitionRecords.Create(workflowDefinitionRecord);

            return RedirectToAction("Edit", new { workflowDefinitionRecord.Id });
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            var workflowDefinitionRecord = _workflowDefinitionRecords.Get(id);

            var viewModel = new AdminEditViewModel {
                WorkflowDefinitionRecord = workflowDefinitionRecord,
                AllActivities = _activities.ToList()
            };

            return View(viewModel);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
