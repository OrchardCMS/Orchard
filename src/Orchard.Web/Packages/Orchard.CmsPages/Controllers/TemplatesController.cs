using System;
using System.Web;
using System.Web.Mvc;
using Orchard.CmsPages.Services;

namespace Orchard.CmsPages.Controllers {
    public class TemplatesController : Controller {
        private readonly IPageManager _pageManager;

        public TemplatesController(IPageManager pageManager) {
            _pageManager = pageManager;
        }

        public ActionResult Show(string slug) {
            //TODO: Validation
            if (slug == null) {
                throw new ArgumentNullException("slug");
            }

            var revision = _pageManager.GetPublishedBySlug(slug);
            if (revision == null) {
                //TODO: Error message
                throw new HttpException(404, "slug " + slug + " was not found");
            }
            return View(revision.TemplateName, revision);
        }

        
    }
}
