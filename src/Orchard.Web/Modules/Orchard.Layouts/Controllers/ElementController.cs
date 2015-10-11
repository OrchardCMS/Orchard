using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;
using Orchard.Layouts.ViewModels;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Controllers {
    [Admin]
    public class ElementController : Controller, IUpdateModel {
        private readonly IElementDisplay _elementDisplay;
        private readonly IElementManager _elementManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly ITransactionManager _transactionManager;
        private readonly ICultureAccessor _cultureAccessor;
        private readonly IContentManager _contentManager;
        private readonly IObjectStore _objectStore;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly ILayoutModelMapper _mapper;

        public ElementController(
            IElementDisplay elementDisplay,
            IElementManager elementManager,
            IShapeFactory shapeFactory,
            ITransactionManager transactionManager,
            ICultureAccessor cultureAccessor,
            IContentManager contentManager, 
            IObjectStore objectStore, 
            IShapeDisplay shapeDisplay,
            ILayoutModelMapper mapper) {

            _elementDisplay = elementDisplay;
            _elementManager = elementManager;
            _shapeFactory = shapeFactory;
            _transactionManager = transactionManager;
            _cultureAccessor = cultureAccessor;
            _contentManager = contentManager;
            _objectStore = objectStore;
            _shapeDisplay = shapeDisplay;
            _mapper = mapper;
        }

        [HttpPost]
        public JsonResult CreateDirect(string typeName, int? contentId = null, string contentType = null) {
            var describeContext = CreateDescribeContext(contentId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, typeName);
            var element = _elementManager.ActivateElement(descriptor);

            var dto = new {
                typeName = typeName,
                typeLabel = descriptor.DisplayText.Text,
                typeClass = descriptor.DisplayText.Text.HtmlClassify(),
                data = element.Data.Serialize(),
                html = RenderElement(element, describeContext)
            };

            return Json(dto);
        }

        public ViewResult Create(string id, string session, int? contentId = null, string contentType = null) {
            var sessionState = new ElementSessionState {
                TypeName = id,
                ElementData = null,
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
                ElementData = element.Data.Serialize(), 
                Submitted = !descriptor.EnableEditorDialog,
                ElementHtml = RenderElement(element, describeContext),
                Tabs = editorResult.CollectTabs().ToArray()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ViewResult Create(ElementDataViewModel model, string session) {
            var sessionState = _objectStore.Get<ElementSessionState>(session);
            var contentId = sessionState.ContentId;
            var contentType = sessionState.ContentType;
            var describeContext = CreateDescribeContext(contentId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, model.TypeName);
            var data = Request.Form.ToDictionary();
            var element = _elementManager.ActivateElement(descriptor, e => e.Data = data);
            var context = CreateEditorContext(session, describeContext.Content, element, postedElementData: data, updater: this);
            var editorResult = _elementManager.UpdateEditor(context);
            var viewModel = new EditElementViewModel {
                SessionKey = session,
                Layout = describeContext.Content.As<ILayoutAspect>(),
                EditorResult = editorResult,
                TypeName = model.TypeName,
                DisplayText = descriptor.DisplayText,
                ElementData = element.Data.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray(),
            };

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
            }
            else {
                viewModel.Submitted = true;
                viewModel.ElementEditorModel = _mapper.ToEditorModel(element, describeContext);
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public RedirectToRouteResult Edit(string session, string typeName, string elementData, string elementEditorData, int? contentId = null, string contentType = null) {
            var state = new ElementSessionState {
                TypeName = typeName,
                ElementData = elementData,
                ElementEditorData = elementEditorData,
                ContentId = contentId,
                ContentType = contentType
            };

            _objectStore.Set(session, state);
            return RedirectToAction("Edit", new {session = session});
        }
        
        public ViewResult Edit(string session) {
            var sessionState = _objectStore.Get<ElementSessionState>(session);
            var contentId = sessionState.ContentId;
            var contentType = sessionState.ContentType;
            var typeName = sessionState.TypeName;
            var elementData = ElementDataHelper.Deserialize(sessionState.ElementData);
            var describeContext = CreateDescribeContext(contentId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, typeName);
            var data = elementData.Combine(ElementDataHelper.Deserialize(sessionState.ElementEditorData));
            var element = _elementManager.ActivateElement(descriptor, e => e.Data = data);
            var context = CreateEditorContext(session, describeContext.Content, element, postedElementData: data, elementData: elementData);
            var editorResult = _elementManager.BuildEditor(context);

            var viewModel = new EditElementViewModel {
                Layout = describeContext.Content.As<ILayoutAspect>(),
                EditorResult = editorResult,
                TypeName = typeName,
                DisplayText = descriptor.DisplayText,
                ElementData = element.Data.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray(),
                SessionKey = session,
                Submitted = !descriptor.EnableEditorDialog,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ViewResult Update(ElementDataViewModel model, string session) {
            var sessionState = _objectStore.Get<ElementSessionState>(session);
            var contentId = sessionState.ContentId;
            var contentType = sessionState.ContentType;
            var describeContext = CreateDescribeContext(contentId, contentType);
            var descriptor = _elementManager.GetElementDescriptorByTypeName(describeContext, model.TypeName);
            var elementData = ElementDataHelper.Deserialize(sessionState.ElementData);
            var data = Request.Form.ToDictionary();
            var element = _elementManager.ActivateElement(descriptor, e => e.Data = data);
            var context = CreateEditorContext(session, describeContext.Content, element, postedElementData: data, elementData: elementData, updater: this);
            var editorResult = _elementManager.UpdateEditor(context);
            var viewModel = new EditElementViewModel {
                Layout = describeContext.Content.As<ILayoutAspect>(),
                EditorResult = editorResult,
                TypeName = model.TypeName,
                DisplayText = descriptor.DisplayText,
                ElementData = element.Data.Serialize(),
                Tabs = editorResult.CollectTabs().ToArray(),
                SessionKey = session
            };

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
            }
            else {
                viewModel.ElementHtml = RenderElement(element, describeContext);
                viewModel.Submitted = true;
                viewModel.ElementEditorModel = _mapper.ToEditorModel(element, describeContext);
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
            Element element,
            ElementDataDictionary elementData = null,
            ElementDataDictionary postedElementData = null,
            IUpdateModel updater = null) {

            postedElementData = postedElementData ?? new ElementDataDictionary();
            var context = new ElementEditorContext {
                Session = session,
                Content = content,
                Element = element,
                Updater = updater,
                ElementData = elementData ?? new ElementDataDictionary(),
                ValueProvider = postedElementData.ToValueProvider(_cultureAccessor.CurrentCulture),
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

        private string RenderElement(Element element, DescribeElementsContext describeContext, string displayType = "Design") {
            return _shapeDisplay.Display(_elementDisplay.DisplayElement(element, describeContext.Content, displayType));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}