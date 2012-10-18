using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Extensions;
using Orchard.ContentTypes.Services;
using Orchard.ContentTypes.Settings;
using Orchard.ContentTypes.ViewModels;
using Orchard.Core.Contents.Controllers;
using Orchard.Core.Contents.Settings;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.ContentTypes.Controllers {
    public class AdminController : Controller, IUpdateModel {
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IPlacementService _placementService;
        private readonly Lazy<IEnumerable<IShellSettingsManagerEventHandler>> _settingsManagerEventHandlers;
        private readonly ShellSettings _settings;

        public AdminController(
            IOrchardServices orchardServices, 
            IContentDefinitionService contentDefinitionService, 
            IContentDefinitionManager contentDefinitionManager,
            IPlacementService placementService,
            Lazy<IEnumerable<IShellSettingsManagerEventHandler>> settingsManagerEventHandlers,
            ShellSettings settings
            ) {
            Services = orchardServices;
            _contentDefinitionService = contentDefinitionService;
            _contentDefinitionManager = contentDefinitionManager;
            _placementService = placementService;
            _settingsManagerEventHandlers = settingsManagerEventHandlers;
            _settings = settings;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public ActionResult Index() { return List(); }

        #region Types

        public ActionResult List() {
            return View("List", new ListContentTypesViewModel {
                Types = _contentDefinitionService.GetTypes()
            });
        }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to create a content type.")))
                return new HttpUnauthorizedResult();

            return View(new CreateTypeViewModel());
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(CreateTypeViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to create a content type.")))
                return new HttpUnauthorizedResult();

            viewModel.DisplayName = viewModel.DisplayName ?? String.Empty;
            viewModel.Name = viewModel.Name ?? String.Empty;

            if (String.IsNullOrWhiteSpace(viewModel.DisplayName)) {
                ModelState.AddModelError("DisplayName", T("The Display Name name can't be empty.").ToString());
            }

            if (String.IsNullOrWhiteSpace(viewModel.Name)) {
                ModelState.AddModelError("Name", T("The Content Type Id can't be empty.").ToString());
            }

            if (_contentDefinitionService.GetTypes().Any(t => String.Equals(t.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase))) {
                ModelState.AddModelError("Name", T("A type with the same Id already exists.").ToString());
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Name) && !viewModel.Name[0].IsLetter()) {
                ModelState.AddModelError("Name", T("The technical name must start with a letter.").ToString());
            }

            if (_contentDefinitionService.GetTypes().Any(t => String.Equals(t.DisplayName.Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase))) {
                ModelState.AddModelError("DisplayName", T("A type with the same Display Name already exists.").ToString());
            }

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }

            var contentTypeDefinition = _contentDefinitionService.AddType(viewModel.Name, viewModel.DisplayName);
            
            // adds CommonPart by default
            _contentDefinitionService.AddPartToType("CommonPart", viewModel.Name);

            var typeViewModel = new EditTypeViewModel(contentTypeDefinition);


            Services.Notifier.Information(T("The \"{0}\" content type has been created.", typeViewModel.DisplayName));

            return RedirectToAction("AddPartsTo", new { id = typeViewModel.Name });
        }

        public ActionResult ContentTypeName(string displayName, int version) {
            return Json(new {
                result = _contentDefinitionService.GenerateContentTypeNameFromDisplayName(displayName),
                version = version
            });
        }

        public ActionResult FieldName(string partName, string displayName, int version) {
            return Json(new {
                result = _contentDefinitionService.GenerateFieldNameFromDisplayName(partName, displayName),
                version = version
            });
        }

        public ActionResult Edit(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return HttpNotFound();

            return View(typeViewModel);
        }

        public ActionResult EditPlacement(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(id);

            if (contentTypeDefinition == null)
                return HttpNotFound();

            var placementModel = new EditPlacementViewModel {
                PlacementSettings = contentTypeDefinition.GetPlacement(PlacementType.Editor),
                AllPlacements = _placementService.GetEditorPlacement(id).OrderBy(x => x.PlacementSettings.Position, new FlatPositionComparer()).ThenBy(x => x.PlacementSettings.ShapeType).ToList(), 
                ContentTypeDefinition = contentTypeDefinition,
            };

            return View(placementModel);
        }

        [HttpPost, ActionName("EditPlacement")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPlacementPost(string id, EditPlacementViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(id);

            if (contentTypeDefinition == null)
                return HttpNotFound();

            var allPlacements = _placementService.GetEditorPlacement(id).ToList();
            var result = new List<PlacementSettings>(contentTypeDefinition.GetPlacement(PlacementType.Editor));

            contentTypeDefinition.ResetPlacement(PlacementType.Editor);

            foreach(var driverPlacement in viewModel.AllPlacements) {
                // if the placement has changed, persist it
                if (!allPlacements.Any(x => x.PlacementSettings.Equals(driverPlacement.PlacementSettings))) {
                    result = result.Where(x => !x.IsSameAs(driverPlacement.PlacementSettings)).ToList();
                    result.Add(driverPlacement.PlacementSettings);
                }
            }

            foreach(var placementSetting in result) {
                contentTypeDefinition.Placement(PlacementType.Editor,
                                placementSetting.ShapeType,
                                placementSetting.Differentiator,
                                placementSetting.Zone,
                                placementSetting.Position);

            }

            // persist changes
            _contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);

            _settingsManagerEventHandlers.Value.Invoke(x => x.Saved(_settings), Logger);

            return RedirectToAction("EditPlacement", new {id});
        }

        [HttpPost, ActionName("EditPlacement")]
        [FormValueRequired("submit.Restore")]
        public ActionResult EditPlacementRestorePost(string id, EditPlacementViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(id);

            if (contentTypeDefinition == null)
                return HttpNotFound();

            contentTypeDefinition.ResetPlacement(PlacementType.Editor);

            // persist changes
            _contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);

            _settingsManagerEventHandlers.Value.Invoke(x => x.Saved(_settings), Logger);

            return RedirectToAction("EditPlacement", new { id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPOST(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return HttpNotFound();

            var edited = new EditTypeViewModel();
            TryUpdateModel(edited);
            typeViewModel.DisplayName = edited.DisplayName ?? string.Empty;

            if ( String.IsNullOrWhiteSpace(typeViewModel.DisplayName) ) {
                ModelState.AddModelError("DisplayName", T("The Content Type name can't be empty.").ToString());
            }

            if ( _contentDefinitionService.GetTypes().Any(t => String.Equals(t.DisplayName.Trim(), typeViewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase) && !String.Equals(t.Name, id)) ) {
                ModelState.AddModelError("DisplayName", T("A type with the same name already exists.").ToString());
            }

            if (!ModelState.IsValid)
                return View(typeViewModel);

            _contentDefinitionService.AlterType(typeViewModel, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(typeViewModel);
            }

            Services.Notifier.Information(T("\"{0}\" settings have been saved.", typeViewModel.DisplayName));

            return RedirectToAction("List");
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public ActionResult Delete(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to delete a content type.")))
                return new HttpUnauthorizedResult();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return HttpNotFound();

            _contentDefinitionService.RemoveType(id, true);

            Services.Notifier.Information(T("\"{0}\" has been removed.", typeViewModel.DisplayName));
            
            return RedirectToAction("List");
        }

        public ActionResult AddPartsTo(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return HttpNotFound();

            var viewModel = new AddPartsViewModel {
                Type = typeViewModel,
                PartSelections = _contentDefinitionService.GetParts(false/*metadataPartsOnly*/)
                    .Where(cpd => !typeViewModel.Parts.Any(p => p.PartDefinition.Name == cpd.Name) && cpd.Settings.GetModel<ContentPartSettings>().Attachable)
                    .Select(cpd => new PartSelectionViewModel { PartName = cpd.Name, PartDisplayName = cpd.DisplayName })
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("AddPartsTo")]
        public ActionResult AddPartsToPOST(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return HttpNotFound();

            var viewModel = new AddPartsViewModel();
            if (!TryUpdateModel(viewModel))
                return AddPartsTo(id);

            var partsToAdd = viewModel.PartSelections.Where(ps => ps.IsSelected).Select(ps => ps.PartName);
            foreach (var partToAdd in partsToAdd) {
                _contentDefinitionService.AddPartToType(partToAdd, typeViewModel.Name);
                Services.Notifier.Information(T("The \"{0}\" part has been added.", partToAdd));
            }

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return AddPartsTo(id);
            }

            return RedirectToAction("Edit", new {id});
        }

        public ActionResult RemovePartFrom(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var typeViewModel = _contentDefinitionService.GetType(id);

            var viewModel = new RemovePartViewModel();
            if (typeViewModel == null
                || !TryUpdateModel(viewModel)
                || !typeViewModel.Parts.Any(p => p.PartDefinition.Name == viewModel.Name))
                return HttpNotFound();

            viewModel.Type = typeViewModel;
            return View(viewModel);
        }

        [HttpPost, ActionName("RemovePartFrom")]
        public ActionResult RemovePartFromPOST(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var typeViewModel = _contentDefinitionService.GetType(id);

            var viewModel = new RemovePartViewModel();
            if (typeViewModel == null
                || !TryUpdateModel(viewModel)
                || !typeViewModel.Parts.Any(p => p.PartDefinition.Name == viewModel.Name))
                return HttpNotFound();

            _contentDefinitionService.RemovePartFromType(viewModel.Name, typeViewModel.Name);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                viewModel.Type = typeViewModel;
                return View(viewModel);
            }

            Services.Notifier.Information(T("The \"{0}\" part has been removed.", viewModel.Name));

            return RedirectToAction("Edit", new {id});
        }

        #endregion

        #region Parts

        public ActionResult ListParts() {
            return View(new ListContentPartsViewModel {
                // only user-defined parts (not code as they are not configurable)
                Parts = _contentDefinitionService.GetParts(true/*metadataPartsOnly*/)
            });
        }

        public ActionResult CreatePart() {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to create a content part.")))
                return new HttpUnauthorizedResult();

            return View(new CreatePartViewModel());
        }

        [HttpPost, ActionName("CreatePart")]
        public ActionResult CreatePartPOST(CreatePartViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to create a content part.")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid)
                return View(viewModel);

            var partViewModel = _contentDefinitionService.AddPart(viewModel);

            if (partViewModel == null) {
                Services.Notifier.Information(T("The content part could not be created."));
                return View(viewModel);
            }

            Services.Notifier.Information(T("The \"{0}\" content part has been created.", partViewModel.Name));
            return RedirectToAction("EditPart", new { id = partViewModel.Name });
        }

        public ActionResult EditPart(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content part.")))
                return new HttpUnauthorizedResult();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null)
                return HttpNotFound();

            return View(partViewModel);
        }

        [HttpPost, ActionName("EditPart")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPartPOST(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content part.")))
                return new HttpUnauthorizedResult();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null)
                return HttpNotFound();

            if (!TryUpdateModel(partViewModel))
                return View(partViewModel);

            _contentDefinitionService.AlterPart(partViewModel, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(partViewModel);
            }

            Services.Notifier.Information(T("\"{0}\" settings have been saved.", partViewModel.Name));

            return RedirectToAction("ListParts");
        }

        [HttpPost, ActionName("EditPart")]
        [FormValueRequired("submit.Delete")]
        public ActionResult DeletePart(string id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to delete a content part.")))
                return new HttpUnauthorizedResult();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null)
                return HttpNotFound();

            _contentDefinitionService.RemovePart(id);
            
            Services.Notifier.Information(T("\"{0}\" has been removed.", partViewModel.DisplayName));

            return RedirectToAction("ListParts");
        }

        public ActionResult AddFieldTo(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content part.")))
                return new HttpUnauthorizedResult();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null) {
                //id passed in might be that of a type w/ no implicit field
                var typeViewModel = _contentDefinitionService.GetType(id);
                if (typeViewModel != null)
                    partViewModel = new EditPartViewModel(new ContentPartDefinition(id));
                else
                    return HttpNotFound();
            }

            var viewModel = new AddFieldViewModel {
                Part = partViewModel,
                Fields = _contentDefinitionService.GetFields().OrderBy(x => x.FieldTypeName)
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("AddFieldTo")]
        public ActionResult AddFieldToPOST(AddFieldViewModel viewModel, string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content part.")))
                return new HttpUnauthorizedResult();

            var partViewModel = _contentDefinitionService.GetPart(id);
            var typeViewModel = _contentDefinitionService.GetType(id);
            if (partViewModel == null) {
                // id passed in might be that of a type w/ no implicit field
                if (typeViewModel != null) {
                    partViewModel = new EditPartViewModel {Name = typeViewModel.Name};
                    _contentDefinitionService.AddPart(new CreatePartViewModel {Name = partViewModel.Name});
                    _contentDefinitionService.AddPartToType(partViewModel.Name, typeViewModel.Name);
                }
                else {
                    return HttpNotFound();
                }
            }

            viewModel.DisplayName = viewModel.DisplayName ?? String.Empty;
            viewModel.DisplayName = viewModel.DisplayName.Trim();
            viewModel.Name = viewModel.Name ?? String.Empty;

            if (String.IsNullOrWhiteSpace(viewModel.DisplayName)) {
                ModelState.AddModelError("DisplayName", T("The Display Name name can't be empty.").ToString());
            }

            if (String.IsNullOrWhiteSpace(viewModel.Name)) {
                ModelState.AddModelError("Name", T("The Technical Name can't be empty.").ToString());
            }

            if (_contentDefinitionService.GetPart(partViewModel.Name).Fields.Any(t => String.Equals(t.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase))) {
                ModelState.AddModelError("Name", T("A field with the same name already exists.").ToString());
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Name) && !viewModel.Name[0].IsLetter()) {
                ModelState.AddModelError("Name", T("The technical name must start with a letter.").ToString());
            }

            if (!String.Equals(viewModel.Name, viewModel.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase)) {
                ModelState.AddModelError("Name", T("The technical name contains invalid characters.").ToString());
            }

            if (_contentDefinitionService.GetPart(partViewModel.Name).Fields.Any(t => String.Equals(t.DisplayName.Trim(), Convert.ToString(viewModel.DisplayName).Trim(), StringComparison.OrdinalIgnoreCase))) {
                ModelState.AddModelError("DisplayName", T("A field with the same Display Name already exists.").ToString());
            }

            if (!ModelState.IsValid) {
                viewModel.Part = partViewModel;
                viewModel.Fields = _contentDefinitionService.GetFields();

                Services.TransactionManager.Cancel();

                return View(viewModel);
            }

            try {
                _contentDefinitionService.AddFieldToPart(viewModel.Name, viewModel.DisplayName, viewModel.FieldTypeName, partViewModel.Name);
            }
            catch (Exception ex) {
                Services.Notifier.Information(T("The \"{0}\" field was not added. {1}", viewModel.DisplayName, ex.Message));
                Services.TransactionManager.Cancel();
                return AddFieldTo(id);
            }

            Services.Notifier.Information(T("The \"{0}\" field has been added.", viewModel.DisplayName));

            if (typeViewModel != null) {
                return RedirectToAction("Edit", new {id});
            }

            return RedirectToAction("EditPart", new { id });
        }

        public ActionResult EditField(string id, string name) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null) {
                return HttpNotFound();
            }

            var fieldViewModel = partViewModel.Fields.FirstOrDefault(x => x.Name == name);

            if(fieldViewModel == null) {
                return HttpNotFound();
            }

            var viewModel = new EditFieldNameViewModel {
                Name = fieldViewModel.Name,
                DisplayName = fieldViewModel.DisplayName
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("EditField")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditFieldPOST(string id, EditFieldNameViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            if (viewModel == null)
                return HttpNotFound();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null) {
                return HttpNotFound();
            }

            // prevent null reference exception in validation
            viewModel.DisplayName = viewModel.DisplayName ?? String.Empty;
            
            // remove extra spaces
            viewModel.DisplayName = viewModel.DisplayName.Trim();

            if (String.IsNullOrWhiteSpace(viewModel.DisplayName)) {
                ModelState.AddModelError("DisplayName", T("The Display Name name can't be empty.").ToString());
            }
            
            if (_contentDefinitionService.GetPart(partViewModel.Name).Fields.Any(t => t.Name != viewModel.Name && String.Equals(t.DisplayName.Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase))) {
                ModelState.AddModelError("DisplayName", T("A field with the same Display Name already exists.").ToString());
            }

            if (!ModelState.IsValid) {
                return View(viewModel);
            }

            var field = _contentDefinitionManager.GetPartDefinition(id).Fields.FirstOrDefault(x => x.Name == viewModel.Name);

            if(field == null) {
                return HttpNotFound();
            }

            field.DisplayName = viewModel.DisplayName;
            _contentDefinitionManager.StorePartDefinition(partViewModel._Definition);

            Services.Notifier.Information(T("Display name changed to {0}.", viewModel.DisplayName));

            // redirect to the type editor if a type exists with this name
            var typeViewModel = _contentDefinitionService.GetType(id);
            if (typeViewModel != null) {
                return RedirectToAction("Edit", new { id });
            }

            return RedirectToAction("EditPart", new { id });
        }

        public ActionResult RemoveFieldFrom(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content part.")))
                return new HttpUnauthorizedResult();

            var partViewModel = _contentDefinitionService.GetPart(id);

            var viewModel = new RemoveFieldViewModel();
            if (partViewModel == null
                || !TryUpdateModel(viewModel)
                || !partViewModel.Fields.Any(p => p.Name == viewModel.Name))
                return HttpNotFound();

            viewModel.Part = partViewModel;
            return View(viewModel);
        }

        [HttpPost, ActionName("RemoveFieldFrom")]
        public ActionResult RemoveFieldFromPOST(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content part.")))
                return new HttpUnauthorizedResult();

            var partViewModel = _contentDefinitionService.GetPart(id);

            var viewModel = new RemoveFieldViewModel();
            if (partViewModel == null
                || !TryUpdateModel(viewModel)
                || !partViewModel.Fields.Any(p => p.Name == viewModel.Name))
                return HttpNotFound();

            _contentDefinitionService.RemoveFieldFromPart(viewModel.Name, partViewModel.Name);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                viewModel.Part = partViewModel;
                return View(viewModel);
            }

            Services.Notifier.Information(T("The \"{0}\" field has been removed.", viewModel.Name));

            if (_contentDefinitionService.GetType(id) != null)
                return RedirectToAction("Edit", new { id });

            return RedirectToAction("EditPart", new { id });
        }

        #endregion

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return base.TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
