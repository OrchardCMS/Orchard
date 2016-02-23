using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using System;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using Orchard.Workflows.ViewModels;
using Orchard.Workflows.Helpers;

namespace Orchard.Workflows.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ISiteService _siteService;
        private readonly IRepository<WorkflowDefinitionRecord> _workflowDefinitionRecords;
        private readonly IRepository<WorkflowRecord> _workflowRecords;
        private readonly IActivitiesManager _activitiesManager;
        private readonly IFormManager _formManager;

        public AdminController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IRepository<WorkflowDefinitionRecord> workflowDefinitionRecords,
            IRepository<WorkflowRecord> workflowRecords,
            IActivitiesManager activitiesManager,
            IFormManager formManager
            ) {
            _siteService = siteService;
            _workflowDefinitionRecords = workflowDefinitionRecords;
            _workflowRecords = workflowRecords;
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

            if (pager.GetStartIndex() > 0) {
                queries = queries.Skip(pager.GetStartIndex());
            }

            if (pager.PageSize > 0) {
                queries = queries.Take(pager.PageSize);
            }

            var results = queries.ToList();

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

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult BulkEdit(AdminIndexOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list workflows")))
                return new HttpUnauthorizedResult();

            var viewModel = new AdminIndexViewModel { WorkflowDefinitions = new List<WorkflowDefinitionEntry>(), Options = new AdminIndexOptions() };

            if (!TryUpdateModel(viewModel)) {
                return View(viewModel);
            }

            var checkedEntries = viewModel.WorkflowDefinitions.Where(t => t.IsChecked);
            switch (options.BulkAction) {
                case WorkflowDefinitionBulk.None:
                    break;
                case WorkflowDefinitionBulk.Delete:
                    if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage workflows")))
                        return new HttpUnauthorizedResult();

                    foreach (var entry in checkedEntries) {
                        var workflowDefinition = _workflowDefinitionRecords.Get(entry.WokflowDefinitionId);

                        if (workflowDefinition != null) {
                            _workflowDefinitionRecords.Delete(workflowDefinition);
                            Services.Notifier.Information(T("Workflow {0} deleted", workflowDefinition.Name));
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index", new { page = pagerParameters.Page, pageSize = pagerParameters.PageSize });
        }

        public ActionResult List(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list workflows")))
                return new HttpUnauthorizedResult();

            var contentItem = Services.ContentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null) {
                return HttpNotFound();
            }

            var workflows = _workflowRecords.Table.Where(x => x.ContentItemRecord == contentItem.Record).ToList();

            var viewModel = New.ViewModel(
                ContentItem: contentItem,
                Workflows: workflows
                );

            return View(viewModel);
        }

        public ActionResult EditProperties(int id = 0)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows.")))
                return new HttpUnauthorizedResult();

            if (id == 0) {
                return View();
            }
            else {
                var workflowDefinition = _workflowDefinitionRecords.Get(id);

                return View(new AdminEditViewModel { WorkflowDefinition = new WorkflowDefinitionViewModel { Name = workflowDefinition.Name, Id = workflowDefinition.Id } });
            }
        }


        [HttpPost, ActionName("EditProperties")]
        public ActionResult EditPropertiesPost(AdminEditViewModel adminEditViewModel, int id = 0) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows.")))
                return new HttpUnauthorizedResult();


            if (String.IsNullOrWhiteSpace(adminEditViewModel.WorkflowDefinition.Name)) {
                ModelState.AddModelError("Name", T("The Name can't be empty.").Text);
            }

            if (!ModelState.IsValid) {
                return View();
            }

            if (id == 0) {
                var workflowDefinitionRecord = new WorkflowDefinitionRecord {
                    Name = adminEditViewModel.WorkflowDefinition.Name
                };

                _workflowDefinitionRecords.Create(workflowDefinitionRecord);

                return RedirectToAction("Edit", new { workflowDefinitionRecord.Id });
            }
            else {
                var workflowDefinition = _workflowDefinitionRecords.Get(id);

                workflowDefinition.Name = adminEditViewModel.WorkflowDefinition.Name;

                return RedirectToAction("Index");
            }
        }

        public JsonResult State(int? id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                throw new AuthenticationException("");

            var workflowDefinitionRecord = id.HasValue ? _workflowDefinitionRecords.Get(id.Value) : null;
            var isRunning = workflowDefinitionRecord != null && workflowDefinitionRecord.WorkflowRecords.Any();
            return Json(new { isRunning = isRunning }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id, string localId, int? workflowId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            // convert the workflow definition into its view model
            var workflowDefinitionRecord = _workflowDefinitionRecords.Get(id);
            var workflowDefinitionViewModel = CreateWorkflowDefinitionViewModel(workflowDefinitionRecord);
            var workflow = workflowId.HasValue ? _workflowRecords.Get(workflowId.Value) : null;

            var viewModel = new AdminEditViewModel {
                LocalId = String.IsNullOrEmpty(localId) ? Guid.NewGuid().ToString() : localId,
                IsLocal = !String.IsNullOrEmpty(localId),
                WorkflowDefinition = workflowDefinitionViewModel,
                AllActivities = _activitiesManager.GetActivities(),
                Workflow = workflow
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage workflows")))
                return new HttpUnauthorizedResult();

            var workflowDefinition = _workflowDefinitionRecords.Get(id);

            if (workflowDefinition != null) {
                _workflowDefinitionRecords.Delete(workflowDefinition);
                Services.Notifier.Information(T("Workflow {0} deleted", workflowDefinition.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteWorkflow(int id, string returnUrl) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage workflows")))
                return new HttpUnauthorizedResult();

            var workflow = _workflowRecords.Get(id);

            if (workflow != null) {
                _workflowRecords.Delete(workflow);
                Services.Notifier.Information(T("Workflow deleted"));
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        private WorkflowDefinitionViewModel CreateWorkflowDefinitionViewModel(WorkflowDefinitionRecord workflowDefinitionRecord) {
            if (workflowDefinitionRecord == null) {
                throw new ArgumentNullException("workflowDefinitionRecord");
            }

            var workflowDefinitionModel = new WorkflowDefinitionViewModel {
                Id = workflowDefinitionRecord.Id,
                Name = workflowDefinitionRecord.Name
            };

            dynamic workflow = new JObject();
            workflow.Activities = new JArray(workflowDefinitionRecord.ActivityRecords.Select(x => {
                dynamic activity = new JObject();
                activity.Name = x.Name;
                activity.Id = x.Id;
                activity.ClientId = x.GetClientId();
                activity.Left = x.X;
                activity.Top = x.Y;
                activity.Start = x.Start;
                activity.State = FormParametersHelper.FromJsonString(x.State);

                return activity;
            }));

            workflow.Connections = new JArray(workflowDefinitionRecord.TransitionRecords.Select(x => {
                dynamic connection = new JObject();
                connection.Id = x.Id;
                connection.SourceId = x.SourceActivityRecord.Name + "_" + x.SourceActivityRecord.Id;
                connection.TargetId = x.DestinationActivityRecord.Name + "_" + x.DestinationActivityRecord.Id;
                connection.SourceEndpoint = x.SourceEndpoint;
                return connection;
            }));

            workflowDefinitionModel.JsonData = FormParametersHelper.ToJsonString(workflow);

            return workflowDefinitionModel;
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPost(int id, string localId, string data, bool clearWorkflows) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            var workflowDefinitionRecord = _workflowDefinitionRecords.Get(id);

            if (workflowDefinitionRecord == null) {
                return HttpNotFound();
            }

            workflowDefinitionRecord.Enabled = true;

            var state = FormParametersHelper.FromJsonString(data);
            var activitiesIndex = new Dictionary<string, ActivityRecord>();

            workflowDefinitionRecord.ActivityRecords.Clear();

            foreach (var activity in state.Activities) {
                ActivityRecord activityRecord;

                workflowDefinitionRecord.ActivityRecords.Add(activityRecord = new ActivityRecord {
                    Name = activity.Name,
                    X = activity.Left,
                    Y = activity.Top,
                    Start = activity.Start,
                    State = FormParametersHelper.ToJsonString(activity.State),
                    WorkflowDefinitionRecord = workflowDefinitionRecord
                });

                activitiesIndex.Add((string)activity.ClientId, activityRecord);
            }

            workflowDefinitionRecord.TransitionRecords.Clear();

            foreach (var connection in state.Connections) {
                workflowDefinitionRecord.TransitionRecords.Add(new TransitionRecord {
                    SourceActivityRecord = activitiesIndex[(string)connection.SourceId],
                    DestinationActivityRecord = activitiesIndex[(string)connection.TargetId],
                    SourceEndpoint = connection.SourceEndpoint,
                    WorkflowDefinitionRecord = workflowDefinitionRecord
                });
            }

            if (clearWorkflows) {
                workflowDefinitionRecord.WorkflowRecords.Clear();
            }
            else {
                foreach (var workflowRecord in workflowDefinitionRecord.WorkflowRecords) {
                    // Update any awaiting activity records with the new activity record.
                    foreach (var awaitingActivityRecord in workflowRecord.AwaitingActivities) {
                        var clientId = awaitingActivityRecord.ActivityRecord.GetClientId();
                        if (activitiesIndex.ContainsKey(clientId)) {
                            awaitingActivityRecord.ActivityRecord = activitiesIndex[clientId];
                        }
                        else {
                            workflowRecord.AwaitingActivities.Remove(awaitingActivityRecord);
                        }
                    }
                    // Remove any workflows with no awaiting activities.
                    if (!workflowRecord.AwaitingActivities.Any()) {
                        workflowDefinitionRecord.WorkflowRecords.Remove(workflowRecord);
                    }
                }
            }

            Services.Notifier.Information(T("Workflow saved successfully"));

            // Don't pass the localId to force the activites to refresh and use the deterministic clientId.
            return RedirectToAction("Edit", new { id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Cancel")]
        public ActionResult EditPostCancel() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            return View();
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

            dynamic shape = New.Activity(activity);

            if (model.State != null) {
                var state = FormParametersHelper.ToDynamic(FormParametersHelper.ToString(model.State));
                shape.State(state);
            } else {
                shape.State(FormParametersHelper.FromJsonString("{}"));
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
        [FormValueRequired("_submit.Save")]
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
                Data = HttpUtility.JavaScriptStringEncode(FormParametersHelper.ToJsonString(formValues))
            };

            TempData["UpdatedViewModel"] = model;

            return RedirectToAction("Edit", new {
                id,
                localId
            });
        }

        [HttpPost, ActionName("EditActivity")]
        [FormValueRequired("_submit.Cancel")]
        public ActionResult EditActivityPostCancel(int id, string localId, string name, string clientId, FormCollection formValues) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit workflows")))
                return new HttpUnauthorizedResult();

            return RedirectToAction("Edit", new { id, localId });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
