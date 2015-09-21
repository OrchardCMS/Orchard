using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Services;
using Orchard.Localization;
using Orchard.UI.Admin;

namespace Orchard.Layouts.Controllers {
    [Admin]
    public class LayoutController : Controller {
        private readonly IContentManager _contentManager;
        private readonly ILayoutManager _layoutManager;
        private readonly ILayoutModelMapper _mapper;

        public LayoutController(
            IContentManager contentManager, 
            ILayoutManager layoutManager, 
            ILayoutModelMapper mapper,
            IOrchardServices orchardServices) {

            _contentManager = contentManager;
            _layoutManager = layoutManager;
            _mapper = mapper;
            Services = orchardServices;

            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        [HttpPost, ValidateInput(enableValidation: false)]
        public ActionResult ApplyTemplate(int? templateId = null, string layoutData = null, int? contentId = null, string contentType = null) {
            var template = templateId != null ? _layoutManager.GetLayout(templateId.Value) : null;
            var templateElements = template != null ? _layoutManager.LoadElements(template).ToList() : default(IEnumerable<Element>);
            var describeContext = CreateDescribeElementsContext(contentId, contentType);
            var elementInstances = _mapper.ToLayoutModel(layoutData, describeContext).ToList();
            var updatedLayout = templateElements != null
                ? _layoutManager.ApplyTemplate(elementInstances, templateElements)
                : _layoutManager.DetachTemplate(elementInstances);

            var canvas = updatedLayout.Single(x => x is Canvas);
            var editorModel = _mapper.ToEditorModel(canvas, describeContext);
            var json = JsonConvert.SerializeObject(editorModel, Formatting.None);
            return Content(json, "application/json");
        }

        private DescribeElementsContext CreateDescribeElementsContext(int? contentId, string contentType) {
            var content = contentId != null && contentId != 0
                ? _contentManager.Get(contentId.Value, VersionOptions.Latest) ?? _contentManager.New(contentType)
                : _contentManager.New(contentType);

            return new DescribeElementsContext { Content = content };
        }
    }
}