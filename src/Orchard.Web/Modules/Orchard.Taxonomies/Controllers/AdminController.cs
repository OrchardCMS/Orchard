using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.ViewModels;
using Orchard.Taxonomies.Services;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.DisplayManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
using Orchard.Mvc.Html;
using Orchard.Utility.Extensions;
using Orchard.Mvc.Extensions;
using System.Web.Routing;

namespace Orchard.Taxonomies.Controllers {

    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ITaxonomyService _taxonomyService;
        private readonly ISiteService _siteService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;
        private string contentType = "Taxonomy";
        public AdminController(
            IOrchardServices services,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ITransactionManager transactionManager,
            ITaxonomyService taxonomyService,
            ISiteService siteService,
            IShapeFactory shapeFactory) {
            Services = services;
            _siteService = siteService;
            _taxonomyService = taxonomyService;
            _contentDefinitionManager = contentDefinitionManager;
            _transactionManager = transactionManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        protected virtual ISite CurrentSite { get; private set; }

        public Localizer T { get; set; }

        public ActionResult Index(PagerParameters pagerParameters) {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var taxonomies = _taxonomyService.GetTaxonomiesQuery().Slice(pager.GetStartIndex(), pager.PageSize);

            var pagerShape = Shape.Pager(pager).TotalItemCount(_taxonomyService.GetTaxonomiesQuery().Count());

            var entries = taxonomies
                    .Select(CreateTaxonomyEntry)
                    .ToList();

            var model = new TaxonomyAdminIndexViewModel { Taxonomies = entries, Pager = pagerShape };

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(FormCollection input) {
            var viewModel = new TaxonomyAdminIndexViewModel { Taxonomies = new List<TaxonomyEntry>(), BulkAction = new TaxonomiesAdminIndexBulkAction() };

            if (!TryUpdateModel(viewModel)) {
                return View(viewModel);
            }

            var checkedEntries = viewModel.Taxonomies.Where(t => t.IsChecked);
            switch (viewModel.BulkAction) {
                case TaxonomiesAdminIndexBulkAction.None:
                    break;
                case TaxonomiesAdminIndexBulkAction.Delete:
                    if (!Services.Authorizer.Authorize(Permissions.ManageTaxonomies, T("Couldn't delete taxonomy")))
                        return new HttpUnauthorizedResult();

                    foreach (var entry in checkedEntries) {
                        _taxonomyService.DeleteTaxonomy(_taxonomyService.GetTaxonomy(entry.Id));
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index");
        }


        public ActionResult Create() {
            var contentItem = _contentManager.New(contentType);

            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, contentItem, T("Cannot create taxonomies")))
                return new HttpUnauthorizedResult();
            var model = _contentManager.BuildEditor(contentItem);
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [Mvc.FormValueRequired("submit.Save")]
        public ActionResult CreatePOST(string returnUrl) {
            return CreatePOST(returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _contentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Create")]
        [Mvc.FormValueRequired("submit.Publish")]
        public ActionResult CreateAndPublishPOST(string returnUrl) {

            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = _contentManager.New(contentType);

            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, dummyContent, T("Couldn't create taxonomies")))
                return new HttpUnauthorizedResult();

            return CreatePOST(returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private ActionResult CreatePOST(string returnUrl, Action<ContentItem> conditionallyPublish) {
            var contentItem = _contentManager.New(contentType);

            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, contentItem, T("Couldn't create taxonomies")))
                return new HttpUnauthorizedResult();

            _contentManager.Create(contentItem, VersionOptions.Draft);

            var model = _contentManager.UpdateEditor(contentItem, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            conditionallyPublish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("Your content has been created.")
                : T("Your {0} has been created.", contentItem.TypeDefinition.DisplayName));
            if (!string.IsNullOrEmpty(returnUrl)) {
                return this.RedirectLocal(returnUrl);
            }
            var adminRouteValues = _contentManager.GetItemMetadata(contentItem).AdminRouteValues;
            return RedirectToRoute(adminRouteValues);
        }

        public ActionResult Edit(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, contentItem, T("Cannot edit content")))
                return new HttpUnauthorizedResult();

            var model = _contentManager.BuildEditor(contentItem);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Save")]
        public ActionResult EditPOST(int id, string returnUrl) {
            return EditPOST(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _contentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Publish")]
        public ActionResult EditAndPublishPOST(int id, string returnUrl) {
            var content = _contentManager.Get(id, VersionOptions.Latest);

            if (content == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, content, T("Couldn't publish content")))
                return new HttpUnauthorizedResult();

            return EditPOST(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private ActionResult EditPOST(int id, string returnUrl, Action<ContentItem> conditionallyPublish) {
            var contentItem = _contentManager.Get(id, VersionOptions.DraftRequired);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, contentItem, T("Couldn't edit content")))
                return new HttpUnauthorizedResult();

            string previousRoute = null;
            if (contentItem.Has<IAliasAspect>()
                && !string.IsNullOrWhiteSpace(returnUrl)
                && Request.IsLocalUrl(returnUrl)
                // only if the original returnUrl is the content itself
                && String.Equals(returnUrl, Url.ItemDisplayUrl(contentItem), StringComparison.OrdinalIgnoreCase)
                ) {
                previousRoute = contentItem.As<IAliasAspect>().Path;
            }

            var model = _contentManager.UpdateEditor(contentItem, this);
            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View("Edit", model);
            }

            conditionallyPublish(contentItem);

            if (!string.IsNullOrWhiteSpace(returnUrl)
                && previousRoute != null
                && !String.Equals(contentItem.As<IAliasAspect>().Path, previousRoute, StringComparison.OrdinalIgnoreCase)) {
                returnUrl = Url.ItemDisplayUrl(contentItem);
            }

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("Your content has been saved.")
                : T("Your {0} has been saved.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Edit", new RouteValueDictionary { { "Id", contentItem.Id } }));
        }


        [HttpPost]
        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageTaxonomies, T("Couldn't delete taxonomy")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(id);
            if (taxonomy == null) {
                return HttpNotFound();
            }

            _taxonomyService.DeleteTaxonomy(taxonomy);

            return RedirectToAction("Index");
        }

