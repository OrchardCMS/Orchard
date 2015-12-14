using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.ViewModels;
using Orchard.Taxonomies.Services;
using Orchard.UI.Notify;

namespace Orchard.Taxonomies.Controllers {

    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ITaxonomyService _taxonomyService;

        public AdminController(
            IOrchardServices services,
            ITaxonomyService taxonomyService) {
            Services = services;
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var taxonomies = _taxonomyService.GetTaxonomies();
            var entries = taxonomies.Select(CreateTaxonomyEntry).ToList();
            var model = new TaxonomyAdminIndexViewModel { Taxonomies = entries };
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

        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, T("Couldn't delete taxonomy")))
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
                    if(String.IsNullOrWhiteSpace(line)) {
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
                    } else if (level == previousLevel) {
                        // same parent term
                        if (parents.Any())
                            parents.Pop();

                        parents.Push(new TermPosition { Term = term });
                    } else if (level < previousLevel) {
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
                    } else {
                        term.Name = line;
                    }

                    var existing = _taxonomyService.GetTermByName(id, term.Name);

                    // a different term exist under the same parent term ?
                    if (existing != null && existing.Container.ContentItem.Record == term.Container.ContentItem.Record) {
                        Services.Notifier.Error(T("The term {0} already exists at this level", term.Name));
                        Services.TransactionManager.Cancel();
                        return View(new ImportViewModel {Taxonomy = taxonomy, Terms = terms});
                    }

                    _taxonomyService.ProcessPath(term);
                    Services.ContentManager.Create(term, VersionOptions.Published);

                    previousLevel = level;
                }
            }

            Services.Notifier.Information(T("The terms have been imported successfully."));

            return RedirectToAction("Index", "TermAdmin", new { taxonomyId = id });
        }

        private static TaxonomyEntry CreateTaxonomyEntry(TaxonomyPart taxonomy) {
            return new TaxonomyEntry {
                Id = taxonomy.Id,
                Name = taxonomy.Name,
                IsInternal = taxonomy.IsInternal,
                ContentItem = taxonomy.ContentItem,
                IsChecked = false,
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
