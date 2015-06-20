using System;
using System.Collections.Generic;
using System.Linq;
using IDeliverable.Slides.Models;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace IDeliverable.Slides.Services
{
    public class SlideshowPlayerEngineManager : ISlideshowPlayerEngineManager
    {
        private readonly Lazy<IEnumerable<ISlideshowPlayerEngine>> _engines;
        public SlideshowPlayerEngineManager(Lazy<IEnumerable<ISlideshowPlayerEngine>> engines)
        {
            _engines = engines;
        }

        public IEnumerable<ISlideshowPlayerEngine> GetEngines()
        {
            return _engines.Value;
        }

        public ISlideshowPlayerEngine GetEngine(string name)
        {
            return GetEngines().SingleOrDefault(x => x.Name == name);
        }

        public ISlideshowPlayerEngine GetEngine(SlideshowProfile profile)
        {
            var engine = profile != null ? GetEngine(profile.SelectedEngine) : GetEngines().First();
            engine.Data = profile != null ? ElementDataHelper.Deserialize(profile.EngineStates[profile.SelectedEngine]) : new ElementDataDictionary();
            return engine;
        }
    }
}