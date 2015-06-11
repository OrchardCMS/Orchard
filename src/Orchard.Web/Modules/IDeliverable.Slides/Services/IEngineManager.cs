using System.Collections.Generic;
using IDeliverable.Slides.Models;
using Orchard;

namespace IDeliverable.Slides.Services
{
    public interface IEngineManager : IDependency
    {
        IEnumerable<IEngine> GetEngines();
        IEngine GetEngine(string name);
        IEngine GetEngine(SlideShowProfile profile);
    }
}