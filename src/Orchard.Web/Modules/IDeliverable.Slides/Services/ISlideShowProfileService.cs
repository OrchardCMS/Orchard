using System;
using System.Collections.Generic;
using IDeliverable.Slides.Models;
using Orchard;

namespace IDeliverable.Slides.Services
{
    public interface ISlideShowProfileService : IDependency
    {
        IEnumerable<SlideShowProfile> GetProfiles();
        SlideShowProfile Find(Func<SlideShowProfile, bool> predicate);
        SlideShowProfile FindById(int id);
        SlideShowProfile FindByName(string name);
    }
}