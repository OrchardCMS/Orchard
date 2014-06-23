﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Themes.Services;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Controllers {

    [ValidateInput(false), Admin]
    public class AdminController : Controller, IUpdateModel {

        private readonly IWidgetsService _widgetsService;
        private readonly ISiteThemeService _siteThemeService;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public AdminController(
            IOrchardServices services,
            IWidgetsService widgetsService,
            IShapeFactory shapeFactory,
            ISiteThemeService siteThemeService,
            IVirtualPathProvider virtualPathProvider) {

            Services = services;
            _widgetsService = widgetsService;
            _siteThemeService = siteThemeService;
            _virtualPathProvider = virtualPathProvider;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic Shape { get; set; }

        public ActionResult Index(int? layerId) {
            ExtensionDescriptor currentTheme = _siteThemeService.GetSiteTheme();
            if (currentTheme == null) {
                Services.Notifier.Error(T("To manage widgets you must have a theme enabled."));
                return RedirectToAction("Index", "Admin", new { area = "Dashboard" });
            }

            IEnumerable<LayerPart> layers = _widgetsService.GetLayers().ToList();

            if (!layers.Any()) {
                Services.Notifier.Error(T("There are no widget layers defined. A layer will need to be added in order to add widgets to any part of the site."));
                return RedirectToAction("AddLayer");
            }

            LayerPart currentLayer = layerId == null
                ? layers.FirstOrDefault()
                : layers.FirstOrDefault(layer => layer.Id == layerId);

            if (currentLayer == null && layerId != null) { // Incorrect layer id passed
                Services.Notifier.Error(T("Layer not found: {0}", layerId));
                return RedirectToAction("Index");
            }

            IEnumerable<string> allZones = _widgetsService.GetZones();
            IEnumerable<string> currentThemesZones = _widgetsService.GetZones(currentTheme);

            string zonePreviewImagePath = string.Format("{0}/{1}/ThemeZonePreview.png", currentTheme.Location, currentTheme.Id);
            string zonePreviewImage = _virtualPathProvider.FileExists(zonePreviewImagePath) ? zonePreviewImagePath : null;

            var viewModel = Shape.ViewModel()
                .CurrentTheme(currentTheme)
                .CurrentLayer(currentLayer)
                .Layers(layers)
                .Widgets(_widgetsService.GetWidgets())
                .Zones(currentThemesZones)
                .OrphanZones(allZones.Except(currentThemesZones))
                .OrphanWidgets(_widgetsService.GetOrphanedWidgets())
                .ZonePreviewImage(zonePreviewImage);

            return View(viewModel);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexWidgetPOST(int widgetId, string returnUrl, int? layerId, string moveUp, string moveDown, string moveHere, string moveOut) {
            if (!string.IsNullOrWhiteSpace(moveOut))
                return DeleteWidget(widgetId, returnUrl);

            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrWhiteSpace(moveUp))
                _widgetsService.MoveWidgetUp(widgetId);
            else if (!string.IsNullOrWhiteSpace(moveDown))
                _widgetsService.MoveWidgetDown(widgetId);
            else if (!string.IsNullOrWhiteSpace(moveHere))
                _widgetsService.MoveWidgetToLayer(widgetId, layerId);

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        private bool IsAuthorizedToManageWidgets() {
            return Services.Authorizer.Authorize(Permissions.ManageWidgets, T("Not authorized to manage widgets."));
        }


        public ActionResult ChooseWidget(int layerId, string zone, string returnUrl) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            if (string.IsNullOrWhiteSpace(zone)) {
                Services.Notifier.Error(T("Need a zone specified for widget placement."));
                return RedirectToAction("Index");
            }

            IEnumerable<LayerPart> layers = _widgetsService.GetLayers().ToList();

            if (!layers.Any()) {
                Services.Notifier.Error(T("Layer not found: {0}", layerId));
                return RedirectToAction("Index");
            }

            LayerPart currentLayer = layers.FirstOrDefault(layer => layer.Id == layerId);
            if (currentLayer == null) { // Incorrect layer id passed
                Services.Notifier.Error(T("Layer not found: {0}", layerId));
                return RedirectToAction("Index");
            }

            var viewModel = Shape.ViewModel()
                .CurrentLayer(currentLayer)
                .Zone(zone)
                .WidgetTypes(_widgetsService.GetWidgetTypes())
                .ReturnUrl(returnUrl);

            return View(viewModel);
        }

        public async Task<ActionResult> AddWidget(int layerId, string widgetType, string zone, string returnUrl) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = Services.ContentManager.New<WidgetPart>(widgetType);
            if (widgetPart == null)
                return HttpNotFound();
            try {
                int widgetPosition = _widgetsService.GetWidgets().Count(widget => widget.Zone == widgetPart.Zone) + 1;
                widgetPart.Position = widgetPosition.ToString(CultureInfo.InvariantCulture);
                widgetPart.Zone = zone;
                widgetPart.LayerPart = _widgetsService.GetLayer(layerId);
                var model = await Services.ContentManager.BuildEditorAsync(widgetPart);

                return View(model);
            }
            catch (Exception exception) {
                Logger.Error(T("Creating widget failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Creating widget failed: {0}", exception.Message));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
        }

        [HttpPost, ActionName("AddWidget")]
        public async Task<ActionResult> AddWidgetPOST([Bind(Prefix = "WidgetPart.LayerId")] int layerId, string widgetType, string returnUrl) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = _widgetsService.CreateWidget(layerId, widgetType, "", "", "");
            if (widgetPart == null)
                return HttpNotFound();

            var model = await Services.ContentManager.UpdateEditorAsync(widgetPart, this);
            try {
                // override the CommonPart's persisting of the current container
                widgetPart.LayerPart = _widgetsService.GetLayer(layerId);
            }
            catch (Exception exception) {
                Logger.Error(T("Creating widget failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Creating widget failed: {0}", exception.Message));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.Notifier.Information(T("Your {0} has been added.", widgetPart.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        public async Task<ActionResult> AddLayer(string name, string description, string layerRule) { // <- hints for a new layer
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            LayerPart layerPart = Services.ContentManager.New<LayerPart>("Layer");
            if (layerPart == null)
                return HttpNotFound();

            var model = await Services.ContentManager.BuildEditorAsync(layerPart);

            // only messing with the hints if they're given
            if (!string.IsNullOrWhiteSpace(name))
                model.Name = name;
            if (!string.IsNullOrWhiteSpace(description))
                model.Description = description;
            if (!string.IsNullOrWhiteSpace(layerRule))
                model.LayerRule = layerRule;

            return View(model);
        }

        [HttpPost, ActionName("AddLayer")]
        public async Task<ActionResult> AddLayerPOST() {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            LayerPart layerPart = _widgetsService.CreateLayer("", "", "");
            if (layerPart == null)
                return HttpNotFound();

            var model = await Services.ContentManager.UpdateEditorAsync(layerPart, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.Notifier.Information(T("Your {0} has been created.", layerPart.TypeDefinition.DisplayName));
            return RedirectToAction("Index", "Admin", new { layerId = layerPart.Id });
        }

        public async Task<ActionResult> EditLayer(int id) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            LayerPart layerPart = _widgetsService.GetLayer(id);
            if (layerPart == null)
                return HttpNotFound();

            var model = await Services.ContentManager.BuildEditorAsync(layerPart);
            return View(model);
        }

        [HttpPost, ActionName("EditLayer")]
        [FormValueRequired("submit.Save")]
        public async Task<ActionResult> EditLayerSavePOST(int id, string returnUrl) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            LayerPart layerPart = _widgetsService.GetLayer(id);
            if (layerPart == null)
                return HttpNotFound();

            var model = await Services.ContentManager.UpdateEditorAsync(layerPart, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.Notifier.Information(T("Your {0} has been saved.", layerPart.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        [HttpPost, ActionName("EditLayer")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditLayerDeletePOST(int id) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            try {
                _widgetsService.DeleteLayer(id);
                Services.Notifier.Information(T("Layer was successfully deleted"));
            } catch (Exception exception) {
                Logger.Error(T("Removing Layer failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Removing Layer failed: {0}", exception.Message));
            }

            return RedirectToAction("Index", "Admin");
        }

        public async Task<ActionResult> EditWidget(int id) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = null;
            widgetPart = _widgetsService.GetWidget(id);
            if (widgetPart == null) {
                Services.Notifier.Error(T("Widget not found: {0}", id));
                return RedirectToAction("Index");
            }
            try {
                var model = await Services.ContentManager.BuildEditorAsync(widgetPart);
                return View(model);
            }
            catch (Exception exception) {
                Logger.Error(T("Editing widget failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Editing widget failed: {0}", exception.Message));

                if (widgetPart != null && widgetPart.LayerPart != null)
                    return RedirectToAction("Index", "Admin", new { layerId = widgetPart.LayerPart.Id });

                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("EditWidget")]
        [FormValueRequired("submit.Save")]
        public async Task<ActionResult> EditWidgetSavePOST(int id, [Bind(Prefix = "WidgetPart.LayerId")] int layerId, string returnUrl) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = null;
            widgetPart = _widgetsService.GetWidget(id);
            if (widgetPart == null)
                return HttpNotFound();
            try {
                var model = await Services.ContentManager.UpdateEditorAsync(widgetPart, this);
                // override the CommonPart's persisting of the current container
                widgetPart.LayerPart = _widgetsService.GetLayer(layerId);
                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }

                Services.Notifier.Information(T("Your {0} has been saved.", widgetPart.TypeDefinition.DisplayName));
            } catch (Exception exception) {
                Logger.Error(T("Editing widget failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Editing widget failed: {0}", exception.Message));
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        [HttpPost, ActionName("EditWidget")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditWidgetDeletePOST(int id, string returnUrl) {
            return DeleteWidget(id, returnUrl);
        }
        private ActionResult DeleteWidget(int id, string returnUrl) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = null;
            widgetPart = _widgetsService.GetWidget(id);
            if (widgetPart == null)
                return HttpNotFound();
            try {
                _widgetsService.DeleteWidget(widgetPart.Id);
                Services.Notifier.Information(T("Widget was successfully deleted"));
            }
            catch (Exception exception) {
                Logger.Error(T("Removing Widget failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Removing Widget failed: {0}", exception.Message));
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return base.TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}