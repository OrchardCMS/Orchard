using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Layouts.ViewModels;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Layouts.Controllers {
    [Admin]
    public class BlueprintAdminController : Controller, IUpdateModel {
        private readonly IElementBlueprintService _elementBlueprintService;
        private readonly INotifier _notifier;
        private readonly IElementManager _elementManager;
        private readonly ICultureAccessor _cultureAccessor;
        private readonly IShapeFactory _shapeFactory;
        private readonly ITransactionManager _transactionManager;
        private readonly ISignals _signals;

        public BlueprintAdminController(
            IElementBlueprintService elementBlueprintService, 
            INotifier notifier, 
            IElementManager elementManager, 
            ICultureAccessor cultureAccessor, 
            IShapeFactory shapeFactory, 
            ITransactionManager transactionManager, 
            ISignals signals,
            IOrchardServices orchardServices) {

            _elementBlueprintService = elementBlueprintService;
            _notifier = notifier;
            _elementManager = elementManager;
            _cultureAccessor = cultureAccessor;
            _shapeFactory = shapeFactory;
            _transactionManager = transactionManager;
            _signals = signals;
            Services = orchardServices;

            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(Permissions.ManageLayouts, T("Not authorized to manage layouts."))) {
                return new HttpUnauthorizedResult();
            }

            var blueprints = _elementBlueprintService.GetBlueprints().ToArray();
            var viewModel = new BlueprintsIndexViewModel {
                Blueprints = blueprints
            };
            return View(viewModel);
        }

        public ActionResult Browse() {
            if (!Services.Authorizer.Authorize(Permissions.ManageLayouts, T("Not authorized to manage layouts."))) {
                return new HttpUnauthorizedResult();
            }

            var categories = RemoveBlueprints(_elementManager.GetCategories(DescribeElementsContext.Empty)).ToArray();
            var viewModel = new BrowseElementsViewModel {
                Categories = categories
            };
            return View(viewModel);
        }

        public ActionResult Create(string id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageLayouts, T("Not authorized to manage layouts."))) {
                return new HttpUnauthorizedResult();
            }

            if (String.IsNullOrWhiteSpace(id))
                return RedirectToAction("Browse");

            var describeContext = DescribeElementsContext.Empty;
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, id);
            var baseElement = _elementManager.ActivateElement(descriptor);
            var viewModel = new CreateElementBlueprintViewModel {
                BaseElement = baseElement
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Create(string id, CreateElementBlueprintViewModel model) {
            if (!Services.Authorizer.Authorize(Permissions.ManageLayouts, T("Not authorized to manage layouts."))) {
                return new HttpUnauthorizedResult();
            }

            var describeContext = DescribeElementsContext.Empty;
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, id);
            var baseElement = _elementManager.ActivateElement(descriptor);

            model.BaseElement = baseElement;

            if (!ModelState.IsValid) {
                return View(model);
            }

            var blueprint = _elementBlueprintService.CreateBlueprint(
                baseElement, 
                model.ElementTypeName.TrimSafe(),
                model.ElementDisplayName.TrimSafe(),
                model.ElementDescription.TrimSafe(),
                model.ElementCategory.TrimSafe());

            return RedirectToAction("Edit", new { id = blueprint.Id });
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageLayouts, T("Not authorized to manage layouts."))) {
                return new HttpUnauthorizedResult();
            }

            var blueprint = _elementBlueprintService.GetBlueprint(id);
            var describeContext = DescribeElementsContext.Empty;
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, blueprint.BaseElementTypeName);
            var data = ElementDataHelper.Deserialize(blueprint.BaseElementState);
            var element = _elementManager.ActivateElement(descriptor, e => e.Data = data);
            var context = CreateEditorContext(element, data);
            var editorResult = _elementManager.BuildEditor(context);

            var viewModel = new EditElementBlueprintViewModel {
                EditorResult = editorResult,
                TypeName = blueprint.BaseElementTypeName,
                DisplayText = descriptor.DisplayText,
                Description = descriptor.Description,
                ElementData = element.Data.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray(),
                BaseElement = element
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int id, ElementDataViewModel model) {
            if (!Services.Authorizer.Authorize(Permissions.ManageLayouts, T("Not authorized to manage layouts."))) {
                return new HttpUnauthorizedResult();
            }

            var blueprint = _elementBlueprintService.GetBlueprint(id);
            var describeContext = DescribeElementsContext.Empty;
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, blueprint.BaseElementTypeName);
            var data = ElementDataHelper.Deserialize(model.ElementData).Combine(Request.Form.ToDictionary());
            var element = _elementManager.ActivateElement(descriptor, e => e.Data = data);
            var context = CreateEditorContext(element, elementData: data);
            var editorResult = _elementManager.UpdateEditor(context);
            var viewModel = new EditElementBlueprintViewModel {
                EditorResult = editorResult,
                TypeName = model.TypeName,
                DisplayText = descriptor.DisplayText,
                Description = descriptor.Description,
                ElementData = element.Data.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray(),
                BaseElement = element
            };

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(viewModel);
            }

            blueprint.BaseElementState = data.Serialize();
            _signals.Trigger(Signals.ElementDescriptors);
            _notifier.Success(T("That blueprint has been saved."));
            return RedirectToAction("Index");
        }

        public ActionResult Properties(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageLayouts, T("Not authorized to manage layouts."))) {
                return new HttpUnauthorizedResult();
            }

            var blueprint = _elementBlueprintService.GetBlueprint(id);
            var describeContext = DescribeElementsContext.Empty;
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, blueprint.BaseElementTypeName);
            var baseElement = _elementManager.ActivateElement(descriptor);
            var viewModel = new ElementBlueprintPropertiesViewModel {
                BaseElement = baseElement,
                ElementDisplayName = blueprint.ElementDisplayName,
                ElementDescription = blueprint.ElementDescription,
                ElementTypeName = blueprint.ElementTypeName,
                ElementCategory = blueprint.ElementCategory
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Properties(int id, ElementBlueprintPropertiesViewModel model) {
            if (!Services.Authorizer.Authorize(Permissions.ManageLayouts, T("Not authorized to manage layouts."))) {
                return new HttpUnauthorizedResult();
            }

            var blueprint = _elementBlueprintService.GetBlueprint(id);
            var describeContext = DescribeElementsContext.Empty;
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, blueprint.BaseElementTypeName);
            var baseElement = _elementManager.ActivateElement(descriptor);

            model.BaseElement = baseElement;
            model.ElementTypeName = blueprint.ElementTypeName;

            if (!ModelState.IsValid) {
                return View(model);
            }

            blueprint.ElementDisplayName = model.ElementDisplayName.TrimSafe();
            blueprint.ElementDescription = model.ElementDescription.TrimSafe();
            blueprint.ElementCategory = model.ElementCategory.TrimSafe();

            _notifier.Success(T("That blueprint's properties have been saved."));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageLayouts, T("Not authorized to manage layouts."))) {
                return new HttpUnauthorizedResult();
            }

            var blueprint = _elementBlueprintService.GetBlueprint(id);

            if (blueprint == null)
                return HttpNotFound();

            _elementBlueprintService.DeleteBlueprint(blueprint);
            _notifier.Success(T("That blueprint has been deleted."));
            return Redirect(Request.UrlReferrer.ToString());
        }

        [FormValueRequired("submit.BulkEdit")]
        [ActionName("Index")]
        [HttpPost]
        public ActionResult BulkDelete(IEnumerable<int> blueprintIds) {
            if (!Services.Authorizer.Authorize(Permissions.ManageLayouts, T("Not authorized to manage layouts."))) {
                return new HttpUnauthorizedResult();
            }

            if (blueprintIds == null || !blueprintIds.Any()) {
                _notifier.Error(T("Please select the blueprints to delete."));
            }
            else {
                var numDeletedBlueprints = _elementBlueprintService.DeleteBlueprints(blueprintIds);
                _notifier.Success(T("{0} blueprints have been deleted.", numDeletedBlueprints));
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        private ElementEditorContext CreateEditorContext(Element element, ElementDataDictionary elementData = null) {
            elementData = elementData ?? new ElementDataDictionary();
            var context = new ElementEditorContext {
                Element = element,
                Updater = this,
                ValueProvider = elementData.ToValueProvider(_cultureAccessor.CurrentCulture),
                ShapeFactory = _shapeFactory
            };
            ValueProvider = context.ValueProvider;
            return context;
        }

        private IEnumerable<CategoryDescriptor> RemoveBlueprints(IEnumerable<CategoryDescriptor> categories) {
            foreach (var descriptor in categories) {
                var d = new CategoryDescriptor(descriptor.Name, descriptor.DisplayName, descriptor.Description, descriptor.Position);

                foreach (var element in descriptor.Elements) {
                    if (!element.StateBag.ContainsKey("Blueprint")) {
                        d.Elements.Add(element);
                    }
                }

                if(d.Elements.Any())
                    yield return d;
            }
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}