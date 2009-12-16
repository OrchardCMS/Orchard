using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Models.ViewModels;

namespace Orchard.UI.Zones {
    public class ZoneCollection : Dictionary<string, ZoneEntry> {
        public void AddRenderPartial(string location, string templateName, object model) {

        }
        public void AddDisplayItem(string location, ItemDisplayModel displayModel) {

        }
        public void AddAction(string location, Action<HtmlHelper> action) {

        }
    }
}