using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Settings;
using Orchard.Wikis.Models;
using Orchard.Wikis.ViewModels;

namespace Orchard.Wikis.Controllers
{
    public class PageController : Controller, IUpdateModel {
        private readonly IRepository<WikiPageRecord> _wikiPageRepository;
        private readonly IContentManager _contentManager;

        public PageController(IRepository<WikiPageRecord> wikiPageRepository, IContentManager contentManager) {
            _wikiPageRepository = wikiPageRepository;
            _contentManager = contentManager;
        }

        public ActionResult Index()
        {
            var pages = _wikiPageRepository.Fetch(x => true, o => o.Asc(x => x.Name));
            var model = new PageIndexViewModel {
                Pages = pages.Select(x => _contentManager.Get<WikiPage>(x.Id)).ToList()
            };
            return View(model);
        }

        public ActionResult Show(int id) {
            var model = new PageShowViewModel {
                Page = _contentManager.Get<WikiPage>(id)
            };
            model.Displays = _contentManager.GetDisplays(model.Page.ContentItem);
            return View(model);
        }

        public ActionResult Create() {
            return View(new PageCreateViewModel());
        }

        public ISite CurrentSite { get; set; }

        [HttpPost]
        public ActionResult Create(PageCreateViewModel model) {
            var page = _contentManager.New<WikiPage>("wikipage");
            page.Record.Name = model.Name;
            page.As<CommonPart>().Container = CurrentSite.ContentItem;
            _contentManager.Create(page);
            return RedirectToAction("show", new {page.ContentItem.Id});
        }


        public ActionResult Edit(int id) {
            var model = new PageEditViewModel {Page = _contentManager.Get<WikiPage>(id)};
            model.Editors = _contentManager.GetEditors(model.Page.ContentItem);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection input) {
            var model = new PageEditViewModel { Page = _contentManager.Get<WikiPage>(id) };
            model.Editors = _contentManager.UpdateEditors(model.Page.ContentItem, this);
            if (!TryUpdateModel(model, input.ToValueProvider()))
                return View(model);

            return RedirectToAction("show", new { id });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
    }
}
