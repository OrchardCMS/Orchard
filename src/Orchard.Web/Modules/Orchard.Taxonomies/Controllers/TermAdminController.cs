using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Taxonomies.Models;
using Orchard.Localization;
using Orchard.Taxonomies.ViewModels;
using Orchard.Taxonomies.Services;
using Orchard.UI.Admin;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using Orchard.Taxonomies.Helpers;

namespace Orchard.Taxonomies.Controllers {
    [ValidateInput(false), Admin]
    public class TermAdminController : Controller, IUpdateModel {
        private readonly ITaxonomyService _taxonomyService;

        public TermAdminController(IOrchardServices services, ITaxonomyService taxonomyService) {
            Services = services;
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(int taxonomyId) {
            var taxonomy = _taxonomyService.GetTaxonomy(taxonomyId);
            var terms = _taxonomyService.GetTerms(taxonomyId);
            var entries = terms.Select(term => term.CreateTermEntry()).ToList();
            var model = new TermAdminIndexViewModel { Terms = entries, Taxonomy = taxonomy, TaxonomyId = taxonomyId };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(FormCollection input) {
            var viewModel = new TermAdminIndexViewModel { Terms = new List<TermEntry>(), BulkAction = new TermsAdminIndexBulkAction() };

            if (!TryUpdateModel(viewModel)) {
                return View(viewModel);
            }

            var checkedEntries = viewModel.Terms.Where(t => t.IsChecked).ToList();
            switch (viewModel.BulkAction) {
                case TermsAdminIndexBulkAction.None:
                    break;
                case TermsAdminIndexBulkAction.Delete:
                    if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Couldn't delete term")))
                        return new HttpUnauthorizedResult();

                    foreach (var entry in checkedEntries) {
                        var term = _taxonomyService.GetTerm(entry.Id);
                        _taxonomyService.DeleteTerm(term);
                    }
                    break;
                case TermsAdminIndexBulkAction.Merge:
                    if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Couldn't delete term")))
                        return new HttpUnauthorizedResult();

                    return RedirectToAction("Merge", new {
                        taxonomyId = viewModel.TaxonomyId,
                        termIds = string.Join(",", checkedEntries.Select(o => o.Id))
                    });
                case TermsAdminIndexBulkAction.Move:
                    if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Couldn't move terms")))
                        return new HttpUnauthorizedResult();

                    return RedirectToAction("MoveTerm", new {
                        taxonomyId = viewModel.TaxonomyId,
                        termIds = string.Join(",", checkedEntries.Select(o => o.Id))
                    });
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Services.Notifier.Information(T("{0} term have been removed.", checkedEntries.Count));

            return RedirectToAction("Index", new { taxonomyId = viewModel.TaxonomyId });
        }

        public ActionResult SelectTerm(int taxonomyId) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTerm, T("Not allowed to create terms")))
                return new HttpUnauthorizedResult();

            var terms = _taxonomyService.GetTerms(taxonomyId).ToList();

            if (terms.Any()) {
                var model = new SelectTermViewModel {
                    Terms = terms,
                    SelectedTermId = -1
                };

                return View(model);
            }

            return RedirectToAction("Create", new { taxonomyId, parentTermId = -1 });
        }

        [HttpPost]
        public ActionResult SelectTerm(int taxonomyId, int selectedTermId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Not allowed to select terms")))
                return new HttpUnauthorizedResult();

            return RedirectToAction("Create", new { taxonomyId, parentTermId = selectedTermId });
        }

        public ActionResult MoveTerm(int taxonomyId, string termIds) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTerm, T("Not allowed to move terms")))
                return new HttpUnauthorizedResult();

            var terms = ResolveTermIds(termIds);

            if(!terms.Any())
                return HttpNotFound();

            var model = new MoveTermViewModel {
                Terms = (from t in _taxonomyService.GetTerms(taxonomyId)
                        from st in terms
                        where !(t.FullPath + "/").StartsWith(st.FullPath + "/")
                        select t).Distinct().ToList(),
                TermIds = terms.Select(x => x.Id),
                SelectedTermId = -1
            };

            return View(model);
        }
            
        [HttpPost]
        public ActionResult MoveTerm(int taxonomyId, int selectedTermId, string termIds) {
            if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Not allowed to move terms")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(taxonomyId);
            var parentTerm = _taxonomyService.GetTerm(selectedTermId);
            var terms = ResolveTermIds(termIds);

            foreach (var term in terms) {
                _taxonomyService.MoveTerm(taxonomy, term, parentTerm);	
            }
            
            return RedirectToAction("Index", new { taxonomyId });
        }

        public ActionResult Create(int taxonomyId, int parentTermId) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTerm, T("Not allowed to create terms")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(taxonomyId);
            var parentTerm = _taxonomyService.GetTerm(parentTermId);
            var term = _taxonomyService.NewTerm(taxonomy, parentTerm);
                        
            var model = Services.ContentManager.BuildEditor(term);
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(int taxonomyId, int parentTermId) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTerm, T("Couldn't create term")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(taxonomyId);
            var parentTerm = _taxonomyService.GetTerm(parentTermId);
            var term = _taxonomyService.NewTerm(taxonomy, parentTerm);

            // Create content item before updating so attached fields save correctly
            Services.ContentManager.Create(term, VersionOptions.Draft);

            var model = Services.ContentManager.UpdateEditor(term, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.ContentManager.Publish(term.ContentItem);
            Services.Notifier.Information(T("The {0} term has been created.", term.Name));

            return RedirectToAction("Index", "TermAdmin", new { taxonomyId });
        }

        public ActionResult Edit(int id) {

            if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Not allowed to manage taxonomies")))
                return new HttpUnauthorizedResult();

            var term = _taxonomyService.GetTerm(id);
            if (term == null)
                return HttpNotFound();

            var model = Services.ContentManager.BuildEditor(term);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageTaxonomies, T("Couldn't edit taxonomy")))
                return new HttpUnauthorizedResult();

            var term = _taxonomyService.GetTerm(id);

            if (term == null)
                return HttpNotFound();

            var contentItem = Services.ContentManager.Get(term.ContentItem.Id, VersionOptions.DraftRequired);
            var model = Services.ContentManager.UpdateEditor(contentItem, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.ContentManager.Publish(contentItem);
            _taxonomyService.ProcessPath(term);
            Services.Notifier.Information(T("Term information updated"));

            return RedirectToAction("Index", "TermAdmin", new { taxonomyId = term.TaxonomyId });
        }

        public ActionResult RenderTermSelect(int taxonomyId, int selectedTermId = -1) {
            if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Not allowed to manage terms")))
                return new HttpUnauthorizedResult();

            var model = new SelectTermViewModel {
                Terms = _taxonomyService.GetTerms(taxonomyId),
                SelectedTermId = selectedTermId
            };

            return PartialView(model);
        }

        private IList<TermPart> ResolveTermIds(string termIds) {
            var ids = termIds != null
                ? termIds.Split(',').Select(x => {
                    var id = 0;
                    int.TryParse(x, out id);
                    return id;
                }).Where(x => x != 0).ToList()
                : new List<int>(0);

            var terms = ids
                .Select(x => _taxonomyService.GetTerm(x))
                .Where(x => x != null)
                .ToList();

            return terms;
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
