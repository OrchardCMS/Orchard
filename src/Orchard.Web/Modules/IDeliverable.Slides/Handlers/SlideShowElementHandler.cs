using System.Linq;
using IDeliverable.Slides.Elements;
using IDeliverable.Slides.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Layouts.Services;

namespace IDeliverable.Slides.Handlers
{
    public class SlideShowElementHandler : ElementEventHandlerBase
    {
        private readonly IOrchardServices _services;

        public SlideShowElementHandler(IOrchardServices services)
        {
            _services = services;
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
                var profiles = _services.WorkContext.CurrentSite.As<SlideShowSettingsPart>().Profiles.ToDictionary(x => x.Id);
                var profile = element.ProfileId != null && profiles.ContainsKey(element.ProfileId.Value) ? profiles[element.ProfileId.Value] : default(SlideShowProfile);
                return profile;
            });
        }
    }
}