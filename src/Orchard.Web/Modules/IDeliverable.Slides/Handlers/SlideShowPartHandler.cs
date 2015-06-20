using IDeliverable.Slides.Models;
using IDeliverable.Slides.Services;
using Orchard;
using Orchard.ContentManagement.Handlers;

namespace IDeliverable.Slides.Handlers
{
    public class SlideshowPartHandler : ContentHandler
    {
        private readonly IOrchardServices _services;
        private readonly ISlideshowProfileService _slideShowProfileService;

        public SlideshowPartHandler(IOrchardServices services, ISlideshowProfileService slideShowProfileService)
        {
            _services = services;
            _slideShowProfileService = slideShowProfileService;
            OnActivated<SlideshowPart>(SetupLazyFields);
        }

        private void SetupLazyFields(ActivatedContentContext context, SlideshowPart part)
        {
            part._profileField.Loader(value =>
            {
                var profile = _slideShowProfileService.FindById(part.ProfileId.GetValueOrDefault());
                return profile;
            });
        }
    }
}