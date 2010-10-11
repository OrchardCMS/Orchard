using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Localization;
using Orchard.Mvc.Results;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;
using Orchard.Widgets.ViewModels;

namespace Orchard.Widgets.Controllers {

    [ValidateInput(false), Admin]
    public class AdminController : Controller, IUpdateModel {

        private const string NotAuthorizedManageWidgetsLabel = "Not authorized to manage widgets";

        private readonly IWidgetsService _widgetsService;

        public AdminController(
            IOrchardServices services,
            IWidgetsService widgetsService) {

            Services = services;
            _widgetsService = widgetsService;

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

                currentLayerWidgets = _widgetsService.GetWidgets().Where(widgetPart => widgetPart.LayerPart.Id == currentLayer.Id);
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
                    return new NotFoundResult();

                dynamic model = Services.ContentManager.BuildEditor(widgetPart);
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Creating widget failed: {0}", exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("AddWidget")]
        public ActionResult AddWidgetPOST(int layerId, string widgetType) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                int widgetPosition = _widgetsService.GetWidgets(layerId).Count() + 1;
                WidgetPart widgetPart = _widgetsService.CreateWidget(layerId, widgetType, "", widgetPosition.ToString(), "");
                if (widgetPart == null)
                    return new NotFoundResult();

                var model = Services.ContentManager.UpdateEditor(widgetPart, this);
                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }

                Services.Notifier.Information(T("Your {0} has been created.", widgetPart.TypeDefinition.DisplayName));
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Creating widget failed: {0}", exception.Message));
                return RedirectToAction("Index");
            }
        }

        public ActionResult AddLayer() {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                LayerPart layerPart = Services.ContentManager.New<LayerPart>("Layer");
                if (layerPart == null)
                    return new NotFoundResult();

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
                    return new NotFoundResult();

                var model = Services.ContentManager.UpdateEditor(layerPart, this);
                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }

                Services.Notifier.Information(T("Your {0} has been created.", layerPart.TypeDefinition.DisplayName));
                return RedirectToAction("Index");
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
                    return new NotFoundResult();
                }

                dynamic model = Services.ContentManager.BuildEditor(layerPart);
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing layer failed: {0}", exception.Message));
                return RedirectToAction("Index");
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
                    return new NotFoundResult();

                var model = Services.ContentManager.UpdateEditor(layerPart, this);
                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }

                Services.Notifier.Information(T("Your {0} has been saved.", layerPart.TypeDefinition.DisplayName));
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing layer failed: {0}", exception.Message));
                return RedirectToAction("Index");
            }
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

            try {
                WidgetPart widgetPart = _widgetsService.GetWidget(id);
                if (widgetPart == null) {
                    Services.Notifier.Error(T("Widget not found: {1}", id));
                    return RedirectToAction("Index");
                }

                dynamic model = Services.ContentManager.BuildEditor(widgetPart);
                return View(model);
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing widget failed: {0}", exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("EditWidget")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditWidgetSavePOST(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                WidgetPart widgetPart = _widgetsService.GetWidget(id);
                if (widgetPart == null)
                    return new NotFoundResult();
            
                var model = Services.ContentManager.UpdateEditor(widgetPart, this);
                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();
                    return View(model);
                }

                Services.Notifier.Information(T("Your {0} has been saved.", widgetPart.TypeDefinition.DisplayName));
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Editing widget failed: {0}", exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("EditWidget")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditWidgetDeletePOST(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageWidgets, T(NotAuthorizedManageWidgetsLabel)))
                return new HttpUnauthorizedResult();

            try {
                _widgetsService.DeleteWidget(id);
                Services.Notifier.Information(T("Widget was successfully deleted"));
            } 
            catch (Exception exception) {
                Services.Notifier.Error(T("Removing Widget failed: {0}", exception.Message));
            }

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return base.TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}