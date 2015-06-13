using System;
using System.Collections.Generic;
using System.Linq;
using IDeliverable.Slides.Models;
using Orchard;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Services
{
    public class SlideShowProfileService : ISlideShowProfileService
    {
        private readonly IWorkContextAccessor _wca;
        public SlideShowProfileService(IWorkContextAccessor wca)
        {
            _wca = wca;
        }

        public IEnumerable<SlideShowProfile> GetProfiles()
        {
            return _wca.GetContext().CurrentSite.As<SlideShowSettingsPart>().Profiles;
        }

        public SlideShowProfile Find(Func<SlideShowProfile, bool> predicate)
        {
            return GetProfiles().SingleOrDefault(predicate);
        }

        public SlideShowProfile FindById(int id)
        {
            return Find(x => x.Id == id);
        }

        public SlideShowProfile FindByName(string name)
        {
            return Find(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}