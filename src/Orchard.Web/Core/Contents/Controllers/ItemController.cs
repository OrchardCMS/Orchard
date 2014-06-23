﻿using System.Threading.Tasks;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Themes;

namespace Orchard.Core.Contents.Controllers {
    [Themed]
    public class ItemController : Controller {
        private readonly IContentManager _contentManager;

        public ItemController(IContentManager contentManager, IShapeFactory shapeFactory, IOrchardServices services) {
            _contentManager = contentManager;
            Shape = shapeFactory;
            Services = services;
            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        // /Contents/Item/Display/72
        public async Task<ActionResult> Display(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Published);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.ViewContent, contentItem, T("Cannot view content"))) {
                return new HttpUnauthorizedResult();
            }

            var model = await _contentManager.BuildDisplayAsync(contentItem);
            return View(model);
        }

        // /Contents/Item/Preview/72
        // /Contents/Item/Preview/72?version=5
        public async Task<ActionResult> Preview(int id, int? version) {
            var versionOptions = VersionOptions.Latest;

            if (version != null)
                versionOptions = VersionOptions.Number((int)version);

            var contentItem = _contentManager.Get(id, versionOptions);
            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.ViewContent, contentItem, T("Cannot preview content"))) {
                return new HttpUnauthorizedResult();
            }

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Cannot preview content"))) {
                return new HttpUnauthorizedResult();
            }

            var model = await _contentManager.BuildDisplayAsync(contentItem);
            return View(model);
        }
    }
}