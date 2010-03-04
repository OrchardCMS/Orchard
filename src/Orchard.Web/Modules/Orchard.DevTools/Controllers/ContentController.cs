using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.DevTools.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.DevTools.Controllers {
    public class ContentController : Controller {
        private readonly IRepository<ContentTypeRecord> _contentTypeRepository;
        private readonly IContentManager _contentManager;

        public ContentController(
            IRepository<ContentTypeRecord> contentTypeRepository,
            IContentManager contentManager) {
            _contentTypeRepository = contentTypeRepository;
            _contentManager = contentManager;
        }

        public ActionResult Index() {
            return View(new ContentIndexViewModel {
                Items = _contentManager.Query().List(),
                Types = _contentTypeRepository.Table.ToList()
            });
        }

        public ActionResult Details(int id, int? version) {
            var model = new ContentDetailsViewModel {
                Item = version == null ? _contentManager.Get(id) : _contentManager.Get(id, VersionOptions.Number((int)version))
            };
            model.PartTypes = model.Item.ContentItem.Parts
                .Select(x => x.GetType())
                .SelectMany(x => AllTypes(x))
                .Distinct();
            model.DisplayModel = _contentManager.BuildDisplayModel(model.Item, "Detail");
            model.EditorModel = _contentManager.BuildEditorModel(model.Item);

            return View(model);
        }

        static IEnumerable<Type> AllTypes(Type type) {
            var scan = type;
            while (scan != null && scan != typeof(Object) && scan != typeof(ContentPart)) {
                yield return scan;
                scan = scan.BaseType;
            }
            foreach (var itf in type.GetInterfaces()) {
                yield return itf;
            }
        }
    }
}