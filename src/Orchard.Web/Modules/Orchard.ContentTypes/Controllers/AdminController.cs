using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Services;
using Orchard.ContentTypes.ViewModels;
using Orchard.Core.Contents.Controllers;
using Orchard.Core.Contents.Settings;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.ContentTypes.Controllers {
    public class AdminController : Controller, IUpdateModel {
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AdminController(IOrchardServices orchardServices, IContentDefinitionService contentDefinitionService, IContentDefinitionManager contentDefinitionManager) {
            Services = orchardServices;
            _contentDefinitionService = contentDefinitionService;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

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

            if (_contentDefinitionService.GetTypes().Any(t => String.Equals(t.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase))) {
                ModelState.AddModelError("Name", T("A type with the same Id already exists.").ToString());
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Name) && !ContentDefinitionService.IsLetter(viewModel.Name[0])) {
                ModelState.AddModelError("Name", T("The technical name must start with a letter.").ToString());
            }

            if (_contentDefinitionService.GetTypes().Any(t => String.Equals(t.DisplayName.Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase))) {
                ModelState.AddModelError("DisplayName", T("A type with the same Name already exists.").ToString());
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

        public ActionResult ContentTypeName(string displayName) {
            return Json(_contentDefinitionService.GenerateContentTypeNameFromDisplayName(displayName));
        }

        public ActionResult Edit(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return HttpNotFound();

            return View(typeViewModel);
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

            _contentDefinitionManager.DeleteTypeDefinition(id);

            // delete all content items (but keep versions)
            var contentItems = Services.ContentManager.Query(id).List();
            foreach (var contentItem in contentItems) {
                Services.ContentManager.Remove(contentItem);
            }

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

            _contentDefinitionManager.DeletePartDefinition(id);
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
                Fields = _contentDefinitionService.GetFields()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("AddFieldTo")]
        public ActionResult AddFieldToPOST(string id) {
            if (!Services.Authorizer.Authorize(Permissions.EditContentTypes, T("Not allowed to edit a content part.")))
                return new HttpUnauthorizedResult();

            var partViewModel = _contentDefinitionService.GetPart(id);
            var typeViewModel = _contentDefinitionService.GetType(id);
            if (partViewModel == null) {
                //id passed in might be that of a type w/ no implicit field
                if (typeViewModel != null) {
                    partViewModel = new EditPartViewModel {Name = typeViewModel.Name};
                    _contentDefinitionService.AddPart(new CreatePartViewModel {Name = partViewModel.Name});
                    _contentDefinitionService.AddPartToType(partViewModel.Name, typeViewModel.Name);
                }
                else {
                    return HttpNotFound();
                }
            }

            var viewModel = new AddFieldViewModel();
            if (!TryUpdateModel(viewModel)) {
                Services.TransactionManager.Cancel();
                return AddFieldTo(id);
            }

            try {
                _contentDefinitionService.AddFieldToPart(viewModel.DisplayName, viewModel.FieldTypeName, partViewModel.Name);
            }
            catch (Exception ex) {
                Services.Notifier.Information(T("The \"{0}\" field was not added. {1}", viewModel.DisplayName, ex.Message));
                Services.TransactionManager.Cancel();
                return AddFieldTo(id);
            }

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return AddFieldTo(id);
            }

            Services.Notifier.Information(T("The \"{0}\" field has been added.", viewModel.DisplayName));

            if (typeViewModel != null)
                return RedirectToAction("Edit", new { id });

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
