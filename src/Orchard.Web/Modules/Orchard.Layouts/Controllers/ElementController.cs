using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Serialization;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;
using Orchard.Layouts.ViewModels;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.Layouts.Controllers {
    public class ElementController : Controller, IUpdateModel {
        private readonly IElementDisplay _elementDisplay;
        private readonly ILayoutSerializer _layoutSerializer;
        private readonly IElementManager _elementManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IWorkContextAccessor _wca;
        private readonly ITransactionManager _transactionManager;
        private readonly ICultureAccessor _cultureAccessor;
        private readonly IContentManager _contentManager;

        public ElementController(
            IElementDisplay elementDisplay,
            ILayoutSerializer layoutSerializer,
            IElementManager elementManager,
            IShapeFactory shapeFactory,
            IWorkContextAccessor wca,
            ITransactionManager transactionManager,
            ICultureAccessor cultureAccessor,
            IContentManager contentManager) {

            _elementDisplay = elementDisplay;
            _layoutSerializer = layoutSerializer;
            _elementManager = elementManager;
            _shapeFactory = shapeFactory;
            _wca = wca;
            _transactionManager = transactionManager;
            _cultureAccessor = cultureAccessor;
            _contentManager = contentManager;
        }

        [Admin]
        public ViewResult Browse(int? layoutId = null, string contentType = null) {
            var context = CreateDescribeContext(layoutId, contentType);
            var categories = _elementManager.GetCategories(context).ToArray();
            var viewModel = new BrowseElementsViewModel {
                Categories = categories,
                LayoutId = layoutId,
                ContentType = contentType
            };
            return View(viewModel);
        }

        [HttpPost]
        [Themed(false)]
        [ValidateInput(false)]
        public ShapeResult Render(string graph, string displayType, int? layoutId = null, string contentType = null, string renderEventName = null, string renderEventArgs = null) {
            var context = CreateDescribeContext(layoutId, contentType);
            var instances = _layoutSerializer.Deserialize(graph, context);
            var shape = _elementDisplay.DisplayElements(instances, context.Content, displayType: displayType, updater: this, renderEventName: renderEventName, renderEventArgs: renderEventArgs);
            return new ShapeResult(this, shape);
        }

        [Admin]
        public ViewResult Create(string id, int? layoutId = null, string contentType = null) {
            var describeContext = CreateDescribeContext(layoutId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, id);
            var element = _elementManager.ActivateElement(descriptor);
            var context = CreateEditorContext(describeContext.Content, element);
            var editorResult = _elementManager.BuildEditor(context);
            var viewModel = new EditElementViewModel {
                Layout = describeContext.Content.As<ILayoutAspect>(),
                EditorResult = editorResult,
                TypeName = id,
                DisplayText = descriptor.DisplayText,
                ElementState = element.State.Serialize(), 
                Submitted = !descriptor.EnableEditorDialog,
                Tabs = editorResult.CollectTabs().ToArray()
            };

            return View(viewModel);
        }

        [Admin]
        [HttpPost]
        [ValidateInput(false)]
        public ViewResult Create(ElementStateViewModel model, int? layoutId = null, string contentType = null) {
            var describeContext = CreateDescribeContext(layoutId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, model.TypeName);
            var state = ElementStateHelper.Deserialize(model.ElementState).Combine(Request.Form.ToDictionary());
            var element = _elementManager.ActivateElement(descriptor, new ActivateElementArgs { ElementState = state });
            var context = CreateEditorContext(describeContext.Content, element, elementState: state);
            var editorResult = _elementManager.UpdateEditor(context);
            var viewModel = new EditElementViewModel {
                Layout = describeContext.Content.As<ILayoutAspect>(),
                EditorResult = editorResult,
                TypeName = model.TypeName,
                DisplayText = descriptor.DisplayText,
                ElementState = element.State.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray()
            };

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
            }
            else {
                viewModel.Submitted = true;
            }
            return View(viewModel);
        }

        [Admin]
        [HttpPost]
        [ValidateInput(false)]
        public ViewResult Edit(string typeName, string elementState, int? layoutId = null, string contentType = null) {
            var describeContext = CreateDescribeContext(layoutId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, typeName);
            var state = ElementStateHelper.Deserialize(elementState);
            var element = _elementManager.ActivateElement(descriptor, new ActivateElementArgs { ElementState = state });
            var context = CreateEditorContext(describeContext.Content, element, elementState: state);
            var editorResult = _elementManager.BuildEditor(context);

            var viewModel = new EditElementViewModel {
                Layout = describeContext.Content.As<ILayoutAspect>(),
                EditorResult = editorResult,
                TypeName = typeName,
                DisplayText = descriptor.DisplayText,
                ElementState = element.State.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray()
            };

            return View(viewModel);
        }

        [Admin]
        [HttpPost]
        [ValidateInput(false)]
        public ViewResult Update(ElementStateViewModel model, int? layoutId = null, string contentType = null) {
            var describeContext = CreateDescribeContext(layoutId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, model.TypeName);
            var state = ElementStateHelper.Deserialize(model.ElementState).Combine(Request.Form.ToDictionary(), removeNonExistingItems: true);
            var element = _elementManager.ActivateElement(descriptor, new ActivateElementArgs { ElementState = state });
            var context = CreateEditorContext(describeContext.Content, element, state);
            var editorResult = _elementManager.UpdateEditor(context);
            var viewModel = new EditElementViewModel {
                Layout = describeContext.Content.As<ILayoutAspect>(),
                EditorResult = editorResult,
                TypeName = model.TypeName,
                DisplayText = descriptor.DisplayText,
                ElementState = element.State.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray()
            };

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
            }
            else {
                viewModel.Submitted = true;
            }
            return View("Edit", viewModel);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext) {
            var workContext = _wca.GetContext();
            workContext.Layout.Metadata.Alternates.Add("Layout__Dialog");
        }

        private ElementEditorContext CreateEditorContext(
            IContent content, 
            IElement element, 
            StateDictionary elementState = null) {

            elementState = elementState ?? new StateDictionary();
            var context = new ElementEditorContext {
                Content = content,
                Element = element,
                Updater = this,
                ValueProvider = new DictionaryValueProvider<string>(elementState, _cultureAccessor.CurrentCulture),
                ShapeFactory = _shapeFactory
            };
            ValueProvider = context.ValueProvider;
            return context;
        }

        private DescribeElementsContext CreateDescribeContext(int? layoutId, string contentType) {
            var part = layoutId != null && layoutId != 0 ? _contentManager.Get<ILayoutAspect>(layoutId.Value)
                ?? _contentManager.New<ILayoutAspect>(contentType)
                : _contentManager.New<ILayoutAspect>(contentType);

            return new DescribeElementsContext {
                Content = part
            };
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}