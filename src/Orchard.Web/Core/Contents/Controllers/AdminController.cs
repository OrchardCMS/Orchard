using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Results;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Core.Contents.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly INotifier _notifier;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITransactionManager _transactionManager;

        public AdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ITransactionManager transactionManager) {
            Services = orchardServices;
            _notifier = notifier;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _transactionManager = transactionManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult List(ListContentsViewModel model) {
            if (model.ContainerId != null && _contentManager.GetLatest((int)model.ContainerId) == null)
                    return new NotFoundResult();

            const int pageSize = 20;
            var skip = (Math.Max(model.Page ?? 0, 1) - 1) * pageSize;

            var query = _contentManager.Query(VersionOptions.Latest, _contentDefinitionManager.ListTypeDefinitions().Select(ctd => ctd.Name).ToArray());

            if (!string.IsNullOrEmpty(model.TypeName)) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.TypeName);
                if (contentTypeDefinition == null)
                    return new NotFoundResult();

                model.TypeDisplayName = !string.IsNullOrWhiteSpace(contentTypeDefinition.DisplayName)
                                            ? contentTypeDefinition.DisplayName
                                            : contentTypeDefinition.Name;
                query = query.ForType(model.TypeName);
            }

            if (model.ContainerId != null)
                query = query.Join<CommonRecord>().Where(cr => cr.Container.Id == model.ContainerId);

            // Ordering
            //-- want something like 
            //switch (model.Options.OrderBy) {
            //    case ContentsOrder.Modified:
            //        query = query.OrderByDescending<CommonRecord, DateTime?>(cr => cr.ModifiedUtc);
            //        break;
            //    case ContentsOrder.Published:
            //        query = query.OrderByDescending<CommonRecord, DateTime?>(cr => cr.PublishedUtc);
            //        break;
            //    case ContentsOrder.Created:
            //        query = query.OrderByDescending<CommonRecord, DateTime?>(cr => cr.CreatedUtc);
            //        break;
            //}

            //-- but resorting to

            IEnumerable<ContentItem> contentItems = query.List();
            switch (model.Options.OrderBy) {
                case ContentsOrder.Modified:
                    contentItems = contentItems.OrderByDescending(ci => ci.VersionRecord.Id);
                    break;
                //case ContentsOrder.Published:
                // would be lying w/out a published date instead of a bool but that only comes with the common aspect
                //    contentItems = contentItems.OrderByDescending(ci => ci.VersionRecord.Published/*Date*/);
                //    break;
                case ContentsOrder.Created:
                    contentItems = contentItems.OrderByDescending(ci => ci.Id);
                    break;
            }

            //-- for the moment
            //-- because I'd rather do this

            //var contentItems = query.Slice(skip, pageSize);

            //-- instead of this (having the ordering and skip/take after the query)

            contentItems = contentItems.Skip(skip).Take(pageSize);

            model.Entries = contentItems.Select(BuildEntry).ToList();
            model.Options.SelectedFilter = model.TypeName;
            model.Options.FilterOptions = _contentDefinitionManager.ListTypeDefinitions()
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Key);

            return View("List", model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ContentOptions options) {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null) {
                routeValues["Options.OrderBy"] = options.OrderBy; //todo: don't hard-code the key
                if (_contentDefinitionManager.ListTypeDefinitions().Any(ctd => string.Equals(ctd.Name, options.SelectedFilter, StringComparison.OrdinalIgnoreCase))) {
                    routeValues["id"] = options.SelectedFilter;
                }
                else {
                    routeValues.Remove("id");
                }
            }

            return RedirectToAction("List", routeValues);
        }
        
        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult ListPOST(ContentOptions options, IEnumerable<int> itemIds, string returnUrl) {
            switch (options.BulkAction) {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.PublishNow:
                    if (!Services.Authorizer.Authorize(Permissions.PublishContent, T("Couldn't publish selected content.")))
                        return new HttpUnauthorizedResult();

                    foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                        _contentManager.Publish(item);
                        Services.ContentManager.Flush();
                    }
                    Services.Notifier.Information(T("Content successfully published."));
                    break;
                case ContentsBulkAction.Unpublish:
                    if (!Services.Authorizer.Authorize(Permissions.PublishContent, T("Couldn't unpublish selected content.")))
                        return new HttpUnauthorizedResult();

                    foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                        _contentManager.Unpublish(item);
                        Services.ContentManager.Flush();
                    }
                    Services.Notifier.Information(T("Content successfully unpublished."));
                    break;
                case ContentsBulkAction.Remove:
                    if (!Services.Authorizer.Authorize(Permissions.PublishContent, T("Couldn't delete selected content.")))
                        return new HttpUnauthorizedResult();

                    foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                        _contentManager.Remove(item);
                        Services.ContentManager.Flush();
                    }
                    Services.Notifier.Information(T("Content successfully removed."));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!String.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("List");
        }

        private ListContentsViewModel.Entry BuildEntry(ContentItem contentItem) {
            var entry = new ListContentsViewModel.Entry {
                ContentItem = contentItem,
                ContentItemMetadata = _contentManager.GetItemMetadata(contentItem),
                ViewModel = _contentManager.BuildDisplayModel(contentItem, "SummaryAdmin"),
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
                Types = _contentDefinitionManager.ListTypeDefinitions()
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
            _contentManager.Create(contentItem, VersionOptions.Draft);
            model.Content = _contentManager.UpdateEditorModel(contentItem, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                PrepareEditorViewModel(model.Content);
                return View("Create", model);
            }

            if (!contentItem.Has<IPublishingControlAspect>()) {
                _contentManager.Publish(contentItem);
                _notifier.Information(T("Created content item"));
            }

            return RedirectToAction("Edit", new RouteValueDictionary { { "Id", contentItem.Id } });
        }

        public ActionResult Edit(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return new NotFoundResult();

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

            if (contentItem == null)
                return new NotFoundResult();

            model.Content = _contentManager.UpdateEditorModel(contentItem, this);
            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                PrepareEditorViewModel(model.Content);
                return View("Edit", model);
            }

            //need to go about this differently - to know when to publish (IPlublishableAspect ?)
            if (!contentItem.Has<IPublishingControlAspect>())
                _contentManager.Publish(contentItem);

            return RedirectToAction("Edit", new RouteValueDictionary { { "Id", contentItem.Id } });
        }

        [HttpPost, ActionName("Remove")]
        public ActionResult RemovePOST(int id, string returnUrl) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);
            if (contentItem != null)
                _contentManager.Remove(contentItem);

            if (!String.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Publish(int id, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.PublishContent, T("Couldn't publish content")))
                return new HttpUnauthorizedResult();

            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return new NotFoundResult();

            _contentManager.Publish(contentItem);
            Services.ContentManager.Flush();
            Services.Notifier.Information(T("{0} successfully published.", contentItem.TypeDefinition.DisplayName));

            if (!String.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Unpublish(int id, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.PublishContent, T("Couldn't unpublish content")))
                return new HttpUnauthorizedResult();

            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return new NotFoundResult();

            _contentManager.Unpublish(contentItem);
            Services.ContentManager.Flush();
            Services.Notifier.Information(T("{0} successfully unpublished.", contentItem.TypeDefinition.DisplayName));

            if (!String.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("List");
        }

        private static void PrepareEditorViewModel(ContentItemViewModel itemViewModel) {
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

    public class FormValueRequiredAttribute : ActionMethodSelectorAttribute {
        private readonly string _submitButtonName;

        public FormValueRequiredAttribute(string submitButtonName) {
            _submitButtonName = submitButtonName;
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
            var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
            return !string.IsNullOrEmpty(value);
        }
    }
}
