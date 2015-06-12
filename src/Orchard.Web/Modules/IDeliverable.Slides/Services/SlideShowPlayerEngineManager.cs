using System;
using System.Collections.Generic;
using System.Linq;
using IDeliverable.Slides.Models;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace IDeliverable.Slides.Services {
    public class SlideShowPlayerEngineManager : ISlideShowPlayerEngineManager
    {
        private readonly Lazy<IEnumerable<ISlideShowPlayerEngine>> _engines;
        public SlideShowPlayerEngineManager(Lazy<IEnumerable<ISlideShowPlayerEngine>> engines)
        {
            _engines = engines;
        }

        public IEnumerable<ISlideShowPlayerEngine> GetEngines()
        {
            return _engines.Value;
        }

        public ISlideShowPlayerEngine GetEngine(string name) {
            return GetEngines().SingleOrDefault(x => x.Name == name);
        }

        public ISlideShowPlayerEngine GetEngine(SlideShowProfile profile) {
            var engine = profile != null ? GetEngine(profile.SelectedEngine) : GetEngines().First();
            engine.Data = profile != null ? ElementDataHelper.Deserialize(profile.EngineStates[profile.SelectedEngine]) : new ElementDataDictionary();
            return engine;
        }
    }
}