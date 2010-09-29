using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Services {
    public class WidgetService : IWidgetService {
        public WidgetService() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        #region IWidgetService Members

        public IEnumerable<Layer> GetLayers() {
            throw new NotImplementedException();
        }

        #endregion
    }
}