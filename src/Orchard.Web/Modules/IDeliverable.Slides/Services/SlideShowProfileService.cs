using System;
using System.Collections.Generic;
using System.Linq;
using IDeliverable.Slides.Models;
using Orchard;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Services
{
    public class SlideshowProfileService : ISlideshowProfileService
    {
        private readonly IWorkContextAccessor _wca;
        public SlideshowProfileService(IWorkContextAccessor wca)
        {
            _wca = wca;
        }

        public IEnumerable<SlideshowProfile> GetProfiles()
        {
            return _wca.GetContext().CurrentSite.As<SlideshowSettingsPart>().Profiles;
        }

        public SlideshowProfile Find(Func<SlideshowProfile, bool> predicate)
        {
            return GetProfiles().SingleOrDefault(predicate);
        }

        public SlideshowProfile FindById(int id)
        {
            return Find(x => x.Id == id);
        }

        public SlideshowProfile FindByName(string name)
        {
            return Find(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}