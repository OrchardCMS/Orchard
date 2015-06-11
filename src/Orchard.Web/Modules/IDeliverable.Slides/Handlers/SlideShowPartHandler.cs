using System.Linq;
using IDeliverable.Slides.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace IDeliverable.Slides.Handlers {
    public class SlideShowPartHandler : ContentHandler
    {
        private readonly IOrchardServices _services;

        public SlideShowPartHandler(IOrchardServices services)
        {
            _services = services;
            OnActivated<SlideShowPart>(SetupLazyFields);
        }

        private void SetupLazyFields(ActivatedContentContext context, SlideShowPart part)
        {
            part._profileField.Loader(value => {
                var profiles = _services.WorkContext.CurrentSite.As<SlideShowSettingsPart>().Profiles.ToDictionary(x => x.Id);
                var profile = part.ProfileId != null && profiles.ContainsKey(part.ProfileId.Value) ? profiles[part.ProfileId.Value] : default(SlideShowProfile);
                return profile;
            });
        }
    }
}