using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Framework.Serialization;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;
using Orchard.Layouts.Settings;
using Orchard.Layouts.ViewModels;
using Orchard.Mvc;
using Orchard.UI.Admin;

namespace Orchard.Layouts.Controllers {
    public class LayoutController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _wca;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly ILayoutManager _layoutManager;
        private readonly ILayoutSerializer _serializer;

        public LayoutController(
            IContentManager contentManager,
            IWorkContextAccessor wca, 
            IShapeDisplay shapeDisplay,
            ILayoutManager layoutManager, 
            ILayoutSerializer serializer) {

            _contentManager = contentManager;
            _wca = wca;
            _shapeDisplay = shapeDisplay;
            _layoutManager = layoutManager;
            _serializer = serializer;
        }

        [Admin]
        public ViewResult Edit(string contentType = null, int? id = null, string state = null) {
            var describeContext = CreateDescribeElementsContext(id, contentType);
            var layoutPart = describeContext.Content.As<LayoutPart>();

            state = !String.IsNullOrWhiteSpace(state) ? state : layoutPart.LayoutState;

            if (id.GetValueOrDefault() == 0 && String.IsNullOrWhiteSpace(state)) {
                var defaultState = layoutPart.TypePartDefinition.Settings.GetModel<LayoutTypePartSettings>().DefaultLayoutState;
                state = !String.IsNullOrWhiteSpace(defaultState) ? defaultState : _serializer.Serialize(_layoutManager.CreateDefaultLayout());
            }

            var viewModel = new LayoutEditorViewModel {
                Templates = _layoutManager.GetTemplates().Where(x => x.Id != layoutPart.Id).ToArray(),
                SelectedTemplateId = layoutPart.TemplateId,
                State = state,
                LayoutRoot = _layoutManager.RenderLayout(state, displayType: "Design", content: layoutPart),
                Content = layoutPart
            };

            var workContext = _wca.GetContext();

            AddThemeStyles(workContext.Layout);

            // Customize the Layout shape.
            workContext.Layout.Metadata.Wrappers.Clear();
            workContext.Layout.Metadata.Wrappers.Add("Layout__Designer__Wrapper");
            workContext.Layout.Metadata.Alternates.Add("Layout__Designer");

            return View(viewModel);
        }

        [HttpPost]
        public ShapeResult ApplyTemplate(int? templateId = null, string layoutState = null, int? layoutId = null, string contentType = null) {
            var layoutPart = layoutId != null ? _layoutManager.GetLayout(layoutId.Value) ?? _contentManager.New<LayoutPart>(contentType) : _contentManager.New<LayoutPart>(contentType);

            if (!String.IsNullOrWhiteSpace(layoutState)) {
                layoutState = ApplyTemplateInternal(templateId, layoutState, layoutId, contentType);
            }

            var layoutShape = _layoutManager.RenderLayout(state: layoutState, displayType: "Design", content: layoutPart);
            return new ShapeResult(this, layoutShape);
        }

        private string ApplyTemplateInternal(int? templateId, string layoutState, int? layoutId = null, string contentType = null) {
            var template = templateId != null ? _layoutManager.GetLayout(templateId.Value) : null;
            var templateElements = template != null ? _layoutManager.LoadElements(template) : default(IEnumerable<IElement>);
            var describeContext = CreateDescribeElementsContext(layoutId, contentType);
            var elementInstances = _serializer.Deserialize(layoutState, describeContext);

            if (templateElements == null)
                return _layoutManager.DetachTemplate(elementInstances);
            return _layoutManager.ApplyTemplate(elementInstances, templateElements);
        }

        private void AddThemeStyles(dynamic layout) {
            // Rendering the layout shape will cause styles to be registered.
            _shapeDisplay.Display(layout);
        }

        private DescribeElementsContext CreateDescribeElementsContext(int? contentId, string contentType) {
            var content = contentId != null && contentId != 0
                ? _contentManager.Get(contentId.Value, VersionOptions.Latest) ?? _contentManager.New(contentType)
                : _contentManager.New(contentType);

            return new DescribeElementsContext { Content = content };
        }
    }
}