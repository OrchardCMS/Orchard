using System;
using System.Collections.Generic;
using System.Globalization;
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
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.Localization.Services;

namespace Orchard.Widgets.Controllers {

    [ValidateInput(false), Admin]
    public class AdminController : Controller, IUpdateModel {

        private readonly IWidgetsService _widgetsService;
        private readonly ISiteThemeService _siteThemeService;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly ICultureManager _cultureManager;

        public AdminController(
            IOrchardServices services,
            IWidgetsService widgetsService,
            IShapeFactory shapeFactory,
            ISiteThemeService siteThemeService,
            IVirtualPathProvider virtualPathProvider,
            ICultureManager cultureManager) {

            Services = services;
            _widgetsService = widgetsService;
            _siteThemeService = siteThemeService;
            _virtualPathProvider = virtualPathProvider;
            _cultureManager = cultureManager;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic Shape { get; set; }

        public ActionResult Index(int? layerId, string culture) {
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

            var widgets = _widgetsService.GetWidgets();

            if (!String.IsNullOrWhiteSpace(culture)) {
                widgets = widgets.Where(x => {
                    if(x.Has<ILocalizableAspect>()) {
                        return String.Equals(x.As<ILocalizableAspect>().Culture, culture, StringComparison.InvariantCultureIgnoreCase);
                    }
                    
                    return false;
                }).ToList();
            }

            var viewModel = Shape.ViewModel()
                .CurrentTheme(currentTheme)
                .CurrentLayer(currentLayer)
                .CurrentCulture(culture)
                .Layers(layers.OrderBy(l => l.Name))
                .Widgets(widgets)
                .Zones(currentThemesZones)
                .Cultures(_cultureManager.ListCultures())
                .OrphanZones(allZones.Except(currentThemesZones))
                .OrphanWidgets(_widgetsService.GetOrphanedWidgets())
                .ZonePreviewImage(zonePreviewImage);

            return View(viewModel);
        }

        private bool CheckWidgetChangeLayerPermissions(int layerIdFrom, int layerIdTo)
        {
            var layerFromPart = _widgetsService.GetLayer(layerIdFrom);
            var layerToPart = _widgetsService.GetLayer(layerIdTo);

            return Services.Authorizer.Authorize(Permissions.ChangeWidgetsPositionAndLayer, T("Not authorized to change widgets position and layer")) &&
                Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, layerFromPart, T("Cannot move widget. You do not have appropriate source layer access rights")) &&
                Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, layerToPart, T("Cannot move widget. You do not have appropriate target layer access rights"));
        }

