using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace TinyMce.Models {
    public class TinyMceSettingsPart : ContentPart {
        public string TinyMceSettingsOverride {
            get { return this.Retrieve(x => x.TinyMceSettingsOverride); }
            set { this.Store(x => x.TinyMceSettingsOverride, value); }
        }
    }
}