using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Sandbox.Models;
using Orchard.Sandbox.ViewModels;
using Orchard.Settings;

namespace Orchard.Sandbox.Controllers
{
    public class PageController : Controller, IUpdateModel {
        private readonly IRepository<SandboxPageRecord> _pageRepository;
        private readonly IContentManager _contentManager;

        public PageController(IRepository<SandboxPageRecord> pageRepository, IContentManager contentManager) {
            _pageRepository = pageRepository;
            _contentManager = contentManager;
        }

        public ActionResult Index()
        {
            var pages = _pageRepository.Fetch(x => true, o => o.Asc(x => x.Name));
            var model = new PageIndexViewModel {
                Pages = pages.Select(x => _contentManager.Get<SandboxPage>(x.Id)).ToList()
            };
            return View(model);
        }

        public ActionResult Show(int id) {
            var model = new PageShowViewModel {
                Page = _contentManager.Get<SandboxPage>(id)
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
            var page = _contentManager.Create<SandboxPage>("sandboxpage", item => {
                item.Record.Name = model.Name;
                item.As<CommonAspect>().Container = CurrentSite.ContentItem;
            });
            return RedirectToAction("show", new { page.ContentItem.Id });
        }


        public ActionResult Edit(int id) {
            var model = new PageEditViewModel {Page = _contentManager.Get<SandboxPage>(id)};
            model.Editors = _contentManager.GetEditors(model.Page.ContentItem);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection input) {
            var model = new PageEditViewModel { Page = _contentManager.Get<SandboxPage>(id) };
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
