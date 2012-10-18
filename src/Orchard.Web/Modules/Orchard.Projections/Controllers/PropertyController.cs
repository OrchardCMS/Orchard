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
    public class PropertyController : Controller {
        public IOrchardServices Services { get; set; }
        private readonly IFormManager _formManager;
        private readonly IProjectionManager _projectionManager;
        private readonly IRepository<PropertyRecord> _repository;
        private readonly IRepository<LayoutRecord> _layoutRepository;
        private readonly IPropertyService _propertyService;

        public PropertyController(
            IOrchardServices services,
            IFormManager formManager,
            IShapeFactory shapeFactory,
            IProjectionManager projectionManager,
            IRepository<PropertyRecord> repository,
            IRepository<LayoutRecord> layoutRepository,
            IPropertyService propertyService,
            IQueryService queryService) {
            Services = services;
            _formManager = formManager;
            _projectionManager = projectionManager;
            _repository = repository;
            _layoutRepository = layoutRepository;
            _propertyService = propertyService;
            Shape = shapeFactory;
        }

        public Localizer T { get; set; }
        public dynamic Shape { get; set; }

        public ActionResult Add(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyAddViewModel { Id = id, Properties = _projectionManager.DescribeProperties() };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Delete(int id, int propertyId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var property = _repository.Get(propertyId);
            if(property == null) {
                return HttpNotFound();
            }

            _repository.Delete(property);
            Services.Notifier.Information(T("Property deleted"));

            return RedirectToAction("Edit", "Layout", new { id });
        }

        public ActionResult Edit(int id, string category, string type, int propertyId = -1) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var property = _projectionManager.DescribeProperties().SelectMany(x => x.Descriptors).Where(x => x.Category == category && x.Type == type).FirstOrDefault();

            if (property == null) {
                return HttpNotFound();
            }

            var viewModel = new PropertyEditViewModel {
                Id = id, 
                Description = String.Empty, 
                Property = property
            };

            dynamic form = null;
            // build the form, and let external components alter it
            if (property.Form != null) {
                form = _formManager.Build(property.Form);
                viewModel.Form = form;
            }

            // bind form with existing values.
            if (propertyId != -1) {
                var propertyRecord = _repository.Get(propertyId);
                if (propertyRecord != null) {
                    viewModel.Description = propertyRecord.Description;
                    if (property.Form != null) {
                        var parameters = FormParametersHelper.FromString(propertyRecord.State);
                        _formManager.Bind(form, new DictionaryValueProvider<string>(parameters, CultureInfo.InvariantCulture));
                    }

                    viewModel.CreateLabel = propertyRecord.CreateLabel;
                    viewModel.ExcludeFromDisplay = propertyRecord.ExcludeFromDisplay;
                    viewModel.Label = propertyRecord.Label;
                    viewModel.LinkToContent = propertyRecord.LinkToContent;

                    viewModel.CustomizeLabelHtml = propertyRecord.CustomizeLabelHtml;
                    viewModel.CustomizePropertyHtml = propertyRecord.CustomizePropertyHtml;
                    viewModel.CustomizeWrapperHtml = propertyRecord.CustomizeWrapperHtml;
                    viewModel.CustomLabelCss = propertyRecord.CustomLabelCss;
                    viewModel.CustomLabelTag = propertyRecord.CustomLabelTag;
                    viewModel.CustomPropertyCss = propertyRecord.CustomPropertyCss;
                    viewModel.CustomPropertyTag = propertyRecord.CustomPropertyTag;
                    viewModel.CustomWrapperCss = propertyRecord.CustomWrapperCss;
                    viewModel.CustomWrapperTag = propertyRecord.CustomWrapperTag;

                    viewModel.NoResultText = propertyRecord.NoResultText;
                    viewModel.ZeroIsEmpty = propertyRecord.ZeroIsEmpty;
                    viewModel.HideEmpty = propertyRecord.HideEmpty;

                    viewModel.RewriteOutput = propertyRecord.RewriteOutput;
                    viewModel.RewriteText = propertyRecord.RewriteText;
                    viewModel.StripHtmlTags = propertyRecord.StripHtmlTags;
                    viewModel.TrimLength = propertyRecord.TrimLength;
                    viewModel.AddEllipsis = propertyRecord.AddEllipsis;
                    viewModel.MaxLength = propertyRecord.MaxLength;
                    viewModel.TrimOnWordBoundary = propertyRecord.TrimOnWordBoundary;
                    viewModel.PreserveLines = propertyRecord.PreserveLines;
                    viewModel.TrimWhiteSpace = propertyRecord.TrimWhiteSpace;
                }
            }

            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id, string category, string type, [DefaultValue(-1)]int propertyId, FormCollection formCollection) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();
            var layout = _layoutRepository.Get(id);

            var property = _projectionManager.DescribeProperties().SelectMany(x => x.Descriptors).Where(x => x.Category == category && x.Type == type).FirstOrDefault();

            var model = new PropertyEditViewModel();
            TryUpdateModel(model);

            // validating form values
            _formManager.Validate(new ValidatingContext { FormName = property.Form, ModelState = ModelState, ValueProvider = ValueProvider });

            if (ModelState.IsValid) {
                var propertyRecord = layout.Properties.Where(f => f.Id == propertyId).FirstOrDefault();

                // add new property record if it's a newly created property
                if (propertyRecord == null) {
                    propertyRecord = new PropertyRecord {
                        Category = category, 
                        Type = type, 
                        Position = layout.Properties.Count
                    };
                    layout.Properties.Add(propertyRecord);
                }

                var dictionary = formCollection.AllKeys.ToDictionary(key => key, formCollection.Get);

                // save form parameters
                propertyRecord.State = FormParametersHelper.ToString(dictionary);
                propertyRecord.Description = model.Description;

                propertyRecord.CreateLabel = model.CreateLabel;
                propertyRecord.ExcludeFromDisplay = model.ExcludeFromDisplay;
                propertyRecord.Label = model.Label;
                propertyRecord.LinkToContent = model.LinkToContent;

                propertyRecord.CustomizeLabelHtml = model.CustomizeLabelHtml;
                propertyRecord.CustomizePropertyHtml = model.CustomizePropertyHtml;
                propertyRecord.CustomizeWrapperHtml = model.CustomizeWrapperHtml;
                propertyRecord.CustomLabelCss = model.CustomLabelCss;
                propertyRecord.CustomLabelTag = model.CustomLabelTag;
                propertyRecord.CustomPropertyCss = model.CustomPropertyCss;
                propertyRecord.CustomPropertyTag = model.CustomPropertyTag;
                propertyRecord.CustomWrapperCss = model.CustomWrapperCss;
                propertyRecord.CustomWrapperTag = model.CustomWrapperTag;

                propertyRecord.NoResultText = model.NoResultText;
                propertyRecord.ZeroIsEmpty = model.ZeroIsEmpty;
                propertyRecord.HideEmpty = model.HideEmpty;

                propertyRecord.RewriteOutput = model.RewriteOutput;
                propertyRecord.RewriteText = model.RewriteText;
                propertyRecord.StripHtmlTags = model.StripHtmlTags;
                propertyRecord.TrimLength = model.TrimLength;
                propertyRecord.AddEllipsis = model.AddEllipsis;
                propertyRecord.MaxLength = model.MaxLength;
                propertyRecord.TrimOnWordBoundary = model.TrimOnWordBoundary;
                propertyRecord.PreserveLines = model.PreserveLines;
                propertyRecord.TrimWhiteSpace = model.TrimWhiteSpace;

                return RedirectToAction("Edit", "Layout", new { id });
            }

            // model is invalid, display it again
            var form = _formManager.Build(property.Form);

            _formManager.Bind(form, formCollection);
            var viewModel = new PropertyEditViewModel { Id = id, Description = model.Description, Property = property, Form = form };

            return View(viewModel);
        }

        public ActionResult Move(string direction, int id, int layoutId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            switch (direction) {
                case "up": _propertyService.MoveUp(id);
                    break;
                case "down": _propertyService.MoveDown(id);
                    break;
                default:
                    throw new ArgumentException("direction");
            }

            return RedirectToAction("Edit", "Layout", new { id = layoutId });
        }
    }
}