using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Models;
using Orchard.Mvc.ViewModels;
using Orchard.Core.Settings.Models;
using Orchard.UI.Models;

namespace Orchard.Core.Settings.ViewModels {
    public class SettingsIndexViewModel : AdminViewModel {
        public IModel Site { get; set; }
        public IEnumerable<ModelTemplate> Editors { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return Site.Id; }
        }

        public string SiteName {
            get { return Site.As<SiteModel>().Record.SiteName; }
            set { Site.As<SiteModel>().Record.SiteName = value; }
        }

        public string SuperUser {
            get { return Site.As<SiteModel>().Record.SuperUser; }
            set { Site.As<SiteModel>().Record.SuperUser = value; }
        }
    }
}
