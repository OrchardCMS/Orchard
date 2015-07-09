using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Environment.Features;
using Orchard.Events;

namespace Orchard.Modules.ImportExport {
    public interface IExportEventHandler : IEventHandler {
        void Exporting(dynamic context);
        void Exported(dynamic context);
    }

    public class FeaturesExportHandler : IExportEventHandler {
        private readonly IFeatureManager _featureManager;

        public FeaturesExportHandler(IFeatureManager featureManager) {
            _featureManager = featureManager;
        }

        public void Exporting(dynamic context) {
            if (!((IEnumerable<string>)context.ExportOptions.CustomSteps).Contains("Features")) {
                return;
            }

            var enabledFeatures = _featureManager.GetEnabledFeatures();

            var root = new XElement("Feature", new XAttribute("enable", String.Join(", ", enabledFeatures.Select(x => x.Id))));
            context.Document.Element("Orchard").Add(root);
        }

        public void Exported(dynamic context) {
        }
    }
}

