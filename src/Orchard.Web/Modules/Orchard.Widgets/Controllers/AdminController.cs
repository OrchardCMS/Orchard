using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.UI.Widgets;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;
using Orchard.Widgets.ViewModels;

namespace Orchard.Widgets.Controllers {

    [ValidateInput(false), Admin]
    public class AdminController : Controller, IUpdateModel {

        private const string NotAuthorizedManageWidgetsLabel = "Not authorized to manage widgets";

        private readonly IWidgetsService _widgetsService;
        private readonly IRuleManager _ruleManager;

        public AdminController(
            IOrchardServices services,
            IWidgetsService widgetsService,
            IRuleManager ruleManager) {

            Services = services;
            _widgetsService = widgetsService;
            _ruleManager = ruleManager;

            T = NullLocalizer.Instance;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(int? id) {
            IEnumerable<LayerPart> layers = _widgetsService.GetLayers();

            LayerPart currentLayer;
            IEnumerable<WidgetPart> currentLayerWidgets;

            if (layers.Count() > 0) {
                currentLayer = id == null ?
                               layers.First() :
                               layers.FirstOrDefault(layer => layer.Id == id);

                if (currentLayer == null &&
                    id != null) {
                    // Incorrect layer id passed
                    Services.Notifier.Error(T("Layer not found: {0}", id));
                    return RedirectToAction("Index");
                }

                currentLayerWidgets = _widgetsService.GetWidgets(currentLayer.Id);
            }
            else {
                currentLayer = null;
                currentLayerWidgets = new List<WidgetPart>();
            }

            WidgetsIndexViewModel widgetsIndexViewModel = new WidgetsIndexViewModel {
                WidgetTypes = _widgetsService.GetWidgetTypes(),
                Layers = layers,
                Zones = _widgetsService.GetZones(),
                CurrentLayer = currentLayer,
                CurrentLayerWidgets = currentLayerWidgets
            };

            return View(widgetsIndexViewModel);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexWidgetPOST(int? id) {
            const string moveDownString = "submit.MoveDown.";
            const string moveUpString = "submit.MoveUp.";

            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                string moveDownAction = HttpContext.Request.Form.AllKeys.FirstOrDefault(key => key.StartsWith(moveDownString));
                if (moveDownAction != null) {
                    moveDownAction = moveDownAction.Substring(moveDownString.Length, moveDownAction.IndexOf(".", moveDownString.Length) - moveDownString.Length);
                    _widgetsService.MoveWidgetDown(int.Parse(moveDownAction));
                }
                else {
                    string moveUpAction = HttpContext.Request.Form.AllKeys.FirstOrDefault(key => key.StartsWith(moveUpString));
                    if (moveUpAction != null) {
                        moveUpAction = moveUpAction.Substring(moveUpString.Length, moveUpAction.IndexOf(".", moveUpString.Length) - moveUpString.Length);
                        _widgetsService.MoveWidgetUp(int.Parse(moveUpAction));
                    }
                }
            } 
            catch (Exception exception) {
                Services.Notifier.Error(T("Moving widget failed: {0}", exception.Message));
            }

            return RedirectToAction("Index", "Admin", new { id });
        }

        public ActionResult AddWidget(int layerId, string widgetType) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                WidgetPart widgetPart = Services.ContentManager.New<WidgetPart>(widgetType);
                if (widgetPart == null)
                    return HttpNotFound();

                widgetPart.LayerPart = _widgetsService.GetLayer(layerId);
                dynamic model = Services.ContentManager.BuildEditor(widgetPart);
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Creating widget failed: {0}", exception.Message));
                return RedirectToAction("Index", "Admin", new { id = layerId });
            }
        }

        [HttpPost, ActionName("AddWidget")]
        public ActionResult AddWidgetPOST(int layerId, string widgetType) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                WidgetPart widgetPart = _widgetsService.CreateWidget(layerId, widgetType, "", "", "");
                if (widgetPart == null)
                    return HttpNotFound();

