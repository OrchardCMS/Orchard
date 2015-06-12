using System.Collections.Generic;
using IDeliverable.Slides.Models;
using Orchard;

namespace IDeliverable.Slides.Services
{
    public interface ISlideShowPlayerEngineManager : IDependency
    {
        IEnumerable<ISlideShowPlayerEngine> GetEngines();
        ISlideShowPlayerEngine GetEngine(string name);
        ISlideShowPlayerEngine GetEngine(SlideShowProfile profile);
    }
}