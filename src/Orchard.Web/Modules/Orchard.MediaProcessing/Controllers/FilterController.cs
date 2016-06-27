using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.Caching;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.MediaProcessing.Models;
using Orchard.MediaProcessing.Services;
using Orchard.MediaProcessing.ViewModels;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using FormParametersHelper = Orchard.Forms.Services.FormParametersHelper;

namespace Orchard.MediaProcessing.Controllers {
    [ValidateInput(false), Admin]
    public class FilterController : Controller {
        public FilterController(
            IOrchardServices services,
            IFormManager formManager,
            IShapeFactory shapeFactory,
            IImageProcessingManager processingManager,
            IRepository<FilterRecord> repository,
            IImageProfileService profileService,
            ISignals signals) {
            Services = services;
            _formManager = formManager;
            _processingManager = processingManager;
            _filterRepository = repository;
            _profileService = profileService;
            _signals = signals;
            Shape = shapeFactory;
        }

        public IOrchardServices Services { get; set; }
        private readonly IFormManager _formManager;
        private readonly IImageProcessingManager _processingManager;
        private readonly IRepository<FilterRecord> _filterRepository;
        private readonly IImageProfileService _profileService;
        private readonly ISignals _signals;
        public Localizer T { get; set; }
        public dynamic Shape { get; set; }

        public ActionResult Add(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage image profiles")))
                return new HttpUnauthorizedResult();

            var viewModel = new FilterAddViewModel {Id = id, Filters = _processingManager.DescribeFilters()};
            return View(viewModel);
        }

        public ActionResult Delete(int id, int filterId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage image profiles")))
                return new HttpUnauthorizedResult();

            var filter = _filterRepository.Get(filterId);
            if (filter == null) {
                return HttpNotFound();
            }

            filter.ImageProfilePartRecord.Filters.Remove(filter);
            _filterRepository.Delete(filter);

            _signals.Trigger("MediaProcessing_Saved_" + filter.ImageProfilePartRecord.Name);

            Services.Notifier.Information(T("Filter deleted"));

            return RedirectToAction("Edit", "Admin", new {id});
        }

        public ActionResult Edit(int id, string category, string type, int filterId = -1) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage image profiles")))
                return new HttpUnauthorizedResult();

            var filter = _processingManager.DescribeFilters().SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == category && x.Type == type);

            if (filter == null) {
                return HttpNotFound();
            }

            // build the form, and let external components alter it
            var form = filter.Form == null ? null : _formManager.Build(filter.Form);

            string description = "";

            // bind form with existing values).
            if (filterId != -1) {
                var profile = _profileService.GetImageProfile(id);
                var filterRecord = profile.Filters.FirstOrDefault(f => f.Id == filterId);
                if (filterRecord != null) {
                    description = filterRecord.Description;
                    var parameters = FormParametersHelper.FromString(filterRecord.State);
                    _formManager.Bind(form, new DictionaryValueProvider<string>(parameters, CultureInfo.InvariantCulture));
                }
            }

            var viewModel = new FilterEditViewModel {Id = id, Description = description, Filter = filter, Form = form};
            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id, string category, string type, [DefaultValue(-1)] int filterId, FormCollection formCollection) {
            var profile = _profileService.GetImageProfile(id);

            var filter = _processingManager.DescribeFilters().SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == category && x.Type == type);

            var model = new FilterEditViewModel();
            TryUpdateModel(model);

            // validating form values
            _formManager.Validate(new ValidatingContext {FormName = filter.Form, ModelState = ModelState, ValueProvider = ValueProvider});

            if (ModelState.IsValid) {
                var filterRecord = profile.Filters.FirstOrDefault(f => f.Id == filterId);

                // add new filter record if it's a newly created filter
                if (filterRecord == null) {
                    filterRecord = new FilterRecord {
                        Category = category,
                        Type = type,
                        Position = profile.Filters.Count
                    };
                    profile.Filters.Add(filterRecord);
                }

                var dictionary = formCollection.AllKeys.ToDictionary(key => key, formCollection.Get);

                // save form parameters
                filterRecord.State = FormParametersHelper.ToString(dictionary);
                filterRecord.Description = model.Description;

                // set profile as updated
                profile.ModifiedUtc = DateTime.UtcNow;
                profile.FileNames.Clear();
                _signals.Trigger("MediaProcessing_Saved_" + profile.Name);

                return RedirectToAction("Edit", "Admin", new {id});
            }

            // model is invalid, display it again
            var form = _formManager.Build(filter.Form);

            _formManager.Bind(form, formCollection);
            var viewModel = new FilterEditViewModel {Id = id, Description = model.Description, Filter = filter, Form = form};

            return View(viewModel);
        }
    }
}