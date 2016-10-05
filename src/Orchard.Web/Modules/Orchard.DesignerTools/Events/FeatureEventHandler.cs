using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.DesignerTools.Models;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DesignerTools.Events {
    public class FeatureEventHandler : IFeatureEventHandler {
        private readonly IOrchardServices _services;

        public FeatureEventHandler(IOrchardServices services) {
            _services = services;
        }

        public void Disabled(Feature feature) {

        }

        public void Disabling(Feature feature) {

        }

        public void Enabled(Feature feature) {
            if(feature.Descriptor.Id != "Orchard.DesignerTools") {
                return;
            }

            // Reset to active each time the module is turned back on so it is less confusing for users.
            _services.WorkContext.CurrentSite.As<ShapeTracingSiteSettingsPart>().IsShapeTracingActive = true;
        }

        public void Enabling(Feature feature) {
            
        }

        public void Installed(Feature feature) {
            
        }

        public void Installing(Feature feature) {
            
        }

        public void Uninstalled(Feature feature) {
            
        }

        public void Uninstalling(Feature feature) {
            
        }
    }
}