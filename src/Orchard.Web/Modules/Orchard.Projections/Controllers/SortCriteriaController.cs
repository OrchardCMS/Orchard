using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Projections.ViewModels;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Projections.Controllers {
    [ValidateInput(false), Admin]
    public class SortCriterionController : Controller {
        public SortCriterionController(
            IOrchardServices services,
            IFormManager formManager,
            IShapeFactory shapeFactory,
            IProjectionManager projectionManager,
            IRepository<SortCriterionRecord> repository,
            IQueryService queryService,
            ISortService sortService) {
            Services = services;
            _formManager = formManager;
            _projectionManager = projectionManager;
            _repository = repository;
            _queryService = queryService;
            _sortService = sortService;
            Shape = shapeFactory;
        }

        public IOrchardServices Services { get; set; }
        private readonly IFormManager _formManager;
        private readonly IProjectionManager _projectionManager;
        private readonly IRepository<SortCriterionRecord> _repository;
        private readonly IQueryService _queryService;
        private readonly ISortService _sortService;
        public Localizer T { get; set; }
        public dynamic Shape { get; set; }

        public ActionResult Add(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var viewModel = new SortCriterionAddViewModel { Id = id, SortCriteria = _projectionManager.DescribeSortCriteria() };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Delete(int id, int sortCriterionId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var sortCriterion = _repository.Get(sortCriterionId);
            if(sortCriterion == null) {
                return HttpNotFound();
            }

            sortCriterion.QueryPartRecord.SortCriteria.Remove(sortCriterion);
            _repository.Delete(sortCriterion);

            Services.Notifier.Success(T("Sort criteria deleted"));

            return RedirectToAction("Edit", "Admin", new { id });
        }

        public ActionResult Edit(int id, string category, string type, int sortCriterionId = -1) {
            if (!Services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var sortCriterion = _projectionManager.DescribeSortCriteria().SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == category && x.Type == type);

            if (sortCriterion == null) {
                return HttpNotFound();
            }

            // if there is no form to edit, save the sort criteria and go back to the query
            if (sortCriterion.Form == null) {
                if (sortCriterionId == -1) {
                    var query = _queryService.GetQuery(id);
                    query.SortCriteria.Add(new SortCriterionRecord {
                        Category = category, 
                        Type = type,
                        Position = query.SortCriteria.Count
                    });
                }

                return RedirectToAction("Edit", "Admin", new { id });
            }

            // build the form, and let external components alter it
            var form = _formManager.Build(sortCriterion.Form);

            var description = String.Empty;

            // bind form with existing values).
            if (sortCriterionId != -1) {
                var query = _queryService.GetQuery(id);
                var sortCriterionRecord = query.SortCriteria.FirstOrDefault(f => f.Id == sortCriterionId);
                if (sortCriterionRecord != null) {
                    description = sortCriterionRecord.Description;
                    var parameters = FormParametersHelper.FromString(sortCriterionRecord.State);
                    _formManager.Bind(form, new DictionaryValueProvider<string>(parameters, CultureInfo.InvariantCulture));
                }
            }

            var viewModel = new SortCriterionEditViewModel { Id = id, Description = description, SortCriterion = sortCriterion, Form = form };
            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id, string category, string type, [DefaultValue(-1)]int sortCriterionId, FormCollection formCollection) {
            var query = _queryService.GetQuery(id);

            var sortCriterion = _projectionManager.DescribeSortCriteria().SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == category && x.Type == type);

            var model = new SortCriterionEditViewModel();
            TryUpdateModel(model);

            // validating form values
            _formManager.Validate(new ValidatingContext { FormName = sortCriterion.Form, ModelState = ModelState, ValueProvider = ValueProvider });

            if (ModelState.IsValid) {
                var sortCriterionRecord = query.SortCriteria.FirstOrDefault(f => f.Id == sortCriterionId);

                // add new sort criteria record if it's a newly created one
                if (sortCriterionRecord == null) {
                    sortCriterionRecord = new SortCriterionRecord {
                        Category = category, 
                        Type = type, 
                        Position = query.SortCriteria.Count
                    };
                    query.SortCriteria.Add(sortCriterionRecord);
                }

                var dictionary = formCollection.AllKeys.ToDictionary(key => key, formCollection.Get);

                // save form parameters
                sortCriterionRecord.State = FormParametersHelper.ToString(dictionary);
                sortCriterionRecord.Description = model.Description;

                return RedirectToAction("Edit", "Admin", new { id });
            }

            // model is invalid, display it again
            var form = _formManager.Build(sortCriterion.Form);

            _formManager.Bind(form, formCollection);
            var viewModel = new SortCriterionEditViewModel { Id = id, Description = model.Description, SortCriterion = sortCriterion, Form = form };

            return View(viewModel);
        }


        public ActionResult Move(string direction, int id, int queryId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageQueries, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            switch (direction) {
                case "up": _sortService.MoveUp(id);
                    break;
                case "down": _sortService.MoveDown(id);
                    break;
                default:
                    throw new ArgumentException("direction");
            }

            return RedirectToAction("Edit", "Admin", new { id = queryId });
        }
    }
}