using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Themes;
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
        private readonly IActivitiesManager _activitiesManager;
        private readonly IFormManager _formManager;

        public AdminController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IRepository<WorkflowDefinitionRecord> workflowDefinitionRecords,
            IActivitiesManager activitiesManager,
            IFormManager formManager
            ) {
            _services = services;
            _siteService = siteService;
            _workflowDefinitionRecords = workflowDefinitionRecords;
            _activitiesManager = activitiesManager;
            _formManager = formManager;
            Services = services;

            T = NullLocalizer.Instance;
            New = shapeFactory;
        }

        dynamic New { get; set; }
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

            var pagerShape = New.Pager(pager).TotalItemCount(queries.Count());

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

        public ActionResult Edit(int id, string localId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            // convert the workflow definition into its view model
            var workflowDefinitionRecord = _workflowDefinitionRecords.Get(id);
            var workflowDefinitionModel = new WorkflowDefinitionViewModel();

            if (workflowDefinitionRecord != null) {
                workflowDefinitionModel.Id = workflowDefinitionRecord.Id;
                
                foreach (var activityRecord in workflowDefinitionRecord.ActivityRecords) {
                    workflowDefinitionModel.Activities.Add(new ActivityViewModel {
                        Name = activityRecord.Type,
                        ClientId = activityRecord.Type + "_" + activityRecord.Id,
                        State = FormParametersHelper.FromString(activityRecord.State)
                    });
                }

                foreach (var transitionRecord in workflowDefinitionRecord.TransitionRecords) {
                    workflowDefinitionModel.Connections.Add(new ConnectionViewModel{
                        Id = transitionRecord.Id,
                        SourceClientId = transitionRecord.SourceActivityRecord.Type + "_" + transitionRecord.SourceActivityRecord.Id,
                        DestinationClientId = transitionRecord.DestinationActivityRecord.Type + "_" + transitionRecord.DestinationActivityRecord.Id,
                        Outcome = transitionRecord.DestinationEndpoint
                    });
                }
            }

            var viewModel = new AdminEditViewModel {
                LocalId = Guid.NewGuid().ToString(),
                WorkflowDefinition = workflowDefinitionModel,
                AllActivities = _activitiesManager.GetActivities()
            };

            return View(viewModel);
        }

        public ActionResult EditLocal(string localId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            var viewModel = new AdminEditViewModel {
                LocalId = localId,
                AllActivities = _activitiesManager.GetActivities(),
            };

            return View("Edit", viewModel);
        }

        [Themed(false)]
        [HttpPost]
        public ActionResult RenderActivity(ActivityViewModel model) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            var activity = _activitiesManager.GetActivityByName(model.Name);

            if (activity == null) {
                return HttpNotFound();
            }

            IShape shape = New.Activity(activity);

            if (model.State != null) {
                var dynamicState = FormParametersHelper.ToDynamic(FormParametersHelper.ToString(model.State));
                ((dynamic)shape).State(dynamicState);
            }

            shape.Metadata.Alternates.Add("Activity__" + activity.Name);

            return new ShapeResult(this, shape);
        }

        public ActionResult EditActivity(string localId, string clientId, ActivityViewModel model) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            var activity = _activitiesManager.GetActivityByName(model.Name);

            if (activity == null) {
                return HttpNotFound();
            }

            // build the form, and let external components alter it
            var form = activity.Form == null ? null : _formManager.Build(activity.Form);

            // form is bound on client side

            var viewModel = New.ViewModel(LocalId: localId, ClientId: clientId, Form: form);
            
            return View(viewModel);
        }

        [HttpPost, ActionName("EditActivity")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditActivityPost(int id, string localId, string name, string clientId, FormCollection formValues) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            var activity = _activitiesManager.GetActivityByName(name);

            if (activity == null) {
                return HttpNotFound();
            }

            // validating form values
            _formManager.Validate(new ValidatingContext { FormName = activity.Form, ModelState = ModelState, ValueProvider = ValueProvider });

            // stay on the page if there are validation errors
            if (!ModelState.IsValid) {

                // build the form, and let external components alter it
                var form = activity.Form == null ? null : _formManager.Build(activity.Form);

                // bind form with existing values.
                _formManager.Bind(form, ValueProvider);
                
                var viewModel = New.ViewModel(Id: id, LocalId: localId, Form: form);

                return View(viewModel);
            }

            var model = new UpdatedActivityModel {
                ClientId = clientId,
                Data = formValues
            };

            TempData["UpdatedViewModel"] = model;

            return RedirectToAction("EditLocal", new {
                localId
            });
        }

        [HttpPost, ActionName("EditActivity")]
        [FormValueRequired("submit.Cancel")]
        public ActionResult EditActivityPostCancel(string localId, string name, string clientId, FormCollection formValues) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            return RedirectToAction("EditLocal", new {localId });
        }
        
        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