        public ActionResult Import(int id, string terms) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, T("Couldn't import terms")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(id);

            if (taxonomy == null) {
                return HttpNotFound();
            }

            return View(new ImportViewModel { Taxonomy = taxonomy, Terms = terms });
        }

        [HttpPost, ActionName("Import")]
        public ActionResult ImportPost(int id, string terms) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, T("Couldn't import terms")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(id);

            if (taxonomy == null) {
                return HttpNotFound();
            }

            var topTerm = new TermPosition();

            using (var reader = new StringReader(terms)) {
                string line;
                var previousLevel = 0;
                var parents = new Stack<TermPosition>();
                TermPosition parentTerm = null;
                while (null != (line = reader.ReadLine())) {

                    // ignore empty lines
                    if (String.IsNullOrWhiteSpace(line)) {
                        continue;
                    }

                    // compute level from tabs
                    var level = 0;
                    while (line[level] == '\t') level++; // number of tabs to know the level

                    // create a new term content item
                    var term = _taxonomyService.NewTerm(taxonomy);

                    // detect parent term
                    if (level == previousLevel + 1) {
                        parentTerm = parents.Peek();
                        parents.Push(new TermPosition { Term = term });
                    }
                    else if (level == previousLevel) {
                        // same parent term
                        if (parents.Any())
                            parents.Pop();

                        parents.Push(new TermPosition { Term = term });
                    }
                    else if (level < previousLevel) {
                        for (var i = previousLevel; i >= level; i--)
                            parents.Pop();

                        parentTerm = parents.Any() ? parents.Peek() : null;
                        parents.Push(new TermPosition { Term = term });
                    }

                    // increment number of children
                    if (parentTerm == null) {
                        parentTerm = topTerm;
                    }

                    parentTerm.Position++;
                    term.Weight = 10 - parentTerm.Position;

                    term.Container = parentTerm.Term == null ? taxonomy.ContentItem : parentTerm.Term.ContentItem;

                    line = line.Trim();
                    var scIndex = line.IndexOf(';'); // seek first semi-colon to extract term and slug

                    // is there a semi-colon
                    if (scIndex != -1) {
                        term.Name = line.Substring(0, scIndex);
                        term.Slug = line.Substring(scIndex + 1);
                    }
                    else {
                        term.Name = line;
                    }

                    var existing = _taxonomyService.GetTermByName(id, term.Name);

                    // a different term exist under the same parent term ?
                    if (existing != null && existing.Container.ContentItem.Record == term.Container.ContentItem.Record) {
                        Services.Notifier.Error(T("The term {0} already exists at this level", term.Name));
                        Services.TransactionManager.Cancel();
                        return View(new ImportViewModel { Taxonomy = taxonomy, Terms = terms });
                    }

                    _taxonomyService.ProcessPath(term);
                    Services.ContentManager.Create(term, VersionOptions.Published);

                    previousLevel = level;
                }
            }

            Services.Notifier.Success(T("The terms have been imported successfully."));

            return RedirectToAction("Index", "TermAdmin", new { taxonomyId = id });
        }

        private static TaxonomyEntry CreateTaxonomyEntry(TaxonomyPart taxonomy) {
            return new TaxonomyEntry {
                Id = taxonomy.Id,
                Name = taxonomy.Name,
                IsInternal = taxonomy.IsInternal,
                ContentItem = taxonomy.ContentItem,
                IsChecked = false,
                HasDraft = taxonomy.ContentItem.HasDraft(),
            };
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        private class TermPosition {
            public TermPart Term { get; set; }
            public int Position { get; set; }
        }
    }
}
