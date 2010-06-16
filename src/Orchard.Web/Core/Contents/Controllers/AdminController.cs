using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Services;
using Orchard.Core.Contents.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Core.Contents.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
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

        #region Types

        public ActionResult Index() {
            return Types();
        }

        public ActionResult Types() {
            return View("Types", new ListContentTypesViewModel {
                Types = _contentDefinitionService.GetTypeDefinitions()
            });
        }

        public ActionResult CreateType(CreateTypeViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentType, T("Not allowed to create a content type.")))
                return new HttpUnauthorizedResult();

            return View(viewModel);
        }

        [HttpPost, ActionName("CreateType")]
        public ActionResult CreateTypePOST(CreateTypeViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.CreateContentType, T("Not allowed to create a content type.")))
                return new HttpUnauthorizedResult();

            var model = new ContentTypeDefinitionStub();
            TryUpdateModel(model);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(viewModel);
            }
            
            _contentDefinitionService.AddTypeDefinition(model);
            
            return RedirectToAction("Index");
        }

        #endregion

        #region Content
        #endregion

        public ActionResult List(ListContentsViewModel model) {
            const int pageSize = 20;
            var skip = (Math.Max(model.Page ?? 0, 1) - 1) * pageSize;

            var query = _contentManager.Query(VersionOptions.Latest);

            if (!string.IsNullOrEmpty(model.Id)) {
                query = query.ForType(model.Id);
            }

            var contentItems = query.Slice(skip, pageSize);

            model.Entries = contentItems.Select(BuildEntry).ToList();

            return View("List", model);
        }

        private ListContentsViewModel.Entry BuildEntry(ContentItem contentItem) {
            var entry = new ListContentsViewModel.Entry {
                ContentItem = contentItem,
                ContentItemMetadata = _contentManager.GetItemMetadata(contentItem),
                ViewModel = _contentManager.BuildDisplayModel(contentItem, "List"),
            };
            if (string.IsNullOrEmpty(entry.ContentItemMetadata.DisplayText)) {
                entry.ContentItemMetadata.DisplayText = string.Format("[{0}#{1}]", contentItem.ContentType, contentItem.Id);
            }
            if (entry.ContentItemMetadata.EditorRouteValues == null) {
                entry.ContentItemMetadata.EditorRouteValues = new RouteValueDictionary {
                    {"Area", "Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Edit"},
                    {"Id", contentItem.Id}
                };
            }
            return entry;
        }

        ActionResult CreatableTypeList() {
            var model = new ListContentTypesViewModel {
                Types = _contentDefinitionService.GetTypeDefinitions()
            };

            return View("CreatableTypeList", model);
        }

        public ActionResult Create(string id) {
            if (string.IsNullOrEmpty(id))
                return CreatableTypeList();

            var contentItem = _contentManager.New(id);
            var model = new CreateItemViewModel {
                Id = id,
                Content = _contentManager.BuildEditorModel(contentItem)
            };
            PrepareEditorViewModel(model.Content);
            return View("Create", model);
        }


        [HttpPost]
        public ActionResult Create(CreateItemViewModel model) {
            //todo: need to integrate permissions into generic content management
            var contentItem = _contentManager.New(model.Id);
            model.Content = _contentManager.UpdateEditorModel(contentItem, this);
            if (ModelState.IsValid) {
                _contentManager.Create(contentItem, VersionOptions.Draft);
                model.Content = _contentManager.UpdateEditorModel(contentItem, this);
            }
            if (ModelState.IsValid) {
                _contentManager.Publish(contentItem);
            }
            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                PrepareEditorViewModel(model.Content);
                return View("Create", model);
            }

            _notifier.Information(T("Created content item"));
            return RedirectToAction("Edit", new RouteValueDictionary { { "Id", contentItem.Id } });
        }

        public ActionResult Edit(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);
            var model = new EditItemViewModel {
                Id = id,
                Content = _contentManager.BuildEditorModel(contentItem)
            };
            PrepareEditorViewModel(model.Content);
            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(EditItemViewModel model) {
            var contentItem = _contentManager.Get(model.Id, VersionOptions.DraftRequired);
            model.Content = _contentManager.UpdateEditorModel(contentItem, this);
            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                PrepareEditorViewModel(model.Content);
                return View("Edit", model);
            }
            _contentManager.Publish(contentItem);
            return RedirectToAction("Edit", new RouteValueDictionary { { "Id", contentItem.Id } });
        }

        private void PrepareEditorViewModel(ContentItemViewModel itemViewModel) {
            if (string.IsNullOrEmpty(itemViewModel.TemplateName)) {
                itemViewModel.TemplateName = "Items/Contents.Item";
            }
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
