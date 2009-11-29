using System;
using System.Web;
using System.Web.Mvc;
using Orchard.CmsPages.Services;

namespace Orchard.CmsPages.Controllers {
    public class TemplatesController : Controller {
        private readonly IPageManager _pageManager;
        private readonly ISlugConstraint _slugConstraint;

        public TemplatesController(IPageManager pageManager, ISlugConstraint slugConstraint) {
            _pageManager = pageManager;
            _slugConstraint = slugConstraint;
        }

        public ActionResult Show(string slug) {
            //TODO: Validation
            if (slug == null) {
                throw new ArgumentNullException("slug");
            }

            var correctedSlug = _slugConstraint.LookupPublishedSlug(slug);

            var revision = _pageManager.GetPublishedBySlug(correctedSlug);
            if (revision == null) {
                //TODO: Error message
                throw new HttpException(404, "slug " + slug + " was not found");
            }
            return View(revision.TemplateName, revision);
        }

        
    }
}
