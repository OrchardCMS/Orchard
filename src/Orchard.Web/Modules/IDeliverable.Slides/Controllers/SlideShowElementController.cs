using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IDeliverable.Slides.Elements;
using IDeliverable.Slides.Filters;
using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Models;
using IDeliverable.Slides.Services;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace IDeliverable.Slides.Controllers
{
    [Admin]
    [Dialog]
    public class SlideshowElementController : Controller
    {
        private readonly IObjectStore _objectStore;
        private readonly INotifier _notifier;
        private readonly IElementManager _elementManager;
        private readonly ISlidesSerializer _serializer;

        public SlideshowElementController(IObjectStore objectStore, INotifier notifier, IElementManager elementManager, ISlidesSerializer serializer)
        {
            _objectStore = objectStore;
            _notifier = notifier;
            _elementManager = elementManager;
            _serializer = serializer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult AddSlides(string session, string slides)
        {
            var addedSlides = _objectStore.Get<IList<Slide>>(slides);
            var slideShowElementState = _objectStore.Get<ElementSessionState>(session);
            var slideShowElementDescriptor = _elementManager.GetElementDescriptorByType<Slideshow>();
            var slideShowElement = _elementManager.ActivateElement<Slideshow>(slideShowElementDescriptor, x => x.Data = ElementDataHelper.Deserialize(slideShowElementState.ElementData));
            var slideShowSlides = GetSlides(slideShowElement);
            var currentSlides = slideShowSlides.ToList();

            currentSlides.AddRange(addedSlides);
            SetSlides(slideShowElement, currentSlides);
            slideShowElementState.ElementData = slideShowElement.Data.Serialize();

            _objectStore.Set(session, slideShowElementState);

            return RedirectToEditor(session, T.Plural("That slide has been added.", "Those slides have been added.", addedSlides.Count));
        }

        public ActionResult EditSlide(string session, int index, bool dialog)
        {
            var slideShowElementState = _objectStore.Get<ElementSessionState>(session);
            var slideShowElement = GetSlideshow(slideShowElementState);
            var slides = GetSlides(slideShowElement);
            var slide = slides.ElementAt(index);
            var key = _objectStore.GenerateKey();
            var returnUrl = Url.Action("UpdateSlide", new { session = session, key = key, index = index, dialog = dialog });

            _objectStore.Set(key, slide);
            return RedirectToAction("Edit", "Slide", new { id = key, returnUrl = returnUrl, dialog = dialog });
        }

        public ActionResult UpdateSlide(string session, string key, int index, bool dialog)
        {
            var slideShowElementState = _objectStore.Get<ElementSessionState>(session);
            var slideShowElement = GetSlideshow(slideShowElementState);
            var slide = _objectStore.Get<Slide>(key);
            var slides = GetSlides(slideShowElement).ToList();

            slides[index] = slide;
            SetSlides(slideShowElement, slides);
            slideShowElementState.ElementData = slideShowElement.Data.Serialize();

            _objectStore.Set(session, slideShowElementState);
            return RedirectToEditor(session, dialog: dialog);
        }

        [HttpPost]
        public ActionResult DeleteSlide(string session, int index, bool dialog)
        {
            var slideShowElementState = _objectStore.Get<ElementSessionState>(session);
            var slideShowElement = GetSlideshow(slideShowElementState);
            var currentSlides = GetSlides(slideShowElement).ToList();

            currentSlides.RemoveAt(index);
            SetSlides(slideShowElement, currentSlides);
            slideShowElementState.ElementData = slideShowElement.Data.Serialize();

            _objectStore.Set(session, slideShowElementState);
            return RedirectToEditor(session, T("That slide has been deleted."), dialog: dialog);
        }

        private RedirectToRouteResult RedirectToEditor(string session, LocalizedString message = null, bool dialog = false)
        {
            if(message != null)
                _notifier.Information(message);

            return RedirectToAction("Edit", "Element", new { session = session, dialog = dialog, area = "Orchard.Layouts" });
        }

        private Slideshow GetSlideshow(ElementSessionState sessionState)
        {
            var slideShowElementDescriptor = _elementManager.GetElementDescriptorByType<Slideshow>();
            var slideShowElement = _elementManager.ActivateElement<Slideshow>(slideShowElementDescriptor, x => x.Data = ElementDataHelper.Deserialize(sessionState.ElementData));

            return slideShowElement;
        }

        private IEnumerable<Slide> GetSlides(Slideshow element)
        {
            var storage = new ElementStorage(element);
            var slidesData = storage.RetrieveSlidesData();
            return _serializer.Deserialize(slidesData);
        }

        private void SetSlides(Slideshow element, IEnumerable<Slide> slides)
        {
            var storage = new ElementStorage(element);
            var slidesData = _serializer.Serialize(slides);
            storage.StoreSlidesData(slidesData);
        }
    }
}