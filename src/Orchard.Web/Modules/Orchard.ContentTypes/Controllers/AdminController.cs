using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Services;
using Orchard.ContentTypes.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Results;
using Orchard.UI.Notify;

namespace Orchard.ContentTypes.Controllers {
    public class AdminController : Controller {
        private readonly INotifier _notifier;
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;

        public AdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentDefinitionService contentDefinitionService,
            IContentManager contentManager,
            ITransactionManager transactionManager) {
            Services = orchardServices;
            _notifier = notifier;
            _contentDefinitionService = contentDefinitionService;
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
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

            var model = new ContentTypeDefinition("");
            TryUpdateModel(model);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }

            _contentDefinitionService.AddTypeDefinition(model);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(string id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var contentTypeDefinition = _contentDefinitionService.GetTypeDefinition(id);

            if (contentTypeDefinition == null)
                return new NotFoundResult();

            return View(new EditTypeViewModel(contentTypeDefinition));
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(string id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to edit a content type.")))
                return new HttpUnauthorizedResult();

            var contentTypeDefinition = _contentDefinitionService.GetTypeDefinition(id);

            if (contentTypeDefinition == null)
                return new NotFoundResult();

            var viewModel = new EditTypeViewModel();
            TryUpdateModel(viewModel);

            if (!ModelState.IsValid)
                return Edit(id);

            var contentTypeDefinitionParts = viewModel.Parts.Select(GenerateTypePart).ToList();
            if (viewModel.Fields.Any())
                contentTypeDefinitionParts.Add(GenerateTypePart(viewModel));

            //todo: apply the changes along the lines of but definately not resembling
            // for now this _might_ just get a little messy -> 
            _contentDefinitionService.AlterTypeDefinition(
                new ContentTypeDefinition(
                    viewModel.Name,
                    viewModel.DisplayName,
                    contentTypeDefinitionParts,
                    viewModel.Settings
                    )
                );

            return RedirectToAction("Index");
        }

        private static ContentTypeDefinition.Part GenerateTypePart(EditTypePartViewModel viewModel) {
            return new ContentTypeDefinition.Part(
                new ContentPartDefinition(
                    viewModel.PartDefinition.Name,
                    viewModel.PartDefinition.Fields.Select(
                        f => new ContentPartDefinition.Field(
                                 new ContentFieldDefinition(f.FieldDefinition.Name),
                                 f.Name,
                                 f.Settings
                                 )
                        ),
                    viewModel.PartDefinition.Settings
                    ),
                viewModel.Settings
                );
        }

        private static ContentTypeDefinition.Part GenerateTypePart(EditTypeViewModel viewModel) {
            return new ContentTypeDefinition.Part(
                new ContentPartDefinition(
                    viewModel.Name,
                    viewModel.Fields.Select(
                        f => new ContentPartDefinition.Field(
                                 new ContentFieldDefinition(f.FieldDefinition.Name),
                                 f.Name,
                                 f.Settings
                                 )
                        ),
                    null
                    ),
                null
                );
        }
        #endregion

        #region Parts

        public ActionResult EditPart(string id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to edit a part.")))
                return new HttpUnauthorizedResult();

            var contentPartDefinition = _contentDefinitionService.GetPartDefinition(id);

            if (contentPartDefinition == null)
                return new NotFoundResult();

            return View(new EditPartViewModel(contentPartDefinition));
        }

        [HttpPost, ActionName("EditPart")]
        public ActionResult EditPartPOST(string id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentTypes, T("Not allowed to edit a part.")))
                return new HttpUnauthorizedResult();

            var contentPartDefinition = _contentDefinitionService.GetPartDefinition(id);

            if (contentPartDefinition == null)
                return new NotFoundResult();

            var viewModel = new EditPartViewModel();
            TryUpdateModel(viewModel);

            if (!ModelState.IsValid)
                return EditPart(id);

            //todo: apply the changes along the lines of but definately not resembling
            // for now this _might_ just get a little messy -> 
            _contentDefinitionService.AlterPartDefinition(GeneratePart(viewModel));

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

        private static ContentPartDefinition GeneratePart(EditPartViewModel viewModel) {
            return new ContentPartDefinition(
                viewModel.Name,
                viewModel.Fields.Select(
                    f => new ContentPartDefinition.Field(
                             new ContentFieldDefinition(f.FieldDefinition.Name),
                             f.Name,
                             f.Settings
                             )
                    ),
                viewModel.Settings
                );
        }

        #endregion

    }
}
