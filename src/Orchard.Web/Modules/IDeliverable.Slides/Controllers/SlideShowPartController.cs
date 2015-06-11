using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Models;
using IDeliverable.Slides.Services;
using Orchard.ContentManagement;
using Orchard.Layouts.Services;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Layouts.Helpers;

namespace IDeliverable.Slides.Controllers
{
    [Admin]
    public class SlideShowPartController : Controller
    {
        private readonly IObjectStore _objectStore;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly ISlidesSerializer _serializer;

        public SlideShowPartController(IObjectStore objectStore, IContentManager contentManager, INotifier notifier, ISlidesSerializer serializer)
        {
            _objectStore = objectStore;
            _contentManager = contentManager;
            _notifier = notifier;
            _serializer = serializer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult AddSlides(int id, string slides)
        {
            var part = _contentManager.Get<SlideShowPart>(id, VersionOptions.DraftRequired);

            if (slides == null)
            {
                return RedirectToEditor(part);
            }

            var addedSlides = _objectStore.Get<IList<Slide>>(slides);
            var currentSlides = GetSlides(part).ToList();

            currentSlides.AddRange(addedSlides);
            SetSlides(part, currentSlides);

            return RedirectToEditor(part, T.Plural("That slide has been added.", "Those slides have been added.", addedSlides.Count));
        }

        public ActionResult EditSlide(int id, int index)
        {
            var slidesPart = _contentManager.Get<SlideShowPart>(id, VersionOptions.Latest);
            var slides = GetSlides(slidesPart).ToList();
            var slide = slides.ElementAt(index);
            var key = _objectStore.GenerateKey();
            var returnUrl = Url.Action("UpdateSlide", new { id = id, key = key, index = index });

            _objectStore.Set(key, slide);
            return RedirectToAction("Edit", "Slide", new { id = key, returnUrl = returnUrl });
        }

        public ActionResult UpdateSlide(int id, int index, string key)
        {
            var slidesPart = _contentManager.Get<SlideShowPart>(id, VersionOptions.Latest);
            var slide = _objectStore.Get<Slide>(key);
            var slides = GetSlides(slidesPart).ToList();

            slides[index] = slide;
            SetSlides(slidesPart, slides);

            return Redirect(Url.ItemEditUrl(slidesPart));
        }

        [HttpPost]
        public ActionResult DeleteSlide(int id, int index)
        {
            var slidesPart = _contentManager.Get<SlideShowPart>(id, VersionOptions.DraftRequired);
            var slides = GetSlides(slidesPart).ToList();

            slides.RemoveAt(index);
            SetSlides(slidesPart, slides);

            return RedirectToEditor(slidesPart, T("That slide has been deleted."));
        }

        private RedirectToRouteResult RedirectToEditor(SlideShowPart part, LocalizedString message = null)
        {
            if (message != null)
                _notifier.Information(message);

            return RedirectToRoute(_contentManager.GetItemMetadata(part).EditorRouteValues);
        }

        private IEnumerable<Slide> GetSlides(SlideShowPart part) {
            var storage = new ContentPartStorage(part);
            var slidesData = storage.RetrieveSlidesData();
            return _serializer.Deserialize(slidesData);
        }

        private void SetSlides(SlideShowPart part, IEnumerable<Slide> slides) {
            var storage = new ContentPartStorage(part);
            var slidesData = _serializer.Serialize(slides);
            storage.StoreSlidesData(slidesData);
        }
    }
}