using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Services;
using Orchard.ContentTypes.ViewModels;
using Orchard.Localization;
using Orchard.Mvc.Results;

namespace Orchard.ContentTypes.Controllers {
    public class AdminController : Controller {
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionEditorEvents _extendViewModels;

        public AdminController(
            IOrchardServices orchardServices,
            IContentDefinitionService contentDefinitionService,
            IContentDefinitionManager contentDefinitionManager,
            IContentDefinitionEditorEvents extendViewModels) {
            Services = orchardServices;
            _contentDefinitionService = contentDefinitionService;
            _contentDefinitionManager = contentDefinitionManager;
            _extendViewModels = extendViewModels;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ActionResult Index() {
            return List();
        }

        #region Types

        public ActionResult List() {
            return View("List", new ListContentTypesViewModel {
                Types = _contentDefinitionService.GetTypeDefinitions()
            });
        }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to create a content type.")))
                return new HttpUnauthorizedResult();

            return View(new CreateTypeViewModel());
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(CreateTypeViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to create a content type.")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid)
                return View(viewModel);

            _contentDefinitionService.AddTypeDefinition(viewModel.DisplayName);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(string id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var contentTypeDefinition = _contentDefinitionService.GetTypeDefinition(id);

            if (contentTypeDefinition == null)
                return new NotFoundResult();

            var viewModel = new EditTypeViewModel(contentTypeDefinition);
            viewModel.Parts = viewModel.Parts.ToArray();
            viewModel.Templates = _extendViewModels.TypeEditor(contentTypeDefinition);

            var entries = viewModel.Parts.Join(contentTypeDefinition.Parts,
                                               m => m.PartDefinition.Name,
                                               d => d.PartDefinition.Name,
                                               (model, definition) => new {model, definition});
            foreach (var entry in entries) {
                entry.model.Templates = _extendViewModels.TypePartEditor(entry.definition);

                var fields = entry.model.PartDefinition.Fields.Join(entry.definition.PartDefinition.Fields,
                                   m => m.FieldDefinition.Name,
                                   d => d.FieldDefinition.Name,
                                   (model, definition) => new { model, definition });

                foreach (var field in fields) {
                    field.model.Templates = _extendViewModels.PartFieldEditor(field.definition);
                }
            }


            //Oy, this action is getting massive :(
            //todo: put this action on a diet
            var contentPartDefinition = _contentDefinitionService.GetPartDefinition(id);
            if (contentPartDefinition != null) {
                viewModel.Fields = viewModel.Fields.ToArray();
                var fields = viewModel.Fields.Join(contentPartDefinition.Fields,
                                    m => m.FieldDefinition.Name,
                                    d => d.FieldDefinition.Name,
                                    (model, definition) => new { model, definition });

                foreach (var field in fields) {
                    field.model.Templates = _extendViewModels.PartFieldEditor(field.definition);
                }
            }
            
            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(EditTypeViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var contentTypeDefinition = _contentDefinitionService.GetTypeDefinition(viewModel.Name);

            if (contentTypeDefinition == null)
                return new NotFoundResult();

            var updater = new Updater(this);
            _contentDefinitionManager.AlterTypeDefinition(viewModel.Name, typeBuilder => {

                typeBuilder.DisplayedAs(viewModel.DisplayName);

                // allow extensions to alter type configuration
                viewModel.Templates = _extendViewModels.TypeEditorUpdate(typeBuilder, updater);

                foreach (var entry in viewModel.Parts.Select((part, index) => new { part, index })) {
                    var partViewModel = entry.part;

                    // enable updater to be aware of changing part prefix
                    var firstHalf = "Parts[" + entry.index + "].";
                    updater._prefix = secondHalf => firstHalf + secondHalf;

                    // allow extensions to alter typePart configuration
                    typeBuilder.WithPart(entry.part.PartDefinition.Name, typePartBuilder => {
                        partViewModel.Templates = _extendViewModels.TypePartEditorUpdate(typePartBuilder, updater);
                    });

                    if (!partViewModel.PartDefinition.Fields.Any())
                        continue;

                    _contentDefinitionManager.AlterPartDefinition(partViewModel.PartDefinition.Name, partBuilder => {
                        foreach (var fieldEntry in partViewModel.PartDefinition.Fields.Select((field, index) => new { field, index })) {
                            var fieldViewModel = fieldEntry.field;

                            // enable updater to be aware of changing field prefix
                            var firstHalfFieldName = "Fields[" + fieldEntry.index + "].";
                            updater._prefix = secondHalf => firstHalfFieldName + secondHalf;

                            // allow extensions to alter partField configuration
                            partBuilder.WithField(fieldViewModel.Name, partFieldBuilder => {
                                fieldViewModel.Templates = _extendViewModels.PartFieldEditorUpdate(partFieldBuilder, updater);
                            });
                        }
                    });
                }

                if (viewModel.Fields.Any()) {
                    _contentDefinitionManager.AlterPartDefinition(viewModel.Name, partBuilder => {
                        foreach (var fieldEntry in viewModel.Fields.Select((field, index) => new { field, index })) {
                            var fieldViewModel = fieldEntry.field;

                            // enable updater to be aware of changing field prefix
                            var firstHalfFieldName = "Fields[" + fieldEntry.index + "].";
                            updater._prefix = secondHalf => firstHalfFieldName + secondHalf;

                            // allow extensions to alter partField configuration
                            partBuilder.WithField(fieldViewModel.Name, partFieldBuilder => {
                                fieldViewModel.Templates = _extendViewModels.PartFieldEditorUpdate(partFieldBuilder, updater);
                            });
                        }
                    });
                }
            });

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Parts

        public ActionResult EditPart(string id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to edit a part.")))
                return new HttpUnauthorizedResult();

            var contentPartDefinition = _contentDefinitionService.GetPartDefinition(id);

            if (contentPartDefinition == null)
                return new NotFoundResult();

            var viewModel = new EditPartViewModel(contentPartDefinition) {
                Templates = _extendViewModels.PartEditor(contentPartDefinition)
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("EditPart")]
        public ActionResult EditPartPOST(EditPartViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to edit a part.")))
                return new HttpUnauthorizedResult();

            var contentPartDefinition = _contentDefinitionService.GetPartDefinition(viewModel.Name);

            if (contentPartDefinition == null)
                return new NotFoundResult();

            var updater = new Updater(this);
            _contentDefinitionManager.AlterPartDefinition(viewModel.Name, partBuilder => {
                // allow extensions to alter part configuration
                viewModel.Templates = _extendViewModels.PartEditorUpdate(partBuilder, updater);
            });

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }

        public ActionResult AddFieldTo(string id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to edit a part.")))
                return new HttpUnauthorizedResult();

            var contentPartDefinition = _contentDefinitionService.GetPartDefinition(id);

            if (contentPartDefinition == null) {
                //id passed in might be that of a type w/ no implicit field
                var contentTypeDefinition = _contentDefinitionService.GetTypeDefinition(id);
                if (contentTypeDefinition != null)
                    contentPartDefinition = new ContentPartDefinition(id);
                else
                    return new NotFoundResult();
            }

            var viewModel = new AddFieldViewModel {
                Part = new EditPartViewModel(contentPartDefinition),
                Fields = _contentDefinitionService.GetFieldDefinitions()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("AddFieldTo")]
        public ActionResult AddFieldToPOST(string id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to edit a part.")))
                return new HttpUnauthorizedResult();

            var viewModel = new AddFieldViewModel();
            TryUpdateModel(viewModel);

            var contentPartDefinition = _contentDefinitionService.GetPartDefinition(id);
            var contentTypeDefinition = _contentDefinitionService.GetTypeDefinition(id);

            if (!ModelState.IsValid)
                return AddFieldTo(id);

            if (contentPartDefinition == null) {
                //id passed in might be that of a type w/ no implicit field
                if (contentTypeDefinition != null) {
                    contentPartDefinition = new ContentPartDefinition(id);
                    var contentTypeDefinitionParts = contentTypeDefinition.Parts.ToList();
                    contentTypeDefinitionParts.Add(new ContentTypeDefinition.Part(contentPartDefinition, null));
                    _contentDefinitionService.AlterTypeDefinition(
                        new ContentTypeDefinition(
                            contentTypeDefinition.Name,
                            contentTypeDefinition.DisplayName,
                            contentTypeDefinitionParts,
                            contentTypeDefinition.Settings
                            )
                        );
                }
                else {
                    return new NotFoundResult();
                }
            }

            var contentPartFields = contentPartDefinition.Fields.ToList();
            contentPartFields.Add(new ContentPartDefinition.Field(new ContentFieldDefinition(viewModel.FieldTypeName), viewModel.DisplayName, null));
            _contentDefinitionService.AlterPartDefinition(new ContentPartDefinition(contentPartDefinition.Name, contentPartFields, contentPartDefinition.Settings));

            if (contentTypeDefinition != null)
                return RedirectToAction("Edit", new { id });

            return RedirectToAction("EditPart", new { id });
        }

        #endregion

        class Updater : IUpdateModel {
            private readonly AdminController _thunk;

            public Updater(AdminController thunk) {
                _thunk = thunk;
            }

            public Func<string, string> _prefix = x => x;

            public bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class {
                return _thunk.TryUpdateModel(model, _prefix(prefix), includeProperties, excludeProperties);
            }

            public void AddModelError(string key, LocalizedString errorMessage) {
                _thunk.ModelState.AddModelError(_prefix(key), errorMessage.ToString());
            }
        }

    }
}
