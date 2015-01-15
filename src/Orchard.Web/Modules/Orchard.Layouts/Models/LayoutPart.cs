using System;
using Orchard.ContentManagement;
using Orchard.Layouts.Settings;

namespace Orchard.Layouts.Models {
    public class LayoutPart : ContentPart<LayoutPartRecord>, ILayoutAspect {

        public string SessionKey {
            get {
                var key = this.Retrieve(x => x.SessionKey);

                if (String.IsNullOrEmpty(key)) {
                    SessionKey = key = Guid.NewGuid().ToString();
                }

                return key;
            }
            set { this.Store(x => x.SessionKey, value); }
        }

        public string LayoutState {
            get { return this.Retrieve(x => x.LayoutState, versioned: true); }
            set { this.Store(x => x.LayoutState, value, versioned: true); }
        }

        public bool IsTemplate {
            get { return TypePartDefinition.Settings.GetModel<LayoutTypePartSettings>().IsTemplate; }
        }

        public int? TemplateId {
            get { return Retrieve(x => x.TemplateId); }
            set { Store(x => x.TemplateId, value); }
        }

        public string GetFlavor() {
            var typePartSettings = Settings.GetModel<LayoutTypePartSettings>();
            return (typePartSettings != null && !string.IsNullOrWhiteSpace(typePartSettings.Flavor))
                       ? typePartSettings.Flavor
                       : PartDefinition.Settings.GetModel<LayoutPartSettings>().FlavorDefault;
        }
    }
}