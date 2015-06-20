using IDeliverable.Slides.Elements;
using IDeliverable.Slides.Services;
using Orchard.Layouts.Services;

namespace IDeliverable.Slides.Handlers
{
    public class SlideshowElementHandler : ElementEventHandlerBase
    {
        private readonly ISlideshowProfileService _slideShowProfileService;

        public SlideshowElementHandler(ISlideshowProfileService slideShowProfileService)
        {
            _slideShowProfileService = slideShowProfileService;
        }

        public override void Created(ElementCreatedContext context)
        {
            var element = context.Element as Slideshow;

            if (element == null)
                return;

            SetupLazyFields(element);
        }

        private void SetupLazyFields(Slideshow element)
        {
            element._profileField.Loader(value =>
            {
                var profile = _slideShowProfileService.FindById(element.ProfileId.GetValueOrDefault());
                return profile;
            });
        }
    }
}