                var model = Services.ContentManager.UpdateEditor(widgetPart, this);
                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }

                Services.Notifier.Information(T("Your {0} has been created.", widgetPart.TypeDefinition.DisplayName));
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Creating widget failed: {0}", exception.Message));
            }

            return RedirectToAction("Index", "Admin", new { id = layerId });
        }

        public ActionResult AddLayer() {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                LayerPart layerPart = Services.ContentManager.New<LayerPart>("Layer");
                if (layerPart == null)
                    return HttpNotFound();

                dynamic model = Services.ContentManager.BuildEditor(layerPart);
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Creating layer failed: {0}", exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("AddLayer")]
        public ActionResult AddLayerPOST() {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                LayerPart layerPart = _widgetsService.CreateLayer("", "", "");
                if (layerPart == null)
                    return HttpNotFound();

                var model = Services.ContentManager.UpdateEditor(layerPart, this);

                ValidateLayer(layerPart);

                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }

                Services.Notifier.Information(T("Your {0} has been created.", layerPart.TypeDefinition.DisplayName));
                return RedirectToAction("Index", "Admin", new { id = layerPart.Id });
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Creating layer failed: {0}", exception.Message));
                return RedirectToAction("Index");
            }
        }

        public ActionResult EditLayer(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                LayerPart layerPart = _widgetsService.GetLayer(id);
                if (layerPart == null) {
                    return HttpNotFound();
                }

                dynamic model = Services.ContentManager.BuildEditor(layerPart);
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing layer failed: {0}", exception.Message));
                return RedirectToAction("Index", "Admin", new { id });
            }
        }

        [HttpPost, ActionName("EditLayer")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditLayerSavePOST(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                LayerPart layerPart = _widgetsService.GetLayer(id);
                if (layerPart == null)
                    return HttpNotFound();

                var model = Services.ContentManager.UpdateEditor(layerPart, this);

                ValidateLayer(layerPart);

                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }

                Services.Notifier.Information(T("Your {0} has been saved.", layerPart.TypeDefinition.DisplayName));
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing layer failed: {0}", exception.Message));
            }

            return RedirectToAction("Index", "Admin", new { id });
        }

        [HttpPost, ActionName("EditLayer")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditLayerDeletePOST(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                _widgetsService.DeleteLayer(id);
                Services.Notifier.Information(T("Layer was successfully deleted"));
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Removing Layer failed: {0}", exception.Message));
            }

            return RedirectToAction("Index");
        }

        public ActionResult EditWidget(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = null;
            try {
                widgetPart = _widgetsService.GetWidget(id);
                if (widgetPart == null) {
                    Services.Notifier.Error(T("Widget not found: {0}", id));
                    return RedirectToAction("Index");
                }

                dynamic model = Services.ContentManager.BuildEditor(widgetPart);
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing widget failed: {0}", exception.Message));

                if (widgetPart != null)
                    return RedirectToAction("Index", "Admin", new { id = widgetPart.LayerPart.Id });

                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("EditWidget")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditWidgetSavePOST(int id, int layerId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = null;
            try {
                widgetPart = _widgetsService.GetWidget(id);
                if (widgetPart == null)
                    return HttpNotFound();

                widgetPart.LayerPart = _widgetsService.GetLayer(layerId);
                var model = Services.ContentManager.UpdateEditor(widgetPart, this);
                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }

                Services.Notifier.Information(T("Your {0} has been saved.", widgetPart.TypeDefinition.DisplayName));
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing widget failed: {0}", exception.Message));
            }

            return widgetPart != null ?
                RedirectToAction("Index", "Admin", new { id = widgetPart.LayerPart.Id }) :
                RedirectToAction("Index");
        }

        [HttpPost, ActionName("EditWidget")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditWidgetDeletePOST(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            WidgetPart widgetPart = null;
            try {
                widgetPart = _widgetsService.GetWidget(id);
                if (widgetPart == null)
                    return HttpNotFound();

                _widgetsService.DeleteWidget(widgetPart.Id);
                Services.Notifier.Information(T("Widget was successfully deleted"));
            } 
            catch (Exception exception) {
                Services.Notifier.Error(T("Removing Widget failed: {0}", exception.Message));
            }

            return widgetPart != null ?
                RedirectToAction("Index", "Admin", new { id = widgetPart.LayerPart.Id }) : 
                RedirectToAction("Index");
        }

        public bool ValidateLayer(LayerPart layer) {
            if ( String.IsNullOrWhiteSpace(layer.LayerRule) ) {
                layer.LayerRule = "true";
            }

            if(_widgetsService.GetLayers().Count(l => String.Equals(l.Name, layer.Name, StringComparison.InvariantCultureIgnoreCase)) > 1) { // the current layer counts for 1
                ModelState.AddModelError("Name", T("A Layer with the same name already exists").Text);
                return false;
            }

            try {
                _ruleManager.Matches(layer.LayerRule);
            }
            catch ( Exception e ) {
                ModelState.AddModelError("Description", T("The rule is not valid: {0}", e.Message).Text);
                return false;
            }

            return true;
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return base.TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}