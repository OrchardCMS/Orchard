using System;
using System.Collections.Generic;
using System.Linq;
using IDeliverable.Slides.Models;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace IDeliverable.Slides.Services {
    public class EngineManager : IEngineManager
    {
        private readonly Lazy<IEnumerable<IEngine>> _engines;
        public EngineManager(Lazy<IEnumerable<IEngine>> engines)
        {
            _engines = engines;
        }

        public IEnumerable<IEngine> GetEngines()
        {
            return _engines.Value;
        }

        public IEngine GetEngine(string name) {
            return GetEngines().SingleOrDefault(x => x.Name == name);
        }

        public IEngine GetEngine(SlideShowProfile profile) {
            var engine = profile != null ? GetEngine(profile.SelectedEngine) : GetEngines().First();
            engine.Data = profile != null ? ElementDataHelper.Deserialize(profile.EngineStates[profile.SelectedEngine]) : new ElementDataDictionary();
            return engine;
        }
    }
}