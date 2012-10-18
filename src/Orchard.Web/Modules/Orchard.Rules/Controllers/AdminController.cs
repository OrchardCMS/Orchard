using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Rules.Models;
using Orchard.Rules.Services;
using Orchard.Rules.ViewModels;
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

namespace Orchard.Rules.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ISiteService _siteService;
        private readonly IRulesManager _rulesManager;
        private readonly IRulesServices _rulesServices;

        public AdminController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IRulesManager rulesManager,
            IRulesServices rulesServices,
            IRepository<RuleRecord> repository) {
            _siteService = siteService;
            _rulesManager = rulesManager;
            _rulesServices = rulesServices;
            Services = services;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(RulesIndexOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list rules")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new RulesIndexOptions();

            var rules = _rulesServices.GetRules();

            switch (options.Filter) {
                case RulesFilter.Disabled:
                    rules = rules.Where(r => r.Enabled == false);
                    break;
                case RulesFilter.Enabled:
                    rules = rules.Where(u => u.Enabled);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search)) {
                rules = rules.Where(r => r.Name.Contains(options.Search));
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(rules.Count());

            switch (options.Order) {
                case RulesOrder.Name:
                    rules = rules.OrderBy(u => u.Name);
                    break;
            }

            var results = rules
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            var model = new RulesIndexViewModel {
                Rules = results.Select(x => new RulesEntry {
                    Rule = x,
                    IsChecked = false,
                    RuleId = x.Id
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
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            var viewModel = new RulesIndexViewModel { Rules = new List<RulesEntry>(), Options = new RulesIndexOptions() };
            UpdateModel(viewModel);

            var checkedEntries = viewModel.Rules.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction) {
                case RulesBulkAction.None:
                    break;
                case RulesBulkAction.Enable:
                    foreach (var entry in checkedEntries) {
                        _rulesServices.GetRule(entry.RuleId).Enabled = true;
                    }
                    break;
                case RulesBulkAction.Disable:
                    foreach (var entry in checkedEntries) {
                        _rulesServices.GetRule(entry.RuleId).Enabled = false;
                    }
                    break;
                case RulesBulkAction.Delete:
                    foreach (var entry in checkedEntries) {
                        _rulesServices.DeleteRule(entry.RuleId);
                    }
                    break;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Move(string direction, int id, int actionId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            switch(direction) {
                case "up" : _rulesServices.MoveUp(actionId);
                    break;
                case "down": _rulesServices.MoveDown(actionId);
                    break;
                default:
                    throw new ArgumentException("direction");
            }

            return RedirectToAction("Edit", new { id });
        }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            return View(new CreateRuleViewModel());
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(CreateRuleViewModel viewModel) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
            }
            else {
                var rule = _rulesServices.CreateRule(viewModel.Name);
                return RedirectToAction("Edit", new { id = rule.Id });
            }

            return View(viewModel);
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to edit rules")))
                return new HttpUnauthorizedResult();

            var rule = _rulesServices.GetRule(id);
            var viewModel = new EditRuleViewModel {
                Id = rule.Id,
                Enabled = rule.Enabled,
                Name = rule.Name
            };

            #region Load events
            var eventEntries = new List<EventEntry>();
            var allEvents = _rulesManager.DescribeEvents().SelectMany(x => x.Descriptors);

            foreach (var eventRecord in rule.Events) {
                var category = eventRecord.Category;
                var type = eventRecord.Type;

                var ev = allEvents.Where(x => category == x.Category && type == x.Type).FirstOrDefault();
                if (ev != null) {
                    var eventParameters = FormParametersHelper.FromString(eventRecord.Parameters);
                    eventEntries.Add(
                        new EventEntry {
                            Category = ev.Category,
                            Type = ev.Type,
                            EventRecordId = eventRecord.Id,
                            DisplayText = ev.Display(new EventContext { Properties = eventParameters })
                        });
                }
            }

            viewModel.Events = eventEntries;

            #endregion

            #region Load actions
            var actionEntries = new List<ActionEntry>();
            var allActions = _rulesManager.DescribeActions().SelectMany(x => x.Descriptors);

            foreach (var actionRecord in rule.Actions.OrderBy(x => x.Position)) {
                var category = actionRecord.Category;
                var type = actionRecord.Type;

                var action = allActions.Where(x => category == x.Category && type == x.Type).FirstOrDefault();
                if (action != null) {
                    var actionParameters = FormParametersHelper.FromString(actionRecord.Parameters);
                    actionEntries.Add(
                        new ActionEntry {
                            Category = action.Category,
                            Type = action.Type,
                            ActionRecordId = actionRecord.Id,
                            DisplayText = action.Display(new ActionContext { Properties = actionParameters })
                        });
                }
            }

            viewModel.Actions = actionEntries;
            #endregion

            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPost(EditRuleViewModel viewModel) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
            }
            else {
                var rule = _rulesServices.GetRule(viewModel.Id);
                rule.Name = viewModel.Name;
                rule.Enabled = viewModel.Enabled;

                Services.Notifier.Information(T("Rule Saved"));
                return RedirectToAction("Edit", new { id = rule.Id });
            }

            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.SaveAndEnable")]
        public ActionResult EditAndEnablePost(EditRuleViewModel viewModel) {
            viewModel.Enabled = true;
            return EditPost(viewModel);
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            var rule = _rulesServices.GetRule(id);

            if (rule != null) {
                _rulesServices.DeleteRule(id);
                Services.Notifier.Information(T("Rule {0} deleted", rule.Name));
            }

            return RedirectToAction("Index");
        }

        public ActionResult Enable(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            var rule = _rulesServices.GetRule(id);

            if (rule != null) {
                rule.Enabled = true;
                Services.Notifier.Information(T("Rule enabled"));
            }

            return RedirectToAction("Index");
        }

        public ActionResult Disable(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage rules")))
                return new HttpUnauthorizedResult();

            var rule = _rulesServices.GetRule(id);

            if (rule != null) {
                rule.Enabled = false;
                Services.Notifier.Information(T("Rule disabled"));
            }

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
