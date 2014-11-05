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
            ISignals signals) {

            _elementBlueprintService = elementBlueprintService;
            _notifier = notifier;
            _elementManager = elementManager;
            _cultureAccessor = cultureAccessor;
            _shapeFactory = shapeFactory;
            _transactionManager = transactionManager;
            _signals = signals;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var blueprints = _elementBlueprintService.GetBlueprints().ToArray();
            var viewModel = new BlueprintsIndexViewModel {
                Blueprints = blueprints
            };
            return View(viewModel);
        }

        public ActionResult Browse() {
            var categories = _elementManager.GetCategories(DescribeElementsContext.Empty).ToArray();
            var viewModel = new BrowseElementsViewModel {
                Categories = categories
            };
            return View(viewModel);
        }

        public ActionResult Create(string id) {
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
                model.ElementCategory.TrimSafe());

            return RedirectToAction("Edit", new { id = blueprint.Id });
        }

        public ViewResult Edit(int id) {
            var blueprint = _elementBlueprintService.GetBlueprint(id);
            var describeContext = DescribeElementsContext.Empty;
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, blueprint.BaseElementTypeName);
            var state = ElementStateHelper.Deserialize(blueprint.BaseElementState);
            var element = _elementManager.ActivateElement(descriptor, new ActivateElementArgs { ElementState = state });
            var context = CreateEditorContext(element, state);
            var editorResult = _elementManager.BuildEditor(context);

            var viewModel = new EditElementBlueprintViewModel {
                EditorResult = editorResult,
                TypeName = blueprint.BaseElementTypeName,
                DisplayText = descriptor.DisplayText,
                ElementState = element.State.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray(),
                BaseElement = element
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int id, ElementStateViewModel model) {
            var blueprint = _elementBlueprintService.GetBlueprint(id);
            var describeContext = DescribeElementsContext.Empty;
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, blueprint.BaseElementTypeName);
            var state = ElementStateHelper.Deserialize(model.ElementState).Combine(Request.Form.ToDictionary());
            var element = _elementManager.ActivateElement(descriptor, new ActivateElementArgs { ElementState = state });
            var context = CreateEditorContext(element, elementState: state);
            var editorResult = _elementManager.UpdateEditor(context);
            var viewModel = new EditElementBlueprintViewModel {
                EditorResult = editorResult,
                TypeName = model.TypeName,
                DisplayText = descriptor.DisplayText,
                ElementState = element.State.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray(),
                BaseElement = element
            };

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(viewModel);
            }

            blueprint.BaseElementState = state.Serialize();
            _signals.Trigger(Signals.ElementDescriptors);
            _notifier.Information(T("That blueprint has been saved."));
            return RedirectToAction("Index");
        }

        public ActionResult Properties(int id) {
            var blueprint = _elementBlueprintService.GetBlueprint(id);
            var describeContext = DescribeElementsContext.Empty;
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, blueprint.BaseElementTypeName);
            var baseElement = _elementManager.ActivateElement(descriptor);
            var viewModel = new ElementBlueprintPropertiesViewModel {
                BaseElement = baseElement,
                ElementDisplayName = blueprint.ElementDisplayName,
                ElementTypeName = blueprint.ElementTypeName,
                ElementCategory = blueprint.ElementCategory
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Properties(int id, ElementBlueprintPropertiesViewModel model) {
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
            blueprint.ElementCategory = model.ElementCategory.TrimSafe();

            _notifier.Information(T("That blueprint's properties have been saved."));
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id) {
            var blueprint = _elementBlueprintService.GetBlueprint(id);

            if (blueprint == null)
                return HttpNotFound();

            _elementBlueprintService.DeleteBlueprint(blueprint);
            _notifier.Information(T("That blueprint has been deleted."));
            return Redirect(Request.UrlReferrer.ToString());
        }

        [FormValueRequired("submit.BulkEdit")]
        [ActionName("Index")]
        public ActionResult BulkDelete(IEnumerable<int> blueprintIds) {
            if (blueprintIds == null || !blueprintIds.Any()) {
                _notifier.Error(T("Please select the blueprints to delete."));
            }
            else {
                var numDeletedBlueprints = _elementBlueprintService.DeleteBlueprints(blueprintIds);
                _notifier.Information(T("{0} blueprints have been deleted.", numDeletedBlueprints));
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        private ElementEditorContext CreateEditorContext(IElement element, StateDictionary elementState = null) {
            elementState = elementState ?? new StateDictionary();
            var context = new ElementEditorContext {
                Element = element,
                Updater = this,
                ValueProvider = new DictionaryValueProvider<string>(elementState, _cultureAccessor.CurrentCulture),
                ShapeFactory = _shapeFactory
            };
            ValueProvider = context.ValueProvider;
            return context;
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}