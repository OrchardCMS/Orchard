using IDeliverable.Slides.Models;
using IDeliverable.Slides.Services;
using Orchard;
using Orchard.ContentManagement.Handlers;

namespace IDeliverable.Slides.Handlers
{
    public class SlideShowPartHandler : ContentHandler
    {
        private readonly IOrchardServices _services;
        private readonly ISlideShowProfileService _slideShowProfileService;

        public SlideShowPartHandler(IOrchardServices services, ISlideShowProfileService slideShowProfileService)
        {
            _services = services;
            _slideShowProfileService = slideShowProfileService;
            OnActivated<SlideShowPart>(SetupLazyFields);
        }

        private void SetupLazyFields(ActivatedContentContext context, SlideShowPart part)
        {
            part._profileField.Loader(value =>
            {
                var profile = _slideShowProfileService.FindById(part.ProfileId.GetValueOrDefault());
                return profile;
            });
        }
    }
}