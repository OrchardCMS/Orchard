using System;
using System.Collections.Generic;
using IDeliverable.Slides.Models;
using Orchard;

namespace IDeliverable.Slides.Services
{
    public interface ISlideshowProfileService : IDependency
    {
        IEnumerable<SlideshowProfile> GetProfiles();
        SlideshowProfile Find(Func<SlideshowProfile, bool> predicate);
        SlideshowProfile FindById(int id);
        SlideshowProfile FindByName(string name);
    }
}