        private bool IsAuthorizedToChangeWidgetsPositionAndLayer()
        {
            return Services.Authorizer.Authorize(Permissions.ChangeWidgetsPositionAndLayer, T("Not authorized to change widgets position and layer"));
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexWidgetPOST(int widgetId, string returnUrl, int? layerId, string moveUp, string moveDown, string moveHere, string moveOut) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            var widget = _widgetsService.GetWidget(widgetId);

            if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, widget))
            {
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }

            if (!string.IsNullOrWhiteSpace(moveOut))
            {
                return DeleteWidget(widgetId, returnUrl);
            }
            else if (!string.IsNullOrWhiteSpace(moveUp))
            {
                if (IsAuthorizedToChangeWidgetsPositionAndLayer())
                {
                    _widgetsService.MoveWidgetUp(widgetId);
                }
            }
            else if (!string.IsNullOrWhiteSpace(moveDown))
            {
                if (IsAuthorizedToChangeWidgetsPositionAndLayer())
                {
                    _widgetsService.MoveWidgetDown(widgetId);
                }
            }
            else if (!string.IsNullOrWhiteSpace(moveHere))
            {
                if (CheckWidgetChangeLayerPermissions(widget.LayerId, layerId.Value))
                {
                    _widgetsService.MoveWidgetToLayer(widgetId, layerId);
                }
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        private bool IsAuthorizedToManageWidgets() {
            return Services.Authorizer.Authorize(Permissions.ManageWidgets, T("Not authorized to manage widgets."));
        }

        private bool HasWidgetTypePublishContentPermission(string widgetType)
        {
            WidgetPart widgetPart = Services.ContentManager.New<WidgetPart>(widgetType);
            if (widgetPart != null)
                return Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, widgetPart);
            return false;
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
                .WidgetTypes(_widgetsService.GetWidgetTypes().Where(t => HasWidgetTypePublishContentPermission(t.Item1)))
                .ReturnUrl(returnUrl);

            return View(viewModel);
        }

        public ActionResult AddWidget(int layerId, string widgetType, string zone, string returnUrl) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = Services.ContentManager.New<WidgetPart>(widgetType);
            LayerPart layerPart = _widgetsService.GetLayer(layerId);
            if (widgetPart == null)
                return HttpNotFound();
            try {
                if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, widgetPart, T("Cannot add widget")))
                {
                    return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
                }

                if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, layerPart, T("Cannot add widget. Insufficient layer permissions")))
                {
                    return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
                }

                int widgetPosition = _widgetsService.GetWidgets().Count(widget => widget.Zone == widgetPart.Zone) + 1;
                widgetPart.Position = widgetPosition.ToString(CultureInfo.InvariantCulture);
                widgetPart.Zone = zone;
                widgetPart.LayerPart = _widgetsService.GetLayer(layerId);
                var model = Services.ContentManager.BuildEditor(widgetPart);

                return View(model);
            }
            catch (Exception exception) {
                Logger.Error(T("Creating widget failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Creating widget failed: {0}", exception.Message));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
        }

        [HttpPost, ActionName("AddWidget")]
        [FormValueRequired("submit.Save")]
        public ActionResult AddWidgetSavePOST([Bind(Prefix = "WidgetPart.LayerId")] int layerId, string widgetType, string returnUrl) {
            return AddWidgetPOST(layerId, widgetType, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    Services.ContentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("AddWidget")]
        [FormValueRequired("submit.Publish")]
        public ActionResult AddWidgetPublishPOST([Bind(Prefix = "WidgetPart.LayerId")] int layerId, string widgetType, string returnUrl) {
            return AddWidgetPOST(layerId, widgetType, returnUrl, contentItem => Services.ContentManager.Publish(contentItem));
        }

        private ActionResult AddWidgetPOST([Bind(Prefix = "WidgetPart.LayerId")] int layerId, string widgetType, string returnUrl, Action<ContentItem> conditionallyPublish) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            WidgetPart virtualWidgetPart = Services.ContentManager.New<WidgetPart>(widgetType);
            LayerPart layerPart = _widgetsService.GetLayer(layerId);

            if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, virtualWidgetPart, T("Cannot add widget")))
            {
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }

            if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, layerPart, T("Cannot add widget. Insufficient layer permissions")))
            {
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }

            var widgetPart = _widgetsService.CreateWidget(layerId, widgetType, "", "", "");
            if (widgetPart == null)
                return HttpNotFound();

            var model = Services.ContentManager.UpdateEditor(widgetPart, this);
            try {
                // override the CommonPart's persisting of the current container
                widgetPart.LayerPart = _widgetsService.GetLayer(layerId);
                conditionallyPublish(widgetPart.ContentItem);
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

        public ActionResult AddLayer(string name, string description, string layerRule) { // <- hints for a new layer
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            LayerPart layerPart = Services.ContentManager.New<LayerPart>("Layer");
            if (layerPart == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, layerPart, T("Cannot add layer")))
            {
                return RedirectToAction("Index");
            }

            var model = Services.ContentManager.BuildEditor(layerPart);

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
        public ActionResult AddLayerPOST() {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            var virtualLayer = Services.ContentManager.New<LayerPart>("Layer");
            if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, virtualLayer, T("Cannot add layer")))
            {
                return RedirectToAction("Index");
            }

            LayerPart layerPart = _widgetsService.CreateLayer("", "", "");
            if (layerPart == null)
                return HttpNotFound();

            var model = Services.ContentManager.UpdateEditor(layerPart, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.Notifier.Information(T("Your {0} has been created.", layerPart.TypeDefinition.DisplayName));
            return RedirectToAction("Index", "Admin", new { layerId = layerPart.Id });
        }

        public ActionResult EditLayer(int id) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            LayerPart layerPart = _widgetsService.GetLayer(id);
            if (layerPart == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, layerPart, T("Cannot edit layer")))
            {
                return RedirectToAction("Index");
            }

            var model = Services.ContentManager.BuildEditor(layerPart);
            return View(model);
        }

        [HttpPost, ActionName("EditLayer")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditLayerSavePOST(int id, string returnUrl) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            LayerPart layerPart = _widgetsService.GetLayer(id);
            if (layerPart == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, layerPart.ContentItem, T("Cannot edit layer")))
            {
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }

            var model = Services.ContentManager.UpdateEditor(layerPart, this);

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
                LayerPart layerPart = _widgetsService.GetLayer(id);
                if (layerPart == null)
                    return HttpNotFound();

                if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.DeleteContent, layerPart.ContentItem, T("Cannot delete layer")))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (_widgetsService.GetWidgets(id).Any())
                {
                    Services.Notifier.Error(T("Cannot delete layer with attached widgets! Please move widgets to another layer or remove them."));
                    return RedirectToAction("Index", "Admin");
                }

                _widgetsService.DeleteLayer(id);
                Services.Notifier.Information(T("Layer was successfully deleted"));
            } catch (Exception exception) {
                Logger.Error(T("Removing Layer failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Removing Layer failed: {0}", exception.Message));
            }

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult EditWidget(int id) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = null;
            widgetPart = _widgetsService.GetWidget(id);
            if (widgetPart == null) {
                Services.Notifier.Error(T("Widget not found: {0}", id));
                return RedirectToAction("Index");
            }
            try {
                if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, widgetPart, T("Cannot edit widget")))
                {
                    return RedirectToAction("Index");
                }

                var model = Services.ContentManager.BuildEditor(widgetPart);
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
        public ActionResult EditWidgetSavePOST(int id, [Bind(Prefix = "WidgetPart.LayerId")] int layerId, string returnUrl) {
            return EditWidgetPOST(id, layerId, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    Services.ContentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("EditWidget")]
        [FormValueRequired("submit.Publish")]
        public ActionResult EditWidgetPublishPOST(int id, [Bind(Prefix = "WidgetPart.LayerId")] int layerId, string returnUrl) {
            return EditWidgetPOST(id, layerId, returnUrl, contentItem => {
                Services.ContentManager.Publish(contentItem);
            });
        }

        private ActionResult EditWidgetPOST(int id, int layerId, string returnUrl, Action<ContentItem> conditionallyPublish) {
            if (!IsAuthorizedToManageWidgets())
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = null;
            widgetPart = Services.ContentManager.Get<WidgetPart>(id, VersionOptions.DraftRequired);

            if (widgetPart == null)
                return HttpNotFound();
            try {
                var oldLayerId = widgetPart.LayerId;
                var oldPosition = widgetPart.Position;
                if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, widgetPart, T("Cannot edit widget")))
                {
                    return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
                }

                var model = Services.ContentManager.UpdateEditor(widgetPart, this);
                // override the CommonPart's persisting of the current container
                widgetPart.LayerPart = _widgetsService.GetLayer(layerId);
                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }
                else if ((oldLayerId != layerId && !CheckWidgetChangeLayerPermissions(oldLayerId, layerId)) ||
                       (oldPosition != widgetPart.Position && !IsAuthorizedToChangeWidgetsPositionAndLayer()))
                {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }

                conditionallyPublish(widgetPart.ContentItem);

                Services.Notifier.Information(T("Your {0} has been saved.", widgetPart.TypeDefinition.DisplayName));
            }
            catch (Exception exception) {
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
                if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.DeleteContent, widgetPart, T("Cannot delete widget")))
                {
                    return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
                }

                if (!Services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, widgetPart.LayerPart, T("Cannot delete widget. Insufficient layer permissions")))
                {
                    return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
                }

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