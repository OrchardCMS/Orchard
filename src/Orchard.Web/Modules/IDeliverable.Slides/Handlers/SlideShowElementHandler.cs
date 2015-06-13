using IDeliverable.Slides.Elements;
using IDeliverable.Slides.Services;
using Orchard.Layouts.Services;

namespace IDeliverable.Slides.Handlers
{
    public class SlideShowElementHandler : ElementEventHandlerBase
    {
        private readonly ISlideShowProfileService _slideShowProfileService;

        public SlideShowElementHandler(ISlideShowProfileService slideShowProfileService)
        {
            _slideShowProfileService = slideShowProfileService;
        }

        public override void Created(ElementCreatedContext context)
        {
            var element = context.Element as SlideShow;

            if (element == null)
                return;

            SetupLazyFields(element);
        }

        private void SetupLazyFields(SlideShow element)
        {
            element._profileField.Loader(value =>
            {
                var profile = _slideShowProfileService.FindById(element.ProfileId.GetValueOrDefault());
                return profile;
            });
        }
    }
}