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
        private readonly ITransactionManager _transactionManager;
        private readonly ICultureAccessor _cultureAccessor;
        private readonly IContentManager _contentManager;
        private readonly IObjectStore _objectStore;

        public ElementController(
            IElementDisplay elementDisplay,
            ILayoutSerializer layoutSerializer,
            IElementManager elementManager,
            IShapeFactory shapeFactory,
            ITransactionManager transactionManager,
            ICultureAccessor cultureAccessor,
            IContentManager contentManager, IObjectStore objectStore) {

            _elementDisplay = elementDisplay;
            _layoutSerializer = layoutSerializer;
            _elementManager = elementManager;
            _shapeFactory = shapeFactory;
            _transactionManager = transactionManager;
            _cultureAccessor = cultureAccessor;
            _contentManager = contentManager;
            _objectStore = objectStore;
        }

        [Admin]
        public ViewResult Browse(string session, int? layoutId = null, string contentType = null) {
            var context = CreateDescribeContext(layoutId, contentType);
            var categories = _elementManager.GetCategories(context).ToArray();
            var viewModel = new BrowseElementsViewModel {
                Categories = categories,
                ContentId = layoutId,
                ContentType = contentType,
                Session = session
            };
            return View(viewModel);
        }

        [HttpPost]
        [Themed(false)]
        [ValidateInput(false)]
        public ShapeResult Render(string graph, string displayType, int? contentId = null, string contentType = null, string renderEventName = null, string renderEventArgs = null) {
            var context = CreateDescribeContext(contentId, contentType);
            var instances = _layoutSerializer.Deserialize(graph, context);
            var shape = _elementDisplay.DisplayElements(instances, context.Content, displayType: displayType, updater: this, renderEventName: renderEventName, renderEventArgs: renderEventArgs);
            return new ShapeResult(this, shape);
        }

        [Admin]
        public ViewResult Create(string id, string session, int? contentId = null, string contentType = null) {
            var sessionState = new ElementSessionState {
                TypeName = id,
                State = null,
                ContentId = contentId,
                ContentType = contentType
            };

            _objectStore.Set(session, sessionState);

            var describeContext = CreateDescribeContext(contentId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, id);
            var element = _elementManager.ActivateElement(descriptor);
            var context = CreateEditorContext(session, describeContext.Content, element);
            var editorResult = _elementManager.BuildEditor(context);
            var viewModel = new EditElementViewModel {
                SessionKey = session,
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
        public ViewResult Create(ElementStateViewModel model, string session) {
            var sessionState = _objectStore.Get<ElementSessionState>(session);
            var contentId = sessionState.ContentId;
            var contentType = sessionState.ContentType;
            var describeContext = CreateDescribeContext(contentId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, model.TypeName);
            var state = ElementStateHelper.Deserialize(model.ElementState).Combine(Request.Form.ToDictionary());
            var element = _elementManager.ActivateElement(descriptor, new ActivateElementArgs { State = state });
            var context = CreateEditorContext(session, describeContext.Content, element, elementState: state, updater: this);
            var editorResult = _elementManager.UpdateEditor(context);
            var viewModel = new EditElementViewModel {
                SessionKey = session,
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
        public RedirectToRouteResult Edit(string session, string typeName, string elementState, int? contentId = null, string contentType = null) {
            var state = new ElementSessionState {
                TypeName = typeName,
                State = elementState,
                ContentId = contentId,
                ContentType = contentType
            };

            _objectStore.Set(session, state);
            return RedirectToAction("Edit", new {session = session});
        }

        [Admin]
        public ViewResult Edit(string session) {
            var sessionState = _objectStore.Get<ElementSessionState>(session);
            var contentId = sessionState.ContentId;
            var contentType = sessionState.ContentType;
            var typeName = sessionState.TypeName;
            var elementState = sessionState.State;
            var describeContext = CreateDescribeContext(contentId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, typeName);
            var state = ElementStateHelper.Deserialize(elementState);
            var element = _elementManager.ActivateElement(descriptor, new ActivateElementArgs { State = state });
            var context = CreateEditorContext(session, describeContext.Content, element, elementState: state);
            var editorResult = _elementManager.BuildEditor(context);

            var viewModel = new EditElementViewModel {
                Layout = describeContext.Content.As<ILayoutAspect>(),
                EditorResult = editorResult,
                TypeName = typeName,
                DisplayText = descriptor.DisplayText,
                ElementState = element.State.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray(),
                SessionKey = session
            };

            return View(viewModel);
        }

        [Admin]
        [HttpPost]
        [ValidateInput(false)]
        public ViewResult Update(ElementStateViewModel model, string session) {
            var sessionState = _objectStore.Get<ElementSessionState>(session);
            var contentId = sessionState.ContentId;
            var contentType = sessionState.ContentType;
            var describeContext = CreateDescribeContext(contentId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, model.TypeName);
            var state = ElementStateHelper.Deserialize(model.ElementState).Combine(Request.Form.ToDictionary());
            var element = _elementManager.ActivateElement(descriptor, new ActivateElementArgs { State = state });
            var context = CreateEditorContext(session, describeContext.Content, element, state, updater: this);
            var editorResult = _elementManager.UpdateEditor(context);
            var viewModel = new EditElementViewModel {
                Layout = describeContext.Content.As<ILayoutAspect>(),
                EditorResult = editorResult,
                TypeName = model.TypeName,
                DisplayText = descriptor.DisplayText,
                ElementState = element.State.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray(),
                SessionKey = session
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
            var workContext = filterContext.GetWorkContext();
            workContext.Layout.Metadata.Alternates.Add("Layout__Dialog");
        }

        private ElementEditorContext CreateEditorContext(
            string session,
            IContent content,
            IElement element,
            StateDictionary elementState = null,
            IUpdateModel updater = null) {

            elementState = elementState ?? new StateDictionary();
            var context = new ElementEditorContext {
                Session = session,
                Content = content,
                Element = element,
                Updater = updater,
                ValueProvider = elementState.ToValueProvider(_cultureAccessor.CurrentCulture),
                ShapeFactory = _shapeFactory
            };
            ValueProvider = context.ValueProvider;
            return context;
        }

        private DescribeElementsContext CreateDescribeContext(int? contentId = null, string contentType = null) {
            if (contentId == null && contentType == null)
                return DescribeElementsContext.Empty;

            var part = contentId != null && contentId != 0 ? _contentManager.Get<ILayoutAspect>(contentId.Value)
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